using System.Text;
using DotNetEnv;
using FluentValidation;
using FluentValidation.AspNetCore;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Application.Validators;
using IndeConnect_Back.Domain;
using IndeConnect_Back.Infrastructure;
using IndeConnect_Back.Infrastructure.Services.Implementations;
using IndeConnect_Back.Web;
using IndeConnect_Back.Web.Attributes;
using IndeConnect_Back.Web.Handlers;
using IndeConnect_Back.Web.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Formatting.Compact;


var builder = WebApplication.CreateBuilder(args);

// Load .env (optional, but you use DotNetEnv)
Env.Load();

// ---------- CONFIGURATION DB ----------
var postgresDb       = Environment.GetEnvironmentVariable("POSTGRES_DB")       ?? "indeconnect";
var postgresUser     = Environment.GetEnvironmentVariable("POSTGRES_USER")     ?? "indeconnect";
var postgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "indeconnect";
var postgresHost     = Environment.GetEnvironmentVariable("POSTGRES_HOST")     ?? "db";
var postgresPort     = Environment.GetEnvironmentVariable("POSTGRES_PORT")     ?? "5432";

var connectionString =
    $"Host={postgresHost};Port={postgresPort};Database={postgresDb};Username={postgresUser};Password={postgresPassword}";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

// ---------- SERVICES APPLICATION / INFRA ----------
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IBrandSubscriptionService, BrandSubscriptionService>();
builder.Services.AddScoped<IDepositService, DepositService>();
builder.Services.AddSingleton<BrandEthicsScorer>();
builder.Services.AddScoped<IGeocodeService, NominatimGeocodeService>();
builder.Services.AddScoped<IAuditTrailService, AuditTrailService>();

builder.Services.AddHttpClient(); // pour Nominatim
builder.Services.AddMemoryCache();

builder.Services.AddAutoMapper(typeof(DomainAssemblyMarker).Assembly);

// ---------- VALIDATION ----------
builder.Services.AddControllers()
    .AddFluentValidation();

builder.Services.AddValidatorsFromAssemblyContaining<LoginAnonymousRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterAnonymousRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<GetBrandsQueryValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateBrandSubscriptionRequestValidator>();

// ---------- AUTH / JWT ----------
var jwtSecret   = builder.Configuration["JWT_SECRET"]   ?? Environment.GetEnvironmentVariable("JWT_SECRET");
var jwtIssuer   = builder.Configuration["JWT_ISSUER"]   ?? "IndeConnect";
var jwtAudience = builder.Configuration["JWT_AUDIENCE"] ?? "IndeConnectClients";

if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException("JWT_SECRET must be configured.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer          = jwtIssuer,
            ValidateIssuer       = true,
            ValidAudience        = jwtAudience,
            ValidateAudience     = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateLifetime     = true,
            ClockSkew            = TimeSpan.Zero
        };
    });
// ---------- LOGGER ----------
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug() // Tu peux mettre .Information() en prod
    .WriteTo.Console()
    .WriteTo.File(
        new CompactJsonFormatter(),          // JSON compact (idéal pour ingestion d'outils)
        "Logs/indeconnect-.json",            // Chemin/fichier, - pour rolling
        rollingInterval: RollingInterval.Day, // 1 fichier/jour
        retainedFileCountLimit: 21           // Garde les 21 derniers fichiers
    )
    .CreateLogger();

builder.Host.UseSerilog();
// ---------- AUTHORIZATION (policies + handlers) ----------
builder.Services.AddSingleton<IAuthorizationHandler, RegisterAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, GetUserIdHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireUserIdMatch", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(new UserIdAttribute());
    });

    // Exemple si tu veux une policy sur la création de comptes
    options.AddPolicy("CanRegisterRole", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(new RoleAuthorizationAttribute());
    });
});

// HttpContext accessor pour UserHelper
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UserHelper>();

// ---------- SWAGGER ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "IndeConnect API",
        Version     = "v1",
        Description = "API REST pour IndeConnect"
    });

    // JWT auth in Swagger
    var securityScheme = new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Description  = "Enter 'Bearer {token}'",
        In           = ParameterLocation.Header,
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        Reference    = new OpenApiReference
        {
            Id   = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

var app = builder.Build();

// ---------- MIDDLEWARE GLOBAL D’ERREURS ----------
app.UseMiddleware<ExceptionHandlingMiddleware>();

// ---------- MIGRATIONS AUTO (optionnel) ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// ---------- PIPELINE ----------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Healthcheck simple
app.MapGet("/health", () => Results.Ok("ok"));

app.Run();

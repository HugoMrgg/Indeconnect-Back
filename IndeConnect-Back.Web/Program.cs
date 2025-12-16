using System.Text;
using System.Text.Json.Serialization;
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
var postgresDb       = Environment.GetEnvironmentVariable("POSTGRES_DB");
var postgresUser     = Environment.GetEnvironmentVariable("POSTGRES_USER");
var postgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
var postgresHost     = Environment.GetEnvironmentVariable("POSTGRES_HOST");
var postgresPort     = Environment.GetEnvironmentVariable("POSTGRES_PORT");
var connectionString =
    $"Host={postgresHost};Port={postgresPort};Database={postgresDb};Username={postgresUser};Password={postgresPassword}";
Console.WriteLine("postgresDb = " + connectionString);

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
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IAuditTrailService, AuditTrailService>();
builder.Services.AddScoped<IEthicsService, EthicsService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IEmailService, SendGridEmailService>();
builder.Services.AddScoped<IOrderEmailTemplateService, OrderEmailTemplateService>();
builder.Services.AddScoped<IPasswordResetTokenService, PasswordResetTokenService>(); 
builder.Services.AddScoped<IShippingAddressService, ShippingAddressService>();
builder.Services.AddScoped<IShippingService, ShippingService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentMethodService, PaymentMethodService>();

// Background service for automatic order progression
builder.Services.AddHostedService<OrderProgressionService>();

builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

builder.Services.AddAutoMapper(typeof(DomainAssemblyMarker).Assembly);
// ---------- CONFIGURATION CORS (AJOUTER CECI) ----------
const string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod(); 
        });
});

// ---------- VALIDATION ----------
builder.Services.AddControllers()
    .AddFluentValidation();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddValidatorsFromAssemblyContaining<LoginAnonymousRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterAnonymousRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<GetBrandsQueryValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateBrandSubscriptionRequestValidator>();

// ---------- AUTH / JWT ----------
var jwtSecret   = Environment.GetEnvironmentVariable("JWT_SECRET");
var jwtIssuer   = Environment.GetEnvironmentVariable("JWT_ISSUER");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

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
    .MinimumLevel.Debug() 
    .WriteTo.Console()
    .WriteTo.File(
        new CompactJsonFormatter(),       
        "Logs/indeconnect-.json",    
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 21         
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

    options.AddPolicy("CanRegister", policy =>
        policy.Requirements.Add(new RoleAuthorizationAttribute()));

    options.AddPolicy("CanInvite", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new RoleAuthorizationAttribute());
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
// ⚠️ ACTIVATION DU ROUTAGE (si ce n'est pas implicite, bonne pratique)
app.UseRouting();

// 💡 ACTIVATION DU MIDDLEWARE CORS (AJOUTER CECI)
// Doit être placé avant l'authentification et l'autorisation
app.UseCors(MyAllowSpecificOrigins);

//app.UseHttpsRedirection();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Healthcheck simple
app.MapGet("/health", () => Results.Ok("ok"));

app.Run();

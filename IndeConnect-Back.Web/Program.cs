using System.Text;
using IndeConnect_Back.Infrastructure;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain;
using IndeConnect_Back.Infrastructure.Services.Implementations;
using FluentValidation;
using FluentValidation.AspNetCore;
using IndeConnect_Back.Application.Validators;
using IndeConnect_Back.Web.Attributes;
using IndeConnect_Back.Web.Handlers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// Charger les variables d'environnement
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Configuration de la base de données
var host = Environment.GetEnvironmentVariable("POSTGRES_HOST");
var port = Environment.GetEnvironmentVariable("POSTGRES_PORT");
var database = Environment.GetEnvironmentVariable("POSTGRES_DB");
var username = Environment.GetEnvironmentVariable("POSTGRES_USER");
var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Services
builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient(); 
builder.Services.AddMemoryCache(); 

builder.Services.AddScoped<BrandEthicsScorer>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IDepositService, DepositService>();
builder.Services.AddScoped<IGeocodeService, NominatimGeocodeService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IBrandSubscriptionService, BrandSubscriptionService>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterAnonymousRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<LoginAnonymousRequestValidator>();

// Token
var jwtSecret = builder.Configuration["JWT_SECRET"];

if (string.IsNullOrEmpty(jwtSecret))
{
    throw new InvalidOperationException("JWT secret not configured");
}

builder.Services.AddSingleton<IAuthorizationHandler, RegisterAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, GetuserIdHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RegisterPolicy", policy =>
    {
        policy.Requirements.Add(new RoleAuthorizationAttribute());
    });
    options.AddPolicy("UserAccessPolicy", policy =>
        policy.Requirements.Add(new UserIdAttribute()));
});

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; 
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true, // ✅ Vérifie l'expiration
            ClockSkew = TimeSpan.Zero
        };
        
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                Console.WriteLine($"Full Authorization Header: '{authHeader}'");
    
                var token = authHeader?.Split(" ").Last();
                Console.WriteLine($" Token length: {token?.Length ?? 0}");
                Console.WriteLine($" Token start: {token?.Substring(0, Math.Min(50, token?.Length ?? 0))}...");
    
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("✅✅✅ TOKEN VALIDATED SUCCESSFULLY ✅✅✅");
                var claims = context.Principal?.Claims;
                if (claims != null)
                {
                    foreach (var claim in claims)
                    {
                        Console.WriteLine($"  Claim: {claim.Type} = {claim.Value}");
                    }
                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"❌❌❌ AUTHENTICATION FAILED ❌❌❌");
                Console.WriteLine($"Exception: {context.Exception.GetType().Name}");
                Console.WriteLine($"Message: {context.Exception.Message}");
                Console.WriteLine($"Stack: {context.Exception.StackTrace}");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"⚠️⚠️⚠️ CHALLENGE ⚠️⚠️⚠️");
                Console.WriteLine($"Error: '{context.Error}'");
                Console.WriteLine($"ErrorDescription: '{context.ErrorDescription}'");
                Console.WriteLine($"AuthenticateFailure: {context.AuthenticateFailure?.Message}");
                return Task.CompletedTask;
            }
        };
    });



builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "IndeConnect API",
        Version = "v1",
        Description = "API REST pour IndeConnect"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Entrez votre token JWT dans le format: Bearer {votre token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Pas de redirection HTTPS en prod locale
// else
// {
//     app.UseHttpsRedirection();
// }


app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok("ok"));

app.Run();
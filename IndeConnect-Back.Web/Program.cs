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

Env.Load();

var builder = WebApplication.CreateBuilder(args);

var host = Environment.GetEnvironmentVariable("POSTGRES_HOST");
var port = Environment.GetEnvironmentVariable("POSTGRES_PORT");
var database = Environment.GetEnvironmentVariable("POSTGRES_DB");
var username = Environment.GetEnvironmentVariable("POSTGRES_USER");
var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

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
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
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
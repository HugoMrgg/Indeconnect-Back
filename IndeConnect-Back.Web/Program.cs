using System.Text;
using IndeConnect_Back.Infrastructure;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Infrastructure.services.Implementations;
using IndeConnect_Back.Infrastructure.Services.Implementations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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

builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();
var jwtSecret = builder.Configuration["JWT_SECRET"]; // ou "Jwt:Secret" selon ton JwtTokenGenerator

if (string.IsNullOrEmpty(jwtSecret))
{
    throw new InvalidOperationException("JWT secret not configured");
}

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // en dev
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
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
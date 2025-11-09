using IndeConnect_Back.Infrastructure;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using IndeConnect_Back.Application.Services.Interfaces;
using IndeConnect_Back.Domain;
using IndeConnect_Back.Infrastructure.Services.Implementations;

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

builder.Services.AddScoped<BrandEthicsScorer>();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<IDepositService, DepositService>();
builder.Services.AddScoped<IGeocodeService, NominatimGeocodeService>();

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
app.MapControllers();

app.MapGet("/health", () => Results.Ok("ok"));

app.Run();
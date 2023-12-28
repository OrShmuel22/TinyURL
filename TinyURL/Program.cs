using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using TinyURL.Core.Interfaces;
using TinyURL.Core.Models;
using TinyURL.Data.Context;
using TinyURL.Data.Repositories;
using TinyURL.Services;
using TinyURL.Services.Caching;

var builder = WebApplication.CreateBuilder(args);

// Add services to the DI container.

// Configuration for MongoDB
builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings"));

builder.Services.AddSingleton<MongoDbContext>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    var logger = sp.GetRequiredService<ILogger<MongoDbContext>>();
    return new MongoDbContext(settings, logger);
});

// Register CustomMemoryCache<T> with a specified capacity (e.g., 100)
builder.Services.AddSingleton<ICustomMemoryCache<UrlEntry>>(sp =>
{
    int cacheCapacity = 100; //cache capacity
    return new CustomMemoryCache<UrlEntry>(cacheCapacity);
});

// Repository registrations
builder.Services.AddScoped<IUrlEntryRepository, UrlEntryRepository>();

// Service registrations
builder.Services.AddScoped<IUrlShorteningService, UrlShorteningService>();

// Add controllers
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();

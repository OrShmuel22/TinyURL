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
builder.Services.Configure<MemoryCacheSetting>(
    builder.Configuration.GetSection("MemoryCacheSetting"));

// Retrieve UrlShorteningSettings
var urlShorteningSettings = builder.Configuration.GetSection("UrlShorteningSettings").Get<UrlShorteningSettings>();

builder.Services.AddSingleton<MongoDbContext>(sp =>
{
    var mongoSettings = sp.GetRequiredService<IOptions<MongoDBSettings>>().Value;
    var logger = sp.GetRequiredService<ILogger<MongoDbContext>>();
    return new MongoDbContext(mongoSettings, logger);
});

// Register CustomMemoryCache<T> with a specified capacity (e.g., 100)
builder.Services.AddSingleton<ICustomMemoryCache<string>>(sp =>
{
    var cacheSettings = sp.GetRequiredService<IOptions<MemoryCacheSetting>>().Value;

    return new CustomMemoryCacheService<string>(cacheSettings);
});



builder.Services.AddSingleton<IBase62Encoding, Base62Service>();

// Repository registrations
builder.Services.AddScoped<IUrlEntryRepository, MongoUrlRepository>();

// Service registrations
// Register UrlShorteningService with the settings
builder.Services.AddScoped<IUrlShorteningService>(serviceProvider =>
    new UrlShorteningService(urlShorteningSettings,
                             serviceProvider.GetRequiredService<IUrlEntryRepository>(),
                             serviceProvider.GetRequiredService<ICustomMemoryCache<string>>(),
                             serviceProvider.GetRequiredService<IBase62Encoding>()));

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

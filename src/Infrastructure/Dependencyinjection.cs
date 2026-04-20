using Application.Common.Interfaces;
using Infrastructure.Caching;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Domain.Interfaces;


namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── PostgreSQL ─────────────────────────────────────────
        // Converts Railway's postgresql:// URL to Npgsql format
        var dbUrl = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Database connection string is missing.");



        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(dbUrl));

        services.AddScoped<ISlideRepository, SlideRepository>();

        // ── Redis ──────────────────────────────────────────────
        // Converts Railway's redis:// URL to StackExchange format
        var redisUrl = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Redis connection string is missing.");


        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisUrl));

        services.AddScoped<ICacheService, RedisCacheService>();

        return services;
    }



}
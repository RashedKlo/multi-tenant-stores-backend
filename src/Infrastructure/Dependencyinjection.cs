using Application.Common.Interfaces;
using Infrastructure.Caching;
using Infrastructure.Persistence;
using Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


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
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // ── Redis ──────────────────────────────────────────────
        // Converts Railway's redis:// URL to StackExchange format
        var redisUrl = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Redis connection string is missing.");


        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisUrl));

        services.AddScoped<ICacheService, RedisCacheService>();

        // ── JWT Authentication ─────────────────────────────────
        services.AddScoped<IJwtService, JwtService>();

        var jwtSecret = configuration["Jwt:Secret"] 
            ?? throw new InvalidOperationException("JWT secret is not configured");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
                };
            });

        services.AddAuthorization();

        return services;
    }



}
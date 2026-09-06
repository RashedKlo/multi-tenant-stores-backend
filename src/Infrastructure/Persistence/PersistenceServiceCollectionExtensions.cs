using System.Data;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Npgsql.NameTranslation;

namespace Infrastructure.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Database connection string is missing.");

        // Labels in DB are PascalCase: 'Pending', 'Confirmed', ...
        // Null translator keeps CLR names as-is (no snake_case).
        var enumNameTranslator = new NpgsqlNullNameTranslator();

        services.AddDbContext<AppDbContext>(options =>
            options
                .UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.MapEnum<OrderStatus>("order_status", nameTranslator: enumNameTranslator);
                    npgsql.MapEnum<PaymentStatus>("payment_status", nameTranslator: enumNameTranslator);
                })
                .UseSnakeCaseNamingConvention()
                .EnableDetailedErrors()
    .EnableSensitiveDataLogging()
                );
        

        // Dapper factory — also map enums so raw connections can read/write them
        services.AddSingleton<IDbConnectionFactory>(_ =>
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.MapEnum<OrderStatus>("order_status", enumNameTranslator);
            dataSourceBuilder.MapEnum<PaymentStatus>("payment_status", enumNameTranslator);
            var dataSource = dataSourceBuilder.Build();

            return new NpgsqlDataSourceConnectionFactory(dataSource);
        });

        return services;
    }
}
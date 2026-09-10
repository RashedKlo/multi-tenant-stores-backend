using Application.Common.Interfaces;
using Infrastructure.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Queries;

public static class QueryServiceCollectionExtensions
{
    public static IServiceCollection AddQueries(this IServiceCollection services)
    {
        services.AddScoped<ICartQueries, CartQueries>();
        services.AddScoped<ICatalogQueries, CatalogQueries>();
        services.AddScoped<IDiscoveryQueries, DiscoveryQueries>();
        return services;
    }
}
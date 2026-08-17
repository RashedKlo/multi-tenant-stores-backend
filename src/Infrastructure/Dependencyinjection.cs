using Application.Common.Interfaces;
using Infrastructure.Caching;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            options.UseNpgsql(dbUrl)
            .UseSnakeCaseNamingConvention());

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IStoreRepository, StoreRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IModuleRepository, ModuleRepository>();
        services.AddScoped<IProductImageRepository, ProductImageRepository>();
        services.AddScoped<IHomeBannerRepository, HomeBannerRepository>();
        services.AddScoped<IModuleBannerRepository, ModuleBannerRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IStoreCategoryRepository, StoreCategoryRepository>();
        services.AddScoped<IStoreBannerRepository, StoreBannerRepository>();
        services.AddScoped<IStoreSectionRepository, StoreSectionRepository>();
        services.AddScoped<IProductOptionGroupRepository, ProductOptionGroupRepository>();
        services.AddScoped<IProductOptionRepository, ProductOptionRepository>();
        services.AddScoped<IDiscountRepository, DiscountRepository>();
        services.AddScoped<IDiscountProductRepository, DiscountProductRepository>();
        services.AddScoped<IDiscountSectionRepository, DiscountSectionRepository>();

        ServiceCollectionExtensions.AddInfrastructureServices(services, configuration);

        return services;
    }



}
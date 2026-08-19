using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Repositories;
namespace Infrastructure.Persistence;

public static class RepositoryServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
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
        services.AddScoped<ICartRepository, CartRepository>();
services.AddScoped<ICustomerRepository, CustomerRepository>();
services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
services.AddScoped<ICustomerAddressRepository, CustomerAddressRepository>();
services.AddScoped<IGuestSessionRepository, GuestSessionRepository>();
services.AddScoped<IFavoriteStoreRepository, FavoriteStoreRepository>();
services.AddScoped<IFavoriteProductRepository, FavoriteProductRepository>();
        

        return services;
    }
}
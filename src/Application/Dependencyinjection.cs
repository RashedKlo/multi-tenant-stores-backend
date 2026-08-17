using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

using Application.HomeBanners.Commands.CreateHomeBanner;
using Application.HomeBanners.Commands.UpdateHomeBanner;
using Application.ModuleBanners.Commands.CreateModuleBanner;
using Application.ModuleBanners.Commands.UpdateModuleBanner;
using Application.Modules.Commands.CreateModule;
using Application.Modules.Commands.UpdateModule;
using Application.Modules.Commands.DeleteModule;
using Application.ProductImages.Commands.CreateProductImage;
using Application.ProductImages.Commands.UpdateProductImage;
using Application.ProductOptions.Commands.CreateProductOption;
using Application.ProductOptions.Commands.UpdateProductOption;
using Application.ProductOptionGroups.Commands.CreateProductOptionGroup;
using Application.ProductOptionGroups.Commands.UpdateProductOptionGroup;
using Application.Stores.Commands.CreateStore;
using Application.Stores.Commands.UpdateStore;
using Application.Tenants.Commands.CreateTenant;
using Application.Tenants.Commands.UpdateTenant;
using Application.Discounts.Products.Commands.CreateDiscountProduct;
using Application.Discounts.Sections.Commands.CreateDiscountSection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // MediatR - scans all handlers in this assembly
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // FluentValidation - scans all validators in this assembly

        services.AddValidatorsFromAssemblyContaining<CreateStoreValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateStoreValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateTenantValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateTenantValidator>();

        services.AddValidatorsFromAssemblyContaining<CreateHomeBannerValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateHomeBannerValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateModuleBannerValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateModuleBannerValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateModuleValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateModuleValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateProductImageValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateProductImageValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateProductOptionValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateProductOptionValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateProductOptionGroupValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateProductOptionGroupValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateDiscountProductValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateDiscountSectionValidator>();

        return services;
    }
}
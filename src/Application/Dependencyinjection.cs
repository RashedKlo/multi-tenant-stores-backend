using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Application.Slides.Commands.CreateSlide;
using Application.Slides.Commands.UpdateSlide;
using Application.Stores.Commands.CreateStore;
using Application.Stores.Commands.UpdateStore;
using Application.Tenants.Commands.CreateTenant;
using Application.Tenants.Commands.UpdateTenant;
using Application.Users.Commands.Register;
using Application.Users.Commands.Login;
using Application.Users.Commands.UpdateUser;
using Application.Users.Commands.DeleteUser;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // MediatR - scans all handlers in this assembly
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // FluentValidation - scans all validators in this assembly
        services.AddValidatorsFromAssemblyContaining<CreateSlideValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateSlideValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateStoreValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateStoreValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateTenantValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateTenantValidator>();
        services.AddValidatorsFromAssemblyContaining<RegisterCommandValidator>();
        services.AddValidatorsFromAssemblyContaining<LoginCommandValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateUserCommandValidator>();

        return services;
    }
}
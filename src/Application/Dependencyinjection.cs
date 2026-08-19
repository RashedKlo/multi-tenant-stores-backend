using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;



namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // MediatR - scans all handlers in this assembly
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // FluentValidation - scans all validators in this assembly

        services.AddValidatorsFromAssemblyContaining(typeof(DependencyInjection));

        return services;
    }
}
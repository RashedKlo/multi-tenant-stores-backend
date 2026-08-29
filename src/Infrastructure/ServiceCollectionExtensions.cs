using Application.Common.Interfaces;
using Domain.Interfaces;
using Infrastructure.Services;
using Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Stripe;

namespace Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.Configure<JwtSettings>(config.GetSection(JwtSettings.SectionName));
        services.Configure<SmtpSettings>(config.GetSection(SmtpSettings.SectionName));
        services.Configure<GoogleAuthSettings>(config.GetSection(GoogleAuthSettings.SectionName));
        services.Configure<StripeSettings>(config.GetSection(StripeSettings.SectionName));

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(config.GetConnectionString("Redis")!));

        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<IVerificationCodeStore, RedisVerificationCodeStore>();
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<IGoogleTokenVerifier, GoogleTokenVerifier>();
        services.AddScoped<IPaymentService,StripePaymentService>();
        services.AddScoped<IStripeWebhookService,StripeWebhookService>();

        return services;
    }
}
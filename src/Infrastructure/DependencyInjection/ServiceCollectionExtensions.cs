using Application.Common.Interfaces;
using Infrastructure.Services;
using Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;


public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        // Options
        services.Configure<JwtSettings>(config.GetSection(JwtSettings.SectionName));
        services.Configure<SmtpSettings>(config.GetSection(SmtpSettings.SectionName));
        services.Configure<GoogleAuthSettings>(config.GetSection(GoogleAuthSettings.SectionName));

        // Redis — one multiplexer shared by cache + verification codes
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(config.GetConnectionString("Redis")!));

        // HTTP context for current user
        services.AddHttpContextAccessor();

        // Auth / common services used by Application.Auth feature
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<IVerificationCodeStore, RedisVerificationCodeStore>();
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<IGoogleTokenVerifier, GoogleTokenVerifier>();

        return services;
    }
}

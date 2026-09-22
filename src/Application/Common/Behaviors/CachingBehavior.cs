using Application.Common.Interfaces;
using MediatR;

namespace Application.Common.Behaviors;

public sealed class CachingBehavior<TRequest, TResponse>(
    ICacheService cache,
    ICurrentLanguageProvider languageProvider)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICacheableQuery cacheable)
            return await next();

        var cacheKey = BuildCacheKey(cacheable);

        var cached = await cache.GetAsync<TResponse>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var response = await next();
        await cache.SetAsync(
            cacheKey,
            response,
            cacheable.Expiration ?? TimeSpan.FromMinutes(5),
            cancellationToken);

        return response;
    }

    private string BuildCacheKey(ICacheableQuery cacheable) =>
        $"{cacheable.CacheKey}:lang:{languageProvider.Language}";
}
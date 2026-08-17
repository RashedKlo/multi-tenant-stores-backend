using Application.Common.Interfaces;
using Application.Discovery.DTOs;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Discovery.Queries.GetModules;

public class GetModulesHandler(IModuleRepository repository, ICacheService cache)
    : IRequestHandler<GetModulesQuery, Result<List<ModuleDto>>>
{
    private const string CacheKey = "modules:all";

    public async Task<Result<List<ModuleDto>>> Handle(
        GetModulesQuery request, CancellationToken cancellationToken)
    {
        var cached = await cache.GetAsync<List<ModuleDto>>(CacheKey, cancellationToken);
        if (cached is not null)
            return Result<List<ModuleDto>>.Success(cached);

        var modules = await repository.GetActiveOrderedAsync(cancellationToken);
        var dtos = modules.Select(ModuleDto.FromEntity).ToList();

        await cache.SetAsync(CacheKey, dtos, TimeSpan.FromMinutes(30), cancellationToken);
        return Result<List<ModuleDto>>.Success(dtos);
    }
}
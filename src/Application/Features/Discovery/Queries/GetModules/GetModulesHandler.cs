using Application.Common.Interfaces;
using Application.Discovery.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Discovery.Queries.GetModules;

public sealed class GetModulesHandler(
    IDiscoveryQueries discoveryQueries,
    ICurrentLanguageProvider currentLanguageProvider)
    : IRequestHandler<GetModulesQuery, Result<IReadOnlyList<ModuleDto>>>
{
    public async Task<Result<IReadOnlyList<ModuleDto>>> Handle(
        GetModulesQuery request,
        CancellationToken cancellationToken)
    {
        var modules = await discoveryQueries.GetModulesAsync(
            currentLanguageProvider.Language,
            cancellationToken);

        return Result<IReadOnlyList<ModuleDto>>.Success(modules);
    }
}
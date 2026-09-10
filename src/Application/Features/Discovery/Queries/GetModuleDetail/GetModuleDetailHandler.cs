using Application.Common.Interfaces;
using Application.Discovery.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Discovery.Queries.GetModuleDetail;

public sealed class GetModuleDetailHandler(
    IDiscoveryQueries discoveryQueries,
    ICurrentLanguageProvider currentLanguageProvider)
    : IRequestHandler<GetModuleDetailQuery, Result<ModuleDetailDto>>
{
    public async Task<Result<ModuleDetailDto>> Handle(
        GetModuleDetailQuery request,
        CancellationToken cancellationToken)
    {
        var module = await discoveryQueries.GetModuleDetailAsync(
            request.ModuleId,
            currentLanguageProvider.Language,
            cancellationToken);

        return module is null
            ? Result<ModuleDetailDto>.Failure(Error.NotFound("Module.NotFound", "Module not found."))
            : Result<ModuleDetailDto>.Success(module);
    }
}
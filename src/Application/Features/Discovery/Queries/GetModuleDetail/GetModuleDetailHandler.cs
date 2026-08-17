using Application.Discovery.DTOs;
using Domain.Interfaces;
using MediatR;
using Domain.Common;
namespace Application.Discovery.Queries.GetModuleDetail;

/// <summary>
/// No cache-aside on purpose: composes three repositories.
/// Invalidating a combined cache correctly is more moving parts than it's worth
/// while the underlying tables stay small and rarely written.
/// </summary>
public class GetModuleDetailHandler(
    IModuleRepository moduleRepository,
    IModuleBannerRepository bannerRepository,
    ICategoryRepository categoryRepository)
    : IRequestHandler<GetModuleDetailQuery, Result<ModuleDetailDto>>
{
    public async Task<Result<ModuleDetailDto>>   Handle(
        GetModuleDetailQuery request, CancellationToken cancellationToken)
    {
        var module = await moduleRepository.GetByIdAsync(request.ModuleId, cancellationToken);

         if (module is null)
            return Result<ModuleDetailDto>.Failure(
                Error.NotFound("Module.NotFound", $"Module with id '{request.ModuleId}' was not found."));


        // Independent reads — run concurrently.
        var bannersTask = bannerRepository.GetByModuleIdAsync(request.ModuleId, cancellationToken);
        var categoriesTask = categoryRepository.GetByModuleIdAsync(request.ModuleId, cancellationToken);
        await Task.WhenAll(bannersTask, categoriesTask);

        return Result<ModuleDetailDto>.Success(ModuleDetailDto.FromEntity(module));
    }
}
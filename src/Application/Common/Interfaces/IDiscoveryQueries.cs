using Application.Common.Models;
using Application.Discovery.DTOs;

namespace Application.Common.Interfaces;

public interface IDiscoveryQueries
{
    Task<IReadOnlyList<HomeBannerDto>> GetHomeBannersAsync(
        Language lang,
        CancellationToken ct = default);

    Task<IReadOnlyList<ModuleDto>> GetModulesAsync(
        Language lang,
        CancellationToken ct = default);

    Task<ModuleDetailDto?> GetModuleDetailAsync(
        Guid moduleId,
        Language lang,
        CancellationToken ct = default);

    Task<PagedResult<StoreSummaryDto>> GetStoresByModuleAsync(
        Guid moduleId,
        Guid? categoryId,
        string? search,
        int page,
        int pageSize,
        Language lang,
        CancellationToken ct = default);
}

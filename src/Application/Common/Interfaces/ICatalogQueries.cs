using Application.Catalog.DTOs;
using Application.Common.Models;

namespace Application.Common.Interfaces;

public interface ICatalogQueries
{
    Task<StoreDetailDto?> GetStoreByIdAsync(
        Guid storeId,
        Guid? customerId,
        Language lang,
        CancellationToken ct = default);

    Task<IReadOnlyList<StoreBannerDto>> GetStoreBannersAsync(
        Guid storeId,
        Language lang,
        CancellationToken ct = default);

    Task<PagedResult<StoreSectionDto>> GetStoreSectionsAsync(
        Guid storeId,
        int page,
        int pageSize,
        Language lang,
        CancellationToken ct = default);

    Task<PagedResult<ProductSummaryDto>> GetProductsBySectionAsync(
        Guid sectionId,
        bool? inStockOnly,
        decimal? minPrice,
        decimal? maxPrice,
        int page,
        int pageSize,
        Language lang,
        CancellationToken ct = default);

    Task<ProductDetailDto?> GetProductByIdAsync(
        Guid productId,
        Guid? customerId,
        Language lang,
        CancellationToken ct = default);
}

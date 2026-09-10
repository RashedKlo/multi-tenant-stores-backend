using Domain.Entities;

namespace Domain.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Untracked, with images + option groups + options eager-loaded —
    // backs GET /api/products/{id}.
    Task<Product?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAndActiveAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    // Backs GET /api/sections/{id}/products?status=&minPrice=&maxPrice=&page=
    Task<(List<Product> Items, int TotalCount)> GetPagedBySectionAsync(
        Guid sectionId,
        bool? inStockOnly,
        decimal? minPrice,
        decimal? maxPrice,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    void Add(Product product);
    void Update(Product product);
    void Delete(Product product);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

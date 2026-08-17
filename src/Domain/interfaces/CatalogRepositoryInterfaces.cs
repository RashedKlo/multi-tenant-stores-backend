using Domain.Entities;

namespace Domain.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Tenant?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    void Add(Tenant tenant);
    void Update(Tenant tenant);
    void Delete(Tenant tenant);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IModuleRepository
{
    Task<Module?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Module>> GetActiveOrderedAsync(CancellationToken cancellationToken = default);

    void Add(Module module);
    void Update(Module module);
    void Delete(Module module);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IHomeBannerRepository
{
    Task<HomeBanner?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<HomeBanner>> GetActiveOrderedAsync(CancellationToken cancellationToken = default);

    void Add(HomeBanner banner);
    void Update(HomeBanner banner);
    void Delete(HomeBanner banner);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IModuleBannerRepository
{
    Task<ModuleBanner?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<ModuleBanner>> GetByModuleIdAsync(Guid moduleId, CancellationToken cancellationToken = default);

    void Add(ModuleBanner banner);
    void Update(ModuleBanner banner);
    void Delete(ModuleBanner banner);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IStoreRepository
{
    // Tracked — use for command flows (create/update a store).
    Task<Store?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Untracked, with images/sections eager-loaded as needed by the caller —
    // use for the customer-facing "store detail" read.
    Task<Store?> GetByIdReadOnlyAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<Store>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    // Backs GET /api/modules/{id}/stores?categoryId=&search=
    Task<(List<Store> Items, int TotalCount)> GetPagedByModuleAsync(
        Guid moduleId,
        Guid? categoryId,
        string? search,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    void Add(Store store);
    void Update(Store store);
    void Delete(Store store);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IStoreSectionRepository
{
    Task<StoreSection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Backs GET /api/stores/{id}/sections?page=&pageSize= ("load more")
    Task<(List<StoreSection> Items, int TotalCount)> GetPagedByStoreIdAsync(
        Guid storeId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    void Add(StoreSection section);
    void Update(StoreSection section);
    void Delete(StoreSection section);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

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

public interface IProductOptionGroupRepository
{
    Task<ProductOptionGroup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<ProductOptionGroup>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);

    void Add(ProductOptionGroup group);
    void Update(ProductOptionGroup group);
    void Delete(ProductOptionGroup group);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IProductOptionRepository
{
    Task<ProductOption?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<ProductOption>> GetByOptionGroupIdAsync(Guid optionGroupId, CancellationToken cancellationToken = default);

    // Used by cart-add validation: fetch selected options with their
    // parent product id in one round trip (see fn_cart_item_options_check_product
    // logic, now moved to the application layer).
    Task<List<ProductOption>> GetByIdsWithGroupAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

    void Add(ProductOption option);
    void Update(ProductOption option);
    void Delete(ProductOption option);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IDiscountRepository
{
    Task<Discount?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Discount>> GetActiveByStoreIdAsync(Guid storeId, DateTimeOffset asOf, CancellationToken cancellationToken = default);

    void Add(Discount discount);
    void Update(Discount discount);
    void Delete(Discount discount);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IDiscountSectionRepository
{
    Task<List<DiscountSection>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DiscountSection?> GetByIdsAsync(Guid discountId, Guid sectionId, CancellationToken cancellationToken = default);

    Task AddAsync(DiscountSection ds, CancellationToken cancellationToken = default);
    void Delete(DiscountSection ds);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
public interface IDiscountProductRepository
{
    Task<List<DiscountProduct>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DiscountProduct?> GetByIdsAsync(Guid discountId, Guid ProductId, CancellationToken cancellationToken = default);

    Task AddAsync(DiscountProduct ds, CancellationToken cancellationToken = default);
    void Delete(DiscountProduct ds);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

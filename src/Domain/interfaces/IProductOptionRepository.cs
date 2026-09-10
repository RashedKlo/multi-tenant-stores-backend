using Domain.Entities;

namespace Domain.Interfaces;

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

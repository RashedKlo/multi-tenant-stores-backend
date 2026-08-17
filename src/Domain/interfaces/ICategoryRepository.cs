using Domain.Entities;

namespace Domain.Interfaces;
public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Category?> GetByIdReadOnlyAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<Category>> GetByModuleIdAsync(Guid moduleId, CancellationToken cancellationToken = default);

    Task<(List<Category> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? moduleId = null,
        CancellationToken cancellationToken = default);

    void Add(Category category);
    void Update(Category category);
    void Delete(Category category);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

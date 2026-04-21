using Domain.Interfaces;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Departments.Commands.DeleteDepartment;

public class DeleteDepartmentHandler : IRequestHandler<DeleteDepartmentCommand, bool>
{
    private readonly IDepartmentRepository _repository;
    private readonly ICacheService _cache;

    public DeleteDepartmentHandler(IDepartmentRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<bool> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (department is null)
            return false;

        _repository.Delete(department);
        await _repository.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cache.RemoveAsync($"departments:store:{department.StoreId}", cancellationToken);
        await _cache.RemoveAsync("departments:all", cancellationToken);

        return true;
    }
}

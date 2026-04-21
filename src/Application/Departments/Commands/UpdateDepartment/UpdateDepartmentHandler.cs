using Application.Common.DTOs;
using Application.Common.Interfaces;
using Domain.Interfaces;
using MediatR;

namespace Application.Departments.Commands.UpdateDepartment;

public class UpdateDepartmentHandler : IRequestHandler<UpdateDepartmentCommand, DepartmentDto?>
{
    private readonly IDepartmentRepository _repository;
    private readonly ICacheService _cache;

    public UpdateDepartmentHandler(IDepartmentRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<DepartmentDto?> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (department is null)
            return null;

        department.Update(request.Name, request.ImageUrl);
        _repository.Update(department);
        await _repository.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cache.RemoveAsync($"departments:store:{department.StoreId}", cancellationToken);
        await _cache.RemoveAsync("departments:all", cancellationToken);

        return new DepartmentDto(
            department.Id, department.StoreId, department.Name,
            department.ImageUrl, department.CreatedAt, department.UpdatedAt
        );
    }
}

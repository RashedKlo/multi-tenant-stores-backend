using Application.Common.DTOs;
using Application.Common.Interfaces;
using Domain.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Departments.Commands.CreateDepartment;

public class CreateDepartmentHandler : IRequestHandler<CreateDepartmentCommand, DepartmentDto>
{
    private readonly IDepartmentRepository _repository;
    private readonly ICacheService _cache;

    public CreateDepartmentHandler(IDepartmentRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<DepartmentDto> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = Department.Create(request.StoreId, request.Name, request.ImageUrl);

        await _repository.AddAsync(department, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        await _cache.RemoveAsync($"departments:store:{request.StoreId}", cancellationToken);
        await _cache.RemoveAsync("departments:all", cancellationToken);

        return new DepartmentDto(
            department.Id, department.StoreId, department.Name,
            department.ImageUrl, department.CreatedAt, department.UpdatedAt
        );
    }
}

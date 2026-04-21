using Application.Common.DTOs;
using Domain.Interfaces;
using MediatR;

namespace Application.Departments.Queries.GetDepartmentById;

public class GetDepartmentByIdHandler : IRequestHandler<GetDepartmentByIdQuery, DepartmentDto?>
{
    private readonly IDepartmentRepository _repository;

    public GetDepartmentByIdHandler(IDepartmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<DepartmentDto?> Handle(GetDepartmentByIdQuery request, CancellationToken cancellationToken)
    {
        var department = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (department is null)
            return null;

        return new DepartmentDto(
            department.Id, department.StoreId, department.Name,
            department.ImageUrl, department.CreatedAt, department.UpdatedAt
        );
    }
}

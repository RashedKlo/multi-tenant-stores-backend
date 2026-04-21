using Application.Common.DTOs;
using MediatR;

namespace Application.Departments.Commands.CreateDepartment;

public record CreateDepartmentCommand(
    Guid StoreId,
    string Name,
    string? ImageUrl
) : IRequest<DepartmentDto>;

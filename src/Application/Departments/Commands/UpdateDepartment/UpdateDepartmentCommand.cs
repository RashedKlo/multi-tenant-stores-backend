using Application.Common.DTOs;
using MediatR;

namespace Application.Departments.Commands.UpdateDepartment;

public record UpdateDepartmentCommand(
    Guid Id,
    string Name,
    string? ImageUrl
) : IRequest<DepartmentDto?>;

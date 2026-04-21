using Application.Common.DTOs;
using MediatR;

namespace Application.Departments.Queries.GetDepartmentById;

public record GetDepartmentByIdQuery(Guid Id) : IRequest<DepartmentDto?>;

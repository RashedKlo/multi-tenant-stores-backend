using Application.Common.DTOs;
using MediatR;

namespace Application.Departments.Queries.GetAllDepartments;

public record GetAllDepartmentsQuery() : IRequest<List<DepartmentDto>>;

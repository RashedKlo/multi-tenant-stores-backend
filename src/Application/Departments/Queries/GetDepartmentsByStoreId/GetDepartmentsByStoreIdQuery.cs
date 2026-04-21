using Application.Common.DTOs;
using MediatR;

namespace Application.Departments.Queries.GetDepartmentsByStoreId;

public record GetDepartmentsByStoreIdQuery(Guid StoreId) : IRequest<List<DepartmentDto>>;

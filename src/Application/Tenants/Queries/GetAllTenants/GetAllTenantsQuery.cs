using Application.Tenants.DTOs;
using MediatR;

namespace Application.Tenants.Queries.GetAllTenants;

public record GetAllTenantsQuery() : IRequest<List<TenantDto>>;

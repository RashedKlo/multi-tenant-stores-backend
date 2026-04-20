using Application.Tenants.DTOs;
using MediatR;

namespace Application.Tenants.Queries.GetTenantById;

public record GetTenantByIdQuery(Guid Id) : IRequest<TenantDto?>;
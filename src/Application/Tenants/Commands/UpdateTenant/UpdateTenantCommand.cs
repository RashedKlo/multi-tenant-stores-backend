using Application.Tenants.DTOs;
using MediatR;

namespace Application.Tenants.Commands.UpdateTenant;

public record UpdateTenantCommand
(
    Guid Id,
 string Name,
 string Email,
 string PasswordHash,
 bool IsActive
    ) : IRequest<TenantDto>;


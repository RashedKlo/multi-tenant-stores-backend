using Application.Tenants.DTOs;
using MediatR;

namespace Application.Tenants.Commands.CreateTenant;

public record CreateTenantCommand
(string Name,
 string Email,
 string PasswordHash
    )
: IRequest<TenantDto>;

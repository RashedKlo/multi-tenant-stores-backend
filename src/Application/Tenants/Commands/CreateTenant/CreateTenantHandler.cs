using Application.Common.Interfaces;
using Application.Tenants.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Tenants.Commands.CreateTenant;

public class CreateTenantHandler(ITenantRepository tenantRepository, ICacheService cache) : IRequestHandler<CreateTenantCommand, TenantDto>
{

    public async Task<TenantDto> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = Tenant.Create(request.Name, request.Email, request.PasswordHash);

        await tenantRepository.AddAsync(tenant, cancellationToken);
        await tenantRepository.SaveChangesAsync(cancellationToken);

        // Invalidate list cache since data changed
        await cache.RemoveAsync("tenants:all", cancellationToken);

        return new TenantDto(
            tenant.Id,
            tenant.Name,
            tenant.Email,
            tenant.IsActive,
            tenant.CreatedAt,
            tenant.UpdatedAt,
            tenant.DeletedAt
        );
    }
}
using Application.Common.DTOs;
using Application.Common.Interfaces;
using MediatR;
using Domain.Interfaces;
using Application.Tenants.Commands.UpdateTenant;
using Application.Tenants.DTOs;
namespace Application.Tenants.Commands.UpdatedTenant;

public class UpdatedTenantHandler(ITenantRepository repository, ICacheService cache) : IRequestHandler<UpdateTenantCommand, TenantDto?>
{

    public async Task<TenantDto?> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        var dTenant = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (dTenant is null) return null;

        dTenant.Update(
               request.Name,
               request.Email,
               request.PasswordHash,
               request.IsActive
           );

        repository.Update(dTenant);
        await repository.SaveChangesAsync(cancellationToken);

        // Invalidate both caches
        await cache.RemoveAsync("tenants:all", cancellationToken);
        await cache.RemoveAsync($"tenants:{dTenant.Id}", cancellationToken);

        return new TenantDto(
            dTenant.Id,
            dTenant.Name,
            dTenant.Email,
            dTenant.IsActive,
            dTenant.CreatedAt,
            dTenant.UpdatedAt,
            dTenant.DeletedAt
        );
    }
}
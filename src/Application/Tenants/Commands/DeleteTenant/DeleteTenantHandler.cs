using Application.Common.Interfaces;
using MediatR;
using Domain.Interfaces;
using Application.Tenants.Commands.DeleteTenant;

namespace Application.dTenants.Commands.DeletedTenant;

public class DeletedTenantHandler : IRequestHandler<DeleteTenantCommand, bool>
{
    private readonly ITenantRepository _repository;
    private readonly ICacheService _cache;

    public DeletedTenantHandler(ITenantRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<bool> Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        var dTenant = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (dTenant is null) return false;

        _repository.Delete(dTenant);
        await _repository.SaveChangesAsync(cancellationToken);

        // Invalidate caches
        await _cache.RemoveAsync("tenants:all", cancellationToken);
        await _cache.RemoveAsync($"tenants:{request.Id}", cancellationToken);

        return true;
    }
}
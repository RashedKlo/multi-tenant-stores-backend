using Application.Common.DTOs;
using Application.Common.Interfaces;
using MediatR;
using Domain.Interfaces;
using Application.Tenants.DTOs;

namespace Application.Tenants.Queries.GetTenantById;

public class GetTenantByIdHandler : IRequestHandler<GetTenantByIdQuery, TenantDto?>
{
    private readonly ITenantRepository _repository;
    private readonly ICacheService _cache;

    public GetTenantByIdHandler(ITenantRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<TenantDto?> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"tenants:{request.Id}";

        var cached = await _cache.GetAsync<TenantDto>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var tenant = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (tenant is null) return null;

        var dto = new TenantDto(
            tenant.Id, tenant.Name, tenant.Email, tenant.IsActive, tenant.CreatedAt, tenant.UpdatedAt, tenant.DeletedAt
        );

        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10), cancellationToken);

        return dto;
    }
}
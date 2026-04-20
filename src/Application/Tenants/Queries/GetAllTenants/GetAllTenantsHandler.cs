using Application.Common.DTOs;
using Application.Common.Interfaces;
using Application.Tenants.DTOs;
using Domain.Interfaces;
using MediatR;

namespace Application.Tenants.Queries.GetAllTenants;

public class GetAllTenantsHandler : IRequestHandler<GetAllTenantsQuery, List<TenantDto>>
{
    private readonly ITenantRepository _repository;
    private readonly ICacheService _cache;
    private const string CacheKey = "tenants:all";

    public GetAllTenantsHandler(ITenantRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<List<TenantDto>> Handle(GetAllTenantsQuery request, CancellationToken cancellationToken)
    {
        // Try cache first
        var cached = await _cache.GetAsync<List<TenantDto>>(CacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        // Fetch from DB
        var Tenants = await _repository.GetAllAsync(cancellationToken);

        var dtos = Tenants.Select(s => new TenantDto(
            s.Id, s.Name, s.Email, s.IsActive, s.CreatedAt, s.UpdatedAt, s.DeletedAt
        )).ToList();

        // Store in cache for 10 minutes
        await _cache.SetAsync(CacheKey, dtos, TimeSpan.FromMinutes(10), cancellationToken);

        return dtos;
    }
}
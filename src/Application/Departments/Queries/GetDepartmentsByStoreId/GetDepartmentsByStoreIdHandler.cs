using Application.Common.DTOs;
using Application.Common.Interfaces;
using Domain.Interfaces;
using MediatR;

namespace Application.Departments.Queries.GetDepartmentsByStoreId;

public class GetDepartmentsByStoreIdHandler : IRequestHandler<GetDepartmentsByStoreIdQuery, List<DepartmentDto>>
{
    private readonly IDepartmentRepository _repository;
    private readonly ICacheService _cache;

    public GetDepartmentsByStoreIdHandler(IDepartmentRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<List<DepartmentDto>> Handle(GetDepartmentsByStoreIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"departments:store:{request.StoreId}";

        // Try cache first
        var cached = await _cache.GetAsync<List<DepartmentDto>>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        // Fetch from DB
        var departments = await _repository.GetByStoreIdAsync(request.StoreId, cancellationToken);

        var dtos = departments.Select(d => new DepartmentDto(
            d.Id, d.StoreId, d.Name, d.ImageUrl, d.CreatedAt, d.UpdatedAt
        )).ToList();

        // Cache the result
        await _cache.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(10), cancellationToken);

        return dtos;
    }
}

using Application.Common.DTOs;
using Application.Common.Interfaces;
using Domain.Interfaces;
using MediatR;

namespace Application.Departments.Queries.GetAllDepartments;

public class GetAllDepartmentsHandler : IRequestHandler<GetAllDepartmentsQuery, List<DepartmentDto>>
{
    private readonly IDepartmentRepository _repository;
    private readonly ICacheService _cache;
    private const string CacheKey = "departments:all";

    public GetAllDepartmentsHandler(IDepartmentRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<List<DepartmentDto>> Handle(GetAllDepartmentsQuery request, CancellationToken cancellationToken)
    {
        // Try cache first
        var cached = await _cache.GetAsync<List<DepartmentDto>>(CacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        // Fetch from DB
        var departments = await _repository.GetAllAsync(cancellationToken);

        var dtos = departments.Select(d => new DepartmentDto(
            d.Id, d.StoreId, d.Name, d.ImageUrl, d.CreatedAt, d.UpdatedAt
        )).ToList();

        // Cache the result
        await _cache.SetAsync(CacheKey, dtos, TimeSpan.FromMinutes(10), cancellationToken);

        return dtos;
    }
}

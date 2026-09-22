using Application.Common.Interfaces;
using Application.Discovery.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Discovery.Queries.GetModuleDetail;

public record GetModuleDetailQuery(Guid ModuleId) : IRequest<Result<ModuleDetailDto>>, ICacheableQuery
{
    public string CacheKey => $"modules:{ModuleId}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
}
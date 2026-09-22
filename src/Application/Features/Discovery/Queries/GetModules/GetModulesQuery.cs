using Application.Common.Interfaces;
using Application.Discovery.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Discovery.Queries.GetModules;

public record GetModulesQuery : IRequest<Result<IReadOnlyList<ModuleDto>>>, ICacheableQuery
{
    public string CacheKey => "modules:list";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
}
using Application.Discovery.DTOs;
using Domain.Common;
using MediatR;
using Application.Common.Models;
namespace Application.Discovery.Queries.GetStoresByModule;

public record GetStoresByModuleQuery(
    Guid ModuleId,
    Guid? CategoryId,
    string? Search,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PagedResult<StoreSummaryDto>>>;
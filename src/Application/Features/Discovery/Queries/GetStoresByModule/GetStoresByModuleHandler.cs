using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Discovery.DTOs;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Discovery.Queries.GetStoresByModule;

public class GetStoresByModuleHandler(
    IStoreRepository storeRepository)
    : IRequestHandler<GetStoresByModuleQuery, Result<PagedResult<StoreSummaryDto>>>
{
    public async Task<Result<PagedResult<StoreSummaryDto>>> Handle(
        GetStoresByModuleQuery request, CancellationToken cancellationToken)
    {
        var (stores, totalCount) = await storeRepository.GetPagedByModuleAsync(
            request.ModuleId,
            request.CategoryId,
            request.Search,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

     var strs = StoreSummaryDto.FromEntities(stores);

        var result = PagedResult<StoreSummaryDto>.Create(
            strs,
            request.PageNumber,
            request.PageSize,
            totalCount);

        return Result<PagedResult<StoreSummaryDto>>.Success(result);
    }
}
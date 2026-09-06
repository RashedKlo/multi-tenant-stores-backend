using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Discovery.DTOs;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Discovery.Queries.GetStoresByModule;

public class GetStoresByModuleHandler(
    IStoreRepository storeRepository,ICacheService cache, ICurrentLanguageProvider currentLanguageProvider)
    : IRequestHandler<GetStoresByModuleQuery, Result<PagedResult<StoreSummaryDto>>>
{
        public async Task<Result<PagedResult<StoreSummaryDto>>> Handle(
        GetStoresByModuleQuery request, CancellationToken cancellationToken)
    {
       var CacheKey = $"module:{request.ModuleId}:stores" +
               $":cat:{request.CategoryId?.ToString() ?? "all"}" +
               $":q:{request.Search ?? ""}" +
               $":p{request.PageNumber}:s{request.PageSize}"+
               $":lang:{currentLanguageProvider.Language}";

          var cached = await cache.GetAsync<PagedResult<StoreSummaryDto>>(CacheKey, cancellationToken);
        if (cached is not null)
        {
            return Result<PagedResult<StoreSummaryDto>>.Success(cached);
        }
        var (stores, totalCount) = await storeRepository.GetPagedByModuleAsync(
            request.ModuleId,
            request.CategoryId,
            request.Search,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

     var strs = StoreSummaryDto.FromEntities(stores, currentLanguageProvider.Language);

        var result = PagedResult<StoreSummaryDto>.Create(
            strs,
            request.PageNumber,
            request.PageSize,
            totalCount);

        await cache.SetAsync(CacheKey, result, TimeSpan.FromMinutes(30), cancellationToken);

        return Result<PagedResult<StoreSummaryDto>>.Success(result);
    }
}
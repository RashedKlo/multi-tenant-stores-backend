using Application.Catalog.DTOs;
using Application.Common.Models;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Catalog.Queries.GetStoreSections;

public class GetStoreSectionsHandler(IStoreSectionRepository repository)
    : IRequestHandler<GetStoreSectionsQuery, Result<PagedResult<StoreSectionDto>>>
{
    public async Task<Result<PagedResult<StoreSectionDto>>> Handle(
        GetStoreSectionsQuery request, CancellationToken cancellationToken)
    {
        var (sections, totalCount) = await repository.GetPagedByStoreIdAsync(
            request.StoreId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var dtos = sections.Select(StoreSectionDto.FromEntity).ToList();

        var result = PagedResult<StoreSectionDto>.Create(
            dtos,
            request.PageNumber,
            request.PageSize,
            totalCount);

        return Result<PagedResult<StoreSectionDto>>.Success(result);
    }
}

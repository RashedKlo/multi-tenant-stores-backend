using Application.Catalog.DTOs;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Catalog.Queries.GetProductsBySection;

public class GetProductsBySectionHandler(
    IProductRepository productRepository)
    : IRequestHandler<GetProductsBySectionQuery, Result<PagedResult<ProductSummaryDto>>>
{
    public async Task<Result<PagedResult<ProductSummaryDto>>> Handle(
        GetProductsBySectionQuery request, CancellationToken cancellationToken)
    {
        var (products, totalCount) = await productRepository.GetPagedBySectionAsync(
            request.SectionId,
            request.InStockOnly,
            request.MinPrice,
            request.MaxPrice,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
        var dtos = products
            .Select(p => ProductSummaryDto.FromEntity(p))
            .ToList();

        var result = PagedResult<ProductSummaryDto>.Create(
            dtos,
            request.PageNumber,
            request.PageSize,
            totalCount);

        return Result<PagedResult<ProductSummaryDto>>.Success(result);
    }
}

using Application.Catalog.DTOs;
using Application.Common.Models;
using Domain.Common;
using MediatR;

namespace Application.Catalog.Queries.GetProductsBySection;

public record GetProductsBySectionQuery(
    Guid SectionId,
    bool? InStockOnly,
    decimal? MinPrice,
    decimal? MaxPrice,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<Result<PagedResult<ProductSummaryDto>>>;

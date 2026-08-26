using Application.Catalog.DTOs;
using Application.Catalog.Queries.GetProductById;
using Application.Catalog.Queries.GetProductsBySection;
using Application.Catalog.Queries.GetStoreBanners;
using Application.Catalog.Queries.GetStoreById;
using Application.Catalog.Queries.GetStoreSections;
using Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api")]
public class CatalogController(IMediator mediator) : ApiControllerBase
{
    [HttpGet("stores/{id:guid}")]
    public async Task<ActionResult<StoreDetailDto>> GetStore(Guid id, CancellationToken ct) =>
        HandleResult(await mediator.Send(new GetStoreByIdQuery(id), ct));

    [HttpGet("stores/{id:guid}/banners")]
    public async Task<ActionResult<List<StoreBannerDto>>> GetStoreBanners(Guid id, CancellationToken ct) =>
        HandleResult(await mediator.Send(new GetStoreBannersQuery(id), ct));

    [HttpGet("stores/{id:guid}/sections")]
    public async Task<ActionResult<PagedResult<StoreSectionDto>>> GetStoreSections(
        Guid id, int page = 1, int pageSize = 20, CancellationToken ct = default) =>
        HandleResult(await mediator.Send(new GetStoreSectionsQuery(id, page, pageSize), ct));

    [HttpGet("sections/{id:guid}/products")]
    public async Task<ActionResult<PagedResult<ProductSummaryDto>>> GetProductsBySection(
        Guid id,
        bool? inStockOnly,
        decimal? minPrice,
        decimal? maxPrice,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default) =>
        HandleResult(await mediator.Send(
            new GetProductsBySectionQuery(id, inStockOnly, minPrice, maxPrice, page, pageSize), ct));

    [HttpGet("products/{id:guid}")]
    public async Task<ActionResult<ProductDetailDto>> GetProduct(Guid id, CancellationToken ct) =>
        HandleResult(await mediator.Send(new GetProductByIdQuery(id), ct));
}

using Application.Catalog.DTOs;
using Application.Catalog.Queries.GetProductById;
using Application.Catalog.Queries.GetProductsBySection;
using Application.Catalog.Queries.GetStoreBanners;
using Application.Catalog.Queries.GetStoreById;
using Application.Catalog.Queries.GetStoreSections;
using Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[Route("api")]
[EnableRateLimiting("fixed")]
public class CatalogController(IMediator mediator) : ApiControllerBase
{
    // -------------------- Store --------------------

    /// <summary>
    /// Returns detailed information about a store.
    /// </summary>
    [HttpGet("stores/{id:guid}")]
    [ProducesResponseType(typeof(StoreDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StoreDetailDto>> GetStore(
        [FromRoute] Guid id,
        CancellationToken ct)
        => HandleResult(await mediator.Send(new GetStoreByIdQuery(id), ct));

    /// <summary>
    /// Returns the active banners of a store.
    /// </summary>
    [HttpGet("stores/{id:guid}/banners")]
    [ProducesResponseType(typeof(IReadOnlyList<StoreBannerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<StoreBannerDto>>> GetStoreBanners(
        [FromRoute] Guid id,
        CancellationToken ct)
        => HandleResult(await mediator.Send(new GetStoreBannersQuery(id), ct));

    /// <summary>
    /// Returns a paged list of sections belonging to a store.
    /// </summary>
    [HttpGet("stores/{id:guid}/sections")]
    [ProducesResponseType(typeof(PagedResult<StoreSectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<StoreSectionDto>>> GetStoreSections(
        [FromRoute] Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => HandleResult(await mediator.Send(new GetStoreSectionsQuery(id, page, pageSize), ct));

    // -------------------- Section / Products --------------------

    /// <summary>
    /// Returns a paged list of products that belong to a section.
    /// Supports optional filtering by stock and price range.
    /// </summary>
    [HttpGet("sections/{id:guid}/products")]
    [ProducesResponseType(typeof(PagedResult<ProductSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<ProductSummaryDto>>> GetProductsBySection(
        [FromRoute] Guid id,
        [FromQuery] bool? inStockOnly,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => HandleResult(await mediator.Send(
            new GetProductsBySectionQuery(id, inStockOnly, minPrice, maxPrice, page, pageSize), ct));

    // -------------------- Product --------------------

    /// <summary>
    /// Returns detailed information about a product (including options, images, etc.).
    /// </summary>
    [HttpGet("products/{id:guid}")]
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDetailDto>> GetProduct(
        [FromRoute] Guid id,
        CancellationToken ct)
        => HandleResult(await mediator.Send(new GetProductByIdQuery(id), ct));
}
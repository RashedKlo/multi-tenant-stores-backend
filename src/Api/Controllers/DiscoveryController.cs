using Application.Common.Models;
using Application.Discovery.DTOs;
using Application.Discovery.Queries.GetHomeBanners;
using Application.Discovery.Queries.GetNearByStores;
using Application.Discovery.Queries.GetModuleDetail;
using Application.Discovery.Queries.GetModules;
using Application.Discovery.Queries.GetStoresByModule;
using Api.Requests.Discovery;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[Route("api")]
public class DiscoveryController(IMediator mediator) : ApiControllerBase
{
    // -------------------- Home --------------------

    /// <summary>
    /// Returns the active home banners ordered by display order.
    /// </summary>
    [HttpGet("home/banners")]
    [ProducesResponseType(typeof(IReadOnlyList<HomeBannerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<HomeBannerDto>>> GetHomeBanners(
        CancellationToken ct)
        => HandleResult(await mediator.Send(new GetHomeBannersQuery(), ct));

    // -------------------- Modules --------------------

    /// <summary>
    /// Returns all active modules (Restaurants, Markets, Pharmacies, …).
    /// </summary>
    [HttpGet("modules")]
    [ProducesResponseType(typeof(IReadOnlyList<ModuleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ModuleDto>>> GetModules(
        CancellationToken ct)
        => HandleResult(await mediator.Send(new GetModulesQuery(), ct));

    /// <summary>
    /// Returns detailed information about a module (including its banners and categories).
    /// </summary>
    [HttpGet("modules/{id:guid}")]
    [ProducesResponseType(typeof(ModuleDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ModuleDetailDto>> GetModuleDetail(
        [FromRoute] GetModuleDetailRequest request,
        CancellationToken ct)
        => HandleResult(await mediator.Send(new GetModuleDetailQuery(request.Id), ct));

    /// <summary>
    /// Returns a paged list of stores that belong to a module.
    /// Supports optional filtering by category and free-text search.
    /// </summary>
    [HttpGet("modules/{id:guid}/stores")]
    [ProducesResponseType(typeof(PagedResult<StoreSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<StoreSummaryDto>>> GetStoresByModule(
        [FromRoute] Guid id,
        [FromQuery] GetStoresByModuleRequest request,
        CancellationToken ct = default)
        => HandleResult(await mediator.Send(
            new GetStoresByModuleQuery(id, request.CategoryId, request.Search, request.Page, request.PageSize), ct));

    /// <summary>
    /// Returns a paged list of active stores within the requested radius.
    /// </summary>
    [HttpGet("stores/nearby")]
    [ProducesResponseType(typeof(PagedResult<NearbyStoreDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<NearbyStoreDto>>> GetNearByStores(
        [FromQuery] GetNearbyStoresRequest request,
        CancellationToken ct = default)
        => HandleResult(await mediator.Send(
            new GetNearByStoresQuery(
                request.Latitude,
                request.Longitude,
                request.RadiusKm,
                request.Page,
                request.PageSize), ct));
}
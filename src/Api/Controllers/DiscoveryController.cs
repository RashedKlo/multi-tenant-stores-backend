using Application.Common.Models;
using Application.Discovery.DTOs;
using Application.Discovery.Queries.GetHomeBanners;
using Application.Discovery.Queries.GetModuleDetail;
using Application.Discovery.Queries.GetModules;
using Application.Discovery.Queries.GetStoresByModule;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[Route("api")]
[EnableRateLimiting("fixed")]
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
        [FromRoute] Guid id,
        CancellationToken ct)
        => HandleResult(await mediator.Send(new GetModuleDetailQuery(id), ct));

    /// <summary>
    /// Returns a paged list of stores that belong to a module.
    /// Supports optional filtering by category and free-text search.
    /// </summary>
    [HttpGet("modules/{id:guid}/stores")]
    [ProducesResponseType(typeof(PagedResult<StoreSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<StoreSummaryDto>>> GetStoresByModule(
        [FromRoute] Guid id,
        [FromQuery] Guid? categoryId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => HandleResult(await mediator.Send(
            new GetStoresByModuleQuery(id, categoryId, search, page, pageSize), ct));
}
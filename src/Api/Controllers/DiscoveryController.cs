using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Discovery.Queries.GetHomeBanners;
using Application.Discovery.Queries.GetModules;
using Application.Discovery.Queries.GetModuleDetail;
using Application.Discovery.Queries.GetStoresByModule;
using Application.Common.Models;
using Application.Discovery.DTOs;

namespace Api.Controllers;

[ApiController]
[Route("api")]
public class DiscoveryController(IMediator mediator) : ApiControllerBase
{
    [HttpGet("home/banners")]
    public async Task<ActionResult<List<HomeBannerDto>>> GetHomeBanners(CancellationToken ct) =>
        HandleResult(await mediator.Send(new GetHomeBannersQuery(), ct));

    [HttpGet("modules")]
    public async Task<ActionResult<List<ModuleDto>>> GetModules(CancellationToken ct) =>
        HandleResult(await mediator.Send(new GetModulesQuery(), ct));

    [HttpGet("modules/{id:guid}")]
    public async Task<ActionResult<ModuleDetailDto>> GetModuleDetail(Guid id, CancellationToken ct) =>
        HandleResult(await mediator.Send(new GetModuleDetailQuery(id), ct));

    [HttpGet("modules/{id:guid}/stores")]
    public async Task<ActionResult<PagedResult<StoreSummaryDto>>> GetStores(
        Guid id,
        Guid? categoryId,
        string? search,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default) =>
        HandleResult(await mediator.Send(new GetStoresByModuleQuery(id, categoryId, search, page, pageSize), ct));
}

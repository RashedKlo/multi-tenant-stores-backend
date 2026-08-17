using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Discovery.Queries.GetHomeBanners;
using Application.Discovery.Queries.GetModules;
using Application.Discovery.Queries.GetModuleDetail;
using Application.Discovery.Queries.GetStoresByModule;
using Application.Common.Models;
using Application.Discovery.DTOs;
using Api.Controllers;

// ============================================================
// 3. DiscoveryController  (as you already had)
// ============================================================
[ApiController]
[Route("api")]
public class DiscoveryController(IMediator mediator) : ApiControllerBase
{
    [HttpGet("home/banners")]
    public async Task<ActionResult<List<HomeBannerDto>>> GetHomeBanners(CancellationToken ct)
    {
    var result = await mediator.Send(new GetHomeBannersQuery(), ct);
    return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpGet("modules")]
    public async Task<ActionResult<List<ModuleDto>>> GetModules(CancellationToken ct)
    {
        var result = await mediator.Send(new GetModulesQuery(), ct);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpGet("modules/{id:guid}")]
    public async Task<ActionResult<ModuleDetailDto>> GetModuleDetail(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetModuleDetailQuery(id), ct);
        return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }

    [HttpGet("modules/{id:guid}/stores")]
    public async Task<ActionResult<PagedResult<StoreSummaryDto>>> GetStores(
        Guid id,
        Guid? categoryId,
        string? search,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
               var result = await mediator.Send(new GetStoresByModuleQuery(id, categoryId, search, page, pageSize), ct);
               return result.IsSuccess ? Ok(result.Value) : HandleFailure(result);
    }
}
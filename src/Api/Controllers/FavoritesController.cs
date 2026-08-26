using Application.Common.Models;
using Application.Favorites.Commands.AddFavoriteProduct;
using Application.Favorites.Commands.AddFavoriteStore;
using Application.Favorites.Commands.RemoveFavoriteProduct;
using Application.Favorites.Commands.RemoveFavoriteStore;
using Application.Favorites.DTOs;
using Application.Favorites.Queries.GetFavoriteProducts;
using Application.Favorites.Queries.GetFavoriteStores;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api")]
public class FavoritesController(IMediator mediator) : ApiControllerBase
{
    [HttpPost("favorites/products/{id:guid}")]
    public async Task<ActionResult> AddFavoriteProduct(Guid id, CancellationToken ct) =>
        HandleResult(await mediator.Send(new AddFavoriteProductCommand(id), ct));

    [HttpDelete("favorites/products/{id:guid}")]
    public async Task<ActionResult> RemoveFavoriteProduct(Guid id, CancellationToken ct) =>
        HandleResult(await mediator.Send(new RemoveFavoriteProductCommand(id), ct));

    [HttpGet("favorites/products")]
    public async Task<ActionResult<PagedResult<FavoriteProductDto>>> GetFavoriteProducts(
        int page = 1, int pageSize = 20, CancellationToken ct = default) =>
        HandleResult(await mediator.Send(new GetFavoriteProductsQuery(page, pageSize), ct));

    [HttpPost("favorites/stores/{id:guid}")]
    public async Task<ActionResult> AddFavoriteStore(Guid id, CancellationToken ct) =>
        HandleResult(await mediator.Send(new AddFavoriteStoreCommand(id), ct));

    [HttpDelete("favorites/stores/{id:guid}")]
    public async Task<ActionResult> RemoveFavoriteStore(Guid id, CancellationToken ct) =>
        HandleResult(await mediator.Send(new RemoveFavoriteStoreCommand(id), ct));

    [HttpGet("favorites/stores")]
    public async Task<ActionResult<PagedResult<FavoriteStoreDto>>> GetFavoriteStores(
        int page = 1, int pageSize = 20, CancellationToken ct = default) =>
        HandleResult(await mediator.Send(new GetFavoriteStoresQuery(page, pageSize), ct));
}

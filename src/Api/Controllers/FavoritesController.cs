using Application.Common.Models;
using Application.Favorites.Commands.AddFavoriteProduct;
using Application.Favorites.Commands.AddFavoriteStore;
using Application.Favorites.Commands.RemoveFavoriteProduct;
using Application.Favorites.Commands.RemoveFavoriteStore;
using Application.Favorites.DTOs;
using Application.Favorites.Queries.GetFavoriteProducts;
using Application.Favorites.Queries.GetFavoriteStores;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[Route("api/favorites")]
[Authorize]
[EnableRateLimiting("fixed")]
public class FavoritesController(IMediator mediator) : ApiControllerBase
{
    // -------------------- Products --------------------

    /// <summary>
    /// Adds a product to the current customer's favorites.
    /// </summary>
    [HttpPost("products/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AddFavoriteProduct(
        [FromRoute] Guid id,
        CancellationToken ct)
        => HandleResult(await mediator.Send(new AddFavoriteProductCommand(id), ct));

    /// <summary>
    /// Removes a product from the current customer's favorites.
    /// </summary>
    [HttpDelete("products/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveFavoriteProduct(
        [FromRoute] Guid id,
        CancellationToken ct)
        => HandleResult(await mediator.Send(new RemoveFavoriteProductCommand(id), ct));

    /// <summary>
    /// Returns a paged list of the current customer's favorite products.
    /// </summary>
    [HttpGet("products")]
    [ProducesResponseType(typeof(PagedResult<FavoriteProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<FavoriteProductDto>>> GetFavoriteProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => HandleResult(await mediator.Send(new GetFavoriteProductsQuery(page, pageSize), ct));

    // -------------------- Stores --------------------

    /// <summary>
    /// Adds a store to the current customer's favorites.
    /// </summary>
    [HttpPost("stores/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AddFavoriteStore(
        [FromRoute] Guid id,
        CancellationToken ct)
        => HandleResult(await mediator.Send(new AddFavoriteStoreCommand(id), ct));

    /// <summary>
    /// Removes a store from the current customer's favorites.
    /// </summary>
    [HttpDelete("stores/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveFavoriteStore(
        [FromRoute] Guid id,
        CancellationToken ct)
        => HandleResult(await mediator.Send(new RemoveFavoriteStoreCommand(id), ct));

    /// <summary>
    /// Returns a paged list of the current customer's favorite stores.
    /// </summary>
    [HttpGet("stores")]
    [ProducesResponseType(typeof(PagedResult<FavoriteStoreDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<FavoriteStoreDto>>> GetFavoriteStores(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => HandleResult(await mediator.Send(new GetFavoriteStoresQuery(page, pageSize), ct));
}
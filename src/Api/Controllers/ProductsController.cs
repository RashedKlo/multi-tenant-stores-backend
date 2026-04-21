using Application.Products.Commands.CreateProduct;
using Application.Products.Commands.DeleteProduct;
using Application.Products.Commands.UpdateProduct;
using Application.Products.Queries.GetAllProducts;
using Application.Products.Queries.GetProductById;
using Application.Products.Queries.GetProductsByDepartmentId;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[Route("api/products")]
[EnableRateLimiting("fixed")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get all active products</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllProductsQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>Get a single product by ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id), cancellationToken);
        if (result is null) return NotFound($"Product with ID {id} was not found.");
        return Ok(result);
    }

    /// <summary>Get all active products by Department ID</summary>
    [HttpGet("department/{departmentId:guid}")]
    public async Task<IActionResult> GetByDepartmentId(Guid departmentId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProductsByDepartmentIdQuery(departmentId), cancellationToken);
        return Ok(result);
    }

    /// <summary>Create a new product</summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update an existing product</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("ID in URL does not match ID in body.");

        var result = await _mediator.Send(command, cancellationToken);
        if (result is null) return NotFound($"Product with ID {id} was not found.");
        return Ok(result);
    }

    /// <summary>Delete a product</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _mediator.Send(new DeleteProductCommand(id), cancellationToken);
        if (!deleted) return NotFound($"Product with ID {id} was not found.");
        return NoContent();
    }
}

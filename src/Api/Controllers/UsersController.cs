using Application.Users.Commands.UpdateUser;
using Application.Users.Commands.DeleteUser;
using Application.Users.Queries.GetAllUsers;
using Application.Users.Queries.GetUserById;
using Application.Users.Queries.GetUserByEmail;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[Route("api/users")]
[EnableRateLimiting("fixed")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _sender.Send(new GetAllUsersQuery(), cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _sender.Send(new GetUserByIdQuery(id), cancellationToken);
        if (user is null) return NotFound($"User with ID {id} was not found.");
        return Ok(user);
    }

    [HttpGet("email/{email}")]
    public async Task<IActionResult> GetByEmail(string email, CancellationToken cancellationToken)
    {
        var user = await _sender.Send(new GetUserByEmailQuery(email), cancellationToken);
        if (user is null) return NotFound($"User with email {email} was not found.");
        return Ok(user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateUserCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");

        var user = await _sender.Send(command, cancellationToken);
        if (user is null) return NotFound($"User with ID {id} was not found.");
        return Ok(user);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _sender.Send(new DeleteUserCommand(id), cancellationToken);
        if (!deleted) return NotFound($"User with ID {id} was not found.");
        return NoContent();
    }
}

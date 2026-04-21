using Application.Users.Commands.Register;
using Application.Users.Commands.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("fixed")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender) => _sender = sender;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterCommand command, CancellationToken cancellationToken)
    {
        var token = await _sender.Send(command, cancellationToken);
        if (token is null) return BadRequest("Registration failed");
        return Ok(new { token });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        var token = await _sender.Send(command, cancellationToken);
        if (token is null) return Unauthorized();
        return Ok(new { token });
    }
}

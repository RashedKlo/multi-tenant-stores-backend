using Application.Auth.Commands.CreateGuestSession;
using Application.Auth.Commands.ForgotPassword;
using Application.Auth.Commands.GoogleLogin;
using Application.Auth.Commands.Login;
using Application.Auth.Commands.Logout;
using Application.Auth.Commands.RefreshToken;
using Application.Auth.Commands.Register;
using Application.Auth.Commands.ResendVerification;
using Application.Auth.Commands.ResetPassword;
using Application.Auth.Commands.VerifyEmail;
using Application.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IMediator mediator) : ApiControllerBase
{
    // ---------- Sessions ----------

    [HttpPost("guest-session")]
    // [EnableRateLimiting("auth-general")]
    public async Task<ActionResult<GuestSessionDto>> CreateGuestSession(CancellationToken ct) =>
        HandleResult(await mediator.Send(new CreateGuestSessionCommand(), ct));

    // ---------- Registration & verification ----------

    [HttpPost("register")]
    // [EnableRateLimiting("auth-email")]
    public async Task<ActionResult<RegisterResultDto>> Register(
        [FromBody] RegisterCommand command, CancellationToken ct) =>
        HandleResult(await mediator.Send(command, ct));

    [HttpPost("verify-email")]
    // [EnableRateLimiting("auth-code")]
    public async Task<ActionResult<AuthTokensDto>> VerifyEmail(
        [FromBody] VerifyEmailCommand command, CancellationToken ct) =>
        HandleResult(await mediator.Send(command, ct));

    [HttpPost("resend-verification")]
    // [EnableRateLimiting("auth-email")]
    public async Task<IActionResult> ResendVerification(
        [FromBody] ResendVerificationCommand command, CancellationToken ct) =>
        HandleResult(await mediator.Send(command, ct));

    // ---------- Login ----------

    [HttpPost("login")]
    // [EnableRateLimiting("auth-login")]
    public async Task<ActionResult<AuthTokensDto>> Login(
        [FromBody] LoginCommand command, CancellationToken ct) =>
        HandleResult(await mediator.Send(command, ct));

    [HttpPost("google")]
    // [EnableRateLimiting("auth-login")]
    public async Task<ActionResult<AuthTokensDto>> GoogleLogin(
        [FromBody] GoogleLoginCommand command, CancellationToken ct) =>
        HandleResult(await mediator.Send(command, ct));

    // ---------- Tokens ----------

    [HttpPost("refresh")]
    // [EnableRateLimiting("auth-general")]
    public async Task<ActionResult<AuthTokensDto>> Refresh(
        [FromBody] RefreshTokenCommand command, CancellationToken ct) =>
        HandleResult(await mediator.Send(command, ct));

    [HttpPost("logout")]
    [Authorize] // requires a valid access token — logout must know who is logging out
    public async Task<IActionResult> Logout(
        [FromBody] LogoutCommand command, CancellationToken ct) =>
        HandleResult(await mediator.Send(command, ct));

    // ---------- Password reset ----------

    [HttpPost("forgot-password")]
    // [EnableRateLimiting("auth-email")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordCommand command, CancellationToken ct) =>
        HandleResult(await mediator.Send(command, ct));

    [HttpPost("reset-password")]
    // [EnableRateLimiting("auth-code")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordCommand command, CancellationToken ct) =>
        HandleResult(await mediator.Send(command, ct));
}

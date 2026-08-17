// ============================================================
// 1. AuthController
// ============================================================
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Common.Models;
using Application.Auth.DTOs;

// adjust namespaces to match your project

[ApiController]
[Route("api")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("auth/guest-session")]
    public async Task<ActionResult<GuestSessionDto>> CreateGuestSession(
        CancellationToken ct) =>
        Ok(await mediator.Send(new CreateGuestSessionCommand(), ct));

    [HttpPost("auth/register")]
    public async Task<ActionResult<RegisterResultDto>> Register(
        [FromBody] RegisterCommand command, CancellationToken ct) =>
        Ok(await mediator.Send(command, ct));

    [HttpPost("auth/verify-email")]
    public async Task<ActionResult<AuthTokensDto>> VerifyEmail(
        [FromBody] VerifyEmailCommand command, CancellationToken ct) =>
        Ok(await mediator.Send(command, ct));

    [HttpPost("auth/resend-verification")]
    public async Task<ActionResult> ResendVerification(
        [FromBody] ResendVerificationCommand command, CancellationToken ct)
    {
        await mediator.Send(command, ct);
        return NoContent();
    }

    [HttpPost("auth/login")]
    public async Task<ActionResult<AuthTokensDto>> Login(
        [FromBody] LoginCommand command, CancellationToken ct) =>
        Ok(await mediator.Send(command, ct));

    [HttpPost("auth/google")]
    public async Task<ActionResult<AuthTokensDto>> GoogleLogin(
        [FromBody] GoogleLoginCommand command, CancellationToken ct) =>
        Ok(await mediator.Send(command, ct));

    [HttpPost("auth/refresh")]
    public async Task<ActionResult<AuthTokensDto>> Refresh(
        [FromBody] RefreshTokenCommand command, CancellationToken ct) =>
        Ok(await mediator.Send(command, ct));

    [HttpPost("auth/logout")]
    public async Task<ActionResult> Logout(
        [FromBody] LogoutCommand command, CancellationToken ct)
    {
        await mediator.Send(command, ct);
        return NoContent();
    }

    [HttpPost("auth/forgot-password")]
    public async Task<ActionResult> ForgotPassword(
        [FromBody] ForgotPasswordCommand command, CancellationToken ct)
    {
        await mediator.Send(command, ct);
        return NoContent();
    }

    [HttpPost("auth/reset-password")]
    public async Task<ActionResult> ResetPassword(
        [FromBody] ResetPasswordCommand command, CancellationToken ct)
    {
        await mediator.Send(command, ct);
        return NoContent();
    }
}
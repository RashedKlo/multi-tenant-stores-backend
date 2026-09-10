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
using Microsoft.AspNetCore.RateLimiting;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("fixed")]
public class AuthController(IMediator mediator) : ApiControllerBase
{
    // -------------------- Sessions --------------------

    /// <summary>
    /// Creates an anonymous guest session used for cart before login.
    /// </summary>
    [HttpPost("guest-session")]
    [ProducesResponseType(typeof(GuestSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GuestSessionDto>> CreateGuestSession(CancellationToken ct)
        => HandleResult(await mediator.Send(new CreateGuestSessionCommand(), ct));

    // -------------------- Registration & verification --------------------

    /// <summary>
    /// Registers a new customer. A verification code is sent by email.
    /// Tokens are issued only after email verification.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisterResultDto>> Register(
        [FromBody] RegisterCommand command,
        CancellationToken ct)
        => HandleResult(await mediator.Send(command, ct));

    /// <summary>
    /// Verifies the email with the one-time code and returns access + refresh tokens.
    /// </summary>
    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(AuthTokensDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthTokensDto>> VerifyEmail(
        [FromBody] VerifyEmailCommand command,
        CancellationToken ct)
        => HandleResult(await mediator.Send(command, ct));

    /// <summary>
    /// Resends the email verification code. Always returns success to avoid email enumeration.
    /// </summary>
    [HttpPost("resend-verification")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> ResendVerification(
        [FromBody] ResendVerificationCommand command,
        CancellationToken ct)
        => HandleResult(await mediator.Send(command, ct));

    // -------------------- Login --------------------

    /// <summary>
    /// Logs in with email and password. Requires a verified email.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthTokensDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuthTokensDto>> Login(
        [FromBody] LoginCommand command,
        CancellationToken ct)
        => HandleResult(await mediator.Send(command, ct));

    /// <summary>
    /// Logs in (or registers) with a Google ID token.
    /// </summary>
    [HttpPost("google")]
    [ProducesResponseType(typeof(AuthTokensDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthTokensDto>> GoogleLogin(
        [FromBody] GoogleLoginCommand command,
        CancellationToken ct)
        => HandleResult(await mediator.Send(command, ct));

    // -------------------- Tokens --------------------

    /// <summary>
    /// Rotates the refresh token and returns a new access + refresh pair.
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthTokensDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthTokensDto>> Refresh(
        [FromBody] RefreshTokenCommand command,
        CancellationToken ct)
        => HandleResult(await mediator.Send(command, ct));

    /// <summary>
    /// Revokes the given refresh token. Requires a valid access token.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Logout(
        [FromBody] LogoutCommand command,
        CancellationToken ct)
        => HandleResult(await mediator.Send(command, ct));

    // -------------------- Password reset --------------------

    /// <summary>
    /// Sends a password-reset code by email. Always returns success to avoid email enumeration.
    /// </summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> ForgotPassword(
        [FromBody] ForgotPasswordCommand command,
        CancellationToken ct)
        => HandleResult(await mediator.Send(command, ct));

    /// <summary>
    /// Resets the password using the one-time code from email.
    /// </summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ResetPassword(
        [FromBody] ResetPasswordCommand command,
        CancellationToken ct)
        => HandleResult(await mediator.Send(command, ct));
}
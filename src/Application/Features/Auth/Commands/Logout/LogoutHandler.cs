// Application/Auth/Commands/Logout/LogoutHandler.cs
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Auth.Commands.Logout;

public sealed class LogoutHandler(
    IRefreshTokenRepository refreshTokens,
    IJwtTokenService jwt)
    : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
    {
        var hash = jwt.HashToken(request.RefreshToken);
        var token = await refreshTokens.GetByHashAsync(hash, ct);

        // Idempotent — already logged out is success
        if (token is null || token.IsRevoked)
            return Result.Success();

        token.Revoke();
        await refreshTokens.SaveChangesAsync(ct);
        return Result.Success();
    }
}
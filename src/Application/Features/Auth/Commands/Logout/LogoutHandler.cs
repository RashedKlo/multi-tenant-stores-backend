using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Auth.Commands.Logout;

public class LogoutHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IJwtTokenService tokenService)
    : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(
        LogoutCommand request, CancellationToken cancellationToken)
    {
        var hash = tokenService.HashToken(request.RefreshToken);
        var token = await refreshTokenRepository.GetByTokenHashAsync(hash, cancellationToken);

        if (token is null || token.RevokedAt is not null)
            return Result.Success(); // idempotent — already logged out

        token.Revoke();
        refreshTokenRepository.Update(token);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

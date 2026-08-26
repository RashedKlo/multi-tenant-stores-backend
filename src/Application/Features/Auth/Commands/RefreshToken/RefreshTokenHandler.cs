using Application.Auth.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Auth.Commands.RefreshToken;

public class RefreshTokenHandler(
    ICustomerRepository customerRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IJwtTokenService tokenService)
    : IRequestHandler<RefreshTokenCommand, Result<AuthTokensDto>>
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);

    public async Task<Result<AuthTokensDto>> Handle(
        RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var hash = tokenService.HashToken(request.RefreshToken);
        var existingToken = await refreshTokenRepository.GetByTokenHashAsync(hash, cancellationToken);

        // Unknown or expired → plain rejection.
        if (existingToken is null || existingToken.IsExpired)
            return Invalid();
        if (existingToken.IsRevoked)
        {
            var survivors = await refreshTokenRepository.GetActiveByCustomerIdAsync(
                existingToken.CustomerId, cancellationToken);
            foreach (var t in survivors)
                t.Revoke();
            await refreshTokenRepository.SaveChangesAsync(cancellationToken);
            return Invalid();
        }

        var customer = await customerRepository.GetByIdAsync(existingToken.CustomerId, cancellationToken);
        if (customer is null || !customer.IsActive || customer.IsDeleted) // adjust if props differ
            return Invalid();

        // Rotation: consume old, issue new.
        existingToken.Revoke();
        existingToken.MarkUsed();   // LastUsedAt snapshot pre-revocation

        var pair = tokenService.GenerateTokenPair(customer.Id, customer.Email);

        var newToken = Domain.Entities.RefreshToken.Create(
            customer.Id,
            tokenService.HashToken(pair.RefreshToken),
            DateTime.UtcNow.Add(RefreshTokenLifetime));

        if (newToken.IsFailure)
            return Result<AuthTokensDto>.Failure(newToken.Errors);

        refreshTokenRepository.Add(newToken.Value!);

        // Single atomic save: revocation + rotation together.
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return Result<AuthTokensDto>.Success(
            new AuthTokensDto(pair.AccessToken, pair.RefreshToken, pair.AccessTokenExpiresAt));
    }

    private static Result<AuthTokensDto> Invalid() =>
        Result<AuthTokensDto>.Failure(
            Error.Forbidden("RefreshToken", "Invalid or expired refresh token."));
}

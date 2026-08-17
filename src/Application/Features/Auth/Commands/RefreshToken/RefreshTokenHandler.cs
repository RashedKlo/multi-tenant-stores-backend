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
    private static readonly DateTime RefreshTokenTtl = DateTime.UtcNow.AddDays(30);

    public async Task<Result<AuthTokensDto>> Handle(
        RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var hash = tokenService.HashToken(request.RefreshToken);
        var existingToken = await refreshTokenRepository.GetByTokenHashAsync(hash, cancellationToken);

        if (existingToken is null || !existingToken.IsActive)
        {
            return Result<AuthTokensDto>.Failure(
                Error.Forbidden("RefreshToken", "Invalid or expired refresh token."));
        }

        var customer = await customerRepository.GetByIdAsync(existingToken.CustomerId, cancellationToken);
        if (customer is null || !customer.IsActive || customer.IsDeleted)
        {
            return Result<AuthTokensDto>.Failure(
                Error.Forbidden("RefreshToken", "Invalid or expired refresh token."));
        }

        // Rotation: old token revoked the moment it's used; a new one is issued.
        // Stolen-token reuse after rotation is immediately detectable.
        existingToken.Revoke();
        existingToken.MarkUsed();
        refreshTokenRepository.Update(existingToken);

        var pair = tokenService.GenerateTokenPair(customer.Id, customer.Email);
        var newHash = tokenService.HashToken(pair.RefreshToken);
        var refreshToken = Domain.Entities.RefreshToken.Create(customer.Id, newHash, RefreshTokenTtl);
       if (refreshToken.IsFailure)
        {
            return Result<AuthTokensDto>.Failure(refreshToken.Errors);
        }
        refreshTokenRepository.Add(refreshToken.Value!);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return Result<AuthTokensDto>.Success(
            new AuthTokensDto(pair.AccessToken, pair.RefreshToken, pair.AccessTokenExpiresAt));
    }
}

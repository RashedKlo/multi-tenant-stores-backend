// Application/Auth/Commands/RefreshToken/RefreshTokenHandler.cs
using Application.Auth.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Auth.Commands.RefreshToken;

public sealed class RefreshTokenHandler(
    ICustomerRepository customers,
    IRefreshTokenRepository refreshTokens,
    IJwtTokenService jwt)
    : IRequestHandler<RefreshTokenCommand, Result<AuthTokensDto>>
{
    public async Task<Result<AuthTokensDto>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var hash = jwt.HashToken(request.RefreshToken);
        var existing = await refreshTokens.GetByHashAsync(hash, ct);

        if (existing is null || !existing.IsActive)
            return Result<AuthTokensDto>.Failure(
                Error.Unauthorized("Auth.RefreshToken.Invalid", "Invalid or expired refresh token."));

        var customer = await customers.GetByIdAsync(existing.CustomerId, ct);
        if (customer is null || customer.IsDeleted || !customer.IsActive)
            return Result<AuthTokensDto>.Failure(
                Error.Unauthorized("Auth.RefreshToken.Invalid", "Invalid or expired refresh token."));

        // Rotate: revoke old, issue new
        existing.Revoke();

        var pair = jwt.GenerateTokenPair(customer.Id, customer.Email);
        var newHash = jwt.HashToken(pair.RefreshToken);

        var createResult = Domain.Entities.RefreshToken.Create(
            customer.Id,
            newHash,
            DateTime.UtcNow.AddDays(30));

        if (createResult.IsFailure)
            return Result<AuthTokensDto>.Failure(createResult.Errors);

        await refreshTokens.AddAsync(createResult.Value!, ct);
        await refreshTokens.SaveChangesAsync(ct);

        return Result<AuthTokensDto>.Success(new AuthTokensDto(
            pair.AccessToken,
            pair.RefreshToken,
            pair.AccessTokenExpiresAt));
    }
}
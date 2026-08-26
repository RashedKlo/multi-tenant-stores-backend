using Application.Auth.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Auth.Commands.Login;

public class LoginHandler(
    ICustomerRepository customerRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService tokenService)
    : IRequestHandler<LoginCommand, Result<AuthTokensDto>>
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);

    public async Task<Result<AuthTokensDto>> Handle(
        LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var customer = await customerRepository.GetByEmailAsync(email, cancellationToken);

        var validCredentials =
            customer is not null
            && customer.PasswordHash is not null
            && passwordHasher.Verify(request.Password, customer.PasswordHash);

        if (!validCredentials)
        {

            return Result<AuthTokensDto>.Failure(
                Error.Unauthorized("Credentials", "Invalid email or password."));
        }

        if (!customer!.IsEmailVerified)
            return Result<AuthTokensDto>.Failure(
                Error.Forbidden("Email.NotVerified", "Please verify your email before logging in."));

        if (!customer.IsActive || customer.IsDeleted) // adjust if these props differ
            return Result<AuthTokensDto>.Failure(
                Error.Forbidden("Account.Inactive", "This account is inactive."));

        var pair = tokenService.GenerateTokenPair(customer.Id, customer.Email);

        var refreshToken = Domain.Entities.RefreshToken.Create(
            customer.Id,
            tokenService.HashToken(pair.RefreshToken),
            DateTime.UtcNow.Add(RefreshTokenLifetime));

        if (refreshToken.IsFailure)
            return Result<AuthTokensDto>.Failure(refreshToken.Errors);

        refreshTokenRepository.Add(refreshToken.Value!);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        // TODO: guest→auth cart handoff when GuestSessionToken is provided:
        //   resolve guest session by hash → move/merge its cart items into the
        //   customer's active cart → revoke the guest session. Single transaction.

        return Result<AuthTokensDto>.Success(
            new AuthTokensDto(pair.AccessToken, pair.RefreshToken, pair.AccessTokenExpiresAt));
    }
}

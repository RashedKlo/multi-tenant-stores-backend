using Application.Auth.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging; // or your logging abstraction

namespace Application.Auth.Commands.GoogleLogin;

public class GoogleLoginHandler(
    ICustomerRepository customerRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IGoogleTokenVerifier googleVerifier,
    IJwtTokenService tokenService,
    ILogger<GoogleLoginHandler> logger)
    : IRequestHandler<GoogleLoginCommand, Result<AuthTokensDto>>
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);

    public async Task<Result<AuthTokensDto>> Handle(
        GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        // Verifier throws on invalid/expired/wrong-audience tokens —
        // translate that to an auth failure, not a 500.
        GoogleUserInfo googleUser;
        try
        {
            googleUser = await googleVerifier.VerifyAsync(request.IdToken, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Google token verification failed");
            return Result<AuthTokensDto>.Failure(
                Error.Unauthorized("Google", "Invalid Google identity token."));
        }

        var email = googleUser.Email.Trim().ToLowerInvariant();

        var customer = await customerRepository.GetByGoogleIdAsync(googleUser.GoogleId, cancellationToken);

        if (customer is null)
        {
            // Link by email ONLY for verified password accounts — otherwise
            // an attacker controlling an unverified address could take over the account.
            customer = await customerRepository.GetByEmailAsync(email, cancellationToken);

            if (customer is not null)
            {
                if (!customer.IsEmailVerified || !customer.IsActive)
                    return Result<AuthTokensDto>.Failure(
                        Error.Forbidden("Google.LinkFailed",
                            "This email is registered but cannot be linked via Google."));

                customer.LinkGoogleAccount(googleUser.GoogleId);
            }
            else
            {
                var created = Customer.Create(
                    googleUser.FirstName.Trim(),
                    googleUser.LastName.Trim(),
                    email,
                    null,               // no password
                    googleUser.GoogleId,true);

                if (created.IsFailure)
                    return Result<AuthTokensDto>.Failure(created.Errors);

                customer = created.Value!;
                customerRepository.Add(customer);
            }
        }

        if (!customer.IsActive || customer.IsDeleted)
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

        // One save covers: link-or-create + new refresh token.
        await customerRepository.SaveChangesAsync(cancellationToken);

        return Result<AuthTokensDto>.Success(
            new AuthTokensDto(pair.AccessToken, pair.RefreshToken, pair.AccessTokenExpiresAt));
    }
}

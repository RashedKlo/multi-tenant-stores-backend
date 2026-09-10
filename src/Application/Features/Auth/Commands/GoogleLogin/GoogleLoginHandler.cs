// Application/Auth/Commands/GoogleLogin/GoogleLoginHandler.cs
using Application.Auth.Commands.VerifyEmail;
using Application.Auth.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Auth.Commands.GoogleLogin;

public sealed class GoogleLoginHandler(
    ICustomerRepository customers,
    IRefreshTokenRepository refreshTokens,
    IGoogleTokenVerifier googleVerifier,
    IJwtTokenService jwt)
    : IRequestHandler<GoogleLoginCommand, Result<AuthTokensDto>>
{
    public async Task<Result<AuthTokensDto>> Handle(GoogleLoginCommand request, CancellationToken ct)
    {
        GoogleUserInfo googleUser;
        try
        {
            googleUser = await googleVerifier.VerifyAsync(request.IdToken, ct);
        }
        catch
        {
            return Result<AuthTokensDto>.Failure(
                Error.Unauthorized("Auth.Google.InvalidToken", "Invalid Google token."));
        }

        var email = googleUser.Email.Trim().ToLowerInvariant();
        var customer = await customers.GetByEmailAsync(email, ct);

        if (customer is null)
        {
            var createResult = Customer.CreateWithGoogle(
                googleUser.FirstName,
                googleUser.LastName,
                email,
                googleUser.GoogleId);

            if (createResult.IsFailure)
                return Result<AuthTokensDto>.Failure(createResult.Errors);

            customer = createResult.Value!;
            await customers.AddAsync(customer, ct);
            await customers.SaveChangesAsync(ct);
        }
        else
        {
            if (customer.IsDeleted || !customer.IsActive)
                return Result<AuthTokensDto>.Failure(
                    Error.Forbidden("Auth.Customer.Inactive", "Account is inactive."));

            // Link Google if not yet linked
            if (customer.GoogleId is null)
            {
                var link = customer.LinkGoogleAccount(googleUser.GoogleId);
                if (link.IsFailure)
                    return Result<AuthTokensDto>.Failure(link.Errors);

                await customers.SaveChangesAsync(ct);
            }
            else if (customer.GoogleId != googleUser.GoogleId)
            {
                return Result<AuthTokensDto>.Failure(
                    Error.Conflict("Auth.Google.Mismatch", "This email is linked to a different Google account."));
            }
        }

        return await VerifyEmailHandler.IssueTokensAsync(customer, refreshTokens, jwt, ct);
    }
}
using Application.Auth.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Auth.Commands.VerifyEmail;

public class VerifyEmailHandler(
    ICustomerRepository customerRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IVerificationCodeStore codeStore,
    IJwtTokenService tokenService)
    : IRequestHandler<VerifyEmailCommand, Result<AuthTokensDto>>
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);

    public async Task<Result<AuthTokensDto>> Handle(
        VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var customer = await customerRepository.GetByEmailAsync(email, cancellationToken);

        // Single generic failure for "no account" or "wrong code" — no enumeration.
        if (customer is null || !await codeStore.ValidateAndConsumeAsync(email, request.Code, cancellationToken))
            return Result<AuthTokensDto>.Failure(
                Error.Validation("Verification.Failed", "Invalid or expired verification code."));

        if (customer.IsEmailVerified)
            return Result<AuthTokensDto>.Failure(
                Error.Conflict("Email.AlreadyVerified", "This email is already verified."));

        customer.VerifyEmail();

        var pair = tokenService.GenerateTokenPair(customer.Id, customer.Email);

        var refreshToken = Domain.Entities.RefreshToken.Create(
            customer.Id,
            tokenService.HashToken(pair.RefreshToken),
            DateTime.UtcNow.Add(RefreshTokenLifetime));

        if (refreshToken.IsFailure)
            return Result<AuthTokensDto>.Failure(refreshToken.Errors);

         refreshTokenRepository.Add(refreshToken.Value!);

        // One unit of work: verification + token persisted together.
        await customerRepository.SaveChangesAsync(cancellationToken);

        return Result<AuthTokensDto>.Success(
            new AuthTokensDto(pair.AccessToken, pair.RefreshToken, pair.AccessTokenExpiresAt));
    }
}

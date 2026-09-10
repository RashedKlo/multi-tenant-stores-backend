// Application/Auth/Commands/VerifyEmail/VerifyEmailHandler.cs
using Application.Auth.DTOs;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Auth.Commands.VerifyEmail;

public sealed class VerifyEmailHandler(
    ICustomerRepository customers,
    IRefreshTokenRepository refreshTokens,
    IVerificationCodeStore codeStore,
    IJwtTokenService jwt)
    : IRequestHandler<VerifyEmailCommand, Result<AuthTokensDto>>
{
    public async Task<Result<AuthTokensDto>> Handle(VerifyEmailCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var isValid = await codeStore.ValidateAndConsumeAsync(email, request.Code, ct);
        if (!isValid)
            return Result<AuthTokensDto>.Failure(
                Error.Validation("Auth.Code.Invalid", "Invalid or expired verification code."));

        var customer = await customers.GetByEmailAsync(email, ct);
        if (customer is null || customer.IsDeleted)
            return Result<AuthTokensDto>.Failure(
                Error.NotFound("Auth.Customer.NotFound", "Customer not found."));

        if (!customer.IsActive)
            return Result<AuthTokensDto>.Failure(
                Error.Forbidden("Auth.Customer.Inactive", "Account is inactive."));

        var verifyResult = customer.VerifyEmail();
        if (verifyResult.IsFailure)
            return Result<AuthTokensDto>.Failure(verifyResult.Errors);

        await customers.SaveChangesAsync(ct);

        return await IssueTokensAsync(customer, refreshTokens, jwt, ct);
    }

    internal static async Task<Result<AuthTokensDto>> IssueTokensAsync(
        Customer customer,
        IRefreshTokenRepository refreshTokens,
        IJwtTokenService jwt,
        CancellationToken ct)
    {
        var pair = jwt.GenerateTokenPair(customer.Id, customer.Email);
        var hash = jwt.HashToken(pair.RefreshToken);

        var tokenResult = Domain.Entities.RefreshToken.Create(
            customer.Id,
            hash,
            DateTime.UtcNow.AddDays(30));

        if (tokenResult.IsFailure)
            return Result<AuthTokensDto>.Failure(tokenResult.Errors);

        await refreshTokens.AddAsync(tokenResult.Value!, ct);
        await refreshTokens.SaveChangesAsync(ct);

        return Result<AuthTokensDto>.Success(new AuthTokensDto(
            pair.AccessToken,
            pair.RefreshToken,
            pair.AccessTokenExpiresAt));
    }
}
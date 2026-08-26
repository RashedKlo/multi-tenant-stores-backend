using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Auth.Commands.ResetPassword;

public class ResetPasswordHandler(
    ICustomerRepository customerRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IVerificationCodeStore codeStore,
    IPasswordHasher passwordHasher)
    : IRequestHandler<ResetPasswordCommand, Result>
{
    public async Task<Result> Handle(
        ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var customer = await customerRepository.GetByEmailAsync(email, cancellationToken);

        // Generic failure covers both "no such account" and "bad code" — no enumeration,
        // and the code isn't burned for nonexistent emails.
        if (customer is null || !await codeStore.ValidateAndConsumeAsync(email, request.Code, cancellationToken))
            return Result.Failure(
                Error.Validation("PasswordReset.Failed", "Invalid or expired reset code."));

        // Explicit domain operation instead of round-tripping every field through Update().
        customer.SetPassword(passwordHasher.Hash(request.NewPassword));

        // Password reset kills every live session — atomic with the save below.
        var activeTokens = await refreshTokenRepository.GetActiveByCustomerIdAsync(
            customer.Id, cancellationToken);

        foreach (var token in activeTokens)
            token.Revoke();
        await customerRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

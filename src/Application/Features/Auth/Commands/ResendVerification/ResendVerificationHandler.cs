// Application/Auth/Commands/ResendVerification/ResendVerificationHandler.cs
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;
using System.Security.Cryptography;

namespace Application.Auth.Commands.ResendVerification;

public class ResendVerificationHandler(
    ICustomerRepository customerRepository,
    IVerificationCodeStore codeStore,
    IEmailService emailService)
    : IRequestHandler<ResendVerificationCommand, Result>
{
    private static readonly TimeSpan CodeTtl = TimeSpan.FromMinutes(10);

    public async Task<Result> Handle(
        ResendVerificationCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var customer = await customerRepository.GetByEmailAsync(email, cancellationToken);

        // Silent success when the account doesn't exist or is already verified —
        // the response must never reveal which emails have accounts.
        if (customer is null || customer.IsEmailVerified)
            return Result.Success();

        // 🔐 Random.Shared is NOT cryptographically secure.
        // A predictable code generator undermines the whole 6-digit scheme.
        var code = RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();

        await codeStore.StoreCodeAsync(customer.Email, code, CodeTtl, cancellationToken);
        await emailService.SendVerificationCodeAsync(customer.Email, code, cancellationToken);

        return Result.Success();
    }
}

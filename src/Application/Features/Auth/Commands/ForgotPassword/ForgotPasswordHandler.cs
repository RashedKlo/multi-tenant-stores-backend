using System.Security.Cryptography;
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Auth.Commands.ForgotPassword;

public class ForgotPasswordHandler(
    ICustomerRepository customerRepository,
    IVerificationCodeStore codeStore,
    IEmailService emailService)
    : IRequestHandler<ForgotPasswordCommand, Result>
{
    private static readonly TimeSpan CodeTtl = TimeSpan.FromMinutes(10);

    public async Task<Result> Handle(
        ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var customer = await customerRepository.GetByEmailAsync(email, cancellationToken);

        // Silent success for BOTH cases — never reveal:
        //   1. whether the email has an account,
        //   2. whether it's a Google-only account without a local password.
        // An attacker probing emails gets an identical response either way.
        if (customer is null || customer.PasswordHash is null)
            return Result.Success();

        var code = RandomNumberGenerator.GetInt32(100_000, 1_000_000).ToString();

        await codeStore.StoreCodeAsync( email, code, CodeTtl, cancellationToken);
        await emailService.SendPasswordResetCodeAsync(email, code, cancellationToken);

        return Result.Success();
    }
}

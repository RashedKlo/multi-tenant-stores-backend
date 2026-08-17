using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

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
        var customer = await customerRepository.GetByEmailAsync(request.Email, cancellationToken);

        // Silent no-op if the email doesn't exist or is already verified —
        // never let this endpoint confirm which emails have accounts.
        if (customer is null || customer.IsEmailVerified)
            return Result.Success();

        // Rate-limit this in the API layer before production.
        var code = Random.Shared.Next(100_000, 999_999).ToString();
        await codeStore.StoreCodeAsync(customer.Email, code, CodeTtl, cancellationToken);
        await emailService.SendVerificationCodeAsync(customer.Email, code, cancellationToken);

        return Result.Success();
    }
}

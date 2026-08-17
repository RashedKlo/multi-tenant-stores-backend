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
        var customer = await customerRepository.GetByEmailAsync(request.Email, cancellationToken);

        // Silent no-op if email doesn't exist — never confirm which emails have accounts.
        if (customer is null)
            return Result.Failure(Error.NotFound("Customer.NotFound", "Customer not found"));

        // Google-only account has no password to reset — silent no-op too.
        if (customer.PasswordHash is null)
            return Result.Failure(Error.Validation("Customer.Invalid", "This account has no password to reset."));

        var code = Random.Shared.Next(100_000, 999_999).ToString();
        await codeStore.StoreCodeAsync(customer.Email, code, CodeTtl, cancellationToken);
        await emailService.SendPasswordResetCodeAsync(customer.Email, code, cancellationToken);

        return Result.Success();
    }
}

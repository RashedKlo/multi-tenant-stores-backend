// Application/Auth/Commands/ForgotPassword/ForgotPasswordHandler.cs
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Auth.Commands.ForgotPassword;

public sealed class ForgotPasswordHandler(
    ICustomerRepository customers,
    IVerificationCodeStore codeStore,
    IEmailService emailService)
    : IRequestHandler<ForgotPasswordCommand, Result>
{
    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var customer = await customers.GetByEmailAsync(email, ct);

        // Always succeed — do not leak account existence
        if (customer is null || customer.IsDeleted || !customer.IsActive)
            return Result.Success();

        var code = Random.Shared.Next(100000, 999999).ToString();
        await codeStore.StoreCodeAsync(email, code, TimeSpan.FromMinutes(15), ct);
        await emailService.SendPasswordResetCodeAsync(email, code, ct);

        return Result.Success();
    }
}
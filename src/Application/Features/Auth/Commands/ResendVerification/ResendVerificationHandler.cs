// Application/Auth/Commands/ResendVerification/ResendVerificationHandler.cs
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Auth.Commands.ResendVerification;

public sealed class ResendVerificationHandler(
    ICustomerRepository customers,
    IVerificationCodeStore codeStore,
    IEmailService emailService)
    : IRequestHandler<ResendVerificationCommand, Result>
{
    public async Task<Result> Handle(ResendVerificationCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var customer = await customers.GetByEmailAsync(email, ct);

        // Do not leak whether the email exists
        if (customer is null || customer.IsDeleted || customer.IsEmailVerified)
            return Result.Success();

        var code = Random.Shared.Next(100000, 999999).ToString();
        await codeStore.StoreCodeAsync(email, code, TimeSpan.FromMinutes(15), ct);
        await emailService.SendVerificationCodeAsync(email, code, ct);

        return Result.Success();
    }
}
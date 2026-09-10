// Application/Auth/Commands/ResetPassword/ResetPasswordHandler.cs
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Auth.Commands.ResetPassword;

public sealed class ResetPasswordHandler(
    ICustomerRepository customers,
    IVerificationCodeStore codeStore,
    IPasswordHasher passwordHasher)
    : IRequestHandler<ResetPasswordCommand, Result>
{
    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var isValid = await codeStore.ValidateAndConsumeAsync(email, request.Code, ct);
        if (!isValid)
            return Result.Failure(
                Error.Validation("Auth.Code.Invalid", "Invalid or expired reset code."));

        var customer = await customers.GetByEmailAsync(email, ct);
        if (customer is null || customer.IsDeleted)
            return Result.Failure(Error.NotFound("Auth.Customer.NotFound", "Customer not found."));

        var newHash = passwordHasher.Hash(request.NewPassword);
        var result = customer.ChangePassword(newHash);
        if (result.IsFailure)
            return result;

        await customers.SaveChangesAsync(ct);
        return Result.Success();
    }
}
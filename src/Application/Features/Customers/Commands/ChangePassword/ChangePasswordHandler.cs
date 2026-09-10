// Application/Features/Customers/Commands/ChangePassword/ChangePasswordHandler.cs
using Application.Common.Interfaces;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Customers.Commands.ChangePassword;

public sealed class ChangePasswordHandler(
    ICustomerRepository customers,
    ICurrentUserService user,
    IPasswordHasher passwordHasher)
    : IRequestHandler<ChangePasswordCommand, Result>
{
    public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        if (user.CustomerId is not Guid customerId)
            return Result.Failure(
                Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var customer = await customers.GetByIdAsync(customerId, ct);
        if (customer is null || customer.IsDeleted)
            return Result.Failure(Error.NotFound("Customer.NotFound", "Customer not found."));

        // Customer has no password yet (Google-only account)
        if (customer.PasswordHash is null)
            return Result.Failure(
                Error.Validation("Customer.Password.NotSet", "No password is set for this account. Use set-password instead."));

        if (!passwordHasher.Verify(request.CurrentPassword, customer.PasswordHash))
            return Result.Failure(
                Error.Validation("Customer.Password.Invalid", "Current password is incorrect."));

        // Optional: prevent reusing the same password
        if (passwordHasher.Verify(request.NewPassword, customer.PasswordHash))
            return Result.Failure(
                Error.Validation("Customer.Password.SameAsOld", "New password must be different from the current password."));

        var newHash = passwordHasher.Hash(request.NewPassword);

        var result = customer.ChangePassword(newHash);
        if (result.IsFailure)
            return result;

        await customers.SaveChangesAsync(ct);
        return Result.Success();
    }
}
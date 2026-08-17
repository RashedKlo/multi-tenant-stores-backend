using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Customers.Commands.ChangePassword;

public class ChangePasswordHandler(
    ICustomerRepository customerRepository,
    IPasswordHasher passwordHasher,
    ICurrentUserService currentUser)
    : IRequestHandler<ChangePasswordCommand, Result>
{
    public async Task<Result> Handle(
        ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.CustomerId is null)
            return Result.Failure(Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var customer = await customerRepository.GetByIdAsync(
            currentUser.CustomerId.Value, cancellationToken);

        if (customer is null || customer.IsDeleted || !customer.IsActive)
            return Result.Failure(Error.NotFound("Customer.NotFound", "Customer not found"));

        // Google-only accounts have no local password
        if (string.IsNullOrEmpty(customer.PasswordHash))
            return Result.Failure(
                Error.Validation("Password.Required", "This account does not have a password. Sign in with Google instead."));

        if (!passwordHasher.Verify(request.CurrentPassword, customer.PasswordHash))
            return Result.Failure(
                Error.Validation("CurrentPassword.Invalid", "Current password is incorrect."));

        var newHash = passwordHasher.Hash(request.NewPassword);

        // Domain Update requires name/email; preserve them and only rotate the hash
        var updateResult = customer.Update(
            customer.FirstName,
            customer.LastName,
            customer.Email,
            passwordHash: newHash,
            googleId: customer.GoogleId);

        if (updateResult.IsFailure)
            return Result.Failure(updateResult.Errors);

        customerRepository.Update(customer);
        await customerRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

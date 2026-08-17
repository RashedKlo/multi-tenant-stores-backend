using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
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
        if (!await codeStore.ValidateAndConsumeAsync(request.Email, request.Code, cancellationToken))
            return Result.Failure(Error.Conflict("Code", "Invalid or expired code."));

        var customer = await customerRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (customer is null)
            return Result.Failure(Error.NotFound("Customer.NotFound", "Customer not found."));

        customer.Update(firstName: customer.FirstName,
         lastName: customer.LastName,
          email: customer.Email, 
          passwordHash: passwordHasher.Hash(request.NewPassword), 
          googleId: customer.GoogleId);
        customerRepository.Update(customer);
        await customerRepository.SaveChangesAsync(cancellationToken);

        // Password reset logs out every existing session — if the account
        // was compromised, leaving old refresh tokens alive defeats the point.
        var activeTokens = await refreshTokenRepository.GetActiveByCustomerIdAsync(
            customer.Id, cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke();
            refreshTokenRepository.Update(token);
        }

        if (activeTokens.Count > 0)
            await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

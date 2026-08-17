using Application.Common.Interfaces;
using Application.Customers.DTOs;
using Domain.Common;
using Domain.Entities;
using Domain.Interfaces;
using MediatR;

namespace Application.Customers.Commands.UpdateProfile;

public class UpdateProfileHandler(
    ICustomerRepository customerRepository,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateProfileCommand, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(
        UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.CustomerId is null)
            return Result<CustomerDto>.Failure(
                Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var customer = await customerRepository.GetByIdAsync(
            currentUser.CustomerId.Value, cancellationToken);

        if (customer is null || customer.IsDeleted || !customer.IsActive)
            return Result<CustomerDto>.Failure(
                Error.NotFound("Customer.NotFound", "Customer not found"));

        // Preserve existing password/google identity — Update overwrites PasswordHash
        // when a value is passed; keep the current hash so we don't wipe credentials.
        var updateResult = customer.Update(
            request.FirstName,
            request.LastName,
            email: customer.Email,
            passwordHash: customer.PasswordHash,
            googleId: customer.GoogleId);

        if (updateResult.IsFailure)
            return Result<CustomerDto>.Failure(updateResult.Errors);


        customerRepository.Update(customer);
        await customerRepository.SaveChangesAsync(cancellationToken);

        return Result<CustomerDto>.Success(CustomerDto.FromEntity(customer));
    }
}

// Application/Features/Customers/Commands/UpdateProfile/UpdateProfileHandler.cs
using Application.Common.Interfaces;
using Application.Customers.DTOs;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Customers.Commands.UpdateProfile;

public sealed class UpdateProfileHandler(
    ICustomerRepository customers,
    ICurrentUserService user)
    : IRequestHandler<UpdateProfileCommand, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        if (user.CustomerId is not Guid customerId)
            return Result<CustomerDto>.Failure(
                Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var customer = await customers.GetByIdAsync(customerId, ct);
        if (customer is null || customer.IsDeleted)
            return Result<CustomerDto>.Failure(
                Error.NotFound("Customer.NotFound", "Customer not found."));

        var result = customer.UpdateProfile(request.FirstName, request.LastName);
        if (result.IsFailure)
            return Result<CustomerDto>.Failure(result.Errors);

        await customers.SaveChangesAsync(ct);
        return Result<CustomerDto>.Success(CustomerDto.FromEntity(customer));
    }
}
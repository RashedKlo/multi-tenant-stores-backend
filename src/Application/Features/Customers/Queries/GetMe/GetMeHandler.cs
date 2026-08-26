using Application.Common.Interfaces;
using Application.Customers.DTOs;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Customers.Queries.GetMe;

public class GetMeHandler(
    ICustomerRepository customerRepository,
    ICurrentUserService currentUser)
    : IRequestHandler<GetMeQuery, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(
        GetMeQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.CustomerId is null)
            return Result<CustomerDto>.Failure(
                Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var customer = await customerRepository.GetByIdAsync(
            currentUser.CustomerId.Value, cancellationToken);

        if (customer is null || customer.IsDeleted || !customer.IsActive)
            return Result<CustomerDto>.Failure(
                Error.NotFound("Customer.NotFound", "Customer not found"));

        return Result<CustomerDto>.Success(CustomerDto.FromEntity(customer));
    }
}
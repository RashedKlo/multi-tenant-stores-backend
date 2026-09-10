// Application/Features/Customers/Queries/GetMe/GetMeQueryHandler.cs
using Application.Common.Interfaces;
using Application.Customers.DTOs;
using Domain.Common;
using Domain.Interfaces;
using MediatR;

namespace Application.Customers.Queries.GetMe;

public sealed class GetMeQueryHandler(
    ICustomerRepository customers,
    ICurrentUserService user)
    : IRequestHandler<GetMeQuery, Result<CustomerDto>>
{
    public async Task<Result<CustomerDto>> Handle(GetMeQuery request, CancellationToken ct)
    {
        if (user.CustomerId is not Guid customerId)
            return Result<CustomerDto>.Failure(
                Error.Unauthorized("Customer.Unauthorized", "Customer must be authenticated."));

        var customer = await customers.GetByIdAsync(customerId, ct);
        if (customer is null || customer.IsDeleted)
            return Result<CustomerDto>.Failure(
                Error.NotFound("Customer.NotFound", "Customer not found."));

        return Result<CustomerDto>.Success(CustomerDto.FromEntity(customer));
    }
}
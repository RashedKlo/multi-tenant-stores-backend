using Application.Customers.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Customers.Queries.GetMe;

public record GetMeQuery : IRequest<Result<CustomerDto>>;
// Application/Features/Customers/Queries/GetMe/GetMeQuery.cs
using Application.Customers.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Customers.Queries.GetMe;

public sealed record GetMeQuery : IRequest<Result<CustomerDto>>;
using Application.Features.Addresses.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Addresses.Queries.GetAddressById;

public record GetAddressByIdQuery(Guid Id) : IRequest<Result<AddressDto>>;

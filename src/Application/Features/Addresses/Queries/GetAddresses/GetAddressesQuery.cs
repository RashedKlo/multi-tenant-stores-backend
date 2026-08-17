using Application.Addresses.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Addresses.Queries.GetAddresses;

public record GetAddressesQuery : IRequest<Result<List<AddressDto>>>;

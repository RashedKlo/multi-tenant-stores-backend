using Application.Addresses.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Addresses.Commands.SetDefaultAddress;

public record SetDefaultAddressCommand(Guid Id) : IRequest<Result<AddressDto>>;

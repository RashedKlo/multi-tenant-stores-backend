using Application.Addresses.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Addresses.Commands.UpdateAddress;

public record UpdateAddressCommand(
    Guid Id,
    string Label,
    decimal Latitude,
    decimal Longitude,
    string AddressText) : IRequest<Result<AddressDto>>;

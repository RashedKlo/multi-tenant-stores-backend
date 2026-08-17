using Application.Addresses.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Addresses.Commands.CreateAddress;

public record CreateAddressCommand(
    string Label,
    decimal Latitude,
    decimal Longitude,
    string AddressText,
    bool IsDefault) : IRequest<Result<AddressDto>>;

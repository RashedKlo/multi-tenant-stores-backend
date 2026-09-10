// Application/Features/Addresses/Commands/CreateAddress/CreateAddressCommand.cs
using Application.Features.Addresses.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Addresses.Commands.CreateAddress;

public sealed record CreateAddressCommand(
    string Label,
    decimal Latitude,
    decimal Longitude,
    string AddressText,
    bool IsDefault = false) : IRequest<Result<AddressDto>>;
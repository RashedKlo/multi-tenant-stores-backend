// Application/Features/Addresses/Commands/UpdateAddress/UpdateAddressCommand.cs
using Application.Features.Addresses.DTOs;
using Domain.Common;
using MediatR;

namespace Application.Addresses.Commands.UpdateAddress;

public sealed record UpdateAddressCommand(
    Guid Id,
    string Label,
    decimal Latitude,
    decimal Longitude,
    string AddressText) : IRequest<Result<AddressDto>>;
// Application/Features/Addresses/DTOs/AddressDto.cs
using Domain.Entities;

namespace Application.Features.Addresses.DTOs;

public sealed record AddressDto(
    Guid Id,
    string Label,
    decimal Latitude,
    decimal Longitude,
    string AddressText,
    bool IsDefault,
    DateTime CreatedAt)
{
    public static AddressDto FromEntity(CustomerAddress a) => new(
        a.Id,
        a.Label,
        a.Latitude,
        a.Longitude,
        a.AddressText,
        a.IsDefault,
        a.CreatedAt);
}
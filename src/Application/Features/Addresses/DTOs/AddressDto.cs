namespace Application.Addresses.DTOs;

public record AddressDto(
    Guid Id,
    string Label,
    decimal Latitude,
    decimal Longitude,
    string AddressText,
    bool IsDefault)
{
    public static AddressDto FromEntity(Domain.Entities.CustomerAddress a) => new(
        a.Id, a.Label, a.Latitude, a.Longitude, a.AddressText, a.IsDefault);
}

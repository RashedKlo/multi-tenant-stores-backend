namespace Api.Requests.Address;

public sealed record UpdateAddressRequest(
    string Label,
    decimal Latitude,
    decimal Longitude,
    string AddressText);

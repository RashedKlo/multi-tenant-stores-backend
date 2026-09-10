namespace Api.Requests.Address;

public sealed record AddAddressRequest(
    string Label,
    decimal Latitude,
    decimal Longitude,
    string AddressText,
    bool IsDefault);

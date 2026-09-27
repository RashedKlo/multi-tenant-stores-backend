namespace Api.Requests.Discovery;

public sealed record GetNearbyStoresRequest(
    decimal Latitude,
    decimal Longitude,
    int RadiusKm = 10,
    int Page = 1,
    int PageSize = 20);
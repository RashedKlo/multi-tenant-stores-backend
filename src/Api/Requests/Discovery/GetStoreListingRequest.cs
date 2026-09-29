namespace Api.Requests.Discovery;

public sealed record GetStoreListingRequest(
    int Page = 1,
    int PageSize = 10);
namespace Api.Requests.Catalog;

public sealed record GetStoreSectionsRequest(
    int Page = 1,
    int PageSize = 20);
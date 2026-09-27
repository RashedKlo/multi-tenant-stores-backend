namespace Api.Requests.Discovery;

public sealed record GetStoresByModuleRequest(
    Guid? CategoryId,
    string? Search,
    int Page = 1,
    int PageSize = 20);
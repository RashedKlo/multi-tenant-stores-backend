namespace Api.Requests.Catalog;

public sealed record GetProductsBySectionRequest(
    bool? InStockOnly,
    decimal? MinPrice,
    decimal? MaxPrice,
    int Page = 1,
    int PageSize = 20);
public sealed record SubmitStoreReviewRequest(
    Guid OrderId,
    short Rating,
    string? Comment);
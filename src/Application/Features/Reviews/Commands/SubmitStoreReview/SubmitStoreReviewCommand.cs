using Domain.Common;
using MediatR;

namespace Application.Features.Reviews.Commands.SubmitStoreReview;

public sealed record SubmitStoreReviewCommand(
    Guid StoreId,
    Guid OrderId,
    short Rating,
    string? Comment) : IRequest<Result>;
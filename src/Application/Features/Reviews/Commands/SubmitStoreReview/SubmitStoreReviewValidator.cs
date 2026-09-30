using FluentValidation;

namespace Application.Features.Reviews.Commands.SubmitStoreReview;

public sealed class SubmitStoreReviewValidator : AbstractValidator<SubmitStoreReviewCommand>
{
    public SubmitStoreReviewValidator()
    {
        RuleFor(x => x.StoreId).NotEmpty();
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Rating).InclusiveBetween((short)1, (short)5);
        RuleFor(x => x.Comment)
            .MaximumLength(1000)
            .When(x => x.Comment is not null);
    }
}
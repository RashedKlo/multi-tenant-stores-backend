using FluentValidation;

namespace Application.Products.Commands.UpdateProduct;

public class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Product title is required")
            .MaximumLength(255).WithMessage("Product title must not exceed 255 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.DiscountPrice)
            .GreaterThan(0).WithMessage("Discount price must be greater than 0")
            .LessThan(x => x.Price).WithMessage("Discount price must be less than regular price")
            .When(x => x.DiscountPrice.HasValue && x.DiscountPrice > 0);

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock must be greater than or equal to 0");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));
    }
}

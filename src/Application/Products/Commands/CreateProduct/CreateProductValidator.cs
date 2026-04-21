using FluentValidation;

namespace Application.Products.Commands.CreateProduct;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Department ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Product title is required")
            .MaximumLength(255).WithMessage("Product title must not exceed 255 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock must be greater than or equal to 0");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(500).WithMessage("Image URL must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));

        RuleFor(x => x.SKU)
            .MaximumLength(100).WithMessage("SKU must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SKU));
    }
}

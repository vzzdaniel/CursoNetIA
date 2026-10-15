using AulaPedidos.Domain.Products;
using FluentValidation;

namespace AulaPedidos.Application.Products;

public sealed class CreateProductValidator : AbstractValidator<CreateProduct>
{
    public CreateProductValidator()
    {
        RuleFor(command => command.Description).NotEmpty().WithMessage("Description is required.")
            .Must(description => description?.Trim().Length <= Product.MaximumDescriptionLength)
            .WithMessage("Description must contain at most 200 characters.");
        RuleFor(command => command.AlternateDescription)
            .Must(description => description is null || description.Trim().Length <= Product.MaximumDescriptionLength)
            .WithMessage("AlternateDescription must contain at most 200 characters.");
        RuleFor(command => command.Price).GreaterThanOrEqualTo(0m).WithMessage("Price must be non-negative.");
    }
}

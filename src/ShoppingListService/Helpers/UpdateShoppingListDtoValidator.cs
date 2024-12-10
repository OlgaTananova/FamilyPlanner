using System;
using FluentValidation;
using ShoppingListService.DTOs;

namespace ShoppingListService.Helpers;

public class UpdateShoppingListDtoValidator : AbstractValidator<UpdateShoppingListDto>
{
    public UpdateShoppingListDtoValidator()
    {
        RuleFor(x => x.Heading)
         .MaximumLength(100)
         .WithMessage("Heading cannot exceed 100 characters.")
         .When(x => !string.IsNullOrEmpty(x.Heading));

        RuleFor(x => x.SalesTax)
            .GreaterThanOrEqualTo(0)
            .WithMessage("SalesTax must be non-negative.")
            .When(x => x.SalesTax.HasValue);

        RuleFor(x => x.IsArchived)
            .NotNull()
            .WithMessage("IsArchived must have a value when provided.")
            .When(x => x.IsArchived.HasValue);

    }
}

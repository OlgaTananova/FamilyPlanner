using System;
using FluentValidation;
using ShoppingListService.DTOs;

namespace ShoppingListService.Helpers;

public class CreateShoppingListDtoValidator : AbstractValidator<CreateShoppingListDto>
{
    public CreateShoppingListDtoValidator()
    {
        RuleFor(x => x.Heading)
        .MinimumLength(1)
         .MaximumLength(100)
         .WithMessage("Heading must be between 1 and 100 characters.")
         .When(x => !string.IsNullOrEmpty(x.Heading));
    }
}

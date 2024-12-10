using System;
using System.Reflection.Metadata;
using FluentValidation;
using Microsoft.Extensions.Azure;
using ShoppingListService.DTOs;
using ShoppingListService.Entities;

namespace ShoppingListService.Helpers;

public class UpdateShoppingListItemDtoValidator : AbstractValidator<UpdateShoppingListItemDto>
{
    public UpdateShoppingListItemDtoValidator()
    {
        RuleFor(x => x.Unit)
            .Must(unit => string.IsNullOrEmpty(unit) || Enum.TryParse(typeof(Units), unit, true, out _))
            .WithMessage("Invalid Unit value.");

        RuleFor(x => x.Status)
            .Must(status => string.IsNullOrEmpty(status) || Enum.TryParse(typeof(Status), status, true, out _))
            .WithMessage("Invalid Status value.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .When(x => x.Quantity.HasValue)
            .WithMessage("Quantity must be non-negative.");

        RuleFor(x => x.PricePerUnit)
            .GreaterThanOrEqualTo(0)
            .When(x => x.PricePerUnit.HasValue)
            .WithMessage("PricePerUnit must be non-negative.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Price.HasValue)
            .WithMessage("Price must be non-negative.");
    }
}


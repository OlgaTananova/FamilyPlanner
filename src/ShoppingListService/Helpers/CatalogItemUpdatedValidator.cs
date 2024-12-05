using System;
using Contracts.Catalog;
using FluentValidation;

namespace ShoppingListService.Helpers;

public class CatalogItemUpdatedValidator : AbstractValidator<CatalogItemUpdated>
{
    public CatalogItemUpdatedValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.Family).NotEmpty().WithMessage("Family is required.");
        RuleFor(x => x.OwnerId).NotEmpty().WithMessage("OwnerId is required.");
        RuleFor(x => x.CategorySKU).NotEmpty().WithMessage("CategorySKU is required.");
        RuleFor(x => x.CategoryName).NotEmpty().WithMessage("Category name is required.");
    }
}

using System;
using Contracts.Catalog;
using FluentValidation;
using FluentValidation.Validators;

namespace ShoppingListService.Helpers;

public class CatalogCategoryUpdateValidator : AbstractValidator<CatalogCategoryUpdated>
{
    public CatalogCategoryUpdateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Category name is required.");
        RuleFor(x => x.Family).NotEmpty().WithMessage("Family is required.");
        RuleFor(x => x.OwnerId).NotEmpty().WithMessage("OwnerId is required.");
        RuleFor(x => x.CategorySKU).NotEmpty().WithMessage("CategorySKU is required.");
    }
}

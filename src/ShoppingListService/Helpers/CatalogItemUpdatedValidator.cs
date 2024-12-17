using System;
using Contracts.Catalog;
using FluentValidation;

namespace ShoppingListService.Helpers;

public class CatalogItemUpdatedValidator : AbstractValidator<CatalogItemUpdated>
{
    public CatalogItemUpdatedValidator()
    {
        RuleFor(x => x.UpdatedItem.Name).NotEmpty().WithMessage("Name is required.");
        RuleFor(x => x.UpdatedItem.Family).NotEmpty().WithMessage("Family is required.");
        RuleFor(x => x.UpdatedItem.OwnerId).NotEmpty().WithMessage("OwnerId is required.");
        RuleFor(x => x.UpdatedItem.CategorySKU).NotEmpty().WithMessage("CategorySKU is required.");
        RuleFor(x => x.UpdatedItem.CategoryName).NotEmpty().WithMessage("Category name is required.");
    }
}

using System;
using Contracts.Catalog;
using FluentValidation;

namespace ShoppingListService.Helpers;

public class CatalogItemDeletedValidator: AbstractValidator<CatalogItemDeleted>
{
    public CatalogItemDeletedValidator()
    {
        RuleFor(x => x.SKU).NotEmpty().WithMessage("Item SKU is required.");
        RuleFor(x => x.Family).NotEmpty().WithMessage("Family is required.");
        RuleFor(x => x.OwnerId).NotEmpty().WithMessage("OwnerId is required.");

    }
}

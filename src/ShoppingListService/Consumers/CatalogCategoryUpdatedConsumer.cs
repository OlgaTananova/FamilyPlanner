using System;
using AutoMapper;
using Contracts.Catalog;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ShoppingListService.Data;
using ShoppingListService.Helpers;

namespace ShoppingListService.Consumers;

public class CatalogCategoryUpdatedConsumer : IConsumer<CatalogCategoryUpdated>
{
    private readonly IMapper _mapper;
    private readonly ShoppingListContext _context;
    public CatalogCategoryUpdatedConsumer(IMapper mapper, ShoppingListContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task Consume(ConsumeContext<CatalogCategoryUpdated> context)
    {
        try
        {
            var validator = new CatalogCategoryUpdateValidator();
            var result = validator.Validate(context.Message);

            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Validation error: {error.ErrorMessage}");
                }
                throw new ArgumentException("Invalid CatalogCategoryUpdated message received.");
            }

            Console.WriteLine("--> Consuming catalog category updated " + context.Message.Sku);

            // Begin a transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            // Update catalog items    
            var catalogItems = await _context.CatalogItems
                .Where(x => x.CategorySKU == context.Message.Sku
                && x.Family == context.Message.Family).ToListAsync();

            if (catalogItems.Any())
            {
                foreach (var catalogItem in catalogItems)
                {
                    catalogItem.CategoryName = context.Message.Name;
                    _context.CatalogItems.Update(catalogItem);
                }

            }
            else
            {
                Console.WriteLine($"No catalog items found for category SKU: {context.Message.Sku}");
                await transaction.RollbackAsync();
                return;
            }

            // if there are shopping list items with the category - update them
            var shoppingListItems = await _context.ShoppingListItems.Where(x => x.CategorySKU == context.Message.Sku
                     && x.Family == context.Message.Family).ToListAsync();
            if (shoppingListItems.Count > 0)
            {
                foreach (var shoppingItem in shoppingListItems)
                {
                    shoppingItem.CategoryName = context.Message.Name;
                    _context.ShoppingListItems.Update(shoppingItem);
                }
            }
            // Save changes to the database
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            Console.WriteLine($"--> Successfully updated category name for SKU: {context.Message.Sku}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

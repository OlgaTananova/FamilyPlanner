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

            Console.WriteLine("--> Consuming catalog category updated " + context.Message.CategorySKU);

            var catalogItems = await _context.CatalogItems
                .Where(x => x.CategorySKU == context.Message.CategorySKU &&
                x.OwnerId == context.Message.OwnerId && x.Family == context.Message.Family).ToListAsync();

            if (catalogItems.Any())
            {
                foreach (var catalogItem in catalogItems)
                {
                    catalogItem.CategoryName = context.Message.Name;
                }
                // Save changes to the database
                await _context.SaveChangesAsync();
                Console.WriteLine("--> Successfully updated catalog items for category: " + context.Message.CategorySKU);

            }
            else
            {
                Console.WriteLine($"No catalog items found for category SKU: {context.Message.CategorySKU}");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

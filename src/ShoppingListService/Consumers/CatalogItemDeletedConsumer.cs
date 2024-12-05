using System;
using AutoMapper;
using Contracts.Catalog;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ShoppingListService.Data;
using ShoppingListService.Helpers;

namespace ShoppingListService.Consumers;

public class CatalogItemDeletedConsumer : IConsumer<CatalogItemDeleted>
{
    private readonly IMapper _mapper;
    private readonly ShoppingListContext _context;

    public CatalogItemDeletedConsumer(IMapper mapper, ShoppingListContext context)
    {
        _mapper = mapper;
        _context = context;
    }
    public async Task Consume(ConsumeContext<CatalogItemDeleted> context)
    {
        try
        {
            var validator = new CatalogItemDeletedValidator();
            var result = validator.Validate(context.Message);

            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Validation error: {error.ErrorMessage}");
                }
                throw new ArgumentException("Invalid CatalogItemDeleted message received.");
            }

            Console.WriteLine("--> Consuming catalog item deleted " + context.Message.SKU);

            var catalogItem = await _context.CatalogItems.FirstOrDefaultAsync(x => x.SKU == context.Message.SKU
                    && x.Family == context.Message.Family && x.OwnerId == context.Message.OwnerId);

            if (catalogItem != null)
            {
                catalogItem.IsDeleted = true;

                // TODO: update ShoppingListItems 

                await _context.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine($"Catalog item with SKU {context.Message.SKU} not found. Skipping delete.");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

    }
}

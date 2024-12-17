using System;
using AutoMapper;
using Contracts.Catalog;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ShoppingListService.Data;
using ShoppingListService.Entities;
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

            // Begin a transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            var catalogItem = await _context.CatalogItems.FirstOrDefaultAsync(x => x.SKU == context.Message.SKU
                    && x.Family == context.Message.Family);

            if (catalogItem == null)
            {

                Console.WriteLine($"Catalog item with SKU {context.Message.SKU} not found. Skipping delete.");
                await transaction.RollbackAsync();
                return;
            }

            catalogItem.IsDeleted = true;

            List<ShoppingListItem> shoppingListItems = await _context.ShoppingListItems
            .Where(x => x.SKU == context.Message.SKU && x.Family == context.Message.Family).ToListAsync();

            foreach (var shoppingListItem in shoppingListItems)
            {
                shoppingListItem.IsOrphaned = true;
                _context.ShoppingListItems.Update(shoppingListItem);
            }
            _context.CatalogItems.Update(catalogItem);

            await _context.SaveChangesAsync();

            // Commit the transaction
            await transaction.CommitAsync();

            Console.WriteLine("Transaction committed successfully.");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

    }
}

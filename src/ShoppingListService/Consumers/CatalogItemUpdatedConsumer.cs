using System;
using System.Transactions;
using AutoMapper;
using Contracts.Catalog;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ShoppingListService.Data;
using ShoppingListService.DTOs;
using ShoppingListService.Entities;
using ShoppingListService.Helpers;

namespace ShoppingListService.Consumers;

public class CatalogItemUpdatedConsumer : IConsumer<CatalogItemUpdated>
{
    private readonly IMapper _mapper;
    private readonly ShoppingListContext _context;

    public CatalogItemUpdatedConsumer(IMapper mapper, ShoppingListContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task Consume(ConsumeContext<CatalogItemUpdated> context)
    {
        try
        {
            // Validate the incoming message
            var validator = new CatalogItemUpdatedValidator();
            var result = validator.Validate(context.Message);

            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Validation error: {error.ErrorMessage}");
                }
                throw new ArgumentException("Invalid CatalogItemUpdated message received.");
            }

            Console.WriteLine("--> Consuming catalog item updated " + context.Message.UpdatedItem.SKU);

            // Begin a transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            // Fetch the catalog item
            var catalogItem = await _context.CatalogItems
                .FirstOrDefaultAsync(x => x.SKU == context.Message.UpdatedItem.SKU && x.Family == context.Message.UpdatedItem.Family);

            if (catalogItem == null)
            {
                Console.WriteLine($"Catalog item with SKU {context.Message.UpdatedItem.SKU} not found. Skipping update.");
                await transaction.RollbackAsync();
                return;
            }

            // Update the catalog item
            catalogItem.Name = context.Message.UpdatedItem.Name;
            catalogItem.CategorySKU = context.Message.UpdatedItem.CategorySKU;
            catalogItem.CategoryName = context.Message.UpdatedItem.CategoryName;
            catalogItem.IsDeleted = context.Message.UpdatedItem.IsDeleted;

            _context.CatalogItems.Update(catalogItem);

            // Fetch associated shopping list items
            var shoppingListItems = await _context.ShoppingListItems
                .Where(x => x.SKU == context.Message.UpdatedItem.SKU && x.Family == context.Message.UpdatedItem.Family)
                .ToListAsync();

            // Update related shopping list items
            foreach (var shoppingListItem in shoppingListItems)
            {
                shoppingListItem.Name = context.Message.UpdatedItem.Name;
                shoppingListItem.CatalogItem = catalogItem;
                shoppingListItem.CategorySKU = context.Message.UpdatedItem.CategorySKU;
                shoppingListItem.CategoryName = context.Message.UpdatedItem.CategoryName;

                _context.ShoppingListItems.Update(shoppingListItem);
            }

            // Save changes to the database
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

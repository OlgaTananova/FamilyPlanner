using System;
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

            Console.WriteLine("--> Consuming catalog item updated " + context.Message.SKU);

            // Begin a transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            // Fetch the catalog item
            var catalogItem = await _context.CatalogItems
                .FirstOrDefaultAsync(x => x.SKU == context.Message.SKU && x.Family == context.Message.Family);

            // Fetch associated shopping list items
            var shoppingListItems = await _context.ShoppingListItems
                .Where(x => x.SKU == context.Message.SKU && x.Family == context.Message.Family)
                .ToListAsync();

            if (catalogItem != null)
            {
                // Update the catalog item
                _mapper.Map(context.Message, catalogItem);
                _context.CatalogItems.Update(catalogItem);

                // Update related shopping list items
                foreach (var shoppingListItem in shoppingListItems)
                {
                    shoppingListItem.Name = context.Message.Name;
                    shoppingListItem.CatalogItem = catalogItem;
                    shoppingListItem.CategorySKU = context.Message.CategorySKU;
                    shoppingListItem.CategoryName = context.Message.CategoryName;

                    _context.ShoppingListItems.Update(shoppingListItem);
                }

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Commit the transaction
                await transaction.CommitAsync();
                CatalogItemDto updatedCatalogItem = _mapper.Map<CatalogItemDto>(catalogItem);

                Console.WriteLine("Transaction committed successfully.");
            }
            else
            {
                Console.WriteLine($"Catalog item with SKU {context.Message.SKU} not found. Skipping update.");
                await transaction.RollbackAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

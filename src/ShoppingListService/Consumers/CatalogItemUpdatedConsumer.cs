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
    ILogger<CatalogItemUpdatedConsumer> _logger;

    public CatalogItemUpdatedConsumer(IMapper mapper, ShoppingListContext context, ILogger<CatalogItemUpdatedConsumer> logger)
    {
        _mapper = mapper;
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CatalogItemUpdated> context)
    {
        // Extract the OperationId from the message headers
        string operationId = context.Headers.Get<string>("OperationId") ?? "Unknown operation Id";
        string SKU = context.Message.UpdatedItem.SKU.ToString() ?? "Unknown sku";

        _logger.LogInformation($"CatalogItemUpdated message received. ShoppingListService, Item SKU: {SKU}, OperationId: {operationId}");

        try
        {
            // Validate the incoming message
            var validator = new CatalogItemUpdatedValidator();
            var result = validator.Validate(context.Message);

            if (!result.IsValid)
            {
                _logger.LogError($"CatalogItemUpdated message: validation failed. ShoppingList Service, Item SKU: {SKU}, Errors: {string.Join(", ", result.Errors.Select(e => e.ErrorMessage))}, OperationId: {operationId}.");

                throw new ArgumentException("Invalid CatalogItemUpdated message received.");
            }

            _logger.LogInformation($"CatalogItemUpdated message: message is being consumed. Shopping List Service. Item SKU: {SKU}, Operation Id: {operationId}");


            // Begin a transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            // Fetch the catalog item
            var catalogItem = await _context.CatalogItems
                .FirstOrDefaultAsync(x => x.SKU == context.Message.UpdatedItem.SKU && x.Family == context.Message.UpdatedItem.Family);

            if (catalogItem == null)
            {
                _logger.LogWarning($"CatalogItemUpdated message: no item found in the category to be updated. ShoppingList Service. Item SKU: {SKU}, OperationId: {operationId}");

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

            _logger.LogInformation($"CatalogItemUpdated message: catalog item updated successfully. Shopping List Service. Item SKU: {SKU}, OperationId: {operationId}");

            // Commit the transaction
            await transaction.CommitAsync();

        }
        catch (Exception ex)
        {
            _logger.LogError($"CatalogItemUpdated message: error occured while processing the message. Shopping List Servive. Item SKU: {SKU}, OperationId: {operationId}, Error: {ex.Message}.");

        }
    }
}

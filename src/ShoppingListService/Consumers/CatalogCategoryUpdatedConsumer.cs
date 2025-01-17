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
    private readonly ILogger<CatalogCategoryUpdatedConsumer> _logger;
    public CatalogCategoryUpdatedConsumer(IMapper mapper, ShoppingListContext context, ILogger<CatalogCategoryUpdatedConsumer> logger)
    {
        _mapper = mapper;
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CatalogCategoryUpdated> context)
    {
        // Extract the OperationId from the message headers
        string operationId = context.Headers.Get<string>("OperationId") ?? "Unknown operation Id";
        string SKU = context.Message.Sku.ToString() ?? "Unknown sku";

        _logger.LogInformation($"CatalogCategoryUpdated message received. Category SKU: {SKU}, OperationId: {operationId}");
        try
        {
            var validator = new CatalogCategoryUpdateValidator();
            var result = validator.Validate(context.Message);

            if (!result.IsValid)
            {
                _logger.LogError($"CatalogCategoryUpdated message: validation failed. Category SKU: {SKU}, Errors: {string.Join(", ", result.Errors.Select(e => e.ErrorMessage))}");

                throw new ArgumentException("Invalid CatalogCategoryUpdated message received.");
            }

            _logger.LogInformation($"CatalogCategoryUpdated message: message is being consumed. Category SKU: {SKU}");

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
                _logger.LogWarning($"CatalogCategoryUpdated message: no item found in the category to be updated. Category SKU: {SKU}, OperationId: {operationId}");
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
            _logger.LogInformation($"CatalogCategoryUpdated message: catalog category updated successfully. Category SKU: {SKU}, OperationId: {operationId}");

        }
        catch (Exception ex)
        {
            _logger.LogError($"CatalogCategoryUpdated message: error occured while processing the message. Category SKU: {SKU}, OperationId: {operationId}, Error: {ex.Message}.");
        }
    }
}

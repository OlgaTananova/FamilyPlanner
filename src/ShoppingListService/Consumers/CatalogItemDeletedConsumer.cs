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
    private readonly ILogger<CatalogItemDeletedConsumer> _logger;

    public CatalogItemDeletedConsumer(IMapper mapper, ShoppingListContext context, ILogger<CatalogItemDeletedConsumer> logger)
    {
        _mapper = mapper;
        _context = context;
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<CatalogItemDeleted> context)
    {
        // Extract the OperationId from the message headers
        string operationId = context.Headers.Get<string>("OperationId") ?? "Unknown operation Id";
        string SKU = context.Message.SKU.ToString() ?? "Unknown sku";

        _logger.LogInformation($"CatalogItemDeleted message received. ShoppingListService, Item SKU: {SKU}, OperationId: {operationId}");
        try
        {
            var validator = new CatalogItemDeletedValidator();
            var result = validator.Validate(context.Message);

            if (!result.IsValid)
            {
                _logger.LogError($"CatalogItemDeleted message: validation failed. ShoppingList Service, Item SKU: {SKU}, Errors: {string.Join(", ", result.Errors.Select(e => e.ErrorMessage))}, OperationId: {operationId}.");

                throw new ArgumentException("Invalid CatalogItemDeleted message received.");
            }

            _logger.LogInformation($"CatalogItemDeleted message: message is being consumed. Shopping List Service. Item SKU: {SKU}, Operation Id: {operationId}");

            // Begin a transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            var catalogItem = await _context.CatalogItems.FirstOrDefaultAsync(x => x.SKU == context.Message.SKU
                    && x.Family == context.Message.Family);

            if (catalogItem == null)
            {

                _logger.LogError($"CatalogItemDeleted message: no item found to be updated. ShoppingList Service. Item SKU: {SKU}, OperationId: {operationId}");
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

            _logger.LogInformation($"CatalogItemDeleted message: catalog item deleted successfully. Shopping List Service. Item SKU: {SKU}, OperationId: {operationId}");


        }
        catch (Exception ex)
        {
            _logger.LogError($"CatalogItemDeleted message: error occured while processing the message. Shopping List Servive. Item SKU: {SKU}, OperationId: {operationId}, Error: {ex.Message}.");

        }

    }
}

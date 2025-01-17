using System;
using AutoMapper;
using Contracts.Catalog;
using MassTransit;
using ShoppingListService.Data;
using ShoppingListService.Entities;
using ShoppingListService.Helpers;

namespace ShoppingListService.Consumers;

public class CatalogItemCreatedConsumer : IConsumer<CatalogItemCreated>
{
    private readonly IMapper _mapper;
    private readonly ShoppingListContext _context;
    private readonly ILogger<CatalogItemCreatedConsumer> _logger;
    public CatalogItemCreatedConsumer(IMapper mapper, ShoppingListContext context, ILogger<CatalogItemCreatedConsumer> logger)
    {
        _mapper = mapper;
        _context = context;
        _logger = logger;

    }
    public async Task Consume(ConsumeContext<CatalogItemCreated> context)
    {
        // Extract the OperationId from the message headers
        string operationId = context.Headers.Get<string>("OperationId") ?? "Unknown operation Id";
        string SKU = context.Message.SKU.ToString() ?? "Unknown sku";

        _logger.LogInformation($"CatalogItemCreated message received. Item SKU: {SKU}, OperationId: {operationId}");

        try
        {
            var validator = new CatalogItemCreatedValidator();
            var result = validator.Validate(context.Message);

            if (!result.IsValid)
            {
                _logger.LogWarning($"CatalogItemCreated message: validation failed. Item SKU: {SKU}, Errors: {string.Join(", ", result.Errors.Select(e => e.ErrorMessage))}");

                throw new ArgumentException("Invalid CatalogItemCreated message received.");
            }

            _logger.LogInformation($"CatalogItemCreated message: message is being consumed. Item SKU: {SKU}");

            var newCatalogItem = _mapper.Map<CatalogItem>(context.Message);

            _context.CatalogItems.Add(newCatalogItem);

            await _context.SaveChangesAsync();

            _logger.LogInformation($"CatalogItemCreated message: catalog item saved successfully. Item SKU: {SKU}, OperationId: {operationId}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"CatalogItemCreated message: error occured while processing the message. Item SKU: {SKU}, OperationId: {operationId}, Error: {ex.Message}.");
        }

    }
}

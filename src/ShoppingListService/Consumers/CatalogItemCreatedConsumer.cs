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
    public CatalogItemCreatedConsumer(IMapper mapper, ShoppingListContext context)
    {
        _mapper = mapper;
        _context = context;
    }
    public async Task Consume(ConsumeContext<CatalogItemCreated> context)
    {
        try
        {
            var validator = new CatalogItemCreatedValidator();
            var result = validator.Validate(context.Message);

            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Validation error: {error.ErrorMessage}");
                }
                throw new ArgumentException("Invalid CatalogItemCreated message received.");
            }
            
            Console.WriteLine("--> Consuming catalog item created" + context.Message.SKU);

            var newCatalogItem = _mapper.Map<CatalogItem>(context.Message);
            _context.CatalogItems.Add(newCatalogItem);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

    }
}

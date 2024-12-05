using System;
using AutoMapper;
using Contracts.Catalog;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ShoppingListService.Data;
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

            var catalogItem = await _context.CatalogItems.FirstOrDefaultAsync(x => x.SKU == context.Message.SKU
                    && x.Family == context.Message.Family && x.OwnerId == context.Message.OwnerId);

            if (catalogItem != null)
            {
                _mapper.Map(context.Message, catalogItem);

                await _context.SaveChangesAsync();
            }
            else
            {

                Console.WriteLine($"Catalog item with SKU {context.Message.SKU} not found. Skipping update.");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

    }
}

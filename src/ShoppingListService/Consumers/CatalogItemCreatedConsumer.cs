using System;
using AutoMapper;
using Contracts.Catalog;
using MassTransit;
using ShoppingListService.Data;
using ShoppingListService.Entities;

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
        Console.WriteLine("--> Consuming catalog item created" + context.Message.Id);

        var newCatalogItem = _mapper.Map<CatalogItem>(context.Message);
        _context.CatalogItems.Add(newCatalogItem);
        await _context.SaveChangesAsync();
    }
}

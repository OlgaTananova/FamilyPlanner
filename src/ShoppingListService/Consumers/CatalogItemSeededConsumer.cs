using System;
using AutoMapper;
using Contracts.Catalog;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ShoppingListService.Data;
using ShoppingListService.Entities;

namespace ShoppingListService.Consumers;

public class CatalogItemSeededConsumer : IConsumer<CatalogItemSeeded>
{
    private readonly ShoppingListContext _dbcontext;
    private readonly IMapper _mapper;

    public CatalogItemSeededConsumer(IMapper mapper, ShoppingListContext context)
    {
        _dbcontext = context;
        _mapper = mapper;
    }
    public async Task Consume(ConsumeContext<CatalogItemSeeded> context)
    {
        Console.WriteLine("--> CatalogItemSeeded message received.");

        var catalogItems = _mapper.Map<List<CatalogItem>>(context.Message.Items);

        if (!_dbcontext.CatalogItems.Any())
        {
            _dbcontext.Database.EnsureCreated();
            await _dbcontext.CatalogItems.AddRangeAsync(catalogItems);
            await _dbcontext.SaveChangesAsync();

            Console.WriteLine("--> Catalog items seeded in ShoppingListService.");
        }
        else
        {
            Console.WriteLine("--> Catalog items already exist, skipping seeding.");
        }
    }
}

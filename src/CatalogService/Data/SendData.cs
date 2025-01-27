using System;
using AutoMapper;
using Contracts.Catalog;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Data;

public static class SendData
{
    public static async Task SendDataToShoppingListService(CatalogDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        var items = await context.Items.ToListAsync();
        var catalogSeededData = mapper.Map<CatalogItemSeeded>(items);

        await Task.Delay(TimeSpan.FromMinutes(1));
        
        Console.WriteLine($"Publishing {catalogSeededData.Items.Count} catalog items.");
        await publishEndpoint.Publish( catalogSeededData);
        await context.SaveChangesAsync();
        Console.WriteLine("CatalogSeededData event published."); ;
    }
}

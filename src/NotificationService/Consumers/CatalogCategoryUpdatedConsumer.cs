using System;
using Contracts.Catalog;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;


namespace ShoppingListService.Consumers;

public class CatalogCategoryUpdatedConsumer : IConsumer<CatalogCategoryUpdated>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    public CatalogCategoryUpdatedConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Consume(ConsumeContext<CatalogCategoryUpdated> context)
    {
        Console.WriteLine("--> catalog category updated message received");

        await _hubContext.Clients.Group(context.Message.Family).SendAsync("CatalogCategoryUpdatedMessage", context.Message);
    }
}

using System;
using Contracts.Catalog;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;


namespace ShoppingListService.Consumers;

public class CatalogItemDeletedConsumer : IConsumer<CatalogItemDeleted>
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public CatalogItemDeletedConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public async Task Consume(ConsumeContext<CatalogItemDeleted> context)
    {
        Console.WriteLine("--> Catalog item deleted message received");
        await _hubContext.Clients.Group(context.Message.Family).SendAsync("CatalogItemDeleted", context.Message);
    }
}

using System;
using Contracts.Catalog;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace ShoppingListService.Consumers;

public class CatalogItemUpdatedConsumer : IConsumer<CatalogItemUpdated>
{

    private readonly IHubContext<NotificationHub> _hubContext;

    public CatalogItemUpdatedConsumer(IHubContext<NotificationHub> hubContext)
    {

        _hubContext = hubContext;
    }

    public async Task Consume(ConsumeContext<CatalogItemUpdated> context)
    {
        Console.WriteLine("--> catalog item updated message received");

        await _hubContext.Clients.Group(context.Message.UpdatedItem.Family).SendAsync("CatalogItemUpdated", context.Message);
    }
}

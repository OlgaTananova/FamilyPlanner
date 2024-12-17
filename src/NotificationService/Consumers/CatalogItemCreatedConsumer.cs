using System;
using Contracts.Catalog;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace ShoppingListService.Consumers;

public class CatalogItemCreatedConsumer : IConsumer<CatalogItemCreated>
{

    private readonly IHubContext<NotificationHub> _hubContext;

    public CatalogItemCreatedConsumer(IHubContext<NotificationHub> hubContext)
    {

        _hubContext = hubContext;
    }

    public async Task Consume(ConsumeContext<CatalogItemCreated> context)
    {
        Console.WriteLine("--> catalog item created message received");

        await _hubContext.Clients.Group(context.Message.Family).SendAsync("CatalogItemCreated", context.Message);
    }
}

using System;
using Contracts.Catalog;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class CatalogCategoryCreatedConsumer : IConsumer<CatalogCategoryCreated>
{

    private readonly IHubContext<NotificationHub> _hubContext;
    public CatalogCategoryCreatedConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Consume(ConsumeContext<CatalogCategoryCreated> context)
    {
        Console.WriteLine("--> catalog category updated message received");

        await _hubContext.Clients.Group(context.Message.Family).SendAsync("CatalogCategoryCreated", context.Message);
    }
}

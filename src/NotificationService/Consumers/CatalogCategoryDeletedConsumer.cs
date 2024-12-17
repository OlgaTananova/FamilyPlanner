using System;
using Contracts.Catalog;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class CatalogCategoryDeletedConsumer : IConsumer<CatalogCategoryDeleted>
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public CatalogCategoryDeletedConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public async Task Consume(ConsumeContext<CatalogCategoryDeleted> context)
    {
        Console.WriteLine("--> Catalog category deleted message received");
        await _hubContext.Clients.Group(context.Message.Family).SendAsync("CatalogCategoryDeleted", context.Message);
    }
}

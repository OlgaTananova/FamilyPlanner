using System;
using Contracts.ShoppingLists;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class ShoppingListItemDeletedConsumer : IConsumer<ShoppingListItemDeleted>
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public ShoppingListItemDeletedConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public async Task Consume(ConsumeContext<ShoppingListItemDeleted> context)
    {
        Console.WriteLine("--> shopping list item deleted message received");

        await _hubContext.Clients.Group(context.Message.Family).SendAsync("ShoppingListItemDeleted", context.Message);

    }
}


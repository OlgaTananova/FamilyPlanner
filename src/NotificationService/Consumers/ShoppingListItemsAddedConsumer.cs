using System;
using Contracts.ShoppingLists;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class ShoppingListItemsAddedConsumer : IConsumer<ShoppingListItemsAdded>
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public ShoppingListItemsAddedConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task Consume(ConsumeContext<ShoppingListItemsAdded> context)
    {
        Console.WriteLine("--> shopping list items added message received");

        await _hubContext.Clients.Group(context.Message.Family).SendAsync("ShoppingListItemsAdded", context.Message);
    }
}

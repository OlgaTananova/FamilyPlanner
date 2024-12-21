using System;
using Contracts.ShoppingLists;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class ShoppingListItemUpdatedConsumer : IConsumer<ShoppingListItemUpdated>
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public ShoppingListItemUpdatedConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public Task Consume(ConsumeContext<ShoppingListItemUpdated> context)
    {
        Console.WriteLine("--> shopping list item updated message received");

        return _hubContext.Clients.Group(context.Message.Family).SendAsync("ShoppingListItemUpdated", context.Message);
    }
}

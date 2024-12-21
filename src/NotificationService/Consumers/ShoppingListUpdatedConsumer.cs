using System;
using Contracts.ShoppingLists;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class ShoppingListUpdatedConsumer : IConsumer<ShoppingListUpdated>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    public ShoppingListUpdatedConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public async Task Consume(ConsumeContext<ShoppingListUpdated> context)
    {
        Console.WriteLine("--> shopping list updated message received");

        await _hubContext.Clients.Group(context.Message.Family).SendAsync("ShoppingListUpdated", context.Message);
    }
}


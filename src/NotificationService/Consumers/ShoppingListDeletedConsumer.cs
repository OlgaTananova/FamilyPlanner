using System;
using Contracts.ShoppingLists;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class ShoppingListDeletedConsumer : IConsumer<ShoppingListDeleted>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    public ShoppingListDeletedConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public async Task Consume(ConsumeContext<ShoppingListDeleted> context)
    {
        Console.WriteLine("--> shopping list deleted message received");

        await _hubContext.Clients.Group(context.Message.Family).SendAsync("ShoppingListDeleted", context.Message);
    }
}

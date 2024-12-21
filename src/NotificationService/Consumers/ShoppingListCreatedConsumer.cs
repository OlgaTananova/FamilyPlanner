using System;
using Contracts.ShoppingLists;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class ShoppingListCreatedConsumer : IConsumer<ShoppingListCreated>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    public ShoppingListCreatedConsumer(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public async Task Consume(ConsumeContext<ShoppingListCreated> context)
    {
        Console.WriteLine("--> shopping list created message received");

        await _hubContext.Clients.Group(context.Message.Family).SendAsync("ShoppingListCreated", context.Message);
    }
}


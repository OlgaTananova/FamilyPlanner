using System;
using Contracts.ShoppingLists;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class ShoppingListDeletedConsumer : IConsumer<ShoppingListDeleted>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<ShoppingListDeletedConsumer> _logger;
    public ShoppingListDeletedConsumer(IHubContext<NotificationHub> hubContext, ILogger<ShoppingListDeletedConsumer> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<ShoppingListDeleted> context)
    {
        string? operationId = context.Headers.Get<string>("OperationId");

        _logger.LogInformation($"Delete Shopping List message received. Service: Notification Service, UserId: {context.Message.OwnerId}, Family: {context.Message.Family}, OperationId: {operationId}");

        var activity = new System.Diagnostics.Activity("ConsumeShoppingListDeleted");

        if (!string.IsNullOrEmpty(operationId))
        {
            activity.SetParentId(operationId);
        }
        activity.Start();

        try
        {
            await _hubContext.Clients.Group(context.Message.Family).SendAsync("ShoppingListDeleted", context.Message);
            _logger.LogInformation($"Delete Shopping List message is sent to the clients. Service: Notification Service, UserId: {context.Message.OwnerId}, Family: {context.Message.Family}, OperationId: {operationId}");

        }
        catch
        {
            _logger.LogError($"Deleted Shopping List message failed. Service: Notification Service, UserId: {context.Message.OwnerId}, Family: {context.Message.Family}, OperationId: {operationId}");

        }
        finally
        {
            activity.Stop();
        }

    }
}

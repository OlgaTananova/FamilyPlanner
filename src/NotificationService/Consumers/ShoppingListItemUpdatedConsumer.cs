using System;
using Contracts.ShoppingLists;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class ShoppingListItemUpdatedConsumer : IConsumer<ShoppingListItemUpdated>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<ShoppingListItemUpdatedConsumer> _logger;

    public ShoppingListItemUpdatedConsumer(IHubContext<NotificationHub> hubContext, ILogger<ShoppingListItemUpdatedConsumer> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<ShoppingListItemUpdated> context)
    {
        string? operationId = context.Headers.Get<string>("OperationId");

        _logger.LogInformation($"Update Shopping List Item message received. Service: Notification Service, UserId: {context.Message.OwnerId}, Family: {context.Message.Family}, OperationId: {operationId}");

        var activity = new System.Diagnostics.Activity("ConsumeShoppingListItemUpdated");

        if (!string.IsNullOrEmpty(operationId))
        {
            activity.SetParentId(operationId);
        }
        activity.Start();
        try
        {
            await _hubContext.Clients.Group(context.Message.Family).SendAsync("ShoppingListItemUpdated", context.Message);
            _logger.LogInformation($"Update Shopping List Item message is sent to the clients. Service: Notification Service, UserId: {context.Message.OwnerId}, Family: {context.Message.Family}, OperationId: {operationId}");
        }
        catch
        {
            _logger.LogError($"Update Shopping List Item message failed. Service: Notification Service, UserId: {context.Message.OwnerId}, Family: {context.Message.Family}, OperationId: {operationId}");

        }
        finally
        {
            activity.Stop();
        }
    }
}

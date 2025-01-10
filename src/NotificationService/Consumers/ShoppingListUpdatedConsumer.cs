using System;
using Contracts.ShoppingLists;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class ShoppingListUpdatedConsumer : IConsumer<ShoppingListUpdated>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<ShoppingListUpdatedConsumer> _logger;
    public ShoppingListUpdatedConsumer(IHubContext<NotificationHub> hubContext, ILogger<ShoppingListUpdatedConsumer> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<ShoppingListUpdated> context)
    {
        string? operationId = context.Headers.Get<string>("OperationId");

        _logger.LogInformation($"Update Shopping List message received. Service: Notification Service, UserId: {context.Message.OwnerId}, Family: {context.Message.Family}, OperationId: {operationId}");

        var activity = new System.Diagnostics.Activity("ConsumeShoppingListUpdated");

        if (!string.IsNullOrEmpty(operationId))
        {
            activity.SetParentId(operationId);
        }
        activity.Start();
        try
        {
            await _hubContext.Clients.Group(context.Message.Family).SendAsync("ShoppingListUpdated", context.Message);
            _logger.LogInformation($"Update Shopping List message is sent to the clients. Service: Notification Service, UserId: {context.Message.OwnerId}, Family: {context.Message.Family}, OperationId: {operationId}");

        }
        catch
        {
            _logger.LogError($"Update Shopping List message failed. Service: Notification Service, UserId: {context.Message.OwnerId}, Family: {context.Message.Family}, OperationId: {operationId}");

        }
        finally
        {
            activity.Stop();
        }
    }
}


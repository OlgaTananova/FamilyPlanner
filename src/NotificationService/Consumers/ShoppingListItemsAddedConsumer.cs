using System;
using Contracts.ShoppingLists;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class ShoppingListItemsAddedConsumer : IConsumer<ShoppingListItemsAdded>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<ShoppingListItemsAddedConsumer> _logger;

    public ShoppingListItemsAddedConsumer(IHubContext<NotificationHub> hubContext, ILogger<ShoppingListItemsAddedConsumer> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ShoppingListItemsAdded> context)
    {
        string? operationId = context.Headers.Get<string>("OperationId");

        _logger.LogInformation($"Add Shopping List Item message received. Service: Notification Service, UserId: {context.Message.OwnerId}, Family: {context.Message.Family}, OperationId: {operationId}");

        var activity = new System.Diagnostics.Activity("ConsumeShoppingListItemAdded");

        if (!string.IsNullOrEmpty(operationId))
        {
            activity.SetParentId(operationId);
        }
        activity.Start();
        try
        {
            await _hubContext.Clients.Group(context.Message.Family).SendAsync("ShoppingListItemsAdded", context.Message);
            _logger.LogInformation($"Add Shopping List Item message is sent to the clients. Service: Notification Service, UserId: {context.Message.OwnerId}, Family: {context.Message.Family}, OperationId: {operationId}");

        }
        catch
        {
            _logger.LogError($"Add Shopping List Item message failed. Service: Notification Service, UserId: {context.Message.OwnerId}, Family: {context.Message.Family}, OperationId: {operationId}");

        }
        finally
        {
            activity.Stop();
        }
    }
}

using System;
using Contracts.ShoppingLists;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class ShoppingListCreatedConsumer : IConsumer<ShoppingListCreated>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<ShoppingListCreatedConsumer> _logger;
    public ShoppingListCreatedConsumer(IHubContext<NotificationHub> hubContext, ILogger<ShoppingListCreatedConsumer> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<ShoppingListCreated> context)
    {
        string? operationId = context.Headers.Get<string>("OperationId");

        _logger.LogInformation($"Create Shopping List message received. Service: Notification Service, UserId: {context.Message.OwnerId}, Family: {context.Message.Family}, OperationId: {operationId}");

        var activity = new System.Diagnostics.Activity("ConsumeShoppingListCreated");

        if (!string.IsNullOrEmpty(operationId))
        {
            activity.SetParentId(operationId);
        }
        activity.Start();
        try
        {
            await _hubContext.Clients.Group(context.Message.Family).SendAsync("ShoppingListCreated", context.Message);
            _logger.LogInformation($"Create Shopping List message is sent to the clients. Service: Notification Service, UserId: {context.Message.OwnerId}, Family: {context.Message.Family}, OperationId: {operationId}");

        }
        catch
        {
            _logger.LogError($"Create Shopping List message failed. Service: Notification Service, UserId: {context.Message.OwnerId}, Family: {context.Message.Family}, OperationId: {operationId}");

        }
        finally
        {
            activity.Stop();
        }

    }
}


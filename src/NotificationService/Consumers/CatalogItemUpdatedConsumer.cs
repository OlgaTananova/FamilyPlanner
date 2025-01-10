using System;
using Contracts.Catalog;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class CatalogItemUpdatedConsumer : IConsumer<CatalogItemUpdated>
{

    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<CatalogItemUpdatedConsumer> _logger;

    public CatalogItemUpdatedConsumer(IHubContext<NotificationHub> hubContext, ILogger<CatalogItemUpdatedConsumer> logger)
    {

        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CatalogItemUpdated> context)
    {
        Console.WriteLine("--> catalog item updated message received");

        string? operationId = context.Headers.Get<string>("OperationId");

        _logger.LogInformation($"Update Item message received. Service: Notification Service, UserId: {context.Message.UpdatedItem.OwnerId}, Family: {context.Message.UpdatedItem.Family}, OperationId: {operationId}");

        var activity = new System.Diagnostics.Activity("ConsumeCatalogItemUpdated");

        if (!string.IsNullOrEmpty(operationId))
        {
            activity.SetParentId(operationId);
        }
        activity.Start();
        try
        {
            await _hubContext.Clients.Group(context.Message.UpdatedItem.Family).SendAsync("CatalogItemUpdated", context.Message);
            _logger.LogInformation($"Update Item message is sent to the clients. Service: Notification Service, UserId: {context.Message.UpdatedItem.OwnerId}, Family: {context.Message.UpdatedItem.Family}, OperationId: {operationId}");

        }
        catch
        {
            _logger.LogError($"Update Item message failed. Service: Notification Service, UserId: {context.Message.UpdatedItem.OwnerId}, Family: {context.Message.UpdatedItem.Family}, OperationId: {operationId}");

        }
        finally
        {
            activity.Stop();
        }

    }
}

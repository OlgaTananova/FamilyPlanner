using System;
using Contracts.Catalog;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;


namespace NotificationService.Consumers;

public class CatalogItemDeletedConsumer : IConsumer<CatalogItemDeleted>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<CatalogItemDeletedConsumer> _logger;

    public CatalogItemDeletedConsumer(IHubContext<NotificationHub> hubContext, ILogger<CatalogItemDeletedConsumer> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<CatalogItemDeleted> context)
    {
        Console.WriteLine("--> Catalog item deleted message received");

        string? operationId = context.Headers.Get<string>("OperationId");
        _logger.LogInformation($"Delete Item message received. Service: Notification Service, UserId: {context.Message.OwnerId}, Family: {context.Message.Family}, OperationId: {operationId}");

        var activity = new System.Diagnostics.Activity("ConsumeCatalogItemDeleted");

        if (!string.IsNullOrEmpty(operationId))
        {
            activity.SetParentId(operationId);
        }
        activity.Start();

        try
        {
            await _hubContext.Clients.Group(context.Message.Family).SendAsync("CatalogItemDeleted", context.Message);
            _logger.LogInformation($"Delete Item message is sent to the clients. Service: Notification Service, UserId: {context.Message.OwnerId}, Family: {context.Message.Family}, OperationId: {operationId}");

        }
        catch
        {
            _logger.LogError($"Delete Item message failed. Service: Notification Service, UserId: {context.Message.OwnerId}, Family: {context.Message.Family}, OperationId: {operationId}");
        }
        finally
        {
            activity.Stop();
        }


    }
}

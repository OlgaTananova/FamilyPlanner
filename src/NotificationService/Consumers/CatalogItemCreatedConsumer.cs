using System;
using Contracts.Catalog;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class CatalogItemCreatedConsumer : IConsumer<CatalogItemCreated>
{

    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<CatalogItemCreatedConsumer> _logger;

    public CatalogItemCreatedConsumer(IHubContext<NotificationHub> hubContext, ILogger<CatalogItemCreatedConsumer> logger)
    {

        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CatalogItemCreated> context)
    {
        Console.WriteLine("--> catalog item created message received");

        string? operationId = context.Headers.Get<string>("OperationId");
        _logger.LogInformation($"Create Item message received. Service: Notification Service, UserId: {context.Message.OwnerId}, Family: {context.Message.Family}, OperationId: {operationId}");

        var activity = new System.Diagnostics.Activity("ConsumeCatalogItemCreated");

        if (!string.IsNullOrEmpty(operationId))
        {
            activity.SetParentId(operationId);
        }
        activity.Start();

        try
        {
            await _hubContext.Clients.Group(context.Message.Family).SendAsync("CatalogItemCreated", context.Message);
            _logger.LogInformation($"Create Item message is sent to the clients. Service: Notification Service, UserId: {context.Message.OwnerId}, Family: {context.Message.Family}, OperationId: {operationId}");
        }
        catch
        {
            _logger.LogError($"Create Item message failed. Service: Notification Service, UserId: {context.Message.OwnerId}, Family: {context.Message.Family}, OperationId: {operationId}");
        }
        finally
        {
            activity.Stop();
        }

    }
}

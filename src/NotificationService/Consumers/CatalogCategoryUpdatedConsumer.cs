using System;
using Contracts.Catalog;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;


namespace NotificationService.Consumers;

public class CatalogCategoryUpdatedConsumer : IConsumer<CatalogCategoryUpdated>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<CatalogCategoryUpdatedConsumer> _logger;
    public CatalogCategoryUpdatedConsumer(IHubContext<NotificationHub> hubContext, ILogger<CatalogCategoryUpdatedConsumer> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CatalogCategoryUpdated> context)
    {
        Console.WriteLine("--> catalog category updated message received");

        string? operationId = context.Headers.Get<string>("OperationId");
        _logger.LogInformation($"Update Category message received. Service: Notification Service, UserId: {context.Message.OwnerId}, Family: {context.Message.Family}, OperationId: {operationId}");

        var activity = new System.Diagnostics.Activity("ConsumeCatalogCategoryUpdated");

        if (!string.IsNullOrEmpty(operationId))
        {
            activity.SetParentId(operationId); // Set the parent OperationId
        }
        activity.Start();
        try
        {
            await _hubContext.Clients.Group(context.Message.Family).SendAsync("CatalogCategoryUpdated", context.Message);
            _logger.LogInformation($"Update Category message is sent to the clients. Service: Notification Service, UserId: {context.Message.OwnerId}, Family: {context.Message.Family}, OperationId: {operationId}");

        }
        catch
        {
            _logger.LogError($"Update Category message failed. Service: Notification Service, UserId: {context.Message.OwnerId}, Family: {context.Message.Family}, OperationId: {operationId}");
        }
        finally
        {
            activity.Stop();
        }


    }
}

using System;
using Contracts.Catalog;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration.UserSecrets;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class CatalogCategoryDeletedConsumer : IConsumer<CatalogCategoryDeleted>
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<CatalogCategoryDeletedConsumer> _logger;

    public CatalogCategoryDeletedConsumer(IHubContext<NotificationHub> hubContext, ILogger<CatalogCategoryDeletedConsumer> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }
    public async Task Consume(ConsumeContext<CatalogCategoryDeleted> context)
    {
        Console.WriteLine("--> Catalog category deleted message received");

        string? operationId = context.Headers.Get<string>("OperationId");

        _logger.LogInformation($"Delete Category message received. Service: Notification Service, UserId: {context.Message.OwnerId}, Family: {context.Message.Family}, OperationId: {operationId}");

        var activity = new System.Diagnostics.Activity("ConsumeCatalogCategoryDeleted");

        if (!string.IsNullOrEmpty(operationId))
        {
            activity.SetParentId(operationId); // Set the parent OperationId
        }
        activity.Start();
        try
        {
            await _hubContext.Clients.Group(context.Message.Family).SendAsync("CatalogCategoryDeleted", context.Message);
            _logger.LogInformation($"Delete Category message is sent to the clients. Service: Notification Service, UserId: {context.Message.OwnerId}, Family: {context.Message.Family}, OperationId: {operationId}");
        }
        catch
        {
            _logger.LogError($"Delete Category message failed. Service: Notification Service, UserId: {context.Message.OwnerId}, Family: {context.Message.Family}, OperationId: {operationId}");
        }
        finally
        {
            activity.Stop();
        }

    }
}

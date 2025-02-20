using Contracts.Catalog;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Hubs;

namespace NotificationService.Consumers;

public class CatalogCategoryCreatedConsumer : IConsumer<CatalogCategoryCreated>
{

    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<CatalogCategoryCreatedConsumer> _logger;
    public CatalogCategoryCreatedConsumer(IHubContext<NotificationHub> hubContext, ILogger<CatalogCategoryCreatedConsumer> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CatalogCategoryCreated> context)
    {
        Console.WriteLine("--> Catalog category created message received");

        // Extract the OperationId from the message headers
        string? operationId = context.Headers.Get<string>("OperationId");
        string? traceId = context.Headers.Get<string>("traceId");
        string? requestId = context.Headers.Get<string>("requestId");

        Console.WriteLine($"OperationId: {operationId}, TraceId: {traceId}, RequestId: {requestId}");

        _logger.LogInformation("Create Category message received. Service: Notification Service, UserId: {UserId}, Family: {Family}, CategoryName: {CategoryName}, OperationId: {OperationId}",
        context.Message.OwnerId, context.Message.Family, context.Message.Name, operationId);
        var activity = new System.Diagnostics.Activity("ConsumeCatalogCategoryCreated");

        if (!string.IsNullOrEmpty(operationId))
        {
            activity.SetParentId(operationId); // Set the parent OperationId
        }

        activity.Start();

        try
        {
            // Notify SignalR clients
            await _hubContext.Clients.Group(context.Message.Family).SendAsync("CatalogCategoryCreated", context.Message);
            _logger.LogInformation("Create Category message is sent. Service: Notification Service, UserId: {UserId}, Family: {Family}, CategoryName: {CategoryName}, OperationId: {OperationId}",
        context.Message.OwnerId, context.Message.Family, context.Message.Name, operationId);
        }
        catch
        {
            _logger.LogError($"Create Category message failed. Service: Notification Service, UserId: {context.Message.OwnerId}, Family: {context.Message.Family}, CategoryName: {context.Message.Name}, OperationId: {operationId}");
        }
        finally
        {
            activity.Stop();
        }
    }
}

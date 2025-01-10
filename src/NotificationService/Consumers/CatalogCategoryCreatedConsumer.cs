using System;
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

        // Create a new Activity or continue the existing one
        var activity = new System.Diagnostics.Activity("ConsumeCatalogCategoryCreated");

        if (!string.IsNullOrEmpty(operationId))
        {
            activity.SetParentId(operationId); // Set the parent OperationId
        }

        // Start the activity
        activity.Start();

        try
        {
            // Log the message with OperationId
            _logger.LogInformation("Catalog category created message received. UserId: {UserId}, Family: {Family}, CategoryName: {CategoryName}, OperationId: {OperationId}",
                context.Message.OwnerId, context.Message.Family, context.Message.Name, operationId);

            // Notify SignalR clients
            await _hubContext.Clients.Group(context.Message.Family).SendAsync("CatalogCategoryCreated", context.Message);
        }
        finally
        {
            // Stop the activity after the operation is complete
            activity.Stop();
        }
    }
}

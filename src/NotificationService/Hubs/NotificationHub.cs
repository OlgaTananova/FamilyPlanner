using System;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace NotificationService.Hubs;

public class NotificationHub : Hub

{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var user = Context.User; // Access the ClaimsPrincipal from the token

        if (user?.Identity?.IsAuthenticated ?? false)
        {
            var familyName = user?.FindFirst("family")?.Value;
            Console.WriteLine($"The family is  {familyName}");

            if (!string.IsNullOrEmpty(familyName))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, familyName);
            }
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }
        else
        {
            // Reject the connection if the token is invalid
            Context.Abort();
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var familyName = Context.User?.FindFirst("family")?.Value;

        if (!string.IsNullOrEmpty(familyName))
        {

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, familyName);
        }
    }
}

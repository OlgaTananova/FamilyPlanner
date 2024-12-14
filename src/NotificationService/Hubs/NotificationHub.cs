using System;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace NotificationService.Hubs;

public class NotificationHub : Hub

{
    private static readonly ConcurrentDictionary<string, HashSet<string>> GroupConnections =
      new ConcurrentDictionary<string, HashSet<string>>();
    public override async Task OnConnectedAsync()
    {
        var user = Context.User; // Access the ClaimsPrincipal from the token
        
        if (user?.Identity?.IsAuthenticated ?? false)
        {
            var familyName = user.Claims.First().Value;
            Console.WriteLine($"user claims coutn {familyName}");

            // if (!string.IsNullOrEmpty(familyName))
            // {
            //     var connections = GroupConnections.GetOrAdd(familyName, _ => new HashSet<string>());
            //     lock (connections)
            //     {
            //         connections.Add(Context.ConnectionId);
            //     }
            //     // Add the user to a SignalR group based on their family name
            //     await Groups.AddToGroupAsync(Context.ConnectionId, familyName);
            // }
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

        if (!string.IsNullOrEmpty(familyName) && GroupConnections.TryGetValue(familyName, out var connections))
        {
            lock (connections)
            {
                connections.Remove(Context.ConnectionId);
                if (connections.Count == 0)
                {
                    GroupConnections.TryRemove(familyName, out _);
                }
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, familyName);
        }
    }

    public bool DoesGroupExist(string groupName)
    {
        return GroupConnections.ContainsKey(groupName);
    }
}

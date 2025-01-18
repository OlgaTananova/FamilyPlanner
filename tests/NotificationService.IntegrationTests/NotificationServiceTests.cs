using System.Collections;
using Contracts.Catalog;
using MassTransit.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using NotificationService.IntegrationTests.Fixtures;
using NotificationService.IntegrationTests.Utils;

namespace NotificationService.IntegrationTests;

[Collection("Shared collection")]
public class NotificationServiceTests : IAsyncLifetime
{
    private readonly CustomWebAppFactory _factory;
    private readonly ITestHarness _testHarness;
    private readonly HubConnection _hubConnection;

    public NotificationServiceTests()
    {
        _factory = new CustomWebAppFactory();
        _testHarness = _factory.Services.GetTestHarness();

        // Configure the SignalR client to connect to the real hub
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_factory.Server.BaseAddress + "notifications", options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                var token = AuthHelper.GetBearerForUser("test-user-id", "test-family");
                options.AccessTokenProvider = () => Task.FromResult(token);

            })
            .Build();

    }

    public async Task DisposeAsync()
    {
        await _testHarness.Stop(); // Stop MassTransit test harness
        await _hubConnection.DisposeAsync(); // Dispose SignalR connection;
    }

    public async Task InitializeAsync()
    {
        await _testHarness.Start(); // Start MassTransit test harness
        await _hubConnection.StartAsync(); // Start the SignalR connection
    }

    [Fact]
    public async Task CatalogCategoryCreatedConsumer_ShouldNotifyClients_WhenMessageIsConsumed()
    {
        // Arrange
        var message = new CatalogCategoryCreated
        {
            OwnerId = "test-user-id",
            Family = "test-family",
            Name = "Test Category",
            Sku = Guid.NewGuid(),
            Id = Guid.NewGuid(),
        };
        // Act
        await _testHarness.Bus.Publish(message);
        await Task.Delay(5000);
        bool consumed = await _testHarness.Consumed.Any<CatalogCategoryCreated>();
        // Wait for the message to be consumed
        Assert.True(consumed);

        // Assert SignalR notifications
        _hubConnection.On<CatalogCategoryCreated>("CatalogCategoryCreated", message =>
         {

             Assert.Equal("Test Category", message.Name);
         });

    }

}

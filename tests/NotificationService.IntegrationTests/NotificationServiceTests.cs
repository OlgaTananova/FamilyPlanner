using System.Collections;
using Contracts.Catalog;
using Contracts.ShoppingLists;
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
                options.AccessTokenProvider = () => Task.FromResult(token)!;

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
        await Task.Delay(500);
        bool consumed = await _testHarness.Consumed.Any<CatalogCategoryCreated>();


        // Assert
        Assert.True(consumed);
        _hubConnection.On<CatalogCategoryCreated>("CatalogCategoryCreated", message =>
         {

             Assert.Equal("Test Category", message.Name);
         });

    }

    [Fact]
    public async Task CatalogCategoryUpdatedConsumer_ShouldNotifyClients_WhenMessageIsConsumed()
    {
        // Arrange
        var message = new CatalogCategoryUpdated
        {
            OwnerId = "test-user-id",
            Family = "test-family",
            Name = "Updated Category",
            Sku = Guid.NewGuid()
        };

        // Act
        await _testHarness.Bus.Publish(message);
        await Task.Delay(500);
        bool consumed = await _testHarness.Consumed.Any<CatalogCategoryUpdated>();

        // Assert
        Assert.True(consumed);
        _hubConnection.On<CatalogCategoryUpdated>("CatalogCategoryUpdated", message =>
        {
            Assert.Equal("Updated Category", message.Name);
        });
    }

    [Fact]
    public async Task CatalogCategoryDeletedConsumer_ShouldNotifyClients_WhenMessageIsConsumed()
    {
        // Arrange
        var message = new CatalogCategoryDeleted
        {
            OwnerId = "test-user-id",
            Family = "test-family",
            Id = Guid.NewGuid(),
            Sku = Guid.NewGuid()
        };

        // Act
        await _testHarness.Bus.Publish(message);
        await Task.Delay(500);
        bool consumed = await _testHarness.Consumed.Any<CatalogCategoryDeleted>();

        // Assert
        Assert.True(consumed);
        _hubConnection.On<CatalogCategoryDeleted>("CatalogCategoryDeleted", message =>
        {
            Assert.NotNull(message);
        });
    }

    [Fact]
    public async Task CatalogItemCreatedConsumer_ShouldNotifyClients_WhenMessageIsConsumed()
    {
        // Arrange
        var message = new CatalogItemCreated
        {
            OwnerId = "test-user-id",
            Family = "test-family",
            Name = "Test Item",
            SKU = Guid.NewGuid(),
            CategorySKU = Guid.NewGuid(),
            CategoryName = "Test Category",
            IsDeleted = false
        };

        // Act
        await _testHarness.Bus.Publish(message);
        await Task.Delay(500);
        bool consumed = await _testHarness.Consumed.Any<CatalogItemCreated>();

        // Assert
        Assert.True(consumed);
        _hubConnection.On<CatalogItemCreated>("CatalogItemCreated", receivedMessage =>
        {
            Assert.Equal("Test Item", receivedMessage.Name);
            Assert.Equal("test-family", receivedMessage.Family);
        });
    }

    [Fact]
    public async Task CatalogItemUpdatedConsumer_ShouldNotifyClients_WhenMessageIsConsumed()
    {
        // Arrange
        var message = new CatalogItemUpdated
        {
            UpdatedItem = new UpdatedItem
            {
                OwnerId = "test-user-id",
                Family = "test-family",
                Name = "Updated Item",
                SKU = Guid.NewGuid(),
                CategorySKU = Guid.NewGuid(),
                CategoryName = "Updated Category",
                IsDeleted = false
            }
        };

        //Act
        await _testHarness.Bus.Publish(message);
        await Task.Delay(500); // Allow some time for the message to be processed
        bool consumed = await _testHarness.Consumed.Any<CatalogItemUpdated>();

        // Assert
        Assert.True(consumed);
        _hubConnection.On<CatalogItemUpdated>("CatalogItemUpdated", receivedMessage =>
        {
            Assert.Equal("Updated Item", receivedMessage.UpdatedItem.Name);
            Assert.Equal("Updated Category", receivedMessage.UpdatedItem.CategoryName);
        });


    }

    [Fact]
    public async Task CatalogItemDeletedConsumer_ShouldNotifyClients_WhenMessageIsConsumed()
    {
        // Arrange
        var message = new CatalogItemDeleted
        {
            OwnerId = "test-user-id",
            Family = "test-family",
            SKU = Guid.NewGuid(),
        };

        // Act

        await _testHarness.Bus.Publish(message);
        await Task.Delay(1000);
        bool consumed = await _testHarness.Consumed.Any<CatalogItemDeleted>();

        // Assert
        Assert.True(consumed);
        // Act
        _hubConnection.On<CatalogItemDeleted>("CatalogItemDeleted", receivedMessage =>
        {
            Assert.NotNull(receivedMessage);
        });

    }

    [Fact]
    public async Task ShoppingListCreatedConsumer_ShouldNotifyClients_WhenMessageIsConsumed()
    {
        // Arrange
        var message = new ShoppingListCreated
        {
            OwnerId = "test-user-id",
            Family = "test-family",
            Heading = "Test Shopping List",
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.Now,
            Items = new List<ShoppingListItemDto>(),
            SalesTax = 0,
            IsArchived = false,
            IsDeleted = false
        };

        _hubConnection.On<ShoppingListCreated>("ShoppingListCreated", receivedMessage =>
        {
            Assert.Equal("Test Shopping List", receivedMessage.Heading);
        });

        // Act
        await _testHarness.Bus.Publish(message);
        await Task.Delay(1000);  // Allow time for processing

        // Assert
        bool consumed = await _testHarness.Consumed.Any<ShoppingListCreated>();
        Assert.True(consumed);
    }

    [Fact]
    public async Task ShoppingListUpdatedConsumer_ShouldNotifyClients_WhenMessageIsConsumed()
    {
        // Arrange
        var message = new ShoppingListUpdated
        {
            OwnerId = "test-user-id",
            Family = "test-family",
            Heading = "Updated Shopping List",
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.Now,
            Items = new List<ShoppingListItemDto>(),
            SalesTax = 0,
            IsArchived = false,
            IsDeleted = false
        };


        _hubConnection.On<ShoppingListUpdated>("ShoppingListUpdated", msg =>
        {
            Assert.Equal(message.Heading, msg.Heading);
        });

        // Act
        await _testHarness.Bus.Publish(message);
        await Task.Delay(5000);

        // Assert
        bool consumed = await _testHarness.Consumed.Any<ShoppingListUpdated>();
        Assert.True(consumed);
    }


    [Fact]
    public async Task ShoppingListDeletedConsumer_ShouldNotifyClients_WhenMessageIsConsumed()
    {
        // Arrange
        var message = new ShoppingListDeleted
        {
            OwnerId = "test-user-id",
            Family = "test-family",
            Id = Guid.NewGuid(),
            IsDeleted = true
        };

        _hubConnection.On<ShoppingListDeleted>("ShoppingListDeleted", receivedMessage =>
        {
            Assert.Equal(message.Id, receivedMessage.Id);
        });

        // Act
        await _testHarness.Bus.Publish(message);
        await Task.Delay(1000);

        // Assert
        bool consumed = await _testHarness.Consumed.Any<ShoppingListDeleted>();
        Assert.True(consumed);
    }

    [Fact]
    public async Task ShoppingListItemsAddedConsumer_ShouldNotifyClients_WhenMessageIsConsumed()
    {
        // Arrange
        var message = new ShoppingListItemsAdded
        {
            Id = Guid.NewGuid(),
            Heading = "Test Shopping List",
            CreatedAt = DateTime.Now,
            Items = new List<ShoppingListItemDto>(),
            SalesTax = 0,
            IsArchived = false,
            IsDeleted = false,
            OwnerId = "test-user-id",
            Family = "test-family"
        };

        ShoppingListItemsAdded? receivedMessage = null;
        _hubConnection.On<ShoppingListItemsAdded>("ShoppingListItemsAdded", msg =>
        {
            receivedMessage = msg;
        });

        // Act
        await _testHarness.Bus.Publish(message);
        await Task.Delay(1000);

        // Assert
        bool consumed = await _testHarness.Consumed.Any<ShoppingListItemsAdded>();
        Assert.True(consumed);

        Assert.NotNull(receivedMessage);
        Assert.Equal(message.Family, receivedMessage.Family);
        Assert.Equal(message.OwnerId, receivedMessage.OwnerId);
    }

    [Fact]
    public async Task ShoppingListItemUpdatedConsumer_ShouldNotifyClients_WhenMessageIsConsumed()
    {
        // Arrange
        var message = new ShoppingListItemUpdated
        {
            Id = Guid.NewGuid(),
            Heading = "Test Shopping List",
            CreatedAt = DateTime.Now,
            Items = new List<ShoppingListItemDto>(),
            SalesTax = 0,
            IsArchived = false,
            IsDeleted = false,
            OwnerId = "test-user-id",
            Family = "test-family"
        };

        ShoppingListItemUpdated? receivedMessage = null;
        _hubConnection.On<ShoppingListItemUpdated>("ShoppingListItemUpdated", msg =>
        {
            receivedMessage = msg;
        });

        // Act
        await _testHarness.Bus.Publish(message);
        await Task.Delay(1000);
        bool consumed = await _testHarness.Consumed.Any<ShoppingListItemUpdated>();

        // Assert
        Assert.True(consumed);
        Assert.NotNull(receivedMessage);
        Assert.Equal(message.Family, receivedMessage.Family);
        Assert.Equal(message.OwnerId, receivedMessage.OwnerId);
    }

    [Fact]
    public async Task ShoppingListItemDeletedConsumer_ShouldNotifyClients_WhenMessageIsConsumed()
    {
        // Arrange
        var message = new ShoppingListItemDeleted
        {
            ShoppingListId = Guid.NewGuid(),
            ItemId = Guid.NewGuid(),
            OwnerId = "test-user-id",
            Family = "test-family"
        };

        ShoppingListItemDeleted? receivedMessage = null;
        _hubConnection.On<ShoppingListItemDeleted>("ShoppingListItemDeleted", msg =>
        {
            receivedMessage = msg;
        });

        // Act
        await _testHarness.Bus.Publish(message);
        await Task.Delay(1000);
        bool consumed = await _testHarness.Consumed.Any<ShoppingListItemDeleted>();

        // Assert
        Assert.True(consumed);
        Assert.NotNull(receivedMessage);
        Assert.Equal(message.Family, receivedMessage.Family);
        Assert.Equal(message.OwnerId, receivedMessage.OwnerId);
    }


}

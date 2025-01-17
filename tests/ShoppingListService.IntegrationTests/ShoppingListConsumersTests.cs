using System;
using Contracts.Catalog;
using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShoppingListService.Data;
using ShoppingListService.Entities;
using ShoppingListService.IntegrationTests.Fixtures;
using ShoppingListService.IntegrationTests.Utils;
using Xunit.Sdk;

namespace ShoppingListService.IntegrationTests;

[Collection("Shared collection")]
public class ShoppingListConsumersTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;

    private ITestHarness _testHarness;
    private readonly ShoppingListContext _context;

    public ShoppingListConsumersTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
        _testHarness = _factory.Services.GetTestHarness();
        _context = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<ShoppingListContext>();

    }
    public async Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ShoppingListContext>();
        DbHelper.ReinitDbForTests(db);
        await _testHarness.Stop();
    }

    public async Task InitializeAsync()
    {
        await _testHarness.Start();
    }

    [Fact]
    public async Task CatalogItemCreatedConsumer_ShouldConsumeMessage_AndAddItemToDatabase()
    {
        // Arrange
        var catalogItemCreated = new CatalogItemCreated
        {
            SKU = Guid.NewGuid(),
            Name = "Test Item",
            CategoryName = "Test Category",
            OwnerId = "test-user-id",
            Family = "test-family",
            CategorySKU = Guid.NewGuid(),
            IsDeleted = false
        };

        // Act
        await _testHarness.Bus.Publish(catalogItemCreated);

        // Assert
        Assert.True(await _testHarness.Consumed.Any<CatalogItemCreated>());
        Assert.True(await _testHarness.Published.Any<CatalogItemCreated>());

        // Verify the database
        var catalogItem = await _context.CatalogItems.FirstOrDefaultAsync(x => x.SKU == catalogItemCreated.SKU);
        Assert.NotNull(catalogItem);
        Assert.Equal(catalogItemCreated.Name, catalogItem.Name);
    }

    [Fact]
    public async Task CatalogItemCreatedConsumer_ShouldLogWarning_WhenNoCatalogItemsFound()
    {
        // Arrange
        var categorySku = Guid.NewGuid();
        var updatedCategoryName = "Updated Category";

        var catalogCategoryUpdated = new CatalogCategoryUpdated
        {
            Sku = categorySku,
            Name = updatedCategoryName,
            Family = "test-family",
            OwnerId = "test-user-id"
        };

        // Act
        await _testHarness.Bus.Publish(catalogCategoryUpdated);

        // Assert
        bool consumed = await _testHarness.Consumed.Any<CatalogCategoryUpdated>();
        Assert.True(consumed, "Message was not consumed");

        // Check if the database is empty
        var catalogItems = await _context.CatalogItems.Where(x => x.CategorySKU == categorySku).ToListAsync();
        Assert.Empty(catalogItems);
    }

    [Fact]
    public async Task CatalogCategoryUpdatedConsumer_ShouldUpdateCatalogAndShoppingListItems_WhenValidMessage()
    {
        // Arrange
        var categorySku = Guid.NewGuid();
        Guid shoppingListID = Guid.NewGuid();
        var updatedCategoryName = "Updated Category";

        CatalogItem catalogItem = new CatalogItem
        {
            Id = Guid.NewGuid(),
            Name = "Item 1",
            CategorySKU = categorySku,
            CategoryName = "Old Category",
            Family = "test-family",
            OwnerId = "test-user-id"
        };

        ShoppingList shoppingList = new()
        {
            Id = shoppingListID,
            Family = "test-family",
            OwnerId = "test-user-id",
        };

        ShoppingListItem shoppingListItem = new()
        {
            Id = Guid.NewGuid(),
            Name = catalogItem.Name,
            ShoppingListId = catalogItem.Id,
            CatalogItemId = catalogItem.Id,
            CategoryName = catalogItem.Name,
            CategorySKU = catalogItem.CategorySKU,
            CatalogItem = catalogItem,
            ShoppingList = shoppingList,
            Family = "test-family",
            OwnerId = "test-user-id",
        };
        // Seed database with initial data
        _context.CatalogItems.Add(catalogItem);

        _context.ShoppingLists.Add(shoppingList);

        _context.ShoppingListItems.Add(shoppingListItem);

        int result = await _context.SaveChangesAsync();

        var catalogCategoryUpdated = new CatalogCategoryUpdated
        {
            Sku = categorySku,
            Name = updatedCategoryName,
            Family = "test-family",
            OwnerId = "test-user-id"
        };

        // Act
        await _testHarness.Bus.Publish(catalogCategoryUpdated);

        // Assert
        var consumed = await _testHarness.Consumed.Any<CatalogCategoryUpdated>();
        Assert.True(consumed, "Message was consumed.");

        // Added so that the context within the consumer finishes its operations
        await Task.Delay(500);

        // use fresh context to check the data

        using var scope = _factory.Services.CreateScope();
        var freshContext = scope.ServiceProvider.GetRequiredService<ShoppingListContext>();

        // Check updated in the database
        var updatedCatalogItem = await freshContext.CatalogItems.FirstOrDefaultAsync(x => x.CategorySKU == categorySku);

        Assert.NotNull(updatedCatalogItem);
        Assert.Equal(updatedCategoryName, updatedCatalogItem.CategoryName);

        var updatedShoppingItem = await freshContext.ShoppingListItems.FirstOrDefaultAsync(x => x.CategorySKU == categorySku);
        Assert.NotNull(updatedShoppingItem);
        Assert.Equal(updatedCategoryName, updatedShoppingItem.CategoryName);
    }

    [Fact]
    public async Task CatalogCategoryUpdatedConsumer_ShouldNotUpdate_WhenValidationFails()
    {
        // Arrange
        var invalidCategoryUpdated = new CatalogCategoryUpdated
        {
            Sku = Guid.Empty, // Invalid SKU
            Name = "",
            Family = "test-family",
            OwnerId = "test-user-id"
        };

        // Act
        await _testHarness.Bus.Publish(invalidCategoryUpdated);

        // Assert
        Assert.True(await _testHarness.Consumed.Any<CatalogCategoryUpdated>());

    }

    [Fact]
    public async Task CatalogItemUpdatedConsumer_ShouldUpdateCatalogAndShoppingListItems_WhenValidMessage()
    {
        // Arrange
        var itemSku = Guid.NewGuid();
        var updatedItemName = "Updated Item Name";
        var updatedCategorySku = Guid.NewGuid();
        var updatedCategoryName = "New Category";

        // Arrange

        CatalogItem catalogItem = new CatalogItem
        {
            SKU = itemSku,
            Name = "Old Item Name",
            CategorySKU = Guid.NewGuid(),
            CategoryName = "Old Category",
            Family = "test-family",
            OwnerId = "test-user-id"
        };

        ShoppingList shoppingList = new()
        {
            Id = Guid.NewGuid(),
            Family = "test-family",
            OwnerId = "test-user-id",
        };

        ShoppingListItem shoppingListItem = new()
        {
            Name = catalogItem.Name,
            SKU = catalogItem.SKU,
            ShoppingListId = catalogItem.Id,
            CatalogItemId = catalogItem.Id,
            CategoryName = catalogItem.Name,
            CategorySKU = catalogItem.CategorySKU,
            CatalogItem = catalogItem,
            ShoppingList = shoppingList,
            Family = "test-family",
            OwnerId = "test-user-id",
        };
        // Seed database with initial data
        _context.CatalogItems.Add(catalogItem);

        _context.ShoppingLists.Add(shoppingList);

        _context.ShoppingListItems.Add(shoppingListItem);

        int result = await _context.SaveChangesAsync();

        var catalogItemUpdatedMessage = new CatalogItemUpdated
        {
            UpdatedItem = new UpdatedItem
            {
                SKU = itemSku,
                Name = updatedItemName,
                CategoryId = Guid.NewGuid(),
                CategoryName = updatedCategoryName,
                CategorySKU = updatedCategorySku,
                Family = "test-family",
                OwnerId = "test-user-id",
                IsDeleted = false,
            },
            PreviousCategorySKU = catalogItem.CategorySKU,
        };

        // Act
        await _testHarness.Bus.Publish(catalogItemUpdatedMessage);



        bool consumed = await _testHarness.Consumed.Any<CatalogItemUpdated>();

        Assert.True(consumed);
        await Task.Delay(500);
        // use fresh context to check the data

        using var scope = _factory.Services.CreateScope();
        var freshContext = scope.ServiceProvider.GetRequiredService<ShoppingListContext>();

        // Assert updated catalog item
        var updatedCatalogItem = await freshContext.CatalogItems.FirstOrDefaultAsync(x => x.SKU == itemSku);
        Assert.NotNull(updatedCatalogItem);
        Assert.Equal(updatedItemName, updatedCatalogItem.Name);
        Assert.Equal(updatedCategoryName, updatedCatalogItem.CategoryName);
        Assert.Equal(updatedCategorySku, updatedCatalogItem.CategorySKU);

        // Assert updated shopping list item
        var updatedShoppingListItem = await freshContext.ShoppingListItems.FirstOrDefaultAsync(x => x.SKU == itemSku);
        Assert.NotNull(updatedShoppingListItem);
        Assert.Equal(updatedItemName, updatedShoppingListItem.Name);
        Assert.Equal(updatedCategoryName, updatedShoppingListItem.CategoryName);
        Assert.Equal(updatedCategorySku, updatedShoppingListItem.CategorySKU);
    }

    [Fact]
    public async Task CatalogItemUpdatedConsumer_ShouldLogWarning_WhenCatalogItemNotFound()
    {
        // Arrange
        var itemSku = Guid.NewGuid();
        var catalogItemUpdatedMessage = new CatalogItemUpdated
        {
            UpdatedItem = new UpdatedItem
            {
                SKU = itemSku,
                Name = "Updated Item",
                CategoryName = "Updated Category",
                CategorySKU = Guid.NewGuid(),
                Family = "test-family",
                OwnerId = "test-user-id",
                IsDeleted = false,
            },
            PreviousCategorySKU = Guid.NewGuid()
        };

        // Act
        await _testHarness.Bus.Publish(catalogItemUpdatedMessage);

        bool consumed = await _testHarness.Consumed.Any<CatalogItemUpdated>();

        // Assert
        Assert.True(consumed);

        await Task.Delay(500);

        // use fresh context to check the data

        using var scope = _factory.Services.CreateScope();
        var freshContext = scope.ServiceProvider.GetRequiredService<ShoppingListContext>();
        // Verify no changes were made to the database
        var catalogItem = await _context.CatalogItems.FirstOrDefaultAsync(x => x.SKU == itemSku);
        Assert.Null(catalogItem);

    }

    [Fact]
    public async Task CatalogItemUpdatedConsumer_ShouldNotProcess_WhenValidationFails()
    {
        // Arrange
        var invalidMessage = new CatalogItemUpdated
        {
            UpdatedItem = new UpdatedItem
            {
                SKU = Guid.Empty, // Invalid SKU
                Name = "",
                Family = "test-family",
                IsDeleted = false,
            }
        };

        // Act
        await _testHarness.Bus.Publish(invalidMessage);

        // Assert
        Assert.True(await _testHarness.Consumed.Any<CatalogItemUpdated>());
    }

    [Fact]
    public async Task CatalogItemDeletedConsumer_ShouldMarkCatalogItemAndShoppingListItems_WhenValidMessage()
    {
        // Arrange
        var itemSku = Guid.NewGuid();

        // Seed database with initial data
        var catalogItem = new CatalogItem
        {
            Id = Guid.NewGuid(),
            SKU = itemSku,
            CategoryName = "Old Category",
            CategorySKU = Guid.NewGuid(),
            Name = "Item 1",
            Family = "test-family",
            OwnerId = "test-user-id",
            IsDeleted = false,
        };
        ShoppingList shoppingList = new()
        {
            Id = Guid.NewGuid(),
            Family = "test-family",
            OwnerId = "test-user-id",
        };

        var shoppingListItem = new ShoppingListItem
        {
            Id = Guid.NewGuid(),
            SKU = itemSku,
            Name = "Item 1",
            Family = "test-family",
            OwnerId = "test-user-id",
            IsOrphaned = false,
            ShoppingListId = shoppingList.Id,
            ShoppingList = shoppingList,
            CatalogItem = catalogItem,
            CatalogItemId = catalogItem.Id,
            CategoryName = catalogItem.Name,
            CategorySKU = catalogItem.CategorySKU,
        };

        _context.CatalogItems.Add(catalogItem);
        _context.ShoppingLists.Add(shoppingList);
        _context.ShoppingListItems.Add(shoppingListItem);

        int result = await _context.SaveChangesAsync();

        var catalogItemDeletedMessage = new CatalogItemDeleted
        {
            SKU = itemSku,
            Family = "test-family",
            OwnerId = "test-user-id",
        };

        // Act
        await _testHarness.Bus.Publish(catalogItemDeletedMessage);

        // Wait for the message to be consumed
        Assert.True(await _testHarness.Consumed.Any<CatalogItemDeleted>());

        await Task.Delay(500);

        // use fresh context to check the data

        using var scope = _factory.Services.CreateScope();
        var freshContext = scope.ServiceProvider.GetRequiredService<ShoppingListContext>();

        // Assert catalog item is marked as deleted
        var updatedCatalogItem = await freshContext.CatalogItems.FirstOrDefaultAsync(x => x.SKU == itemSku);
        Assert.NotNull(updatedCatalogItem);
        Assert.True(updatedCatalogItem.IsDeleted);

        // Assert shopping list items are marked as orphaned
        var updatedShoppingListItem = await freshContext.ShoppingListItems.FirstOrDefaultAsync(x => x.SKU == itemSku);
        Assert.NotNull(updatedShoppingListItem);
        Assert.True(updatedShoppingListItem.IsOrphaned);
    }

    [Fact]
    public async Task CatalogItemDeletedConsumer_ShouldLogError_WhenCatalogItemNotFound()
    {
        // Arrange
        var itemSku = Guid.NewGuid();

        var catalogItemDeletedMessage = new CatalogItemDeleted
        {
            SKU = itemSku,
            Family = "test-family",
            OwnerId = "test-user-id",
        };

        // Act
        await _testHarness.Bus.Publish(catalogItemDeletedMessage);

        // Assert
        Assert.True(await _testHarness.Consumed.Any<CatalogItemDeleted>());

        // Verify no changes were made to the database
        var catalogItem = await _context.CatalogItems.FirstOrDefaultAsync(x => x.SKU == itemSku);
        Assert.Null(catalogItem);
    }
    [Fact]
    public async Task CatalogItemDeletedConsumer_ShouldProcessMessageNoDatabaseChange_WhenValidationFails()
    {
        // Arrange
        var invalidMessage = new CatalogItemDeleted
        {
            SKU = Guid.Empty, // Invalid SKU
            Family = "test-family",
            OwnerId = "test-user-id"
        };

        // Act
        await _testHarness.Bus.Publish(invalidMessage);

        // Assert
        Assert.True(await _testHarness.Consumed.Any<CatalogItemDeleted>());

        var deletedItem = await _context.CatalogItems.FirstOrDefaultAsync(x => x.SKU == invalidMessage.SKU);
        Assert.Null(deletedItem);
    }

}

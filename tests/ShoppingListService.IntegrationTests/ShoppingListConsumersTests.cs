using System;
using Contracts.Catalog;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShoppingListService.Data;
using ShoppingListService.IntegrationTests.Fixtures;
using ShoppingListService.IntegrationTests.Utils;

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
            Family = "test-family"
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
}

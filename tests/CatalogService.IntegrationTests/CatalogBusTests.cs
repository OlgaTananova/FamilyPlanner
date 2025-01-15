using System;
using System.Net;
using System.Net.Http.Json;
using CatalogService.Data;
using CatalogService.DTOs;
using CatalogService.Entities;
using CatalogService.IntegrationTests.Fixtures;
using CatalogService.IntegrationTests.Utils;
using Contracts.Catalog;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogService.IntegrationTests;

[Collection("Shared collection")]
public class CatalogBusTests : IAsyncLifetime
{
    private readonly CustomWebAppFactory _factory;
    private readonly HttpClient _httpClient;

    private ITestHarness _testHarness;

    public CatalogBusTests(CustomWebAppFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
        _testHarness = _factory.Services.GetTestHarness();
    }
    public Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        DbHelper.ReinitDbForTests(db);
        return Task.CompletedTask;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateItem_ShouldPublishCatalogItemCreated_WhenValidObject()
    {
        // Arrange

        CreateItemDto newItem = new()
        {
            Name = "NewItem",
            CategorySKU = Guid.Parse("1625f681-af55-4f80-a88f-c65b666d701d"),
        };

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));
        var response = await _httpClient.PostAsJsonAsync("/api/Catalog/items", newItem);

        //Assert

        response.EnsureSuccessStatusCode();
        Assert.True(await _testHarness.Published.Any<CatalogItemCreated>());

    }

}

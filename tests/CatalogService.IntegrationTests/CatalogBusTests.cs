using System;
using System.Net;
using System.Net.Http.Json;
using CatalogService.Data;
using CatalogService.DTOs;
using CatalogService.Entities;
using CatalogService.IntegrationTests.Fixtures;
using CatalogService.IntegrationTests.Utils;
using Contracts.Catalog;
using ICSharpCode.SharpZipLib.Core;
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
    public async Task CreateCategory_ShouldPublishCatalogCategoryCreated_WhenValidCategory()
    {
        // Arrange
        var newCategory = new CreateCategoryDto
        {
            Name = "NewCategory"
        };

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/Catalog/categories", newCategory);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(await _testHarness.Published.Any<CatalogCategoryCreated>());
    }

    [Fact]
    public async Task UpdateCategory_ShouldPublishCatalogCategoryUpdated_WhenCategoryIsUpdated()
    {
        // Arrange
        Guid categorySku = Guid.Parse("1625f681-af55-4f80-a88f-c65b666d701d");
        var updatedCategory = new UpdateCategoryDto
        {
            Name = "UpdatedCategory"
        };

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));

        // Act
        var response = await _httpClient.PutAsJsonAsync($"/api/Catalog/categories/{categorySku}", updatedCategory);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(await _testHarness.Published.Any<CatalogCategoryUpdated>());
    }

    [Fact]
    public async Task DeleteCategory_ShouldPublishCatalogCategoryDeleted_WhenCategoryIsDeleted()
    {
        // Arrange
        Category newCategory = new()
        {
            Name = "NewCategory2",
            Id = new Guid(),
            SKU = new Guid(),
            OwnerId = "test-user-id",
            Family = "test-family"
        };
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            db.Categories.Add(newCategory);
            db.SaveChanges();
        }

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));

        // Act
        var response = await _httpClient.DeleteAsync($"/api/Catalog/categories/{newCategory.SKU}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(await _testHarness.Published.Any<CatalogCategoryDeleted>());
    }


    [Fact]
    public async Task CreateItem_ShouldPublishCatalogItemCreated_WhenValidObject()
    {
        // Arrange

        CreateItemDto newItem = new()
        {
            Name = "NewItem",
            CategorySKU = Guid.Parse("ac23ec89-92f5-4299-9872-7dacc7c966bd"),
        };

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));
        var response = await _httpClient.PostAsJsonAsync("/api/Catalog/items", newItem);

        //Assert

        response.EnsureSuccessStatusCode();
        Assert.True(await _testHarness.Published.Any<CatalogItemCreated>());

    }

    [Fact]
    public async Task UpdateItem_ShouldPublishCatalogItemUpdated_WhenItemIsUpdated()
    {
        // Arrange
        Guid itemSku = Guid.Parse("f4f69463-467c-4875-aead-2ec4cf6d7ead");
        Guid categorySku = Guid.Parse("1625f681-af55-4f80-a88f-c65b666d701d");

        var updateItemDto = new UpdateItemDto
        {
            Name = "UpdatedItem",
            CategorySKU = categorySku
        };

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));

        // Act
        var response = await _httpClient.PutAsJsonAsync($"/api/Catalog/items/{itemSku}", updateItemDto);

        // Assert
        response.EnsureSuccessStatusCode();


        Assert.True(await _testHarness.Published.Any<CatalogItemUpdated>());
        var publishedMessage = _testHarness.Published.Select<CatalogItemUpdated>().FirstOrDefault();
        Assert.NotNull(publishedMessage);
    }

    [Fact]
    public async Task DeleteItem_ShouldPublishCatalogItemDeleted_WhenItemIsDeleted()
    {
        // Arrange
        Guid itemSku = Guid.Parse("f4f69463-467c-4875-aead-2ec4cf6d7ead");

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));

        // Act
        var response = await _httpClient.DeleteAsync($"/api/Catalog/items/{itemSku}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(await _testHarness.Published.Any<CatalogItemDeleted>());
    }

}

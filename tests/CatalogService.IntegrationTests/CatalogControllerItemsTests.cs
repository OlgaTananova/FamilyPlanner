using System;
using System.Net;
using System.Net.Http.Json;
using CatalogService.Data;
using CatalogService.DTOs;
using CatalogService.Entities;
using CatalogService.IntegrationTests.Fixtures;
using CatalogService.IntegrationTests.Utils;
using Contracts.Catalog;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogService.IntegrationTests;

[Collection("Shared collection")]
public class CatalogControllerItemsTests : IAsyncLifetime
{
    private readonly CustomWebAppFactory _factory;
    private readonly HttpClient _httpClient;
    public CatalogControllerItemsTests(CustomWebAppFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
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
    public async Task GetItemBySku_ShouldReturnItem_WhenExists()
    {
        // Arrange
        var existingItemSku = Guid.Parse("f4f69463-467c-4875-aead-2ec4cf6d7ead");
        string itemName = "Apples";
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));

        // Act
        var response = await _httpClient.GetFromJsonAsync<ItemDto>($"api/Catalog/items/{existingItemSku}");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(itemName, response?.Name);
    }

    [Fact]
    public async Task GetItemBySku_ShouldReturnNotFound_WhenNotExists()
    {
        // Arrange
        var nonExistingSku = Guid.NewGuid();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));

        // Act
        var response = await _httpClient.GetAsync($"api/Catalog/items/{nonExistingSku}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateItem_ShouldReturnCreatedItem_WhenSuccessful()
    {
        // Arrange
        Guid categorySku = Guid.Parse("1625f681-af55-4f80-a88f-c65b666d701d");
        //Guid categoryId = Guid.Parse("02d928a0-d6cb-4686-a5dd-7c180db3868a");
        //string categoryName = "Veggies";


        var newItem = new CreateItemDto
        {
            Name = "New Item",
            CategorySKU = categorySku,
        };

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));

        // Act
        var response = await _httpClient.PostAsJsonAsync("api/Catalog/items", newItem);

        // Assert
        response.EnsureSuccessStatusCode();
        var createdItem = await response.Content.ReadFromJsonAsync<ItemDto>();
        Assert.NotNull(createdItem);
        Assert.Equal(newItem.Name, createdItem?.Name);
    }

    [Fact]
    public async Task CreateItem_ShouldReturnBadRequest_WhenItemNameExists()
    {
        // Arrange
        var existingItem = new Item
        {
            Name = "Existing Item",
            OwnerId = "test-user-id",
            Family = "test-family",
            CategorySKU = Guid.Parse("1625f681-af55-4f80-a88f-c65b666d701d"),
            CategoryId = Guid.Parse("02d928a0-d6cb-4686-a5dd-7c180db3868a"),
            CategoryName = "Veggies",
        };

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            db.Items.Add(existingItem);
            db.SaveChanges();
        }
        var newItem = new CreateItemDto { Name = "Existing Item", CategorySKU = Guid.NewGuid() };

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));

        // Act
        var response = await _httpClient.PostAsJsonAsync("api/Catalog/items", newItem);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("The item with this name already exists.", errorMessage);
    }

    [Fact]
    public async Task CreateItem_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        var newItem = new CreateItemDto
        {
            Name = "New Item",
            CategorySKU = Guid.NewGuid()
        };

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));

        // Act
        var response = await _httpClient.PostAsJsonAsync("api/Catalog/items", newItem);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("Cannot find the category of the newly created item.", errorMessage);
    }

    [Fact]
    public async Task UpdateItem_ShouldReturnUpdatedItem_WhenSuccessful()
    {
        // Arrange

        Guid categorySku = Guid.Parse("1625f681-af55-4f80-a88f-c65b666d701d");
        Guid itemSku = Guid.Parse("f4f69463-467c-4875-aead-2ec4cf6d7ead");


        var updatedItem = new UpdateItemDto { Name = "Updated Item", CategorySKU = categorySku };

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));

        // Act
        var response = await _httpClient.PutAsJsonAsync($"api/Catalog/items/{itemSku}", updatedItem);


        // Assert
        response.EnsureSuccessStatusCode();
        var updatedItemResponse = await response.Content.ReadFromJsonAsync<CatalogItemUpdated>();
        Console.WriteLine($"Response Content: {updatedItemResponse}"); // Log response content
        Assert.NotNull(updatedItemResponse);
        Assert.Equal(updatedItem.Name, updatedItemResponse.UpdatedItem?.Name);
    }

    [Fact]
    public async Task UpdateItem_ShouldReturnNotFound_WhenItemOrCategoryDoesNotExist()
    {
        // Arrange
        var updateItemDto = new UpdateItemDto { Name = "Non-Existing Item", CategorySKU = Guid.NewGuid() };

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));

        // Act
        var response = await _httpClient.PutAsJsonAsync($"api/Catalog/items/{Guid.NewGuid()}", updateItemDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteItem_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var item = new Item
        {
            Id = Guid.NewGuid(),
            SKU = Guid.NewGuid(),
            Name = "Item to Delete",
            OwnerId = "test-user-id",
            Family = "test-family",
            CategorySKU = Guid.Parse("1625f681-af55-4f80-a88f-c65b666d701d"),
            CategoryId = Guid.Parse("02d928a0-d6cb-4686-a5dd-7c180db3868a"),
            CategoryName = "Veggies",
        };

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            db.Items.Add(item);
            db.SaveChanges();
        }

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));

        // Act
        var response = await _httpClient.DeleteAsync($"api/Catalog/items/{item.SKU}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }


    [Fact]
    public async Task DeleteItem_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        var nonExistingSku = Guid.NewGuid();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));

        // Act
        var response = await _httpClient.DeleteAsync($"api/Catalog/items/{nonExistingSku}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SearchItems_ShouldReturnMatchingItems()
    {

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));

        // Act
        var response = await _httpClient.GetFromJsonAsync<List<ItemDto>>($"api/Catalog/items/search?query=Banan");

        // Assert
        Assert.NotNull(response);
        Assert.Single(response);
        Assert.Equal("Bananas", response[0].Name);

    }

    [Fact]
    public async Task SearchItems_ShouldReturnBadRequest_WhenQueryIsEmpty()
    {
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));

        // Act
        var response = await _httpClient.GetAsync("api/Catalog/items/search?query=");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("Query parameter cannot be empty.", errorMessage);
    }
}

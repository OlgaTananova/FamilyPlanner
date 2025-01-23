using System;
using System.Net;
using System.Net.Http.Json;
using Contracts.ShoppingLists;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using ShoppingListService.Data;
using ShoppingListService.DTOs;
using ShoppingListService.Entities;
using ShoppingListService.IntegrationTests.Fixtures;
using ShoppingListService.IntegrationTests.Utils;

namespace ShoppingListService.IntegrationTests;

[Collection("Shared collection")]
public class ShoppingListControllerTests : IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _httpClient;

    private ITestHarness _testHarness;
    private readonly ShoppingListContext _context;

    private readonly string _ownerId = "test-user-id";
    private readonly string _family = "test-family";

    public ShoppingListControllerTests(CustomWebApplicationFactory factory)
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

    #region GetDataTests
    [Fact]
    public async Task GetCatalogItems_ShouldReturnCatalogItems_WhenItemsExist()
    {
        // Arrange

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(_ownerId, _family));

        // Act
        var response = await _httpClient.GetAsync("api/ShoppingLists/catalogitems");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<CatalogItemDto>>();
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetCatalogItems_ShouldReturn401Unauthorized_WhenNoValidToken()
    {
        // Act
        var response = await _httpClient.GetAsync("api/ShoppingLists/catalogitems");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetFrequentlyBoughtItems_ShouldReturnItems_WhenFrequentItemsExist()
    {
        // Arrange
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(_ownerId, _family));

        // Act
        var response = await _httpClient.GetAsync("api/ShoppingLists/catalogitems/freq-bought");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<CatalogItemDto>>();
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task SearchCatalogItems_ShouldReturnItems_WhenQueryMatches()
    {
        // Arrange

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(_ownerId, _family));

        // Act
        var response = await _httpClient.GetAsync("api/ShoppingLists/catalogitems/search?query=Cof");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<CatalogItemDto>>();
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Coffee", result.First().Name);
    }

    [Fact]
    public async Task SearchCatalogItems_ShouldReturnBadRequest_WhenQueryIsEmpty()
    {
        // Arrange
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(_ownerId, _family));

        // Act
        var response = await _httpClient.GetAsync("api/ShoppingLists/catalogitems/search?query=");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SearchCatalogItems_ShouldReturnEmptyList_WhenNoMatchingItemsFound()
    {
        // Arrange
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(_ownerId, _family));

        // Act
        var response = await _httpClient.GetAsync("api/ShoppingLists/catalogitems/search?query=NonExistent");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<CatalogItemDto>>();
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region ShoppingListTests
    [Fact]
    public async Task CreateShoppingList_ShouldReturnCreated_WhenValidRequest()
    {
        // Arrange
        var shoppingListDto = new CreateShoppingListDto
        {
            Heading = "Grocery List",
            SKUs = new List<Guid> {
                Guid.Parse("57134ac7-5446-4377-88f4-b0f9b71bf57f"),
                Guid.Parse("6b0eaee4-c2b8-481b-98d7-ff73b4f4f8d7")
             }
        };

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(_ownerId, _family));

        // Act
        var response = await _httpClient.PostAsJsonAsync("api/ShoppingLists", shoppingListDto);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdList = await response.Content.ReadFromJsonAsync<ShoppingListDto>();
        Assert.NotNull(createdList);
        Assert.Equal("Grocery List", createdList.Heading);

        // Verify message sent to message broker
        bool published = await _testHarness.Published.Any<ShoppingListCreated>();
        Assert.True(published);
    }

    [Fact]
    public async Task CreateShoppingList_ShouldReturnBadRequest_WhenNoSKUsFound()
    {
        // Arrange
        var shoppingListDto = new CreateShoppingListDto
        {
            Heading = "Grocery List",
            SKUs = new List<Guid> { Guid.NewGuid() } // Assuming no items exist for this SKU
        };

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(_ownerId, _family));

        // Act
        var response = await _httpClient.PostAsJsonAsync("api/ShoppingLists", shoppingListDto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("No catalog items found for the provided SKUs", errorMessage);
    }

    [Fact]
    public async Task GetShoppingLists_ShouldReturnEmptyList_WhenNoListsExist()
    {
        // Arrange
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(_ownerId, _family));

        // Act
        var response = await _httpClient.GetAsync("api/ShoppingLists");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<ShoppingListDto>>();
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetShoppingLists_ShouldReturnShoppingLists_WhenListsExist()
    {
        // Arrange
        var shoppingLists = new List<ShoppingList>
    {
        new ShoppingList {
            Id = Guid.NewGuid(),
            Heading = "List1",
            Family = _family,
            OwnerId = _ownerId
            },
        new ShoppingList {
            Id = Guid.NewGuid(),
            Heading = "List2",
            Family = _family,
            OwnerId = _ownerId
             }
    };

        _context.ShoppingLists.AddRange(shoppingLists);
        await _context.SaveChangesAsync();

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(_ownerId, _family));

        // Act
        var response = await _httpClient.GetAsync("api/ShoppingLists");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<List<ShoppingListDto>>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetShoppingList_ShouldReturnShoppingList_WhenListExists()
    {
        // Arrange
        var shoppingList = new ShoppingList
        {
            Id = Guid.NewGuid(),
            Heading = "List1",
            Family = _family,
            OwnerId = _ownerId
        };

        _context.ShoppingLists.Add(shoppingList);
        await _context.SaveChangesAsync();

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(_ownerId, _family));

        // Act
        var response = await _httpClient.GetAsync($"api/ShoppingLists/{shoppingList.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ShoppingListDto>();
        Assert.NotNull(result);
        Assert.Equal("List1", result.Heading);
    }

    [Fact]
    public async Task GetShoppingList_ShouldReturnNotFound_WhenListDoesNotExist()
    {
        // Arrange
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(_ownerId, _family));

        // Act
        var response = await _httpClient.GetAsync($"api/ShoppingLists/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateShoppingList_ShouldReturnOk_WhenValidRequest()
    {
        // Arrange
        var shoppingList = new ShoppingList
        {
            Id = Guid.NewGuid(),
            Heading = "Initial Heading",
            Family = _family,
            OwnerId = _ownerId
        };

        _context.ShoppingLists.Add(shoppingList);
        await _context.SaveChangesAsync();

        var updateDto = new UpdateShoppingListDto
        {
            Heading = "Updated Heading"
        };

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(_ownerId, _family));

        // Act
        var response = await _httpClient.PutAsJsonAsync($"api/ShoppingLists/{shoppingList.Id}", updateDto);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updatedList = await response.Content.ReadFromJsonAsync<ShoppingListDto>();
        Assert.NotNull(updatedList);
        Assert.Equal("Updated Heading", updatedList.Heading);

        // Verify message sent to message broker
        bool published = await _testHarness.Published.Any<ShoppingListUpdated>();
        Assert.True(published);
    }

    [Fact]
    public async Task UpdateShoppingList_ShouldReturnNotFound_WhenListDoesNotExist()
    {
        // Arrange
        var updateDto = new UpdateShoppingListDto
        {
            Heading = "Updated Heading"
        };

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(_ownerId, _family));

        // Act
        var response = await _httpClient.PutAsJsonAsync($"api/ShoppingLists/{Guid.NewGuid()}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("Cannot find the shopping list", errorMessage);
    }

    [Fact]
    public async Task DeleteShoppingList_ShouldReturnNoContent_WhenShoppingListExists()
    {
        // Arrange
        var shoppingList = new ShoppingList
        {
            Id = Guid.NewGuid(),
            Heading = "Test Shopping List",
            Family = _family,
            OwnerId = _ownerId,
        };
        _context.ShoppingLists.Add(shoppingList);
        await _context.SaveChangesAsync();

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(_ownerId, _family));

        // Act
        var response = await _httpClient.DeleteAsync($"api/ShoppingLists/{shoppingList.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using var freshContext = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<ShoppingListContext>();
        var deletedShoppingList = await freshContext.ShoppingLists.FindAsync(shoppingList.Id);
        Assert.Equal(true, deletedShoppingList?.IsDeleted);

        // Verify message sent to message broker
        bool published = await _testHarness.Published.Any<ShoppingListDeleted>();
        Assert.True(published);
    }

    [Fact]
    public async Task DeleteShoppingList_ShouldReturnNotFound_WhenShoppingListDoesNotExist()
    {
        // Arrange
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(_ownerId, _family));

        // Act
        var response = await _httpClient.DeleteAsync($"api/ShoppingLists/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("Cannot find the shopping list", errorMessage);
    }

    #endregion

    #region ShoppingListItemTests

    [Fact]
    public async Task CreateShoppingListItem_ShouldReturnOk_WhenItemsAreAddedSuccessfully()
    {
        // Arrange
        var shoppingList = new ShoppingList
        {
            Id = Guid.NewGuid(),
            Heading = "Test Shopping List",
            Family = _family,
            OwnerId = _ownerId
        };

        _context.ShoppingLists.Add(shoppingList);
        await _context.SaveChangesAsync();

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(_ownerId, _family));

        var request = new CreateShoppingListItemDto
        {
            SKUs = new List<Guid> { Guid.Parse("57134ac7-5446-4377-88f4-b0f9b71bf57f") }
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync($"api/ShoppingLists/{shoppingList.Id}/items", request);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadFromJsonAsync<ShoppingListDto>();
        Assert.NotNull(responseContent);
        Assert.Single(responseContent.Items);

        // Verify message sent to message broker
        bool published = await _testHarness.Published.Any<ShoppingListItemsAdded>();
        Assert.True(published);
    }

    [Fact]
    public async Task CreateShoppingListItem_ShouldReturnNotFound_WhenShoppingListDoesNotExist()
    {
        // Arrange
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(_ownerId, _family));

        var request = new CreateShoppingListItemDto
        {
            SKUs = new List<Guid> { Guid.NewGuid() }
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync($"api/ShoppingLists/{Guid.NewGuid()}/items", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("Cannot find the shopping list", errorMessage);
    }

    [Fact]
    public async Task CreateShoppingListItem_ShouldReturnNotFound_WhenCatalogItemDoesNotExist()
    {
        // Arrange
        var shoppingList = new ShoppingList
        {
            Id = Guid.NewGuid(),
            Heading = "Test Shopping List",
            Family = _family,
            OwnerId = _ownerId
        };

        _context.ShoppingLists.Add(shoppingList);
        await _context.SaveChangesAsync();

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(_ownerId, _family));

        var request = new CreateShoppingListItemDto
        {
            SKUs = new List<Guid> { Guid.NewGuid() } // Non-existing SKU
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync($"api/ShoppingLists/{shoppingList.Id}/items", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("Cannot find the catalog item", errorMessage);
    }

    [Fact]
    public async Task UpdateShoppingListItem_ShouldReturnOk_WhenItemUpdatedSuccessfully()
    {
        // Arrange
        var shoppingList = new ShoppingList
        {
            Id = Guid.NewGuid(),
            Heading = "Test Shopping List",
            Family = _family,
            OwnerId = _ownerId
        };

        var newCatalogItem = new CatalogItem
        {
            Id = Guid.NewGuid(),
            SKU = Guid.NewGuid(),
            Name = "Banana",
            CategorySKU = Guid.NewGuid(),
            CategoryName = "New Category",
            Family = _family,
            OwnerId = _ownerId
        };

        var item = new ShoppingListItem
        {
            Id = Guid.NewGuid(),
            ShoppingListId = shoppingList.Id,
            Name = newCatalogItem.Name,
            CatalogItemId = newCatalogItem.Id,
            CategorySKU = newCatalogItem.CategorySKU,
            CategoryName = newCatalogItem.CategoryName,
            ShoppingList = shoppingList,
            CatalogItem = newCatalogItem,
            Family = _family,
            OwnerId = _ownerId,
        };

        _context.ShoppingLists.Add(shoppingList);
        _context.CatalogItems.Add(newCatalogItem);
        _context.ShoppingListItems.Add(item);
        await _context.SaveChangesAsync();



        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(_ownerId, _family));

        var updateRequest = new UpdateShoppingListItemDto
        {
            Quantity = 5
        };

        // Act
        var response = await _httpClient.PutAsJsonAsync($"api/ShoppingLists/{shoppingList.Id}/items/{item.Id}", updateRequest);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadFromJsonAsync<ShoppingListDto>();
        Assert.NotNull(responseContent);
        Assert.Contains(responseContent.Items, i => i.Quantity == 5);

        // Verify the message sent to the message broker
        bool published = await _testHarness.Published.Any<ShoppingListItemUpdated>();
        Assert.True(published);
    }

    [Fact]
    public async Task UpdateShoppingListItem_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        var shoppingList = new ShoppingList
        {
            Id = Guid.NewGuid(),
            Heading = "Test Shopping List",
            Family = _family,
            OwnerId = _ownerId
        };

        _context.ShoppingLists.Add(shoppingList);
        await _context.SaveChangesAsync();

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(_ownerId, _family));

        var updateRequest = new UpdateShoppingListItemDto
        {
            Quantity = 3
        };

        // Act
        var response = await _httpClient.PutAsJsonAsync($"api/ShoppingLists/{shoppingList.Id}/items/{Guid.NewGuid()}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("Cannot find the item", errorMessage);
    }

    [Fact]
    public async Task UpdateShoppingListItem_ShouldReturnBadRequest_WhenValidationFails()
    {
        // Arrange
        var shoppingList = new ShoppingList
        {
            Id = Guid.NewGuid(),
            Heading = "Test Shopping List",
            Family = "test-family",
            OwnerId = "test-user-id"
        };

        _context.ShoppingLists.Add(shoppingList);
        await _context.SaveChangesAsync();

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(_ownerId, _family));

        Guid updatedItemGuid = Guid.Parse("6b0eaee4-c2b8-481b-98d7-ff73b4f4f8d7");

        var invalidUpdateRequest = new UpdateShoppingListItemDto
        {
            Quantity = -5 // Invalid negative quantity
        };

        // Act
        var response = await _httpClient.PutAsJsonAsync($"api/ShoppingLists/{shoppingList.Id}/items/{updatedItemGuid}", invalidUpdateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateShoppingListItem_ShouldReturnNotFound_WhenShoppingListDoesNotExist()
    {
        // Arrange
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(_ownerId, _family));

        var updateRequest = new UpdateShoppingListItemDto
        {
            Quantity = 5
        };

        // Act
        var response = await _httpClient.PutAsJsonAsync($"api/ShoppingLists/{Guid.NewGuid()}/items/{Guid.NewGuid()}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("Cannot find the item", errorMessage);
    }


    [Fact]
    public async Task DeleteShoppingListItem_ShouldReturnNoContent_WhenItemDeletedSuccessfully()
    {
        // Arrange
        var shoppingList = new ShoppingList
        {
            Id = Guid.NewGuid(),
            Heading = "Test Shopping List",
            Family = _family,
            OwnerId = _ownerId
        };

        var newCatalogItem = new CatalogItem
        {
            Id = Guid.NewGuid(),
            SKU = Guid.NewGuid(),
            Name = "Banana",
            CategorySKU = Guid.NewGuid(),
            CategoryName = "New Category",
            Family = _family,
            OwnerId = _ownerId
        };

        var item = new ShoppingListItem
        {
            Id = Guid.NewGuid(),
            ShoppingListId = shoppingList.Id,
            Name = newCatalogItem.Name,
            CatalogItemId = newCatalogItem.Id,
            CategorySKU = newCatalogItem.CategorySKU,
            CategoryName = newCatalogItem.CategoryName,
            ShoppingList = shoppingList,
            CatalogItem = newCatalogItem,
            Family = _family,
            OwnerId = _ownerId,
        };

        _context.ShoppingLists.Add(shoppingList);
        _context.CatalogItems.Add(newCatalogItem);
        _context.ShoppingListItems.Add(item);
        await _context.SaveChangesAsync();

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(_ownerId, _family));

        // Act
        var response = await _httpClient.DeleteAsync($"api/ShoppingLists/{shoppingList.Id}/items/{item.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        using var freshContext = _factory.Services.CreateScope().ServiceProvider.GetRequiredService<ShoppingListContext>();
        var deletedItem = await freshContext.ShoppingListItems.FindAsync(item.Id);
        Assert.Null(deletedItem);

        // Verify the message sent to the message broker
        bool published = await _testHarness.Published.Any<ShoppingListItemDeleted>();
        Assert.True(published);
    }

    [Fact]
    public async Task DeleteShoppingListItem_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        var shoppingList = new ShoppingList
        {
            Id = Guid.NewGuid(),
            Heading = "Test Shopping List",
            Family = _family,
            OwnerId = _ownerId
        };

        _context.ShoppingLists.Add(shoppingList);
        await _context.SaveChangesAsync();

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(_ownerId, _family));

        // Act
        var response = await _httpClient.DeleteAsync($"api/ShoppingLists/{shoppingList.Id}/items/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("Cannot find the item", errorMessage);
    }


    [Fact]
    public async Task DeleteShoppingListItem_ShouldReturnNotFound_WhenShoppingListDoesNotExist()
    {
        // Arrange
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(_ownerId, _family));

        // Act
        var response = await _httpClient.DeleteAsync($"api/ShoppingLists/{Guid.NewGuid()}/items/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("Cannot find the item", errorMessage);
    }

    #endregion
}
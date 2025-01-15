using System;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using CatalogService.Data;
using CatalogService.DTOs;
using CatalogService.Entities;
using CatalogService.IntegrationTests.Fixtures;
using CatalogService.IntegrationTests.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using WebMotions.Fake.Authentication.JwtBearer;

namespace CatalogService.IntegrationTests;

[Collection("Shared collection")]
public class CatalogControllerCategoryTests : IAsyncLifetime
{
    private readonly CustomWebAppFactory _factory;
    private readonly HttpClient _httpClient;
    public CatalogControllerCategoryTests(CustomWebAppFactory factory)
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

    #region CategoryTests
    [Fact]
    public async Task GetCategories_ShouldReturnListOfCategoryDto()
    {
        // Arrange

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));


        //Act
        var response = await _httpClient.GetFromJsonAsync<List<CategoryDto>>("api/Catalog/categories");

        //Assert
        Assert.Equal(3, response?.Count);
    }


    [Fact]
    public async Task GetCategoryBySku_ShouldReturnNotFound_WhenNotExists()
    {
        // Arrange

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));


        //Act
        var response = await _httpClient.GetAsync($"api/Catalog/categories/{new Guid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

    }

    [Fact]
    public async Task GetCategoryBySku_ShouldReturnCategory_WhenExists()
    {
        // Arrange
        var existingCategorySku = Guid.Parse("1625f681-af55-4f80-a88f-c65b666d701d"); // Replace with a seeded category SKU
        string categoryName = "Veggies";
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));

        // Act
        var response = await _httpClient.GetFromJsonAsync<CategoryDto>($"api/Catalog/categories/{existingCategorySku}");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(categoryName, response?.Name);
    }
    [Fact]
    public async Task CreateCategory_ShouldReturnCreatedCategory_WhenSuccessful()
    {
        // Arrange
        var newCategory = new CreateCategoryDto { Name = "New Category" };

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));

        // Act
        var response = await _httpClient.PostAsJsonAsync("api/Catalog/categories", newCategory);

        // Assert
        response.EnsureSuccessStatusCode();
        var createdCategory = await response.Content.ReadFromJsonAsync<CategoryDto>();
        Assert.NotNull(createdCategory);
        Assert.Equal(newCategory.Name, createdCategory?.Name);
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnBadRequest_WhenCategoryNameExists()
    {
        // Arrange


        var newCategory = new CreateCategoryDto { Name = "Dairy" };

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));

        // Act
        var response = await _httpClient.PostAsJsonAsync("api/Catalog/categories", newCategory);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("The category with this name already exists.", errorMessage);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnUpdatedCategory_WhenSuccessful()
    {
        // Arrange

        Guid existingCategorySku = Guid.Parse("1625f681-af55-4f80-a88f-c65b666d701d");

        var updatedCategory = new UpdateCategoryDto { Name = "Updated Category" };

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));

        // Act
        var response = await _httpClient.PutAsJsonAsync($"api/Catalog/categories/{existingCategorySku}", updatedCategory);

        // Assert
        response.EnsureSuccessStatusCode();
        var updatedCategoryResponse = await response.Content.ReadFromJsonAsync<CategoryDto>();
        Assert.NotNull(updatedCategoryResponse);
        Assert.Equal(updatedCategory.Name, updatedCategoryResponse?.Name);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        var nonExistingSku = Guid.NewGuid();
        var updateCategoryDto = new UpdateCategoryDto { Name = "Non-Existing Category" };

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));

        // Act
        var response = await _httpClient.PutAsJsonAsync($"api/Catalog/categories/{nonExistingSku}", updateCategoryDto);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("The category was not found", errorMessage);
    }

    [Fact]
    public async Task DeleteCategory_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        var nonExistingSku = Guid.NewGuid();

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));

        // Act
        var response = await _httpClient.DeleteAsync($"api/Catalog/categories/{nonExistingSku}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("Cannot find the category to delete.", errorMessage);
    }

    [Fact]
    public async Task DeleteCategory_ShouldReturnBadRequest_WhenCategoryNotEmpty()
    {
        // Arrange
        var categoryWithItemsSku = Guid.Parse("1625f681-af55-4f80-a88f-c65b666d701d");


        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));

        // Act
        var response = await _httpClient.DeleteAsync($"api/Catalog/categories/{categoryWithItemsSku}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errorMessage = await response.Content.ReadAsStringAsync();
        Assert.Contains("Cannot delete non empty category.", errorMessage);
    }


    [Fact]
    public async Task DeleteCategory_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var categoryToDelete = new Category
        {
            Id = Guid.NewGuid(),
            SKU = Guid.NewGuid(),
            Name = "Category to Delete",
            OwnerId = "test-user-id",
            Family = "test-family"
        };

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            db.Categories.Add(categoryToDelete);
            db.SaveChanges();
        }
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));

        // Act
        var response = await _httpClient.DeleteAsync($"api/Catalog/categories/{categoryToDelete.SKU}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    #endregion

}

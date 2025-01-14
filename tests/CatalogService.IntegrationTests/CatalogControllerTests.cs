using System;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using CatalogService.Data;
using CatalogService.DTOs;
using CatalogService.IntegrationTests.Fixtures;
using CatalogService.IntegrationTests.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using WebMotions.Fake.Authentication.JwtBearer;

namespace CatalogService.IntegrationTests;

public class CatalogControllerTests : IClassFixture<CustomWebAppFactory>, IAsyncLifetime
{
    private readonly CustomWebAppFactory _factory;
    private readonly HttpClient _httpClient;
    public CatalogControllerTests(CustomWebAppFactory factory)
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
    public async Task GetCategories_ShouldReturnListOfCategoryDto()
    {
        // Arrange

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user-id", "test-family"));


        //Act
        var response = await _httpClient.GetFromJsonAsync<List<CategoryDto>>("api/Catalog/categories");

        //Assert

        // TODO check why the database creates 3 entries. 
        Assert.IsType<List<CategoryDto>>(response);
    }
}

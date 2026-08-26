using AutoMapper;
using CatalogService.Data;
using CatalogService.DTOs;
using CatalogService.Entities;
using CatalogService.RequestHelpers;
using CatalogService.Services;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;

namespace CatalogService.UnitTests;

public class CatalogBusinessServiceTests
{
    private readonly Mock<ICatalogRepository> _repositoryMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<ILogger<CatalogBusinessService>> _loggerMock;
    private readonly Mock<IRequestContextService> _requestContextMock;

    private readonly ICatalogBusinessService _service;

    public CatalogBusinessServiceTests()
    {
        _repositoryMock = new Mock<ICatalogRepository>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _loggerMock = new Mock<ILogger<CatalogBusinessService>>();
        _requestContextMock = new Mock<IRequestContextService>();

        var mapperConfig = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingProfiles>(),
            Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);

        var mapper = mapperConfig.CreateMapper();

        _requestContextMock
            .Setup(x => x.UserId)
            .Returns("test-user-id");

        _requestContextMock
            .Setup(x => x.FamilyName)
            .Returns("test-family");

        _requestContextMock
            .Setup(x => x.OperationId)
            .Returns("test-operation-id");

        _requestContextMock
            .Setup(x => x.TraceId)
            .Returns("test-trace-id");

        _requestContextMock
            .Setup(x => x.RequestId)
            .Returns("test-request-id");

        _requestContextMock
            .Setup(x => x.RequestMethod)
            .Returns("POST");

        _requestContextMock
            .Setup(x => x.RequestPath)
            .Returns("/api/Catalog/categories");

        _service = new CatalogBusinessService(
            _repositoryMock.Object,
            mapper,
            _publishEndpointMock.Object,
            _loggerMock.Object,
            _requestContextMock.Object);
    }

    [Fact]
    public async Task CreateCategoryAsync_WhenSaveSucceeds_ShouldReturnSuccessAndPublishEvent()
    {
        // Arrange
        var categoryDto = new CreateCategoryDto
        {
            Name = "Produce"
        };

        _repositoryMock
            .Setup(x => x.GetCategoryEntityByName(categoryDto.Name, "test-family"))
            .ReturnsAsync((Category?)null);

        _repositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _service.CreateCategoryAsync(categoryDto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(categoryDto.Name, result.Data.Name);

        _repositoryMock.Verify(
            x => x.AddCategory(It.Is<Category>(
                category =>
                    category.Name == categoryDto.Name &&
                    category.OwnerId == "test-user-id" &&
                    category.Family == "test-family")),
            Times.Once);

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);

        Assert.Contains(
            _publishEndpointMock.Invocations,
            invocation => invocation.Method.Name == "Publish");
    }

    [Fact]
    public async Task CreateCategoryAsync_WhenSaveFails_ShouldReturnFailureAndNotPublishEvent()
    {
        // Arrange
        var categoryDto = new CreateCategoryDto
        {
            Name = "Produce"
        };

        _repositoryMock
            .Setup(x => x.GetCategoryEntityByName(categoryDto.Name, "test-family"))
            .ReturnsAsync((Category?)null);

        _repositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(false);

        // Act
        var result = await _service.CreateCategoryAsync(categoryDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);

        Assert.DoesNotContain(
            _publishEndpointMock.Invocations,
            invocation => invocation.Method.Name == "Publish");
    }

    [Fact]
    public async Task UpdateCategoryAsync_WhenSaveFails_ShouldReturnFailureAndNotPublishEvent()
    {
        // Arrange
        var categorySku = Guid.NewGuid();

        var existingCategory = new Category
        {
            Id = Guid.NewGuid(),
            SKU = categorySku,
            Name = "Produce",
            OwnerId = "test-user-id",
            Family = "test-family"
        };

        var updateDto = new UpdateCategoryDto
        {
            Name = "Fresh Produce"
        };

        _repositoryMock
            .Setup(x => x.GetCategoryEntityBySku(categorySku, "test-family"))
            .ReturnsAsync(existingCategory);

        _repositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(false);

        // Act
        var result = await _service.UpdateCategoryAsync(categorySku, updateDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);

        Assert.DoesNotContain(
            _publishEndpointMock.Invocations,
            invocation => invocation.Method.Name == "Publish");
    }

    [Fact]
    public async Task DeleteCategoryAsync_WhenSaveFails_ShouldReturnFailureAndNotPublishEvent()
    {
        // Arrange
        var categorySku = Guid.NewGuid();

        var existingCategory = new Category
        {
            Id = Guid.NewGuid(),
            SKU = categorySku,
            Name = "Produce",
            OwnerId = "test-user-id",
            Family = "test-family",
            Items = new List<Item>()
        };

        _repositoryMock
            .Setup(x => x.GetCategoryEntityBySku(categorySku, "test-family"))
            .ReturnsAsync(existingCategory);

        _repositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(false);

        // Act
        var result = await _service.DeleteCategoryAsync(categorySku);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);

        Assert.DoesNotContain(
            _publishEndpointMock.Invocations,
            invocation => invocation.Method.Name == "Publish");
    }

    [Fact]
    public async Task CreateItemAsync_WhenSaveFails_ShouldReturnFailureAndNotPublishEvent()
    {
        var categorySku = Guid.NewGuid();

        var category = new Category
        {
            Id = Guid.NewGuid(),
            SKU = categorySku,
            Name = "Produce",
            OwnerId = "test-user-id",
            Family = "test-family"
        };

        var itemDto = new CreateItemDto
        {
            Name = "Apples",
            CategorySKU = categorySku
        };

        _repositoryMock
        .Setup(x => x.GetItemEntityByNameAsync(itemDto.Name, "test-family"))
        .ReturnsAsync((Item?)null);

        _repositoryMock
            .Setup(x => x.GetCategoryEntityBySku(categorySku, "test-family"))
            .ReturnsAsync(category);

        _repositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(false);

        // Act
        var result = await _service.CreateItemAsync(itemDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);

        Assert.DoesNotContain(
            _publishEndpointMock.Invocations,
            invocation => invocation.Method.Name == "Publish");
    }

    [Fact]
    public async Task UpdateItemAsync_WhenSaveFails_ShouldReturnFailureAndNotPublishEvent()
    {
        // Arrange
        var itemSku = Guid.NewGuid();
        var categorySku = Guid.NewGuid();

        var category = new Category
        {
            Id = Guid.NewGuid(),
            SKU = categorySku,
            Name = "Produce",
            OwnerId = "test-user-id",
            Family = "test-family"
        };

        var item = new Item
        {
            Id = Guid.NewGuid(),
            SKU = itemSku,
            Name = "Apples",
            OwnerId = "test-user-id",
            Family = "test-family",
            CategoryId = category.Id,
            CategorySKU = categorySku,
            CategoryName = category.Name,
            Category = category
        };

        var updateDto = new UpdateItemDto
        {
            Name = "Green Apples",
            CategorySKU = categorySku
        };

        _repositoryMock
            .Setup(x => x.GetItemEntityBySkuAsync(itemSku, "test-family"))
            .ReturnsAsync(item);

        _repositoryMock
            .Setup(x => x.GetCategoryEntityBySku(categorySku, "test-family"))
            .ReturnsAsync(category);

        _repositoryMock
            .Setup(x => x.UpdateItemAsync(item, updateDto))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(false);

        // Act
        var result = await _service.UpdateItemAsync(itemSku, updateDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);

        Assert.DoesNotContain(
            _publishEndpointMock.Invocations,
            invocation => invocation.Method.Name == "Publish");
    }

    [Fact]
    public async Task DeleteItemAsync_WhenSaveFails_ShouldReturnFailureAndNotPublishEvent()
    {
        // Arrange
        var itemSku = Guid.NewGuid();

        var category = new Category
        {
            Id = Guid.NewGuid(),
            SKU = Guid.NewGuid(),
            Name = "Produce",
            OwnerId = "test-user-id",
            Family = "test-family"
        };

        var item = new Item
        {
            Id = Guid.NewGuid(),
            SKU = itemSku,
            Name = "Apples",
            OwnerId = "test-user-id",
            Family = "test-family",
            CategoryId = category.Id,
            CategorySKU = category.SKU,
            CategoryName = category.Name,
            Category = category
        };

        _repositoryMock
            .Setup(x => x.GetItemEntityBySkuAsync(itemSku, "test-family"))
            .ReturnsAsync(item);

        _repositoryMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(false);

        // Act
        var result = await _service.DeleteItemAsync(itemSku);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);

        Assert.DoesNotContain(
            _publishEndpointMock.Invocations,
            invocation => invocation.Method.Name == "Publish");
    }
}

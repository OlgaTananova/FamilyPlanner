using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using AutoFixture;
using AutoMapper;
using CatalogService.Controllers;
using CatalogService.Data;
using CatalogService.DTOs;
using CatalogService.Entities;
using CatalogService.RequestHelpers;
using Contracts.Catalog;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Moq;
using Xunit.Sdk;

namespace CatalogService.UnitTests;

public class CatalogServiceControllerTests
{
    public readonly Mock<ICatalogRepository> _repo;

    public readonly Mock<IPublishEndpoint> _publisher;
    public readonly Fixture _fixture;
    public readonly CatalogController _controller;
    public IMapper _mapper;


    public CatalogServiceControllerTests()
    {
        _fixture = new Fixture();
        _repo = new Mock<ICatalogRepository>();
        _publisher = new Mock<IPublishEndpoint>();

        var mockMapperCofiguration = new MapperConfiguration(mc =>
        {
            mc.AddMaps(typeof(MappingProfiles).Assembly);
        }).CreateMapper().ConfigurationProvider;

        _mapper = new Mapper(mockMapperCofiguration);

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockHttpContext = new DefaultHttpContext();
        mockHttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
        new Claim("family", "MockFamilyName"),
        new Claim("userId", "MockUserId")
    }, "mock"));
        mockHttpContext.Request.Headers["TraceParent"] = "MockOperationId";
        mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(mockHttpContext);

        var mockLogger = new Mock<ILogger<CatalogController>>();
        _controller = new CatalogController(_mapper, _repo.Object, mockHttpContextAccessor.Object, _publisher.Object, mockLogger.Object);

    }

    [Fact]
    public async Task GetAllCategories_ShouldReturnListOfCategories()
    {
        // Arrange
        var categories = _fixture.CreateMany<CategoryDto>(10).ToList();
        _repo.Setup(r => r.GetAllCategoriesAsync(It.IsAny<string>()))
             .ReturnsAsync(categories);

        // Act
        var result = await _controller.GetAllCategories();

        // Assert
        var objectResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCategories = Assert.IsType<List<CategoryDto>>(objectResult.Value);
        Assert.Equal(categories.Count, returnedCategories.Count);
    }

    [Fact]
    public async Task GetCategoryBySku_ShouldReturnCategory()
    {
        // Arrange 
        var category = _fixture.Create<CategoryDto>();
        _repo.Setup(r => r.GetCategoryBySkuAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync(category);

        // Act 
        var result = await _controller.GetCategoryBySku(It.IsAny<Guid>());

        // Assert
        Assert.IsType<ActionResult<CategoryDto>>(result);

    }

    [Fact]
    public async Task GetCategoryBySku_ShouldReturnNotFound()
    {
        // Arrange 
        var category = _fixture.Create<CategoryDto>();
        _repo.Setup(r => r.GetCategoryBySkuAsync(It.IsAny<Guid>(), It.IsAny<string>())).ReturnsAsync((CategoryDto)null);

        // Act 
        var result = await _controller.GetCategoryBySku(It.IsAny<Guid>());

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);


    }

    [Fact]
    public async Task CreateCategory_ShouldReturnBadRequest_WhenCategoryAlreadyExists()
    {
        // Arrange
        // Configure Fixture to handle circular references
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
               .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var categoryDto = _fixture.Create<CreateCategoryDto>();
        var existingCategory = _fixture.Create<Category>();


        _repo.Setup(r => r.GetCategoryEntityByName(categoryDto.Name, It.IsAny<string>()))
             .ReturnsAsync(existingCategory);

        // Act
        var result = await _controller.CreateCategory(categoryDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("The category with this name already exists.", badRequestResult.Value);
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnBadRequest_WhenSaveChangesFails()
    {
        // Arrange
        var categoryDto = _fixture.Create<CreateCategoryDto>();

        _repo.Setup(r => r.GetCategoryEntityByName(categoryDto.Name, It.IsAny<string>()))
             .ReturnsAsync((Category)null);

        _repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(false);

        // Act
        var result = await _controller.CreateCategory(categoryDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Could not save changes to the DB", badRequestResult.Value);
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnOk_WhenCategoryIsCreatedSuccessfully()
    {
        // Arrange
        var categoryDto = _fixture.Create<CreateCategoryDto>();

        // Configure Fixture to handle circular references
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
               .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var category = _fixture.Build<Category>()
                               .With(c => c.Name, categoryDto.Name)
                               .Create();
        var categoryDtoResult = _fixture.Create<CategoryDto>();

        _repo.Setup(r => r.GetCategoryEntityByName(categoryDto.Name, It.IsAny<string>()))
             .ReturnsAsync((Category)null);

        _repo.Setup(r => r.AddCategory(It.IsAny<Category>())).Verifiable();
        _repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        _repo.Setup(r => r.AddCategory(It.IsAny<Category>())).Callback<Category>(c => _mapper.Map(category, categoryDtoResult));

        // Act
        var result = await _controller.CreateCategory(categoryDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCategory = Assert.IsType<CategoryDto>(okResult.Value);
    }

    [Fact]
    public async Task CreateItem_ShouldReturnBadRequest_WhenItemAlreadyExists()
    {
        // Arrange
        // Configure Fixture to handle circular references
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var itemDto = _fixture.Create<CreateItemDto>();
        var existingItem = _fixture.Create<Item>();

        _repo.Setup(r => r.GetItemEntityByNameAsync(itemDto.Name, It.IsAny<string>()))
             .ReturnsAsync(existingItem);

        // Act
        var result = await _controller.CreateItem(itemDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("The item with this name already exists.", badRequestResult.Value);
    }

    [Fact]
    public async Task CreateItem_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        var itemDto = _fixture.Create<CreateItemDto>();

        _repo.Setup(r => r.GetItemEntityByNameAsync(itemDto.Name, It.IsAny<string>()))
             .ReturnsAsync((Item)null);

        _repo.Setup(r => r.GetCategoryEntityBySku(itemDto.CategorySKU, It.IsAny<string>()))
             .ReturnsAsync((Category)null);

        // Act
        var result = await _controller.CreateItem(itemDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Cannot find the category of the newly created item.", notFoundResult.Value);
    }

    [Fact]
    public async Task CreateItem_ShouldReturnBadRequest_WhenSaveChangesFails()
    {
        // Arrange

        // Configure Fixture to handle circular references
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
               .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());


        var itemDto = _fixture.Create<CreateItemDto>();
        var category = _fixture.Create<Category>();


        _repo.Setup(r => r.GetItemEntityByNameAsync(itemDto.Name, It.IsAny<string>()))
             .ReturnsAsync((Item)null);

        _repo.Setup(r => r.GetCategoryEntityBySku(itemDto.CategorySKU, It.IsAny<string>()))
             .ReturnsAsync(category);

        _repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(false);

        // Act
        var result = await _controller.CreateItem(itemDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Could not save changes to the DB.", badRequestResult.Value);
    }

    [Fact]
    public async Task CreateItem_ShouldReturnOk_WhenItemIsCreatedSuccessfully()
    {
        // Arrange

        // Configure Fixture to handle circular references
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
               .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());


        var itemDto = _fixture.Create<CreateItemDto>();
        var category = _fixture.Create<Category>();
        var item = _fixture.Create<Item>();
        var itemDtoResult = _fixture.Create<ItemDto>();

        _repo.Setup(r => r.GetItemEntityByNameAsync(itemDto.Name, It.IsAny<string>()))
             .ReturnsAsync((Item)null);

        _repo.Setup(r => r.GetCategoryEntityBySku(itemDto.CategorySKU, It.IsAny<string>()))
             .ReturnsAsync(category);

        _repo.Setup(r => r.SaveChangesAsync()).ReturnsAsync(true);

        _repo.Setup(r => r.AddItem(It.IsAny<Item>())).Callback<Item>(i => _mapper.Map(i, itemDtoResult));

        // Act
        var result = await _controller.CreateItem(itemDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedItem = Assert.IsType<ItemDto>(okResult.Value);

    }




}

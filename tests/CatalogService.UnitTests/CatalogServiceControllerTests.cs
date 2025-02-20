using System.Diagnostics;
using System.Security.Claims;
using AutoFixture;
using AutoMapper;
using CatalogService.Controllers;
using CatalogService.DTOs;
using CatalogService.RequestHelpers;
using CatalogService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;

namespace CatalogService.UnitTests;

public class HttpActivityFeature : IHttpActivityFeature
{
    public Activity Activity { get; set; } = new Activity("Default");
}

public class CatalogServiceControllerTests
{

    private readonly Mock<ICatalogBusinessService> _catalogBusinessServiceMock;
    private readonly ProblemDetailsFactory _problemDetailsFactory;
    private readonly CatalogController _controller;
    private readonly Fixture _fixture;

    public CatalogServiceControllerTests()
    {
        _fixture = new Fixture();
        _catalogBusinessServiceMock = new Mock<ICatalogBusinessService>();

        var mapperConfiguration = new MapperConfiguration(mc =>
        {
            mc.AddMaps(typeof(MappingProfiles).Assembly);
        });
        var mapper = mapperConfiguration.CreateMapper();

        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockHttpContext = new DefaultHttpContext();

        // Set user claims
        mockHttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("family", "MockFamilyName"),
            new Claim("userId", "MockUserId")
        }, "mock"));

        // Set TraceParent header
        mockHttpContext.Request.Headers["TraceParent"] = "MockOperationId";

        var activity = new Activity("TestActivity");
        activity.SetIdFormat(ActivityIdFormat.W3C);
        activity.Start();

        var activityFeature = new HttpActivityFeature { Activity = activity };
        mockHttpContext.Features.Set<IHttpActivityFeature>(activityFeature);

        mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(mockHttpContext);

        var mockLogger = new Mock<ILogger<CatalogController>>();

        _problemDetailsFactory = new TestProblemDetailsFactory();

        _controller = new CatalogController(
            _catalogBusinessServiceMock.Object,
            _problemDetailsFactory,
            new ControllerContext { HttpContext = mockHttpContext }
        );

    }

    [Fact]
    public async Task GetAllCategories_ShouldReturnOk_WithListOfCategories()
    {
        // Arrange
        var categories = _fixture.CreateMany<CategoryDto>(5);
        var serviceResponse = ServiceResult<List<CategoryDto>>.SuccessResult(categories.ToList());

        _catalogBusinessServiceMock
            .Setup(s => s.GetAllCategoriesAsync())
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.GetAllCategories();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCategories = Assert.IsType<List<CategoryDto>>(okResult.Value);
        Assert.Equal(categories.Count(), returnedCategories.Count);
    }

    [Fact]
    public async Task GetCategoryBySku_ShouldReturnOk_WhenCategoryExists()
    {
        // Arrange
        var category = _fixture.Create<CategoryDto>();
        var serviceResponse = ServiceResult<CategoryDto>.SuccessResult(category);

        _catalogBusinessServiceMock
            .Setup(s => s.GetCategoryBySkuAsync(It.IsAny<Guid>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.GetCategoryBySku(Guid.NewGuid());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCategory = Assert.IsType<CategoryDto>(okResult.Value);
        Assert.Equal(category.Name, returnedCategory.Name);
    }

    [Fact]
    public async Task GetCategoryBySku_ShouldReturnProblemDetails_WhenCategoryNotFound()
    {
        // Arrange
        var serviceResponse = ServiceResult<CategoryDto>.FailureResult("Category not found", 404);

        _catalogBusinessServiceMock
            .Setup(s => s.GetCategoryBySkuAsync(It.IsAny<Guid>()))
            .ReturnsAsync(serviceResponse);

        _problemDetailsFactory.CreateProblemDetails(_controller.HttpContext, 404, "Category not found", "Category not found");

        // Act
        var result = await _controller.GetCategoryBySku(Guid.NewGuid());

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);

        Assert.Equal(404, problemDetails.Status);
        Assert.Equal("Category not found", problemDetails.Title);
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnOk_WhenCategoryIsCreatedSuccessfully()
    {
        // Arrange
        var categoryDto = _fixture.Create<CreateCategoryDto>();
        var createdCategory = _fixture.Create<CategoryDto>();

        var serviceResponse = ServiceResult<CategoryDto>.SuccessResult(createdCategory);

        _catalogBusinessServiceMock
            .Setup(s => s.CreateCategoryAsync(It.IsAny<CreateCategoryDto>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.CreateCategory(categoryDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCategory = Assert.IsType<CategoryDto>(okResult.Value);
        Assert.Equal(createdCategory.Name, returnedCategory.Name);
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnProblemDetails_WhenCategoryAlreadyExists()
    {
        // Arrange
        var categoryDto = _fixture.Create<CreateCategoryDto>();
        var serviceResponse = ServiceResult<CategoryDto>.FailureResult("Category already exists", 400);

        _catalogBusinessServiceMock
            .Setup(s => s.CreateCategoryAsync(It.IsAny<CreateCategoryDto>()))
            .ReturnsAsync(serviceResponse);

        _problemDetailsFactory.CreateProblemDetails(_controller.HttpContext, 404, "Category already exists", "Category already exists");

        _catalogBusinessServiceMock
            .Setup(s => s.CreateCategoryAsync(It.IsAny<CreateCategoryDto>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.CreateCategory(categoryDto);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);

        Assert.Equal(400, problemDetails.Status);
        Assert.Equal("Category already exists", problemDetails.Title);
    }

    [Fact]
    public async Task CreateCategory_ShouldReturnProblemDetails_WhenSaveFails()
    {
        // Arrange
        var categoryDto = _fixture.Create<CreateCategoryDto>();
        var serviceResponse = ServiceResult<CategoryDto>.FailureResult("Failed to save category", 500);

        _catalogBusinessServiceMock
            .Setup(s => s.CreateCategoryAsync(It.IsAny<CreateCategoryDto>()))
            .ReturnsAsync(serviceResponse);

        var expectedProblemDetails = new ProblemDetails
        {
            Status = 500,
            Title = "Failed to save category",
            Detail = "Failed to save category"
        };

        // Act
        var result = await _controller.CreateCategory(categoryDto);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);

        Assert.Equal(500, problemDetails.Status);
        Assert.Equal("Failed to save category", problemDetails.Title);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnOk_WhenUpdateIsSuccessful()
    {
        // Arrange
        var sku = Guid.NewGuid();
        var updateCategoryDto = _fixture.Create<UpdateCategoryDto>();
        var updatedCategory = _fixture.Create<CategoryDto>();

        var serviceResponse = ServiceResult<CategoryDto>.SuccessResult(updatedCategory);

        _catalogBusinessServiceMock
            .Setup(s => s.UpdateCategoryAsync(sku, It.IsAny<UpdateCategoryDto>()))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.UpdateCategory(sku, updateCategoryDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedCategory = Assert.IsType<CategoryDto>(okResult.Value);
        Assert.Equal(updatedCategory.Name, returnedCategory.Name);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnProblemDetails_WhenCategoryNotFound()
    {
        // Arrange
        var sku = Guid.NewGuid();
        var updateCategoryDto = _fixture.Create<UpdateCategoryDto>();

        var serviceResponse = ServiceResult<CategoryDto>.FailureResult("Category not found", 404);

        _catalogBusinessServiceMock
            .Setup(s => s.UpdateCategoryAsync(sku, It.IsAny<UpdateCategoryDto>()))
            .ReturnsAsync(serviceResponse);

        var expectedProblemDetails = new ProblemDetails
        {
            Status = 404,
            Title = "Category not found",
            Detail = "Category not found"
        };

        // Act
        var result = await _controller.UpdateCategory(sku, updateCategoryDto);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);

        Assert.Equal(404, problemDetails.Status);
        Assert.Equal("Category not found", problemDetails.Title);
    }

    [Fact]
    public async Task UpdateCategory_ShouldReturnProblemDetails_WhenUpdateFails()
    {
        // Arrange
        var sku = Guid.NewGuid();
        var updateCategoryDto = _fixture.Create<UpdateCategoryDto>();

        var serviceResponse = ServiceResult<CategoryDto>.FailureResult("Failed to update category", 500);

        _catalogBusinessServiceMock
            .Setup(s => s.UpdateCategoryAsync(sku, It.IsAny<UpdateCategoryDto>()))
            .ReturnsAsync(serviceResponse);

        var expectedProblemDetails = new ProblemDetails
        {
            Status = 500,
            Title = "Failed to update category",
            Detail = "Failed to update category"
        };

        // Act
        var result = await _controller.UpdateCategory(sku, updateCategoryDto);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);

        Assert.Equal(500, problemDetails.Status);
        Assert.Equal("Failed to update category", problemDetails.Title);
    }

    [Fact]
    public async Task DeleteCategory_ShouldReturnNoContent_WhenDeletionIsSuccessful()
    {
        // Arrange
        var sku = Guid.NewGuid();
        CategoryDto categoryDto = _fixture.Create<CategoryDto>();
        var serviceResponse = ServiceResult<CategoryDto>.SuccessResult(categoryDto);

        _catalogBusinessServiceMock
            .Setup(s => s.DeleteCategoryAsync(sku))
            .ReturnsAsync(serviceResponse);

        // Act
        var result = await _controller.DeleteCategory(sku);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteCategory_ShouldReturnProblemDetails_WhenCategoryNotFound()
    {
        // Arrange
        var sku = Guid.NewGuid();
        CategoryDto categoryDto = _fixture.Create<CategoryDto>();
        var serviceResponse = ServiceResult<CategoryDto>.FailureResult("Category not found", 404);

        _catalogBusinessServiceMock
            .Setup(s => s.DeleteCategoryAsync(sku))
            .ReturnsAsync(serviceResponse);

        var expectedProblemDetails = new ProblemDetails
        {
            Status = 404,
            Title = "Category not found",
            Detail = "Category not found"
        };

        // Act
        var result = await _controller.DeleteCategory(sku);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);

        Assert.Equal(404, problemDetails.Status);
        Assert.Equal("Category not found", problemDetails.Title);
    }

    [Fact]
    public async Task DeleteCategory_ShouldReturnProblemDetails_WhenDeletionFails()
    {
        // Arrange
        var sku = Guid.NewGuid();
        CategoryDto categoryDto = _fixture.Create<CategoryDto>();
        var serviceResponse = ServiceResult<CategoryDto>.FailureResult("Failed to delete category", 500);

        _catalogBusinessServiceMock
            .Setup(s => s.DeleteCategoryAsync(sku))
            .ReturnsAsync(serviceResponse);

        var expectedProblemDetails = new ProblemDetails
        {
            Status = 500,
            Title = "Failed to delete category",
            Detail = "Failed to delete category"
        };

        // Act
        var result = await _controller.DeleteCategory(sku);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);

        Assert.Equal(500, problemDetails.Status);
        Assert.Equal("Failed to delete category", problemDetails.Title);
    }

}

using AutoMapper;
using CatalogService.Data;
using CatalogService.DTOs;
using CatalogService.Entities;
using CatalogService.RequestHelpers;
using Contracts.Catalog;
using MassTransit;

namespace CatalogService.Services;

public class CatalogBusinessService : ICatalogBusinessService
{
    private readonly ICatalogRepository _repo;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CatalogBusinessService> _logger;
    private readonly IRequestContextService _requestContextService;
    public CatalogBusinessService(ICatalogRepository repo, IMapper mapper, IPublishEndpoint publishEndpoint, ILogger<CatalogBusinessService> logger, IRequestContextService requestContextService)
    {
        _repo = repo;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
        _requestContextService = requestContextService;
    }

    public async Task<ServiceResult<List<CategoryDto>>> GetAllCategoriesAsync()
    {
        string familyName = _requestContextService.FamilyName;
        List<CategoryDto> categories = await _repo.GetAllCategoriesAsync(familyName);
        return ServiceResult<List<CategoryDto>>.SuccessResult(categories);
    }

    public async Task<ServiceResult<CategoryDto>> GetCategoryBySkuAsync(Guid sku)
    {
        string familyName = _requestContextService.FamilyName;
        CategoryDto category = await _repo.GetCategoryBySkuAsync(sku, familyName);
        if (category == null)
        {
            return ServiceResult<CategoryDto>.FailureResult($"The category with SKU {sku} was not found.", 404);
        }
        return ServiceResult<CategoryDto>.SuccessResult(category);
    }

    public async Task<ServiceResult<CategoryDto>> UpdateCategoryAsync(Guid sku, UpdateCategoryDto categoryDto)
    {
        string familyName = _requestContextService.FamilyName;
        string userId = _requestContextService.UserId;
        string operationId = _requestContextService.OperationId;
        string traceId = _requestContextService.TraceId;
        string requestId = _requestContextService.RequestId;

        StructuredLogger.LogInformation(_logger, "Update Category request received.", _requestContextService);

        Category category = await _repo.GetCategoryEntityBySku(sku, familyName);

        if (category == null)
        {
            StructuredLogger.LogWarning(_logger, "Update Category request failed: Category not found.", _requestContextService);

            return ServiceResult<CategoryDto>.FailureResult($"The category with SKU {sku} was not found.", 404);
        }

        await _repo.UpdateCategoryAsync(category, categoryDto);

        CategoryDto updatedCategory = _mapper.Map<CategoryDto>(category);

        await _publishEndpoint.Publish(_mapper.Map<CatalogCategoryUpdated>(updatedCategory), context =>
            {
                context.Headers.Set("OperationId", operationId);
                context.Headers.Set("traceId", traceId);
                context.Headers.Set("requestId", requestId);
            });

        if (!await _repo.SaveChangesAsync())
        {
            StructuredLogger.LogError(_logger, "Update Category request failed: Database save error.", _requestContextService);

            return ServiceResult<CategoryDto>.FailureResult("Could not save changes to the database.", 500);
        }

        StructuredLogger.LogInformation(_logger, "Update Category request succeeded.", _requestContextService);

        return ServiceResult<CategoryDto>.SuccessResult(updatedCategory);

    }
    async Task<ServiceResult<CategoryDto>> ICatalogBusinessService.CreateCategoryAsync(CreateCategoryDto categoryDto)
    {
        string familyName = _requestContextService.FamilyName;
        string userId = _requestContextService.UserId;
        string operationId = _requestContextService.OperationId;
        string traceId = _requestContextService.TraceId;
        string requestId = _requestContextService.RequestId;

        StructuredLogger.LogInformation(_logger, "Create Category request received.", _requestContextService,
        new Dictionary<string, object> { { "category", categoryDto.Name } });

        // Check if category already exists
        if (await _repo.GetCategoryEntityByName(categoryDto.Name, familyName) != null)
        {
            StructuredLogger.LogWarning(_logger, "Create Category request failed: Category already exists.", _requestContextService);

            return ServiceResult<CategoryDto>.FailureResult($"A category with the name '{categoryDto.Name}' already exists.", 400);
        }

        // Create and map category
        var category = _mapper.Map<Category>(categoryDto);
        category.OwnerId = userId;
        category.Family = familyName;
        _repo.AddCategory(category);

        var newCategory = _mapper.Map<CategoryDto>(category);

        // Publish event
        await _publishEndpoint.Publish(_mapper.Map<CatalogCategoryCreated>(newCategory), context =>
        {
            context.Headers.Set("OperationId", operationId);
            context.Headers.Set("traceId", traceId);
            context.Headers.Set("requestId", requestId);
        });

        // Save changes
        if (!await _repo.SaveChangesAsync())
        {
            StructuredLogger.LogError(_logger, "Create Category request failed: Database save error.", _requestContextService, new Dictionary<string, object> { { "category", categoryDto.Name } });

            return ServiceResult<CategoryDto>.FailureResult("Could not save changes to the database.", 500);
        }

        StructuredLogger.LogInformation(_logger, "Create Category request succeeded.", _requestContextService, new Dictionary<string, object> { { "category", category.Name } });

        return ServiceResult<CategoryDto>.SuccessResult(newCategory);
    }

    public async Task<ServiceResult<CategoryDto>> DeleteCategoryAsync(Guid sku)
    {
        string familyName = _requestContextService.FamilyName;
        string operationId = _requestContextService.OperationId;
        string traceId = _requestContextService.TraceId;
        string requestId = _requestContextService.RequestId;

        StructuredLogger.LogInformation(_logger, "Delete Category request received.", _requestContextService, new Dictionary<string, object> { { "sku", sku } });

        Category category = await _repo.GetCategoryEntityBySku(sku, familyName);

        if (category == null)
        {
            StructuredLogger.LogWarning(_logger, "Delete Category request failed: Category not found.", _requestContextService, new Dictionary<string, object> { { "sku", sku } });

            return ServiceResult<CategoryDto>.FailureResult($"The category with SKU {sku} was not found.", 404);
        }

        if (category.Items.Any())
        {
            StructuredLogger.LogWarning(_logger, "Delete Category request failed: Category not empty.", _requestContextService, new Dictionary<string, object> { { "sku", sku } });

            return ServiceResult<CategoryDto>.FailureResult("The category is not empty and cannot be deleted.", 400);
        }

        category.IsDeleted = true;

        CategoryDto deletedCategory = _mapper.Map<CategoryDto>(category);

        await _publishEndpoint.Publish(_mapper.Map<CatalogCategoryDeleted>(deletedCategory), context =>
        {
            context.Headers.Set("OperationId", operationId);
            context.Headers.Set("traceId", traceId);
            context.Headers.Set("requestId", requestId);

        });

        if (!await _repo.SaveChangesAsync())
        {
            StructuredLogger.LogError(_logger, "Delete Category request failed: Database save error.", _requestContextService, new Dictionary<string, object> { { "sku", sku } });

            return ServiceResult<CategoryDto>.FailureResult("Could not save changes to the database.", 500);
        }
        return ServiceResult<CategoryDto>.SuccessResult(deletedCategory);
    }

    public async Task<ServiceResult<List<ItemDto>>> GetAllItemsAsync()
    {
        string familyName = _requestContextService.FamilyName;

        var result = await _repo.GetAllItemsAsync(familyName);
        return ServiceResult<List<ItemDto>>.SuccessResult(result);
    }

    public async Task<ServiceResult<ItemDto>> GetItemBySkuAsync(Guid sku)
    {
        string familyName = _requestContextService.FamilyName;
        var item = await _repo.GetItemBySkuAsync(sku, familyName);

        if (item == null)
        {
            StructuredLogger.LogWarning(_logger, "Get Item request failed: Item not found.", _requestContextService, new Dictionary<string, object> { { "sku", sku } });

            return ServiceResult<ItemDto>.FailureResult($"The item with SKU {sku} was not found.", 404);
        }

        return ServiceResult<ItemDto>.SuccessResult(item);
    }

    public async Task<ServiceResult<ItemDto>> CreateItemAsync(CreateItemDto itemDto)
    {
        string familyName = _requestContextService.FamilyName;
        string userId = _requestContextService.UserId;
        string operationId = _requestContextService.OperationId;
        string traceId = _requestContextService.TraceId;
        string requestId = _requestContextService.RequestId;

        StructuredLogger.LogInformation(_logger, "Create Item request received.", _requestContextService, new Dictionary<string, object> { { "itemName", itemDto.Name } });
        Item existingItem = await _repo.GetItemEntityByNameAsync(itemDto.Name, familyName);

        if (existingItem != null)
        {
            StructuredLogger.LogWarning(_logger, "Create Item request failed: Item already exists.", _requestContextService, new Dictionary<string, object> { { "itemName", itemDto.Name } });
            return ServiceResult<ItemDto>.FailureResult($"An item with the name '{itemDto.Name}' already exists.", 400);
        }

        Category category = await _repo.GetCategoryEntityBySku(itemDto.CategorySKU, familyName);

        if (category == null)
        {
            StructuredLogger.LogWarning(_logger, "Create Item request failed: Category not found.", _requestContextService, new Dictionary<string, object> { { "categorySKU", itemDto.CategorySKU } });
            return ServiceResult<ItemDto>.FailureResult($"The category with SKU {itemDto.CategorySKU} was not found.", 404);
        }

        Item item = _mapper.Map<Item>(itemDto);
        item.OwnerId = userId;
        item.Family = familyName;
        item.Category = category;
        item.CategorySKU = category.SKU;
        item.CategoryName = category.Name;

        _repo.AddItem(item);

        // Send a created item to the rabbitmq
        var newItem = _mapper.Map<ItemDto>(item);

        await _publishEndpoint.Publish(_mapper.Map<CatalogItemCreated>(newItem), context =>
        {
            context.Headers.Set("OperationId", operationId);
            context.Headers.Set("traceId", traceId);
            context.Headers.Set("requestId", requestId);
        });

        StructuredLogger.LogInformation(_logger, "Create Item request succeeded.", _requestContextService, new Dictionary<string, object> { { "itemName", item.Name } });

        if (!await _repo.SaveChangesAsync())
        {
            StructuredLogger.LogError(_logger, "Create Item request failed: Database save error.", _requestContextService, new Dictionary<string, object> { { "itemName", itemDto.Name } });
            return ServiceResult<ItemDto>.FailureResult("Could not save changes to the database.", 500);
        }
        StructuredLogger.LogInformation(_logger, "Create Item request succeeded.", _requestContextService, new Dictionary<string, object> { { "itemName", item.Name } });

        return ServiceResult<ItemDto>.SuccessResult(newItem);
    }

    public async Task<ServiceResult<CatalogItemUpdated>> UpdateItemAsync(Guid sku, UpdateItemDto itemDto)
    {
        string familyName = _requestContextService.FamilyName;
        string userId = _requestContextService.UserId;
        string operationId = _requestContextService.OperationId;
        string traceId = _requestContextService.TraceId;
        string requestId = _requestContextService.RequestId;
        StructuredLogger.LogInformation(_logger, "Update Item request received.", _requestContextService, new Dictionary<string, object> { { "sku", sku }, { "itemName", itemDto.Name }, { "categorySKU", itemDto.CategorySKU } });

        Item item = await _repo.GetItemEntityBySkuAsync(sku, familyName);

        Category category = await _repo.GetCategoryEntityBySku(itemDto.CategorySKU, familyName);

        if (item == null || category == null)
        {
            StructuredLogger.LogWarning(_logger, "Update Item request failed: Item or Category not found.", _requestContextService, new Dictionary<string, object> { { "sku", sku }, { "itemName", itemDto.Name }, { "categorySKU", itemDto.CategorySKU } });

            return ServiceResult<CatalogItemUpdated>.FailureResult("The item or category was not found.", 404);
        }

        Guid previousCategorySKU = item.CategorySKU != itemDto.CategorySKU ? item.CategorySKU : itemDto.CategorySKU;

        await _repo.UpdateItemAsync(item, itemDto);

        CatalogItemUpdated catalogItemUpdated = new CatalogItemUpdated
        {
            UpdatedItem = _mapper.Map<UpdatedItem>(item),
            PreviousCategorySKU = previousCategorySKU
        };
        await _publishEndpoint.Publish(catalogItemUpdated, context =>
        {
            context.Headers.Set("OperationId", operationId);
            context.Headers.Set("traceId", traceId);
            context.Headers.Set("requestId", requestId);
        });

        if (!await _repo.SaveChangesAsync())
        {
            StructuredLogger.LogError(_logger, "Update Item request failed: Database save error.", _requestContextService, new Dictionary<string, object> { { "sku", sku }, { "itemName", itemDto.Name }, { "operationId", operationId }, { "family", familyName } });

            return ServiceResult<CatalogItemUpdated>.FailureResult("Could not save changes to the database.", 500);
        }

        return ServiceResult<CatalogItemUpdated>.SuccessResult(catalogItemUpdated);
    }

    public async Task<ServiceResult<ItemDto>> DeleteItemAsync(Guid sku)
    {
        string familyName = _requestContextService.FamilyName;
        string userId = _requestContextService.UserId;
        string operationId = _requestContextService.OperationId;
        string traceId = _requestContextService.TraceId;
        string requestId = _requestContextService.RequestId;

        StructuredLogger.LogInformation(_logger, "Delete Item request received.", _requestContextService, new Dictionary<string, object> { { "sku", sku } });


        Item item = await _repo.GetItemEntityBySkuAsync(sku, familyName);

        if (item == null)
        {
            StructuredLogger.LogWarning(_logger, "Delete Item request failed: Item not found.", _requestContextService, new Dictionary<string, object> { { "sku", sku } });
            return ServiceResult<ItemDto>.FailureResult($"The item with SKU {sku} was not found.", 404);
        }

        item.IsDeleted = true;

        ItemDto deletedItem = _mapper.Map<ItemDto>(item);

        await _publishEndpoint.Publish(_mapper.Map<CatalogItemDeleted>(deletedItem), context =>
        {
            context.Headers.Set("OperationId", operationId);
            context.Headers.Set("traceId", traceId);
            context.Headers.Set("requestId", requestId);
        });

        if (!await _repo.SaveChangesAsync())
        {
            StructuredLogger.LogError(_logger, "Delete Item request failed: Database save error.", _requestContextService, new Dictionary<string, object> { { "sku", sku } });
            return ServiceResult<ItemDto>.FailureResult("Could not save changes to the database.", 500);
        }

        StructuredLogger.LogInformation(_logger, "Delete Item request succeeded.", _requestContextService, new Dictionary<string, object> { { "sku", sku } });
        return ServiceResult<ItemDto>.SuccessResult(deletedItem);
    }

    public async Task<ServiceResult<List<ItemDto>>> SearchItemsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return ServiceResult<List<ItemDto>>.FailureResult("Query parameter is required.", 400);
        }
        string familyName = _requestContextService.FamilyName;

        var items = await _repo.SearchItemsAsync(query, familyName);

        var catalogItemDtos = _mapper.Map<List<ItemDto>>(items);

        return ServiceResult<List<ItemDto>>.SuccessResult(catalogItemDtos);
    }
}

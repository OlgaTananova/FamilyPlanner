using AutoMapper;
using CatalogService.RequestHelpers;
using Contracts.ShoppingLists;
using MassTransit;
using ShoppingListService.Data;
using ShoppingListService.DTOs;
using ShoppingListService.Entities;
using ShoppingListService.Helpers;

namespace ShoppingListService.Services;

public class ShoppingListBusinessService : IShoppingListBusinessService
{

    private readonly IShoppingListService _repo;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ShoppingListBusinessService> _logger;
    private readonly IRequestContextService _requestContextService;

    public ShoppingListBusinessService(IShoppingListService repo, IMapper mapper, IPublishEndpoint publishEndpoint, ILogger<ShoppingListBusinessService> logger, IRequestContextService requestContextService)
    {
        _repo = repo;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
        _requestContextService = requestContextService;
    }
    public async Task<ServiceResult<ShoppingListDto>> CreateShoppingListAsync(CreateShoppingListDto shoppingListDto)
    {
        StructuredLogger.LogInformation(_logger, "Create Shopping List request received.", _requestContextService);
        string familyName = _requestContextService.FamilyName;
        string operationId = _requestContextService.OperationId;
        string userId = _requestContextService.UserId;
        string traceId = _requestContextService.TraceId;
        string requestId = _requestContextService.RequestId;

        if (shoppingListDto == null)
        {
            StructuredLogger.LogWarning(_logger, "Create Shopping List request failed: Request body is null.", _requestContextService);
            return ServiceResult<ShoppingListDto>.FailureResult("Request body is null.", 400);
        }

        var validator = new CreateShoppingListDtoValidator();
        var validationResult = validator.Validate(shoppingListDto);
        if (!validationResult.IsValid)
        {
            StructuredLogger.LogWarning(_logger, "Create shopping list request failed: One or more validation errors occurred.",
             _requestContextService, new Dictionary<string, object> { { "validationErrors", validationResult.Errors.Select(e => e.ErrorMessage) } });

            return ServiceResult<ShoppingListDto>.FailureResult("One or more validation errors occurred.", 400);
        }
        var shoppingList = new ShoppingList
        {
            OwnerId = userId,
            Family = familyName,
        };
        shoppingList.Heading = shoppingListDto.Heading ?? shoppingList.Heading;

        // Find catalog items by SKUs
        if (shoppingListDto.SKUs != null && shoppingListDto.SKUs.Count > 0)
        {
            var catalogItems = await _repo.GetCatalogItemsBySKUsAsync(shoppingListDto.SKUs, familyName);

            if (catalogItems == null || catalogItems.Count == 0)
            {
                StructuredLogger.LogWarning(_logger, "Create Shopping List request failed: No items found with provided SKUs.",
                _requestContextService, new Dictionary<string, object> { { "skus", shoppingListDto.SKUs } });

                return ServiceResult<ShoppingListDto>.FailureResult("No items found with provided SKUs.", 400);
            }

            var shoppingListItems = _mapper.Map<List<ShoppingListItem>>(catalogItems);
            shoppingList.Items.AddRange(shoppingListItems);
        }

        _repo.AddShoppingList(shoppingList);

        ShoppingListDto newShoppingList = _mapper.Map<ShoppingListDto>(shoppingList);

        // public a newly created shopping list to rabbitmq
        await _publishEndpoint.Publish(_mapper.Map<ShoppingListCreated>(newShoppingList), context =>
        {
            context.Headers.Set("OperationId", operationId);
            context.Headers.Set("traceId", traceId);
            context.Headers.Set("requestId", requestId);
        });

        bool result = await _repo.SaveChangesAsync();

        if (!result)
        {
            StructuredLogger.LogError(_logger, "Create Shopping List request failed: Database save error.", _requestContextService);

            return ServiceResult<ShoppingListDto>.FailureResult("Create Shopping List request failed: Database save error.", 500);
        }

        StructuredLogger.LogInformation(_logger, "Create Shopping List request successful.", _requestContextService, new Dictionary<string, object> { { "shoppingListId", shoppingList.Id } });

        return ServiceResult<ShoppingListDto>.SuccessResult(newShoppingList);
    }

    public async Task<ServiceResult<ShoppingListDto>> CreateShoppingListItemAsync(Guid id, CreateShoppingListItemDto items)
    {
        string familyName = _requestContextService.FamilyName;
        string operationId = _requestContextService.OperationId;
        string userId = _requestContextService.UserId;
        string traceId = _requestContextService.TraceId;
        string requestId = _requestContextService.RequestId;

        StructuredLogger.LogInformation(_logger, "Create Shopping List Item request received.", _requestContextService, new Dictionary<string, object> { { "shoppingListId", id } });

        ShoppingList shoppingList = await _repo.GetShoppingListById(id, familyName);

        if (shoppingList == null)
        {
            StructuredLogger.LogWarning(_logger, "Create Shopping List Item request failed: Shopping List not found.", _requestContextService, new Dictionary<string, object> { { "shoppingListId", id } });

            return ServiceResult<ShoppingListDto>.FailureResult("Shopping List not found.", 404);
        }

        List<CatalogItem> catalogItems = new List<CatalogItem>();
        foreach (var sku in items.SKUs)
        {
            var catalogItem = await _repo.GetCatalogItemBySKU(sku, familyName);
            if (catalogItem == null)
            {
                StructuredLogger.LogWarning(_logger, "Create Shopping List Item request failed: Some catalog items not found.", _requestContextService, new Dictionary<string, object> { { "shoppingListId", id } });

                return ServiceResult<ShoppingListDto>.FailureResult("Some catalog items were not found.", 404);
            }
            catalogItems.Add(catalogItem);
        }
        if (catalogItems.Count == 0)
        {
            StructuredLogger.LogError(_logger, "Create Shopping List Item request failed: No valid catalog items found.", _requestContextService, new Dictionary<string, object> { { "shoppingListId", id } });

            return ServiceResult<ShoppingListDto>.FailureResult("No valid catalog items found.", 404);
        }
        foreach (var catalogItem in catalogItems)
        {
            ShoppingListItem shoppingListItem = _mapper.Map<ShoppingListItem>(catalogItem);
            shoppingListItem.ShoppingListId = id;

            _repo.AddShoppingListItem(shoppingListItem);
        }
        // Map the updated shopping list to the DTO
        var shoppingListDto = _mapper.Map<ShoppingListDto>(shoppingList);

        // Publish the message to the message broker    
        await _publishEndpoint.Publish(_mapper.Map<ShoppingListItemsAdded>(shoppingListDto), context =>
        {
            context.Headers.Set("OperationId", operationId);
            context.Headers.Set("traceId", traceId);
            context.Headers.Set("requestId", requestId);
        });

        bool result = await _repo.SaveChangesAsync();
        if (!result)
        {
            StructuredLogger.LogError(_logger, "Create Shopping List Item request failed: Database save error.", _requestContextService, new Dictionary<string, object> { { "shoppingListId", id } });

            return ServiceResult<ShoppingListDto>.FailureResult("Create Shopping List Item request failed: Database save error.", 500);
        }

        StructuredLogger.LogInformation(_logger, "Create Shopping List Item request succeeded.", _requestContextService, new Dictionary<string, object> { { "shoppingListId", id } });
        return ServiceResult<ShoppingListDto>.SuccessResult(shoppingListDto);
    }


    public async Task<ServiceResult<ShoppingListDto>> DeleteShoppingListAsync(Guid id)
    {
        string familyName = _requestContextService.FamilyName;
        string operationId = _requestContextService.OperationId;
        string userId = _requestContextService.UserId;
        string traceId = _requestContextService.TraceId;
        string requestId = _requestContextService.RequestId;

        StructuredLogger.LogInformation(_logger, "Delete Shopping List request received.", _requestContextService, new Dictionary<string, object> { { "shoppingListId", id } });

        ShoppingList shoppingList = await _repo.GetShoppingListById(id, familyName);
        if (shoppingList == null)
        {
            StructuredLogger.LogWarning(_logger, "Delete Shopping List request failed: Shopping List not found.", _requestContextService, new Dictionary<string, object> { { "shoppingListId", id }, });

            return ServiceResult<ShoppingListDto>.FailureResult("Shopping List not found.", 404);
        }
        _repo.DeleteShoppingList(shoppingList);

        // Send the message to the message broker    
        await _publishEndpoint.Publish(_mapper.Map<ShoppingListDeleted>(shoppingList), context =>
        {
            context.Headers.Set("OperationId", operationId);
            context.Headers.Set("traceId", traceId);
            context.Headers.Set("requestId", requestId);
        });

        bool result = await _repo.SaveChangesAsync();
        if (!result)
        {
            StructuredLogger.LogError(_logger, "Delete Shopping List request failed: Database save error.", _requestContextService, new Dictionary<string, object> { { "shoppingListId", id } });

            return ServiceResult<ShoppingListDto>.FailureResult("Delete Shopping List request failed: Database save error.", 500);
        }

        StructuredLogger.LogInformation(_logger, "Delete Shopping List request succeeded.", _requestContextService, new Dictionary<string, object> { { "shoppingListId", id } });
        return ServiceResult<ShoppingListDto>.SuccessResult(_mapper.Map<ShoppingListDto>(shoppingList));
    }


    public async Task<ServiceResult<ShoppingListDto>> DeleteShoppingListItemAsync(Guid id, Guid itemId)
    {
        string familyName = _requestContextService.FamilyName;
        string operationId = _requestContextService.OperationId;
        string userId = _requestContextService.UserId;
        string traceId = _requestContextService.TraceId;
        string requestId = _requestContextService.RequestId;

        StructuredLogger.LogInformation(_logger, "Delete Shopping List Item request received.", _requestContextService, new Dictionary<string, object> { { "shoppingListId", id }, { "itemId", itemId } });

        ShoppingListItem shoppingListItem = await _repo.GetShoppingListItemById(itemId, id, familyName);
        ShoppingList shoppingList = await _repo.GetShoppingListById(id, familyName);
        if (shoppingListItem == null || shoppingListItem == null)
        {
            StructuredLogger.LogWarning(_logger, "Delete Shopping List Item request failed: Shopping List or Item not found.", _requestContextService, new Dictionary<string, object> { { "shoppingListId", id }, { "itemId", itemId }, });

            return ServiceResult<ShoppingListDto>.FailureResult("Shopping List or Item not found.", 404);
        }
        var message = _mapper.Map<ShoppingListItemDeleted>(shoppingListItem);
        _repo.DeleteShoppingListItem(shoppingListItem);

        await _publishEndpoint.Publish(message, context =>
        {
            context.Headers.Set("OperationId", operationId);
            context.Headers.Set("traceId", traceId);
            context.Headers.Set("requestId", requestId);
        });

        bool result = await _repo.SaveChangesAsync();

        if (!result)
        {
            StructuredLogger.LogError(_logger, "Delete Shopping List Item request failed: Database save error.", _requestContextService, new Dictionary<string, object> { { "shoppingListId", id }, { "itemId", itemId }, });

            return ServiceResult<ShoppingListDto>.FailureResult("Delete Shopping List Item request failed: Database save error.", 500);
        }
        StructuredLogger.LogInformation(_logger, "Delete Shopping List Item request succeeded.", _requestContextService, new Dictionary<string, object> { { "shoppingListId", id }, { "itemId", itemId } });
        return ServiceResult<ShoppingListDto>.SuccessResult(_mapper.Map<ShoppingListDto>(shoppingList));
    }

    public async Task<ServiceResult<List<CatalogItemDto>>> GetCatalogItemsAsync()
    {
        string familyName = _requestContextService.FamilyName;
        var result = await _repo.GetCatalogItemsAsync(familyName);
        return ServiceResult<List<CatalogItemDto>>.SuccessResult(_mapper.Map<List<CatalogItemDto>>(result));
    }

    public async Task<ServiceResult<List<CatalogItemDto>>> GetFrequentlyBoughtItemsAsync()
    {
        string familyName = _requestContextService.FamilyName;

        var items = await _repo.GetFrequentlyBoughtItemsAsync(familyName);
        List<CatalogItemDto> result = _mapper.Map<List<CatalogItemDto>>(items);
        return ServiceResult<List<CatalogItemDto>>.SuccessResult(result);

    }

    public async Task<ServiceResult<ShoppingListDto>> GetShoppingListAsync(Guid id)
    {
        string familyName = _requestContextService.FamilyName;
        ShoppingList list = await _repo.GetShoppingListById(id, familyName);

        if (list == null)
        {
            return ServiceResult<ShoppingListDto>.FailureResult("Shopping list not found.", 404);
        }
        return ServiceResult<ShoppingListDto>.SuccessResult(_mapper.Map<ShoppingListDto>(list));
    }

    public async Task<ServiceResult<List<ShoppingListDto>>> GetShoppingListsAsync()
    {
        string familyName = _requestContextService.FamilyName;

        var result = await _repo.GetShoppingListsAsync(familyName);

        return ServiceResult<List<ShoppingListDto>>.SuccessResult(_mapper.Map<List<ShoppingListDto>>(result));
    }

    public async Task<ServiceResult<List<CatalogItemDto>>> SearchCatalogItemsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return ServiceResult<List<CatalogItemDto>>.FailureResult("Query parameter cannot be empty.", 400);
        }
        string familyName = _requestContextService.FamilyName;
        var catalogItems = await _repo.AutocompleteCatalogItemsAsync(query, familyName);

        var catalogItemDtos = _mapper.Map<List<CatalogItemDto>>(catalogItems);

        return ServiceResult<List<CatalogItemDto>>.SuccessResult(catalogItemDtos);
    }

    public async Task<ServiceResult<ShoppingListDto>> UpdateShoppingListAsync(Guid id, UpdateShoppingListDto shoppingListDto)
    {
        string familyName = _requestContextService.FamilyName;
        string operationId = _requestContextService.OperationId;
        string userId = _requestContextService.UserId;
        string traceId = _requestContextService.TraceId;
        string requestId = _requestContextService.RequestId;

        StructuredLogger.LogInformation(_logger, "Update Shopping List request received.", _requestContextService, new Dictionary<string, object> { { "shoppingListId", id }, { "operationId", operationId } });
        // Validate the incoming DTO
        var validator = new UpdateShoppingListDtoValidator();
        var validationResult = validator.Validate(shoppingListDto);

        if (!validationResult.IsValid)
        {
            StructuredLogger.LogWarning(_logger, "Update Shopping List request failed: Invalid data.", _requestContextService,
            new Dictionary<string, object> { { "shoppingListId", id }, { "operationId", operationId }, { "validationErrors", validationResult.Errors.Select(e => e.ErrorMessage) } });

            return ServiceResult<ShoppingListDto>.FailureResult("Update Shopping List request failed: Invalid data.", 400);
        }

        ShoppingList shoppingList = await _repo.GetShoppingListById(id, familyName);

        if (shoppingList == null)
        {
            StructuredLogger.LogWarning(_logger, "Update Shopping List request failed: Shopping List not found.", _requestContextService, new Dictionary<string, object> { { "shoppingListId", id } });

            return ServiceResult<ShoppingListDto>.FailureResult("Shopping List not found.", 404);
        }
        await _repo.UpdateShoppingList(shoppingList, shoppingListDto);

        ShoppingListDto updatedShoppingList = _mapper.Map<ShoppingListDto>(shoppingList);

        await _publishEndpoint.Publish(_mapper.Map<ShoppingListUpdated>(updatedShoppingList), context =>
        {
            context.Headers.Set("OperationId", operationId);
            context.Headers.Set("traceId", traceId);
            context.Headers.Set("requestId", requestId);
        });

        bool result = await _repo.SaveChangesAsync();

        if (!result)
        {
            StructuredLogger.LogError(_logger, "Update Shopping List request failed: Database save error.", _requestContextService, new Dictionary<string, object> { { "shoppingListId", id } });

            return ServiceResult<ShoppingListDto>.FailureResult("Update Shopping List request failed: Database save error.", 500);
        }
        StructuredLogger.LogInformation(_logger, "Update Shopping List request succeeded.", _requestContextService, new Dictionary<string, object> { { "shoppingListId", id } });

        return ServiceResult<ShoppingListDto>.SuccessResult(updatedShoppingList);

    }

    public async Task<ServiceResult<ShoppingListDto>> UpdateShoppingListItemAsync(Guid id, Guid itemId, UpdateShoppingListItemDto updateShoppingListItemDto)
    {
        StructuredLogger.LogInformation(_logger, "Update Shopping List Item request received.", _requestContextService, new Dictionary<string, object> { { "shoppingListId", id }, { "itemId", itemId } });

        string familyName = _requestContextService.FamilyName;
        string operationId = _requestContextService.OperationId;
        string userId = _requestContextService.UserId;
        string traceId = _requestContextService.TraceId;
        string requestId = _requestContextService.RequestId;

        // Validate the DTO using FluentValidation
        var validator = new UpdateShoppingListItemDtoValidator();
        var validationResult = validator.Validate(updateShoppingListItemDto);

        if (!validationResult.IsValid)
        {
            StructuredLogger.LogWarning(_logger, "Update Shopping List Item request failed: Validation Error.", _requestContextService, new Dictionary<string, object> { { "shoppingListId", id }, { "itemId", itemId }, { "validationErrors", validationResult.Errors.Select(e => e.ErrorMessage) } });

            return ServiceResult<ShoppingListDto>.FailureResult("Update Shopping List Item request failed: Validation Error.", 400);
        }

        ShoppingList updatedShoppingList = await _repo.GetShoppingListById(id, familyName);
        ShoppingListItem shoppingListItem = await _repo.GetShoppingListItemById(itemId, id, familyName);

        if (shoppingListItem == null || updatedShoppingList == null)
        {
            StructuredLogger.LogWarning(_logger, "Update Shopping List Item request failed: Shopping List or Item not found.", _requestContextService, new Dictionary<string, object> { { "shoppingListId", id }, { "itemId", itemId }, });
            return ServiceResult<ShoppingListDto>.FailureResult("Shopping List or Item not found.", 404);
        }

        _repo.UpdateShoppingListItem(shoppingListItem, updateShoppingListItemDto);

        // Map the updated shopping list to the DTO
        var shoppingListDto = _mapper.Map<ShoppingListDto>(updatedShoppingList);

        await _publishEndpoint.Publish(_mapper.Map<ShoppingListItemUpdated>(shoppingListDto), context =>
        {
            context.Headers.Set("OperationId", operationId);
            context.Headers.Set("traceId", traceId);
            context.Headers.Set("requestId", requestId);
        });

        bool result = await _repo.SaveChangesAsync();

        if (!result)
        {
            StructuredLogger.LogError(_logger, "Update Shopping List Item request failed: Database save error.", _requestContextService, new Dictionary<string, object> { { "shoppingListId", id }, { "itemId", itemId }, });

            return ServiceResult<ShoppingListDto>.FailureResult("Update Shopping List Item request failed: Database save error.", 500);
        }

        StructuredLogger.LogInformation(_logger, "Update Shopping List Item request succeeded.", _requestContextService, new Dictionary<string, object> { { "shoppingListId", id }, { "itemId", itemId } });

        return ServiceResult<ShoppingListDto>.SuccessResult(shoppingListDto);
    }
}
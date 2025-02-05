using System.Net.Http;
using AutoMapper;
using CatalogService.RequestHelpers;
using Contracts.ShoppingLists;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ShoppingListService.Data;
using ShoppingListService.DTOs;
using ShoppingListService.Entities;
using ShoppingListService.Helpers;

namespace ShoppingListService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ShoppingListsController : ControllerBase
    {
        private readonly ShoppingListContext _context;
        private readonly IMapper _mapper;
        private readonly IShoppingListService _shoppingListService;
        private readonly string _familyName;
        private readonly string _userId;
        private IPublishEndpoint _publisher;
        private readonly ILogger<ShoppingListsController> _logger;
        private readonly string _operationId;
        private ProblemDetailsFactory _problemDetailsFactory;
        private readonly IHttpContextAccessor _httpContext;
        public ShoppingListsController(ShoppingListContext context, IMapper mapper, IShoppingListService service, IHttpContextAccessor httpContextAccessor, IPublishEndpoint publishEndpoint, ILogger<ShoppingListsController> logger, ProblemDetailsFactory problemDetailsFactory)
        {
            _mapper = mapper;
            _context = context;
            _shoppingListService = service;
            _publisher = publishEndpoint;
            _httpContext = httpContextAccessor;
            _logger = logger;
            _problemDetailsFactory = problemDetailsFactory;
            // Centralized family and user ID extraction
            _familyName = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "family")?.Value;
            _userId = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            _operationId = httpContextAccessor.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity.TraceId.ToString();

            if (string.IsNullOrEmpty(_familyName) || string.IsNullOrEmpty(_userId))
            {
                throw new UnauthorizedAccessException("Family or user information is missing.");
            }
        }

        [HttpGet("catalogitems")]
        public async Task<ActionResult<List<CatalogItemDto>>> GetCatalogItems()
        {
            var result = await _shoppingListService.GetCatalogItemsAsync(_familyName);

            if (result.Count > 0)
            {
                List<CatalogItemDto> items = _mapper.Map<List<CatalogItemDto>>(result);
                return Ok(items);
            }
            return Ok(new List<CatalogItemDto>());

        }

        [HttpPost]
        public async Task<ActionResult<ShoppingListDto>> CreateShoppingList(CreateShoppingListDto shoppingListDto)
        {
            StructuredLogger.LogInformation(_logger, HttpContext,
            "Create Shopping List request received.",
            _userId,
            _familyName,
            new Dictionary<string, object>
            {
                { "operationId", _operationId },
            });
            // Create a new shopping list with default properties
            var shoppingList = new ShoppingList
            {
                OwnerId = _userId,
                Family = _familyName,
            };

            if (shoppingListDto != null)
            {
                var validator = new CreateShoppingListDtoValidator();
                var validationResult = validator.Validate(shoppingListDto);
                if (!validationResult.IsValid)
                {
                    StructuredLogger.LogWarning(_logger, HttpContext,
                    "Create Shopping List request failed: Invalid data.",
                    _userId,
                    _familyName,
                    new Dictionary<string, object>
                    {
                        { "validationErrors", validationResult.Errors.Select(e => e.ErrorMessage) }
                    });

                    var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
                        HttpContext,
                        StatusCodes.Status400BadRequest,
                        "Invalid data.",
                        "One or more validation errors occurred.",
                        _problemDetailsFactory);

                    BadRequest(problemDetails);
                }
                shoppingList.Heading = shoppingListDto.Heading ?? shoppingList.Heading;
                // Find catalog items by SKUs
                if (shoppingListDto.SKUs != null && shoppingListDto.SKUs.Count > 0)
                {
                    var catalogItems = await _shoppingListService.GetCatalogItemsBySKUsAsync(shoppingListDto.SKUs, _familyName);

                    if (catalogItems == null || catalogItems.Count == 0)
                    {
                        StructuredLogger.LogWarning(_logger, HttpContext,
                        "Create Shopping List request failed: No items found with provided SKUs.",
                        _userId,
                        _familyName,
                        new Dictionary<string, object>
                        {
                            { "skus", shoppingListDto.SKUs }
                        });

                        var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
                            HttpContext,
                            StatusCodes.Status400BadRequest,
                            "Invalid SKUs",
                            "No catalog items found for the provided SKUs.",
                            _problemDetailsFactory);
                        return BadRequest(problemDetails);
                    }

                    var shoppingListItems = _mapper.Map<List<ShoppingListItem>>(catalogItems);
                    shoppingList.Items.AddRange(shoppingListItems);
                }
            }

            _shoppingListService.AddShoppingList(shoppingList);

            ShoppingListDto newShoppingList = _mapper.Map<ShoppingListDto>(shoppingList);

            // public a newly created shopping list to rabbitmq
            await _publisher.Publish(_mapper.Map<ShoppingListCreated>(newShoppingList), context =>
            {
                context.Headers.Set("OperationId", _operationId);
                context.Headers.Set("traceId", _operationId);
                context.Headers.Set("requestId", _httpContext.HttpContext.TraceIdentifier);
            });

            bool result = await _shoppingListService.SaveChangesAsync();

            if (!result)
            {
                StructuredLogger.LogError(_logger, HttpContext,
                "Create Shopping List request failed: Database save error.",
                _userId,
                _familyName,
                new Dictionary<string, object> { });

                var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
                    HttpContext,
                    StatusCodes.Status400BadRequest,
                    "Database Error",
                    "Could not save changes to the database while creating the shopping list.",
                    _problemDetailsFactory);
                return BadRequest(problemDetails);
            }
            ;
            StructuredLogger.LogInformation(_logger, HttpContext,
                "Create Shopping List request succeeded.",
                _userId,
                _familyName,
                new Dictionary<string, object>
                {
                    { "shoppingListId", shoppingList.Id }
                });
            return CreatedAtAction(nameof(GetShoppingList), new
            {
                shoppingList.Id
            }, _mapper.Map<ShoppingListDto>(shoppingList));
        }

        [HttpGet]
        public async Task<ActionResult<List<ShoppingListDto>>> GetShoppingLists()
        {
            var result = await _shoppingListService.GetShoppingListsAsync(_familyName);

            List<ShoppingListDto> shoppingLists;

            if (result.Count > 0)
            {
                shoppingLists = _mapper.Map<List<ShoppingListDto>>(result);
                return Ok(shoppingLists);
            }

            return Ok(new List<ShoppingListDto>());

        }

        // Get shopping list by Id
        [HttpGet("{id}")]
        public async Task<ActionResult<ShoppingListDto>> GetShoppingList(Guid id)
        {
            ShoppingList list = await _shoppingListService.GetShoppingListById(id, _familyName);

            if (list == null)
            {
                var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
                    HttpContext,
                    StatusCodes.Status404NotFound,
                    "Shopping List Not Found",
                    $"The shopping list with ID {id} was not found.",
                    _problemDetailsFactory
                );
                return NotFound(problemDetails);
            }
            return Ok(_mapper.Map<ShoppingListDto>(list));
        }

        // Update shopping list
        [HttpPut("{id}")]
        public async Task<ActionResult<ShoppingListDto>> UpdateShoppingList(Guid id, UpdateShoppingListDto shoppingListDto)
        {
            StructuredLogger.LogInformation(_logger, HttpContext,
            "Update Shopping List request received.",
            _userId,
            _familyName,
            new Dictionary<string, object>
            {
                { "shoppingListId", id },
                { "operationId", _operationId }
            });
            // Validate the incoming DTO
            var validator = new UpdateShoppingListDtoValidator();
            var validationResult = validator.Validate(shoppingListDto);

            if (!validationResult.IsValid)
            {
                StructuredLogger.LogWarning(_logger, HttpContext,
                "Update Shopping List request failed: Invalid data.",
                _userId,
                _familyName,
                new Dictionary<string, object>
                {
                    { "shoppingListId", id },
                    { "operationId", _operationId },
                    { "validationErrors", validationResult.Errors.Select(e => e.ErrorMessage) }
                });

                var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
                    HttpContext,
                    StatusCodes.Status400BadRequest,
                    "Invalid Data",
                    "One or more validation errors occurred.",
                    _problemDetailsFactory
                );
                return BadRequest(problemDetails);
            }

            try
            {
                ShoppingList shoppingList = await _shoppingListService.GetShoppingListById(id, _familyName);

                if (shoppingList == null)
                {
                    StructuredLogger.LogWarning(_logger, HttpContext,
                    "Update Shopping List request failed: Shopping List not found.",
                    _userId,
                    _familyName,
                    new Dictionary<string, object>
                    {
                        { "shoppingListId", id }
                    });

                    var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
                        HttpContext,
                        StatusCodes.Status404NotFound,
                        "Shopping List Not Found",
                        $"Cannot find the shopping list with ID {id}.",
                        _problemDetailsFactory
                    );
                    return NotFound(problemDetails);
                }
                await _shoppingListService.UpdateShoppingList(shoppingList, shoppingListDto);

                ShoppingListDto updatedShoppingList = _mapper.Map<ShoppingListDto>(shoppingList);

                await _publisher.Publish(_mapper.Map<ShoppingListUpdated>(updatedShoppingList), context =>
                {
                    context.Headers.Set("OperationId", _operationId);
                    context.Headers.Set("traceId", _operationId);
                    context.Headers.Set("requestId", _httpContext.HttpContext.TraceIdentifier);
                });

                bool result = await _shoppingListService.SaveChangesAsync();

                if (!result)
                {
                    StructuredLogger.LogError(_logger, HttpContext,
                    "Update Shopping List request failed: Database save error.",
                    _userId,
                    _familyName,
                    new Dictionary<string, object>
                    {
                        { "shoppingListId", id }
                    });

                    var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
                        HttpContext,
                        StatusCodes.Status400BadRequest,
                        "Database Error",
                        "Could not save changes to the database while updating the shopping list.",
                        _problemDetailsFactory
                    );

                    return BadRequest(problemDetails);
                }
                StructuredLogger.LogInformation(_logger, HttpContext,
                "Update Shopping List request succeeded.",
                _userId,
                _familyName,
                new Dictionary<string, object>
                {
                    { "shoppingListId", id }
                });

                return Ok(updatedShoppingList);
            }
            catch (Exception ex)
            {
                StructuredLogger.LogError(_logger, HttpContext,
                "Unhandled exception during Update Shopping List request.",
                _userId,
                _familyName,
                new Dictionary<string, object>
                {
                    { "shoppingListId", id },
                    { "exception", ex.Message },
                    { "operationId", _operationId },
                    { "stackTrace", ex.StackTrace ?? "No StackTrace" }
                });

                var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
                    HttpContext,
                    StatusCodes.Status400BadRequest,
                    "Internal Server Error",
                    "An unexpected error occurred while updating the shopping list.",
                    _problemDetailsFactory
                    );
                return BadRequest(problemDetails);
            }

        }

        // Create a new shopping list item within the shopping list
        [HttpPost("{id}/items")]
        public async Task<ActionResult<ShoppingListDto>> CreateShoppingListItem(Guid id, CreateShoppingListItemDto items)
        {
            StructuredLogger.LogInformation(_logger, HttpContext,
            "Create Shopping List Item request received.",
            _userId, _familyName,
            new Dictionary<string, object>
            {
                { "shoppingListId", id }
            });
            try
            {
                ShoppingList shoppingList = await _shoppingListService.GetShoppingListById(id, _familyName);

                if (shoppingList == null)
                {
                    StructuredLogger.LogWarning(_logger, HttpContext,
                    "Create Shopping List Item request failed: Shopping List not found.",
                    _userId,
                    _familyName,
                    new Dictionary<string, object>
                    {
                    { "shoppingListId", id }
                    });

                    var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
                        HttpContext,
                        StatusCodes.Status404NotFound,
                        "Shopping List Not Found",
                        $"Cannot find the shopping list with ID {id} to add a new item.",
                        _problemDetailsFactory
                    );
                    return NotFound(problemDetails);
                }

                List<CatalogItem> catalogItems = new List<CatalogItem>();
                foreach (var sku in items.SKUs)
                {
                    var catalogItem = await _shoppingListService.GetCatalogItemBySKU(sku, _familyName);
                    if (catalogItem == null)
                    {
                        StructuredLogger.LogWarning(_logger, HttpContext,
                        "Create Shopping List Item request failed: Some catalog items not found.",
                        _userId,
                        _operationId,
                        new Dictionary<string, object>
                        {
                        { "shoppingListId", id }
                        });

                        var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
                            HttpContext,
                            StatusCodes.Status404NotFound,
                            "Catalog Items Not Found",
                            "Some catalog items could not be found.",
                            _problemDetailsFactory
                            );
                        return NotFound(problemDetails);
                    }
                    catalogItems.Add(catalogItem);
                }
                if (catalogItems.Count == 0)
                {
                    StructuredLogger.LogError(_logger, HttpContext,
                    "Create Shopping List Item request failed: No valid catalog items found.",
                    _userId,
                    _familyName,
                    new Dictionary<string, object>
                    {
                    { "shoppingListId", id }
                    });

                    var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
                        HttpContext,
                        StatusCodes.Status400BadRequest,
                        "Invalid SKUs",
                        "No catalog items found for the provided SKUs.",
                        _problemDetailsFactory
                    );

                    return BadRequest(problemDetails);
                }
                foreach (var catalogItem in catalogItems)
                {
                    ShoppingListItem shoppingListItem = _mapper.Map<ShoppingListItem>(catalogItem);
                    shoppingListItem.ShoppingListId = id;

                    _shoppingListService.AddShoppingListItem(shoppingListItem);
                }
                // Map the updated shopping list to the DTO
                var shoppingListDto = _mapper.Map<ShoppingListDto>(shoppingList);

                // Publish the message to the message broker    
                await _publisher.Publish(_mapper.Map<ShoppingListItemsAdded>(shoppingListDto), context =>
                {
                    context.Headers.Set("OperationId", _operationId);
                    context.Headers.Set("traceId", _operationId);
                    context.Headers.Set("requestId", _httpContext.HttpContext.TraceIdentifier);
                });

                bool result = await _shoppingListService.SaveChangesAsync();
                if (!result)
                {
                    StructuredLogger.LogError(_logger, HttpContext,
                    "Create Shopping List Item request failed: Database save error.",
                    _userId,
                    _operationId,
                    new Dictionary<string, object>
                    {
                    { "shoppingListId", id }
                    });

                    var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
                        HttpContext,
                        StatusCodes.Status400BadRequest,
                        "Database Error",
                        "Could not save changes to the database while adding items to the shopping list.",
                        _problemDetailsFactory
                    );
                    return BadRequest(problemDetails);
                }

                StructuredLogger.LogInformation(_logger, HttpContext,
                "Create Shopping List Item request succeeded.",
                _userId,
                _familyName,
                new Dictionary<string, object>
                {
                { "shoppingListId", id }
                });
                return Ok(shoppingListDto);
            }
            catch (Exception ex)
            {
                StructuredLogger.LogError(_logger, HttpContext,
                "Unhandled exception during Create Shopping List Item request.",
                _userId,
                _familyName,
                new Dictionary<string, object>
                {
                    { "shoppingListId", id },
                    { "exception", ex.Message },
                    { "operationId", _operationId },
                    { "stackTrace", ex.StackTrace ?? "No StackTrace" }
                });

                var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
                    HttpContext,
                    StatusCodes.Status500InternalServerError,
                    "Internal Server Error",
                    "An unexpected error occurred while adding items to the shopping list.",
                    _problemDetailsFactory
                );

                return StatusCode(StatusCodes.Status500InternalServerError, problemDetails);
            }
        }

        // Update the item in the shopping list
        [HttpPut("{id}/items/{itemId}")]
        public async Task<ActionResult<ShoppingListDto>> UpdateShoppingListItem(Guid id, Guid itemId, UpdateShoppingListItemDto updateShoppingListItemDto)
        {
            StructuredLogger.LogInformation(_logger, HttpContext,
            "Update Shopping List Item request received.",
            _userId,
            _familyName,
            new Dictionary<string, object>
            {
                { "shoppingListId", id },
                { "itemId", itemId }
            });
            // Validate the DTO using FluentValidation
            var validator = new UpdateShoppingListItemDtoValidator();
            var validationResult = validator.Validate(updateShoppingListItemDto);

            if (!validationResult.IsValid)
            {
                StructuredLogger.LogWarning(_logger, HttpContext,
                "Update Shopping List Item request failed: Validation Error.",
                _userId, _familyName,
                new Dictionary<string, object>
                {
                    { "shoppingListId", id },
                    { "itemId", itemId },
                    { "validationErrors", validationResult.Errors.Select(e => e.ErrorMessage) }
                });

                var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
                    HttpContext,
                    StatusCodes.Status400BadRequest,
                    "Invalid Data",
                    "One or more validation errors occurred.",
                    _problemDetailsFactory
                  );
                return BadRequest(problemDetails);
            }

            try
            {
                ShoppingList updatedShoppingList = await _shoppingListService.GetShoppingListById(id, _familyName);
                ShoppingListItem shoppingListItem = await _shoppingListService.GetShoppingListItemById(itemId, id, _familyName);

                if (shoppingListItem == null || updatedShoppingList == null)
                {
                    StructuredLogger.LogWarning(_logger, HttpContext,
                    "Update Shopping List Item request failed: Shopping List or Item not found.",
                    _userId,
                    _familyName,
                    new Dictionary<string, object>
                    {
                        { "shoppingListId", id },
                        { "itemId", itemId },
                    });

                    var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
                        HttpContext,
                        StatusCodes.Status404NotFound,
                        "Shopping List or Item not found",
                        $"Cannot find the shopping list with ID {id} or the item with ID {itemId}.",
                        _problemDetailsFactory
                    );
                    return NotFound(problemDetails);
                }

                _shoppingListService.UpdateShoppingListItem(shoppingListItem, updateShoppingListItemDto);

                // Map the updated shopping list to the DTO
                var shoppingListDto = _mapper.Map<ShoppingListDto>(updatedShoppingList);

                await _publisher.Publish(_mapper.Map<ShoppingListItemUpdated>(shoppingListDto), context =>
                {
                    context.Headers.Set("OperationId", _operationId);
                    context.Headers.Set("traceId", _operationId);
                    context.Headers.Set("requestId", _httpContext.HttpContext.TraceIdentifier);
                });

                bool result = await _shoppingListService.SaveChangesAsync();

                if (!result)
                {
                    StructuredLogger.LogError(_logger, HttpContext,
                    _userId,
                    _familyName,
                    "Update Shopping List Item request failed: Database save error.",
                    new Dictionary<string, object>
                    {
                        { "shoppingListId", id },
                        { "itemId", itemId },
                    });

                    var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
                        HttpContext,
                        StatusCodes.Status400BadRequest,
                        "Database Error",
                        "Could not save changes to the database while updating the shopping list item.",
                        _problemDetailsFactory
                    );
                    return BadRequest(problemDetails);
                }
                StructuredLogger.LogInformation(_logger, HttpContext,
                "Update Shopping List Item request succeeded.",
                _userId,
                _familyName,
                new Dictionary<string, object>
                {
                    { "shoppingListId", id },
                    { "itemId", itemId },
                });
                return Ok(shoppingListDto);
            }
            catch (Exception ex)
            {
                StructuredLogger.LogError(_logger, HttpContext,
                "Unhandled exception during Update Shopping List Item request.",
                _userId,
                _familyName,
                new Dictionary<string, object>
                {
                    { "shoppingListId", id },
                    { "itemId", itemId },
                    { "exception", ex.Message },
                    { "operationId", _operationId },
                    { "stackTrace", ex.StackTrace ?? "No StackTrace" }
                });

                var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
                    HttpContext,
                    StatusCodes.Status500InternalServerError,
                    "Internal Server Error",
                    "An unexpected error occurred while updating the shopping list item.",
                    _problemDetailsFactory
                );
                return BadRequest(problemDetails);
            }
        }

        // Delete shopping list
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShoppingList(Guid id)
        {
            StructuredLogger.LogInformation(_logger, HttpContext,
            "Delete Shopping List request received.",
            _userId,
            _familyName,
            new Dictionary<string, object>
            {
                { "shoppingListId", id }
            });

            try
            {
                ShoppingList shoppingList = await _shoppingListService.GetShoppingListById(id, _familyName);
                if (shoppingList == null)
                {
                    StructuredLogger.LogWarning(_logger, HttpContext,
                    "Delete Shopping List request failed: Shopping List not found.",
                    _userId,
                    _familyName,
                    new Dictionary<string, object>
                    {
                        { "shoppingListId", id },
                    });

                    var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
                        HttpContext,
                        StatusCodes.Status404NotFound,
                        "Shopping List Not Found",
                        $"Cannot find the shopping list with ID {id} to delete.",
                        _problemDetailsFactory
                    );
                    return NotFound(problemDetails);
                }
                _shoppingListService.DeleteShoppingList(shoppingList);

                // Send the message to the message broker    
                await _publisher.Publish(_mapper.Map<ShoppingListDeleted>(shoppingList), context =>
                {
                    context.Headers.Set("OperationId", _operationId);
                    context.Headers.Set("traceId", _operationId);
                    context.Headers.Set("requestId", _httpContext.HttpContext.TraceIdentifier);
                });

                bool result = await _shoppingListService.SaveChangesAsync();
                if (!result)
                {
                    StructuredLogger.LogError(_logger, HttpContext,
                    "Delete Shopping List request failed: Database save error.",
                    _userId,
                    _familyName,
                    new Dictionary<string, object>
                    {
                        { "shoppingListId", id }
                    });

                    var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
                        HttpContext,
                        StatusCodes.Status400BadRequest,
                        "Database Error",
                        "Could not save changes to the database while deleting the shopping list.",
                        _problemDetailsFactory
                    );
                    return BadRequest(problemDetails);
                }

                StructuredLogger.LogInformation(_logger, HttpContext,
                "Delete Shopping List request succeeded.",
                _userId, _familyName,
                new Dictionary<string, object>
                {
                    { "shoppingListId", id }
                });
                return NoContent();
            }
            catch (Exception ex)
            {

                StructuredLogger.LogError(_logger, HttpContext,
                "Unhandled exception during Delete Shopping List request.",
                _userId, _familyName,
                new Dictionary<string, object>
                {
                    { "shoppingListId", id },
                    { "exception", ex.Message },
                    { "operationId", _operationId },
                    { "stackTrace", ex.StackTrace ?? "No StackTrace" }
                });

                var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
                    HttpContext,
                    StatusCodes.Status400BadRequest,
                    "Internal Server Error",
                    "An unexpected error occurred while deleting the shopping list.",
                    _problemDetailsFactory
                );
                return BadRequest(problemDetails);
            }
        }

        // Delete shopping list item 
        [HttpDelete("{id}/items/{itemId}")]
        public async Task<ActionResult> DeleteShoppingListItem(Guid id, Guid itemId)
        {

            StructuredLogger.LogInformation(_logger, HttpContext,
            "Delete Shopping List Item request received.",
            _userId,
            _familyName,
            new Dictionary<string, object>
            {
                    { "shoppingListId", id },
                    { "itemId", itemId }
            });
            try
            {
                ShoppingListItem shoppingListItem = await _shoppingListService.GetShoppingListItemById(itemId, id, _familyName);
                ShoppingList shoppingList = await _shoppingListService.GetShoppingListById(id, _familyName);
                if (shoppingListItem == null || shoppingListItem == null)
                {
                    StructuredLogger.LogWarning(_logger, HttpContext,
                    "Delete Shopping List Item request failed: Shopping List or Item not found.",
                    _userId,
                    _familyName,
                    new Dictionary<string, object>
                    {
                        { "shoppingListId", id },
                        { "itemId", itemId },
                    });

                    var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
                        HttpContext,
                        StatusCodes.Status404NotFound,
                        "Shopping List or Item not found",
                        $"Cannot find the shopping list with ID {id} or the item with ID {itemId}.",
                        _problemDetailsFactory
                    );
                    return NotFound(problemDetails);
                }
                var message = _mapper.Map<ShoppingListItemDeleted>(shoppingListItem);
                _shoppingListService.DeleteShoppingListItem(shoppingListItem);

                await _publisher.Publish(message, context =>
                {
                    context.Headers.Set("OperationId", _operationId);
                    context.Headers.Set("traceId", _operationId);
                    context.Headers.Set("requestId", _httpContext.HttpContext.TraceIdentifier);
                });

                bool result = await _shoppingListService.SaveChangesAsync();

                if (!result)
                {
                    StructuredLogger.LogError(_logger, HttpContext,
                    "Delete Shopping List Item request failed: Database save error.",
                    _userId,
                    _familyName,
                    new Dictionary<string, object>
                    {
                        { "shoppingListId", id },
                        { "itemId", itemId },
                    });

                    var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
                        HttpContext,
                        StatusCodes.Status400BadRequest,
                        "Database Error",
                        "Could not save changes to the database while deleting the shopping list item.",
                        _problemDetailsFactory
                    );
                    return BadRequest(problemDetails);
                }
                StructuredLogger.LogInformation(_logger, HttpContext,
                "Delete Shopping List Item request succeeded.",
                _userId, _familyName,
                new Dictionary<string, object>
                {
                    { "shoppingListId", id },
                    { "itemId", itemId }
                });
                return NoContent();
            }
            catch (Exception ex)
            {
                StructuredLogger.LogError(_logger, HttpContext,
                "Unhandled exception during Delete Shopping List Item request.",
                _userId, _familyName,
                new Dictionary<string, object>
                {
                    { "shoppingListId", id },
                    { "itemId", itemId },
                    { "exception", ex.Message },
                    { "operationId", _operationId },
                    { "stackTrace", ex.StackTrace ?? "No StackTrace" }
                });

                var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
                    HttpContext,
                    StatusCodes.Status400BadRequest,
                    "Internal Server Error",
                    "An unexpected error occurred while deleting the shopping list item.",
                    _problemDetailsFactory
                );

                return BadRequest(problemDetails);
            }
        }

        // Get frequently bought catalog items
        [HttpGet("catalogitems/freq-bought")]
        public async Task<ActionResult<List<CatalogItemDto>>> GetFrequentlyBoughtItems()
        {
            var items = await _shoppingListService.GetFrequentlyBoughtItemsAsync(_familyName);

            if (items.Count > 0)
            {
                List<CatalogItemDto> result = _mapper.Map<List<CatalogItemDto>>(items);
                return Ok(result);
            }
            return Ok(new List<CatalogItemDto>());
        }

        [HttpGet("catalogitems/search")]
        public async Task<ActionResult<List<CatalogItemDto>>> SearchCatalogItems([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                StructuredLogger.LogWarning(_logger, HttpContext,
                "Search Catalog Items request failed: Query parameter is empty.",
                _userId, _familyName,
                new Dictionary<string, object> { });

                var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
                    HttpContext,
                    StatusCodes.Status400BadRequest,
                    "Invalid Query",
                    "Query parameter cannot be empty.",
                    _problemDetailsFactory
                );
                return BadRequest(problemDetails);
            }

            var catalogItems = await _shoppingListService.AutocompleteCatalogItemsAsync(query, _familyName);

            var catalogItemDtos = _mapper.Map<List<CatalogItemDto>>(catalogItems);

            return Ok(catalogItemDtos);
        }

    }
}

using System.Diagnostics;
using AutoMapper;
using AutoMapper.Configuration.Annotations;
using CatalogService.Data;
using CatalogService.DTOs;
using CatalogService.Entities;
using Contracts.Catalog;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CatalogController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ICatalogRepository _repo;
        private readonly string _familyName;
        private readonly string _userId;
        private readonly IPublishEndpoint _publishEndpoint;
        private ILogger<CatalogController> _logger;
        private IHttpContextAccessor _httpContext;
        private readonly string _operationId;

        public CatalogController(
        IMapper mapper, ICatalogRepository repo,
        IHttpContextAccessor httpContextAccessor,
        IPublishEndpoint publishEndpoint,
        ILogger<CatalogController> logger
        )
        {
            _mapper = mapper;
            _repo = repo;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
            _httpContext = httpContextAccessor;

            // Centralized family and user ID extraction
            _familyName = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "family")?.Value;
            _userId = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            _operationId = httpContextAccessor.HttpContext?.Request.Headers.TraceParent;

            if (string.IsNullOrEmpty(_familyName) || string.IsNullOrEmpty(_userId))
            {
                throw new UnauthorizedAccessException("Family or user information is missing.");
            }
        }

        #region CategoryActions

        [HttpGet("categories")]
        public async Task<ActionResult<List<CategoryDto>>> GetAllCategories()
        {
            return Ok(await _repo.GetAllCategoriesAsync(_familyName));
        }

        [HttpGet("categories/{sku}")]
        public async Task<ActionResult<CategoryDto>> GetCategoryBySku(Guid sku)
        {
            CategoryDto category = await _repo.GetCategoryBySkuAsync(sku, _familyName);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(category);
        }
        [HttpPost("categories")]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto categoryDto)
        {
            _logger.LogInformation($"Create Category request received. Service: Catalog Service. User: {_userId}, Family: {_familyName}, Category: {categoryDto.Name}, OperationId : {_operationId}");
            Category existingCategory = await _repo.GetCategoryEntityByName(categoryDto.Name, _familyName);

            if (existingCategory != null)
            {
                _logger.LogError($"Create Category request failed: Category with name {categoryDto.Name} already exists. Service: Catalog Service, User: {_userId}, Family: {_familyName}, OperationId : {_operationId}");
                return BadRequest("The category with this name already exists.");
            }

            var category = _mapper.Map<Category>(categoryDto);

            category.OwnerId = _userId;
            category.Family = _familyName;

            _repo.AddCategory(category);

            CategoryDto newCategory = _mapper.Map<CategoryDto>(category);

            await _publishEndpoint.Publish(_mapper.Map<CatalogCategoryCreated>(newCategory), context =>
                {
                    context.Headers.Set("OperationId", _operationId);

                });

            var result = await _repo.SaveChangesAsync();

            if (!result)
            {
                _logger.LogError($"Create Category request failed: Cannot save changes to db. Service: Catalog Service, User: {_userId}, Family: {_familyName}, OperationId : {_operationId}");
                return BadRequest("Could not save changes to the DB");

            };
            _logger.LogInformation($"Create Category request succeded:Category {category.Name} created successfully. Service: Catalog Service, User: {_userId}, Family: {_familyName}, OperationId: {_operationId}");
            return Ok(newCategory);
        }

        [HttpPut("categories/{sku}")]
        public async Task<ActionResult<CategoryDto>> UpdateCategory(Guid sku, UpdateCategoryDto categoryDto)
        {
            _logger.LogInformation($"Update Category request received. Service: Catalog Service. User: {_userId}, Family: {_familyName}, Category: {categoryDto.Name}, OperationId : {_operationId}");

            using var transaction = await _repo.BeginTransactionAsync();
            try
            {
                Category category = await _repo.GetCategoryEntityBySku(sku, _familyName);

                if (category == null)
                {
                    _logger.LogError($"Update Category request failed: Category with name {categoryDto.Name} was not found. Service: Catalog Service, User: {_userId}, Family: {_familyName}, Category: {categoryDto.Name}, OperationId : {_operationId}");
                    return NotFound("The category was not found or you are not allowed to update the category created by another user.");
                }

                await _repo.UpdateCategoryAsync(category, categoryDto);

                CategoryDto updatedCategory = _mapper.Map<CategoryDto>(category);

                await _publishEndpoint.Publish(_mapper.Map<CatalogCategoryUpdated>(updatedCategory), context =>
                        {
                            context.Headers.Set("OperationId", _operationId);
                        });

                bool result = await _repo.SaveChangesAsync();

                if (!result)
                {
                    _logger.LogError($"Update Category request failed: Cannot save changes to db. Service: Catalog Service, User: {_userId}, Family: {_familyName}, Category: {categoryDto.Name}, OperationId : {_operationId}");

                    return BadRequest("Problem with updating the category.");
                }

                await transaction.CommitAsync();
                _logger.LogInformation($"Update Category request succeded:Category {category.Name} created successfully. Service: Catalog Service, User: {_userId}, Family: {_familyName}, Category: {categoryDto.Name}, OperationId: {_operationId}");
                return Ok(updatedCategory);

            }
            catch (Exception ex)
            {
                // Rollback the transaction on any exception
                await transaction.RollbackAsync();
                _logger.LogError($"Update Category request failed: transaction problem {ex.Message}. Service: Catalog Service, User: {_userId}, Family: {_familyName}, Category: {categoryDto.Name}, OperationId : {_operationId}");
                return BadRequest("Problem with updating the category.");
            }

        }

        [HttpDelete("categories/{sku}")]
        public async Task<ActionResult> DeleteCategory(Guid sku)
        {
            using var transaction = await _repo.BeginTransactionAsync();
            try
            {
                _logger.LogInformation($"Delete Category request received. Service: Catalog Service. User: {_userId}, Family: {_familyName}, OperationId : {_operationId}");

                Category category = await _repo.GetCategoryEntityBySku(sku, _familyName);

                if (category == null)
                {
                    _logger.LogError($"Delete Category request failed: Category with sku {sku} not found. Service: Catalog Service, User: {_userId}, Family: {_familyName}, OperationId : {_operationId}");
                    return NotFound("Cannot find the category to delete.");
                }
                if (category.Items.Any())
                {
                    _logger.LogWarning($"Delete Category request failed: Category with name {category.Name} is not empty. Service: Catalog Service, User: {_userId}, Family: {_familyName}, OperationId : {_operationId}");
                    return BadRequest("Cannot delete non empty category.");
                }

                category.IsDeleted = true;

                CategoryDto deletedCategory = _mapper.Map<CategoryDto>(category);

                await _publishEndpoint.Publish(_mapper.Map<CatalogCategoryDeleted>(deletedCategory), context =>
                {
                    context.Headers.Set("OperationId", _operationId);

                });

                bool result = await _repo.SaveChangesAsync();
                if (!result)
                {
                    _logger.LogError($"Delete Category request failed: cannot save changes to the db. Service: Catalog Service, User: {_userId}, Family: {_familyName}, OperationId : {_operationId}");
                    return BadRequest("Could not delete category and save changed to the database.");
                };

                _logger.LogInformation($"Delete Category request succeded. Service: Catalog Service, User: {_userId}, Family: {_familyName}, OperationId: {_operationId}");
                await transaction.CommitAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                // Rollback the transaction on any exception
                _logger.LogError($"Delete Category request failed: transaction errror {ex.Message}. Service: Catalog Service, User: {_userId}, Family: {_familyName}, OperationId : {_operationId}");
                await transaction.RollbackAsync();
                return BadRequest("Could not delete category and save changed to the database.");
            }

        }

        #endregion

        #region ItemsActions

        [HttpGet("items")]
        public async Task<ActionResult<List<ItemDto>>> GetAllItems()
        {

            return Ok(await _repo.GetAllItemsAsync(_familyName));

        }

        [HttpGet("items/{sku}")]
        public async Task<ActionResult<ItemDto>> GetItemBySku(Guid sku)
        {

            var item = await _repo.GetItemBySkuAsync(sku, _familyName);

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        [HttpPut("items/{sku}")]

        [HttpPost("items")]
        public async Task<ActionResult<ItemDto>> CreateItem(CreateItemDto itemDto)
        {
            _logger.LogInformation($"Create Item request received. Service: Catalog Service, User: {_userId}, Family: {_familyName}, Item: {itemDto.Name} OperationId : {_operationId}");

            Item existingItem = await _repo.GetItemEntityByNameAsync(itemDto.Name, _familyName);

            if (existingItem != null)
            {
                _logger.LogError($"Create Item request failed: Item with a name {itemDto.Name} already exists. Service: Catalog Service, User: {_userId}, Family: {_familyName}, Item: {itemDto.Name}, OperationId : {_operationId}");

                return BadRequest("The item with this name already exists.");
            }

            Category category = await _repo.GetCategoryEntityBySku(itemDto.CategorySKU, _familyName);

            if (category == null)
            {
                _logger.LogError($"Create Item request failed: Item's category is not found. Service: Catalog Service, User: {_userId}, Family: {_familyName}, Item: {itemDto.Name}, OperationId : {_operationId}");
                return NotFound("Cannot find the category of the newly created item.");
            }

            Item item = _mapper.Map<Item>(itemDto);
            item.OwnerId = _userId;
            item.Family = _familyName;
            item.Category = category;
            item.CategorySKU = category.SKU;
            item.CategoryName = category.Name;

            _repo.AddItem(item);
            // Send a created item to the rabbitmq
            var newItem = _mapper.Map<ItemDto>(item);

            _logger.LogInformation($"Create Item request is sent to the message brocker. Service: Catalog Service,  UserId: {_userId}, Family: {_familyName}, Item: {itemDto.Name}, OperationId : {_operationId}");

            await _publishEndpoint.Publish(_mapper.Map<CatalogItemCreated>(newItem), context =>
            {
                context.Headers.Set("OperationId", _operationId);
            });


            bool result = await _repo.SaveChangesAsync();

            if (!result)
            {
                _logger.LogError($"Create Item request failed: Could not save to the database. Service: Catalog Service, User: {_userId}, Family: {_familyName}, Item: {itemDto.Name}, OperationId : {_operationId}");
                return BadRequest("Could not save changes to the DB.");
            }

            _logger.LogInformation($"Create Item request sucessfully handled. Service: Catalog Service, User: {_userId}, Family: {_familyName}, Item: {itemDto.Name}, OperationId : {_operationId}");
            return Ok(_mapper.Map<ItemDto>(item));
        }
        public async Task<ActionResult<CatalogItemUpdated>> UpdateItem(Guid sku, UpdateItemDto itemDto)
        {
            _logger.LogInformation($"Update Item request received. Service: Catalog Service, User: {_userId}, Family: {_familyName}, Item: {itemDto.Name}, OperationId: {_operationId}");

            using var transaction = await _repo.BeginTransactionAsync();

            try
            {
                Item item = await _repo.GetItemEntityBySkuAsync(sku, _familyName);

                Category category = await _repo.GetCategoryEntityBySku(itemDto.CategorySKU, _familyName);

                if (item == null || category == null)
                {
                    _logger.LogError($"Update Item request failed: Item or Category  is not found. Service: Catalog Service, User: {_userId}, Family: {_familyName}, Item: {itemDto.Name}, OperationId : {_operationId}");
                    return NotFound("The item or category was not found.");
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
                    context.Headers.Set("OperationId", _operationId);
                });

                bool result = await _repo.SaveChangesAsync();

                if (!result)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError($"Update Item request failed: Could not save to the database. Service: Catalog Service, User: {_userId}, Family: {_familyName}, Item: {itemDto.Name}, OperationId : {_operationId}");
                    return BadRequest("Failed to save changes to the database.");
                }

                await transaction.CommitAsync();
                _logger.LogInformation($"Update Item request sucessfully handled. Service: Catalog Service, User: {_userId}, Family: {_familyName}, Item: {itemDto.Name}, OperationId : {_operationId}");

                return Ok(catalogItemUpdated);
            }
            catch (Exception ex)
            {
                // Rollback the transaction on any exception
                await transaction.RollbackAsync();
                _logger.LogError($"Update Item request failed: transaction error {ex.Message}. Service: Catalog Service, User: {_userId}, Family: {_familyName}, Item: {itemDto.Name}, OperationId : {_operationId}");
                return BadRequest("Problem with commiting transaction. Possibly another user tried to change the data.");
            }
        }

        [HttpDelete("items/{sku}")]
        public async Task<ActionResult> DeleteItem(Guid sku)
        {
            _logger.LogInformation($"Delete Item request received. Service: Catalog Service, User: {_userId}, Family: {_familyName}, OperationId: {_operationId}");

            using var transaction = await _repo.BeginTransactionAsync();
            try
            {
                Item item = await _repo.GetItemEntityBySkuAsync(sku, _familyName);

                if (item == null)
                {
                    _logger.LogError($"Delete Item request failed: Item is not found. Service: Catalog Service, User: {_userId}, Family: {_familyName}, OperationId : {_operationId}");
                    return NotFound("Cannot find the item to delete or you are not allowed to deleted the item created by another user.");
                }

                item.IsDeleted = true;

                ItemDto deletedItem = _mapper.Map<ItemDto>(item);

                await _publishEndpoint.Publish(_mapper.Map<CatalogItemDeleted>(deletedItem), context =>
                {
                    context.Headers.Set("OperationId", _operationId);
                });

                bool result = await _repo.SaveChangesAsync();
                if (!result)
                {
                    _logger.LogError($"Delete Item request failed: cannot save to the db. Service: Catalog Service, User: {_userId}, Family: {_familyName}, OperationId : {_operationId}");
                    return BadRequest("Could not delete item and save changed to the database.");
                };
                await transaction.CommitAsync();
                _logger.LogInformation($"Delete Item request sucessfully handled. Service: Catalog Service, User: {_userId}, Family: {_familyName}, OperationId : {_operationId}");
                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"Delete Item request failed: transaction error {ex.Message}. Service: Catalog Service, User: {_userId}, Family: {_familyName}, OperationId : {_operationId}");
                return BadRequest("Problem with commiting transaction. Possibly another user tried to change the data.");

            }
        }

        [HttpGet("items/search")]
        public async Task<ActionResult<List<ItemDto>>> SearchItems([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Query parameter cannot be empty.");
            }

            var items = await _repo.SearchItemsAsync(query, _familyName);

            var catalogItemDtos = _mapper.Map<List<ItemDto>>(items);

            return Ok(catalogItemDtos);
        }
    }

    #endregion
}

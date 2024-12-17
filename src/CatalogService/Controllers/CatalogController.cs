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
        private readonly CatalogDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICatalogRepository _repo;
        private readonly string _familyName;
        private readonly string _userId;
        private readonly IPublishEndpoint _publishEndpoint;

        public CatalogController(CatalogDbContext context,
        IMapper mapper, ICatalogRepository repo,
        IHttpContextAccessor httpContextAccessor,
        IPublishEndpoint publishEndpoint
        )
        {
            _context = context;
            _mapper = mapper;
            _repo = repo;
            _publishEndpoint = publishEndpoint;
            // Centralized family and user ID extraction
            _familyName = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "family")?.Value;
            _userId = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

            if (string.IsNullOrEmpty(_familyName) || string.IsNullOrEmpty(_userId))
            {
                throw new UnauthorizedAccessException("Family or user information is missing.");
            }
        }

        [HttpGet("categories")]
        public async Task<ActionResult<List<CategoryDto>>> GetAllCategories()
        {

            return await _repo.GetAllCategoriesAsync(_familyName);
        }

        [HttpGet("categories/{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategoryById(Guid id)
        {
            CategoryDto category = await _repo.GetCategoryByIdAsync(id, _familyName);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }

        [HttpGet("items")]
        public async Task<ActionResult<List<ItemDto>>> GetAllItems()
        {

            return await _repo.GetAllItemsAsync(_familyName);

        }

        [HttpGet("items/{id}")]
        public async Task<ActionResult<ItemDto>> GetItemById(Guid id)
        {

            var item = await _repo.GetItemByIdAsync(id, _familyName);

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        [HttpPost("categories")]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto categoryDto)
        {

            Category existingCategory = await _repo.GetCategoryEntityByName(categoryDto.Name, _familyName);

            if (existingCategory != null)
            {
                return BadRequest("The category with this name already exists.");
            }

            var category = _mapper.Map<Category>(categoryDto);

            category.OwnerId = _userId;
            category.Family = _familyName;

            _repo.AddCategory(category);

            CategoryDto newCategory = _mapper.Map<CategoryDto>(category);

            await _publishEndpoint.Publish(_mapper.Map<CatalogCategoryCreated>(newCategory));

            var result = await _repo.SaveChangesAsync();

            if (!result) return BadRequest("Could not save changes to the DB");

            return CreatedAtAction(nameof(GetCategoryById), new { category.Id }, newCategory);
        }

        [HttpPost("items")]
        public async Task<ActionResult<ItemDto>> CreateItem(CreateItemDto itemDto)
        {
            Item existingItem = await _repo.GetItemEntityByNameAsync(itemDto.Name, _familyName);

            if (existingItem != null)
            {
                return BadRequest("The item with this name already exists.");
            }

            Category category = await _repo.GetCategoryEntityById(itemDto.CategoryId, _familyName);

            if (category == null)
            {
                return NotFound("Cannot find the category of the newly created item.");
            }

            Item item = _mapper.Map<Item>(itemDto);
            item.OwnerId = _userId;
            item.Family = _familyName;
            item.Category = category;

            _repo.AddItem(item);
            // Send a created item to the rabbitmq
            var newItem = _mapper.Map<ItemDto>(item);

            await _publishEndpoint.Publish(_mapper.Map<CatalogItemCreated>(newItem));


            bool result = await _repo.SaveChangesAsync();

            if (!result) return BadRequest("Could not save changes to the DB.");

            return CreatedAtAction(nameof(GetItemById), new { item.Id }, _mapper.Map<ItemDto>(item));
        }

        [HttpPut("categories/{id}")]
        public async Task<ActionResult<CategoryDto>> UpdateCategory(Guid id, UpdateCategoryDto categoryDto)
        {
            using var transaction = await _repo.BeginTransactionAsync();
            try
            {
                Category category = await _repo.GetCategoryEntityById(id, _familyName);

                if (category == null)
                {
                    return NotFound("The category was not found or you are not allowed to update the category created by another user.");
                }

                category.Name = categoryDto.Name ?? category.Name;

                CategoryDto updatedCategory = _mapper.Map<CategoryDto>(category);

                await _publishEndpoint.Publish(_mapper.Map<CatalogCategoryUpdated>(updatedCategory));

                bool result = await _repo.SaveChangesAsync();

                if (!result) return BadRequest("Problem saving changes");

                await transaction.CommitAsync();
                return Ok(updatedCategory);

            }
            catch (Exception ex)
            {
                // Rollback the transaction on any exception
                await transaction.RollbackAsync();
                return BadRequest("Problem with commiting transaction. Possibly another user tried to change the data.");
            }

        }

        [HttpPut("items/{id}")]
        public async Task<ActionResult<CatalogItemUpdated>> UpdateItem(Guid id, UpdateItemDto itemDto)
        {
            using var transaction = await _repo.BeginTransactionAsync();

            try
            {
                Item item = await _repo.GetItemEntityByIdAsync(id, _familyName);

                Category category = await _repo.GetCategoryEntityById(itemDto.CategoryId, _familyName);

                if (item == null || category == null)
                {
                    return NotFound("The item or category was not found.");
                }

                Guid previousCategoryId = item.CategoryId != itemDto.CategoryId ? item.CategoryId : itemDto.CategoryId;

                await _repo.UpdateItemAsync(item, itemDto);


                CatalogItemUpdated catalogItemUpdated = new CatalogItemUpdated
                {
                    UpdatedItem = _mapper.Map<UpdatedItem>(item),
                    PreviousCategoryId = previousCategoryId
                };
                await _publishEndpoint.Publish(catalogItemUpdated);

                bool result = await _repo.SaveChangesAsync();
                if (!result) return BadRequest("Problem saving changes");
                await transaction.CommitAsync();
                return Ok(catalogItemUpdated);
            }
            catch (Exception ex)
            {
                // Rollback the transaction on any exception
                await transaction.RollbackAsync();
                return BadRequest("Problem with commiting transaction. Possibly another user tried to change the data.");
            }
        }

        [HttpDelete("categories/{id}")]
        public async Task<ActionResult> DeleteCategory(Guid id)
        {

            using var transaction = await _repo.BeginTransactionAsync();
            try
            {
                Category category = await _repo.GetCategoryEntityById(id, _familyName);

                if (category == null)
                {
                    return NotFound("Cannot find the category to delete.");
                }
                if (category.Items.Any())
                {

                    return BadRequest("Cannot delete non empty category.");
                }

                category.IsDeleted = true;

                await _publishEndpoint.Publish(_mapper.Map<CatalogCategoryDeleted>(category));

                bool result = await _repo.SaveChangesAsync();
                if (!result) return BadRequest("Could not delete category and save changed to the database.");
                await transaction.CommitAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                // Rollback the transaction on any exception
                await transaction.RollbackAsync();
                return BadRequest("Problem with commiting transaction. Possibly another user tried to delete the data.");
            }

        }

        [HttpDelete("items/{id}")]
        public async Task<ActionResult> DeleteItem(Guid id)
        {
            using var transaction = await _repo.BeginTransactionAsync();
            try
            {
                Item item = await _repo.GetItemEntityByIdAsync(id, _familyName);

                if (item == null)
                {
                    return NotFound("Cannot find the item to delete or you are not allowed to deleted the item created by another user.");
                }

                item.IsDeleted = true;

                ItemDto deletedItem = _mapper.Map<ItemDto>(item);

                await _publishEndpoint.Publish(_mapper.Map<CatalogItemDeleted>(deletedItem));

                bool result = await _repo.SaveChangesAsync();
                if (!result) return BadRequest("Could not delete item and save changed to the database.");
                await transaction.CommitAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest("Problem with commiting transaction. Possibly another user tried to change the data.");

            }
        }
    }
}

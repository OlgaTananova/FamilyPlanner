using AutoMapper;
using AutoMapper.Configuration.Annotations;
using CatalogService.Data;
using CatalogService.DTOs;
using CatalogService.Entities;
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

        public CatalogController(CatalogDbContext context, IMapper mapper, ICatalogRepository repo)
        {
            _context = context;
            _mapper = mapper;
            _repo = repo;
        }

        protected (string, string) GetFamilyNameAndUserId()
        {
            string familyName = User.Claims.FirstOrDefault(c => c.Type == "family")?.Value;
            string userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            Console.WriteLine($"User details: {familyName} {userId}");
            return new(familyName, userId);
        }

        [HttpGet("categories")]
        public async Task<ActionResult<List<CategoryDto>>> GetAllCategories()
        {
            // Extract family name from user claims
            (string familyName, string userId) = GetFamilyNameAndUserId();

            if (string.IsNullOrEmpty(familyName) || string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Family or user information is missing.");
            }
            return await _repo.GetAllCategoriesAsync(familyName);
        }

        [HttpGet("categories/{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategoryById(Guid id)
        {
            // Extract family name from user claims
            (string familyName, string userId) = GetFamilyNameAndUserId();

            if (string.IsNullOrEmpty(familyName) || string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Family or user information is missing.");
            }

            CategoryDto category = await _repo.GetCategoryByIdAsync(id, familyName);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }

        [HttpGet("items")]
        public async Task<ActionResult<List<ItemDto>>> GetAllItems()
        {
            // Extract family name from user claims
            (string familyName, string userId) = GetFamilyNameAndUserId();

            if (string.IsNullOrEmpty(familyName) || string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Family or user information is missing.");
            }
            return await _repo.GetAllItemsAsync(familyName);

        }

        [HttpGet("items/{id}")]
        public async Task<ActionResult<ItemDto>> GetItemById(Guid id)
        {
            // Extract family name from user claims
            (string familyName, string userId) = GetFamilyNameAndUserId();

            if (string.IsNullOrEmpty(familyName) || string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Family or user information is missing.");
            }
            var item = await _repo.GetItemByIdAsync(id, familyName);

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        [HttpPost("categories")]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto categoryDto)
        {
            // Extract family name from user claims
            (string familyName, string userId) = GetFamilyNameAndUserId();

            if (string.IsNullOrEmpty(familyName) || string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Family or user information is missing.");
            }
            Category existingCategory = await _repo.GetCategoryEntityByName(categoryDto.Name, familyName);

            if (existingCategory != null)
            {
                return BadRequest("The category with this name already exists.");
            }

            var category = _mapper.Map<Category>(categoryDto);

            category.OwnerId = userId;
            category.Family = familyName;

            // Todo add current user as owner
            _repo.AddCategory(category);

            CategoryDto newCategory = _mapper.Map<CategoryDto>(category);

            var result = await _repo.SaveChangesAsync();

            if (!result) return BadRequest("Could not save changes to the DB");

            return CreatedAtAction(nameof(GetCategoryById), new { category.Id }, newCategory);
        }

        [HttpPost("items")]
        public async Task<ActionResult<ItemDto>> CreateItem(CreateItemDto itemDto)
        {
            // Extract family name from user claims
            (string familyName, string userId) = GetFamilyNameAndUserId();

            if (string.IsNullOrEmpty(familyName) || string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Family or user information is missing.");
            }

            Item existingItem = await _repo.GetItemEntityByNameAsync(itemDto.Name, familyName);

            if (existingItem != null)
            {
                return BadRequest("The item with this name already exists.");
            }

            Item item = _mapper.Map<Item>(itemDto);
            item.OwnerId = userId;
            item.Family = familyName;

            _repo.AddItem(item);

            bool result = await _repo.SaveChangesAsync();

            if (!result) return BadRequest("Could not save changes to the DB.");

            return CreatedAtAction(nameof(GetItemById), new { item.Id }, _mapper.Map<ItemDto>(item));
        }

        [HttpPut("categories/{id}")]
        public async Task<ActionResult<CategoryDto>> UpdateCategory(Guid id, UpdateCategoryDto categoryDto)
        {
            // Extract family name from user claims
            (string familyName, string userId) = GetFamilyNameAndUserId();

            if (string.IsNullOrEmpty(familyName) || string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Family or user information is missing.");
            }
            Category category = await _repo.GetCategoryEntityById(id, familyName, userId);

            if (category == null)
            {
                return NotFound("The category was not found or you are not allowed to update the category created by another user.");
            }

            category.Name = categoryDto.Name ?? category.Name;

            bool result = await _repo.SaveChangesAsync();

            if (result) return Ok(_mapper.Map<CategoryDto>(category));

            return BadRequest("Problem saving changes");

        }

        [HttpPut("items/{id}")]
        public async Task<ActionResult<CategoryDto>> UpdateItem(Guid id, UpdateItemDto itemDto)
        {
            // Extract family name from user claims
            (string familyName, string userId) = GetFamilyNameAndUserId();

            if (string.IsNullOrEmpty(familyName) || string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Family or user information is missing.");
            }

            Item item = await _repo.GetItemEntityByIdAsync(id, familyName, userId);

            Category category = await _repo.GetCategoryEntityById(itemDto.CategoryId, familyName, userId);

            if (item == null || category == null)
            {
                return NotFound("The item or category was not found or you are not allowed to update the category created by another user.");
            }

            item.Name = itemDto.Name ?? item.Name;
            item.CategoryId = itemDto.CategoryId;

            if (!category.Items.Contains(item))
            {
                category.Items.Add(item);
            }

            bool result = await _repo.SaveChangesAsync();

            if (result) return Ok(_mapper.Map<ItemDto>(item));

            return BadRequest("Problem saving changes");

        }

        [HttpDelete("categories/{id}")]
        public async Task<ActionResult> DeleteCategory(Guid id)
        {

            // Extract family name from user claims
            (string familyName, string userId) = GetFamilyNameAndUserId();

            if (string.IsNullOrEmpty(familyName) || string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Family or user information is missing.");
            }

            Category category = await _repo.GetCategoryEntityById(id, familyName, userId);

            if (category == null)
            {
                return NotFound("Cannot find the category to delete or you are not allowed to deleted the category created by another user.");
            }
            if (category.Items.Any())
            {

                return BadRequest("Cannot delete non empty category.");
            }

            category.IsDeleted = true;

            bool result = await _repo.SaveChangesAsync();

            if (!result) return BadRequest("Could not delete category and save changed to the database.");

            return NoContent();
        }

        [HttpDelete("items/{id}")]
        public async Task<ActionResult> DeleteItem(Guid id)
        {
            // Extract family name from user claims
            (string familyName, string userId) = GetFamilyNameAndUserId();

            if (string.IsNullOrEmpty(familyName) || string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Family or user information is missing.");
            }

            Item item = await _repo.GetItemEntityByIdAsync(id, familyName, userId);

            if (item == null)
            {
                return NotFound("Cannot find the item to delete or you are not allowed to deleted the item created by another user.");
            }

            item.IsDeleted = true;

            bool result = await _repo.SaveChangesAsync();

            if (!result) return BadRequest("Could not delete item and save changed to the database.");

            return NoContent();
        }
    }
}

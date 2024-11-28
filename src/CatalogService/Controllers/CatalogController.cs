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
        private readonly string _familyName;
        private readonly string _userId;

        public CatalogController(CatalogDbContext context, IMapper mapper, ICatalogRepository repo, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            _repo = repo;
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

            Item item = _mapper.Map<Item>(itemDto);
            item.OwnerId = _userId;
            item.Family = _familyName;

            _repo.AddItem(item);

            bool result = await _repo.SaveChangesAsync();

            if (!result) return BadRequest("Could not save changes to the DB.");

            return CreatedAtAction(nameof(GetItemById), new { item.Id }, _mapper.Map<ItemDto>(item));
        }

        [HttpPut("categories/{id}")]
        public async Task<ActionResult<CategoryDto>> UpdateCategory(Guid id, UpdateCategoryDto categoryDto)
        {
            Category category = await _repo.GetCategoryEntityById(id, _familyName);

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

            Item item = await _repo.GetItemEntityByIdAsync(id, _familyName);

            Category category = await _repo.GetCategoryEntityById(itemDto.CategoryId, _familyName);

            if (item == null || category == null)
            {
                return NotFound("The item or category was not found.");
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

            bool result = await _repo.SaveChangesAsync();

            if (!result) return BadRequest("Could not delete category and save changed to the database.");

            return NoContent();
        }

        [HttpDelete("items/{id}")]
        public async Task<ActionResult> DeleteItem(Guid id)
        {

            Item item = await _repo.GetItemEntityByIdAsync(id, _familyName);

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

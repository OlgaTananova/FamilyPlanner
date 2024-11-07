using AutoMapper;
using AutoMapper.Configuration.Annotations;
using CatalogService.Data;
using CatalogService.DTOs;
using CatalogService.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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

        [HttpGet("categories")]
        public async Task<ActionResult<List<CategoryDto>>> GetAllCategories()
        {

            return await _repo.GetAllCategoriesAsync();
        }

        [HttpGet("categories/{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategoryById(Guid id)
        {
            CategoryDto category = await _repo.GetCategoryByIdAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return Ok(category);
        }

        [HttpGet("items")]
        public async Task<ActionResult<List<ItemDto>>> GetAllItems()
        {
            return await _repo.GetAllItemsAsync();

        }

        [HttpGet("items/{id}")]
        public async Task<ActionResult<ItemDto>> GetItemById(Guid id)
        {
            var item = await _repo.GetItemByIdAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        [HttpPost("categories")]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto categoryDto)
        {
            Category existingCategory = await _repo.GetCategoryEntityByName(categoryDto.Name);

            if (existingCategory != null)
            {
                return BadRequest("The category with this name already exists.");
            }

            var category = _mapper.Map<Category>(categoryDto);

            category.OwnerId = "test";

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
            Item existingItem = await _repo.GetItemEntityByNameAsync(itemDto.Name);
            if (existingItem != null)
            {
                return BadRequest("The item with this name already exists.");
            }

            Item item = _mapper.Map<Item>(itemDto);
            item.OwnerId = "test";
            // Todo add current user as owner

            _repo.AddItem(item);

            bool result = await _repo.SaveChangesAsync();

            if (!result) return BadRequest("Could not save changes to the DB.");

            return CreatedAtAction(nameof(GetItemById), new { item.Id }, _mapper.Map<ItemDto>(item));
        }

        [HttpPut("categories/{id}")]
        public async Task<ActionResult<CategoryDto>> UpdateCategory(Guid id, UpdateCategoryDto categoryDto)
        {
            Category category = await _repo.GetCategoryEntityById(id);

            if (category == null)
            {
                return NotFound("The category was not found");
            }

            // TODO: check ownerId == userId

            category.Name = categoryDto.Name ?? category.Name;

            bool result = await _repo.SaveChangesAsync();

            if (result) return Ok(_mapper.Map<CategoryDto>(category));

            return BadRequest("Problem saving changes");

        }

        [HttpPut("items/{id}")]
        public async Task<ActionResult<CategoryDto>> UpdateItem(Guid id, UpdateItemDto itemDto)
        {
            Item item = await _repo.GetItemEntityByIdAsync(id);

            Category category = await _repo.GetCategoryEntityById(itemDto.CategoryId);

            if (item == null || category == null)
            {
                return NotFound("The item or category was not found");
            }

            // TODO: check ownerId == userId

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
            Category category = await _repo.GetCategoryEntityById(id);

            if (category == null)
            {
                return NotFound("Cannot find the category to delete.");
            }
            if (category.Items.Any())
            {

                return BadRequest("Cannot delete non empty category.");
            }

            // TODO: check the owner id

            category.IsDeleted = true;

            bool result = await _repo.SaveChangesAsync();

            if (!result) return BadRequest("Could not delete category and save changed to the database.");

            return NoContent();
        }

        [HttpDelete("items/{id}")]
        public async Task<ActionResult> DeleteItem(Guid id)
        {
            Item item = await _repo.GetItemEntityByIdAsync(id);

            if (item == null)
            {
                return NotFound("Cannot find the item to delete.");
            }

            // TODO: check the owner id

            item.IsDeleted = true;

            bool result = await _repo.SaveChangesAsync();

            if (!result) return BadRequest("Could not delete item and save changed to the database.");

            return NoContent();
        }
    }
}

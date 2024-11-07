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

        public CatalogController(CatalogDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("categories")]
        public async Task<ActionResult<List<CategoryDto>>> GetAllCategories()
        {

            var categories = await _context.Categories
            .Where(c => !c.IsDeleted)
            .Include(x => x.Items.Where(i => !i.IsDeleted))
            .ToListAsync();
            return _mapper.Map<List<CategoryDto>>(categories);
        }

        [HttpGet("categories/{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategoryById(Guid id)
        {
            var category = await _context.Categories
            .Where(c => !c.IsDeleted)
            .Include(x => x.Items.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return _mapper.Map<CategoryDto>(category);
        }

        [HttpGet("items")]
        public async Task<ActionResult<List<ItemDto>>> GetAllItems()
        {
            var items = await _context.Items
            .Where(c => !c.IsDeleted)
            .Include(x => x.Category)
            .ToListAsync();

            return _mapper.Map<List<ItemDto>>(items);

        }

        [HttpGet("items/{id}")]
        public async Task<ActionResult<ItemDto>> GetItemById(Guid id)
        {
            var item = await _context.Items
            .Where(c => !c.IsDeleted)
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            return _mapper.Map<ItemDto>(item);
        }

        [HttpPost("categories")]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto categoryDto)
        {
            var category = _mapper.Map<Category>(categoryDto);
            category.OwnerId = "test";

            if (category.Name.ToLower() == categoryDto.Name.ToLower())
            {
                return BadRequest("The category with this name already exist.");
            }
            // Todo add current user as owner
            _context.Categories.Add(category);

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Could not save changes to the DB");

            return CreatedAtAction(nameof(GetCategoryById), new { category.Id }, _mapper.Map<CategoryDto>(category));
        }

        [HttpPost("items")]
        public async Task<ActionResult<ItemDto>> CreateItem(CreateItemDto itemDto)
        {
            var item = _mapper.Map<Item>(itemDto);
            item.OwnerId = "test";
            // Todo add current user as owner
            if (_context.Items.Any(x => x.Name.ToLower() == itemDto.Name.ToLower()))
            {
                return BadRequest("The item with this name already exists.");
            }
            _context.Items.Add(item);

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Could not save changes to the DB.");

            return CreatedAtAction(nameof(GetItemById), new { item.Id }, _mapper.Map<ItemDto>(item));
        }

        [HttpPut("categories/{id}")]
        public async Task<ActionResult<CategoryDto>> UpdateCategory(Guid id, UpdateCategoryDto categoryDto)
        {
            var category = await _context.Categories
            .Where(x => !x.IsDeleted)
            .Include(x => x.Items.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == id);

            if (category == null)
            {
                return NotFound("The category was not found");
            }

            // TODO: check ownerId == userId

            category.Name = categoryDto.Name ?? categoryDto.Name;

            var result = await _context.SaveChangesAsync() > 0;

            if (result) return Ok(_mapper.Map<CategoryDto>(category));

            return BadRequest("Problem saving changes");

        }

        [HttpPut("items/{id}")]
        public async Task<ActionResult<CategoryDto>> UpdateItem(Guid id, UpdateItemDto itemDto)
        {
            var item = await _context.Items
            .Where(x => !x.IsDeleted)
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id);

            var category = await _context.Categories
            .Where(x => !x.IsDeleted)
            .Include(x => x.Items.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == itemDto.CategoryId);

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

            var result = await _context.SaveChangesAsync() > 0;

            if (result) return Ok(_mapper.Map<ItemDto>(item));

            return BadRequest("Problem saving changes");

        }

        [HttpDelete("categories/{id}")]
        public async Task<ActionResult> DeleteCategory(Guid id)
        {
            var category = await _context.Categories
            .Where(x => !x.IsDeleted)
            .Include(c => c.Items.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(c => c.Id == id);

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

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Could not delete category and save changed to the database.");

            return NoContent();
        }

        [HttpDelete("items/{id}")]
        public async Task<ActionResult> DeleteItem(Guid id)
        {
            var item = await _context.Items
            .Where(x => !x.IsDeleted)
            .Include(c => c.Category)
            .FirstOrDefaultAsync(c => c.Id == id);

            if (item == null)
            {
                return NotFound("Cannot find the item to delete.");
            }

            // TODO: check the owner id

            item.IsDeleted = true;

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Could not delete item and save changed to the database.");

            return NoContent();
        }
    }
}

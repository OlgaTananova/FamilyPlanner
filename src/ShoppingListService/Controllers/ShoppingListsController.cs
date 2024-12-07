using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShoppingListService.Data;
using ShoppingListService.DTOs;
using ShoppingListService.Entities;

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
        public ShoppingListsController(ShoppingListContext context, IMapper mapper, IShoppingListService service, IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
            _context = context;
            _shoppingListService = service;
            // Centralized family and user ID extraction
            _familyName = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "family")?.Value;
            _userId = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

            if (string.IsNullOrEmpty(_familyName) || string.IsNullOrEmpty(_userId))
            {
                throw new UnauthorizedAccessException("Family or user information is missing.");
            }
        }

        [HttpGet("catalogitems")]
        public async Task<ActionResult<List<CatalogItemDto>>> GetCatalogItems()
        {
            var result = await _shoppingListService.GetCatalogItemsAsync(_familyName);
            return Ok(result);

        }
        // Create a new shopping list
        [HttpPost]
        public async Task<ActionResult<ShoppingListDto>> CreateShoppingList()
        {
            ShoppingList shoppingList = new ShoppingList()
            {
                OwnerId = _userId,
                Family = _familyName,
            };
            _shoppingListService.AddShoppingList(shoppingList);
            bool result = await _shoppingListService.SaveChangesAsync();
            if (!result) return BadRequest("Could not save changes to the DB");
            return CreatedAtAction(nameof(GetShoppingList), new { shoppingList.Id }, shoppingList);
        }

        // Get shopping list by Id
        [HttpGet("{id}")]
        public async Task<ActionResult<ShoppingListDto>> GetShoppingList(Guid id)
        {
            ShoppingList list = await _shoppingListService.GetShoppingListById(id, _familyName);

            if (list == null)
            {
                return NotFound("Cannot find the requested shopping list");
            }
            return Ok(_mapper.Map<ShoppingListDto>(list));
        }

        // Update shopping list
        [HttpPut("{id}")]
        public async Task<ActionResult<ShoppingListDto>> UpdateShoppingList(Guid id, UpdateShoppingListDto shoppingListDto)
        {
            ShoppingList shoppingList = await _shoppingListService.GetShoppingListById(id, _familyName);

            if (shoppingList == null)
            {
                return NotFound($"Cannot find the shopping list with id {id}");
            }
            await _shoppingListService.UpdateShoppingList(shoppingList, shoppingListDto);

            bool result = await _shoppingListService.SaveChangesAsync();

            ShoppingListDto updatedShoppingList = _mapper.Map<ShoppingListDto>(shoppingList);
            if (!result)
            {
                return BadRequest("Cannot save changes to the database.");
            }
            return Ok(updatedShoppingList);

        }

        // Create a new shopping list item within the shopping list
        [HttpPost("{id}/items")]
        public async Task<ActionResult<ShoppingListDto>> CreateShoppingListItem(Guid id, CreateShoppingListItemDto item)
        {
            ShoppingList shoppingList = await _shoppingListService.GetShoppingListById(id, _familyName);
            if (shoppingList == null)
            {
                return NotFound($"Cannot find the shopping list with id {id} to add a new item");
            }
            CatalogItem catalogItem = await _shoppingListService.GetCatalogItemBySKU(item.SKU, _familyName);

            if (catalogItem == null)
            {
                return NotFound($"Cannot find the catalog item wi SKU {item.SKU}.");
            }

            ShoppingListItem shoppingListItem = _mapper.Map<ShoppingListItem>(catalogItem);
            shoppingListItem.ShoppingListId = id;

            _shoppingListService.AddShoppingListItem(shoppingListItem);
            bool result = await _shoppingListService.SaveChangesAsync();

            if (!result)
            {
                return BadRequest("Could not save changes to the DB");
            }
            // Fetch the updated shopping list to include the newly added item
            var updatedShoppingList = await _shoppingListService.GetShoppingListById(id, _familyName);

            // Map the updated shopping list to the DTO
            var shoppingListDto = _mapper.Map<ShoppingListDto>(updatedShoppingList);

            // Return the updated shopping list DTO
            return Ok(shoppingListDto);

        }

        // Update the item in the shopping list
        [HttpPut("{id}/items/{itemId}")]
        public async Task<ActionResult<ShoppingListDto>> UpdateShoppingListItem(Guid id, Guid itemId, UpdateShoppingListItemDto updateShoppingListItemDto)
        {
            ShoppingListItem shoppingListItem = await _shoppingListService.GetShoppingListItemById(itemId, id, _familyName);
            if (shoppingListItem == null)
            {
                return NotFound($"Cannot find the item with id {itemId} within the shopping list with id {id}.");
            }

            _shoppingListService.UpdateShoppingListItem(shoppingListItem, updateShoppingListItemDto);

            bool result = await _shoppingListService.SaveChangesAsync();

            if (!result)
            {
                return BadRequest("Could not save changes to the DB");
            }

            // Fetch the updated shopping list to include the newly added item
            var updatedShoppingList = await _shoppingListService.GetShoppingListById(id, _familyName);

            // Map the updated shopping list to the DTO
            var shoppingListDto = _mapper.Map<ShoppingListDto>(updatedShoppingList);

            // Return the updated shopping list DTO
            return Ok(shoppingListDto);
        }

        // Delete shopping list
        [HttpDelete("{id}")]
        public async Task DeleteShoppingList(Guid id)
        {
            await Task.CompletedTask;
        }

        // Delete shopping list item 
        [HttpDelete("{id}/items/{itemId}")]
        public async Task DeleteShoppingListItem(Guid id, Guid itemId)
        {
            await Task.CompletedTask;
        }

    }
}

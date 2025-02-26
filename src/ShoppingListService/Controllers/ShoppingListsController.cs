using CatalogService.RequestHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ShoppingListService.DTOs;
using ShoppingListService.Helpers;
using ShoppingListService.Services;

namespace ShoppingListService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ShoppingListsController : ControllerBase
    {
        private readonly IRequestContextService _requestContextService;
        private readonly ProblemDetailsFactory _problemDetailsFactory;

        private readonly IShoppingListBusinessService _shoppingListBusinessService;
        public ShoppingListsController(IShoppingListBusinessService shoppingListBusinessService, IRequestContextService requestContextService, ProblemDetailsFactory problemDetailsFactory)
        {
            _shoppingListBusinessService = shoppingListBusinessService;
            _requestContextService = requestContextService;
            _problemDetailsFactory = problemDetailsFactory;
        }

        [HttpGet("catalogitems")]
        public async Task<ActionResult<List<CatalogItemDto>>> GetCatalogItems()
        {
            var result = await _shoppingListBusinessService.GetCatalogItemsAsync();
            return HandleServiceResult(result);
        }

        [HttpPost]
        public async Task<ActionResult<ShoppingListDto>> CreateShoppingList(CreateShoppingListDto shoppingListDto)
        {
            var result = await _shoppingListBusinessService.CreateShoppingListAsync(shoppingListDto);

            return HandleServiceResult(result);
        }

        [HttpGet]
        public async Task<ActionResult<List<ShoppingListDto>>> GetShoppingLists()
        {
            var result = await _shoppingListBusinessService.GetShoppingListsAsync();
            return HandleServiceResult(result);
        }

        // Get shopping list by Id
        [HttpGet("{id}")]
        public async Task<ActionResult<ShoppingListDto>> GetShoppingList(Guid id)
        {
            var result = await _shoppingListBusinessService.GetShoppingListAsync(id);
            return HandleServiceResult(result);
        }

        // Update shopping list
        [HttpPut("{id}")]
        public async Task<ActionResult<ShoppingListDto>> UpdateShoppingList(Guid id, UpdateShoppingListDto shoppingListDto)
        {
            var result = await _shoppingListBusinessService.UpdateShoppingListAsync(id, shoppingListDto);
            return HandleServiceResult(result);

        }

        // Create a new shopping list item within the shopping list
        [HttpPost("{id}/items")]
        public async Task<ActionResult<ShoppingListDto>> CreateShoppingListItem(Guid id, CreateShoppingListItemDto items)
        {
            var result = await _shoppingListBusinessService.CreateShoppingListItemAsync(id, items);
            return HandleServiceResult(result);
        }

        // Update the item in the shopping list
        [HttpPut("{id}/items/{itemId}")]
        public async Task<ActionResult<ShoppingListDto>> UpdateShoppingListItem(Guid id, Guid itemId, UpdateShoppingListItemDto updateShoppingListItemDto)
        {
            var result = await _shoppingListBusinessService.UpdateShoppingListItemAsync(id, itemId, updateShoppingListItemDto);
            return HandleServiceResult(result);
        }

        // Delete shopping list
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShoppingList(Guid id)
        {
            var result = await _shoppingListBusinessService.DeleteShoppingListAsync(id);
            return HandleEmptyServiceResult(result);
        }

        // Delete shopping list item 
        [HttpDelete("{id}/items/{itemId}")]
        public async Task<ActionResult> DeleteShoppingListItem(Guid id, Guid itemId)
        {
            var result = await _shoppingListBusinessService.DeleteShoppingListItemAsync(id, itemId);
            return HandleEmptyServiceResult(result);
        }

        // Get frequently bought catalog items
        [HttpGet("catalogitems/freq-bought")]
        public async Task<ActionResult<List<CatalogItemDto>>> GetFrequentlyBoughtItems()
        {
            var result = await _shoppingListBusinessService.GetFrequentlyBoughtItemsAsync();
            return HandleServiceResult(result);
        }

        [HttpGet("catalogitems/search")]
        public async Task<ActionResult<List<CatalogItemDto>>> SearchCatalogItems([FromQuery] string query)
        {
            var result = await _shoppingListBusinessService.SearchCatalogItemsAsync(query);
            return HandleServiceResult(result);
        }


        private ActionResult HandleServiceResult<T>(ServiceResult<T> result)
        {
            if (!result.Success)
            {
                var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
                    HttpContext, result.StatusCode, result.Message, result.Message, _problemDetailsFactory);
                return new ObjectResult(problemDetails) { StatusCode = result.StatusCode };
            }

            return Ok(result.Data);
        }

        private ActionResult HandleEmptyServiceResult<T>(ServiceResult<T> result)
        {
            if (!result.Success)
            {
                var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
                    HttpContext, result.StatusCode, result.Message, result.Message, _problemDetailsFactory);
                return new ObjectResult(problemDetails) { StatusCode = result.StatusCode };
            }

            return NoContent();
        }
    }
}
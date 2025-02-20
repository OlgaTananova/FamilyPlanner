using CatalogService.DTOs;
using CatalogService.RequestHelpers;
using CatalogService.Services;
using Contracts.Catalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;


namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CatalogController : ControllerBase
    {
        private readonly ProblemDetailsFactory _problemDetailsFactory;
        private readonly ICatalogBusinessService _catalogBusinessService;

        public CatalogController(
            ICatalogBusinessService catalogBusinessService,
            ProblemDetailsFactory problemDetailsFactory)
        {
            _problemDetailsFactory = problemDetailsFactory;
            _catalogBusinessService = catalogBusinessService;
        }

        #region CategoryActions

        [HttpGet("categories")]
        public async Task<ActionResult<List<CategoryDto>>> GetAllCategories()
        {
            var result = await _catalogBusinessService.GetAllCategoriesAsync();
            return HandleServiceResult(result);
        }

        [HttpGet("categories/{sku}")]
        public async Task<ActionResult<CategoryDto>> GetCategoryBySku(Guid sku)
        {
            var result = await _catalogBusinessService.GetCategoryBySkuAsync(sku);

            return HandleServiceResult(result);
        }

        [HttpPost("categories")]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto categoryDto)
        {

            var result = await _catalogBusinessService.CreateCategoryAsync(categoryDto);

            return HandleServiceResult(result);
        }

        [HttpPut("categories/{sku}")]
        public async Task<ActionResult<CategoryDto>> UpdateCategory(Guid sku, UpdateCategoryDto categoryDto)
        {
            var result = await _catalogBusinessService.UpdateCategoryAsync(sku, categoryDto);
            return HandleServiceResult(result);
        }

        [HttpDelete("categories/{sku}")]
        public async Task<ActionResult> DeleteCategory(Guid sku)
        {

            var result = await _catalogBusinessService.DeleteCategoryAsync(sku);
            return HandleEmptyServiceResult(result);

        }

        #endregion

        #region ItemsActions

        [HttpGet("items")]
        public async Task<ActionResult<List<ItemDto>>> GetAllItems()
        {
            var result = await _catalogBusinessService.GetAllItemsAsync();
            return HandleServiceResult(result);
        }

        [HttpGet("items/{sku}")]
        public async Task<ActionResult<ItemDto>> GetItemBySku(Guid sku)
        {
            var result = await _catalogBusinessService.GetItemBySkuAsync(sku);

            return HandleServiceResult(result);
        }

        [HttpPost("items")]
        public async Task<ActionResult<ItemDto>> CreateItem(CreateItemDto itemDto)
        {
            var result = await _catalogBusinessService.CreateItemAsync(itemDto);

            return HandleServiceResult(result);
        }

        [HttpPut("items/{sku}")]
        public async Task<ActionResult<CatalogItemUpdated>> UpdateItem(Guid sku, UpdateItemDto itemDto)
        {
            var result = await _catalogBusinessService.UpdateItemAsync(sku, itemDto);
            return HandleServiceResult(result);
        }

        [HttpDelete("items/{sku}")]
        public async Task<ActionResult> DeleteItem(Guid sku)
        {
            var result = await _catalogBusinessService.DeleteItemAsync(sku);
            return HandleEmptyServiceResult(result);
        }

        [HttpGet("items/search")]
        public async Task<ActionResult<List<ItemDto>>> SearchItems([FromQuery] string query)
        {
            var result = await _catalogBusinessService.SearchItemsAsync(query);
            return HandleServiceResult(result);
        }

        #endregion

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

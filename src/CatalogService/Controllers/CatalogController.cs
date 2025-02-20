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
            return Ok(result.Data);
        }

        [HttpGet("categories/{sku}")]
        public async Task<ActionResult<CategoryDto>> GetCategoryBySku(Guid sku)
        {
            var result = await _catalogBusinessService.GetCategoryBySkuAsync(sku);

            if (!result.Success)
            {
                var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(HttpContext, result.StatusCode, result.Message, result.Message, _problemDetailsFactory);
                return new ObjectResult(problemDetails) { StatusCode = result.StatusCode };
            }
            return Ok(result.Data);
        }

        [HttpPost("categories")]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto categoryDto)
        {

            var result = await _catalogBusinessService.CreateCategoryAsync(categoryDto);

            if (!result.Success)
            {
                var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(HttpContext, result.StatusCode, result.Message, result.Message, _problemDetailsFactory);
                return new ObjectResult(problemDetails) { StatusCode = result.StatusCode };
            }

            return Ok(result.Data);
        }

        [HttpPut("categories/{sku}")]
        public async Task<ActionResult<CategoryDto>> UpdateCategory(Guid sku, UpdateCategoryDto categoryDto)
        {
            var result = await _catalogBusinessService.UpdateCategoryAsync(sku, categoryDto);
            if (!result.Success)
            {
                var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(HttpContext, result.StatusCode, result.Message, result.Message, _problemDetailsFactory);
                return new ObjectResult(problemDetails) { StatusCode = result.StatusCode };
            }
            return Ok(result.Data);
        }

        [HttpDelete("categories/{sku}")]
        public async Task<ActionResult> DeleteCategory(Guid sku)
        {

            var result = await _catalogBusinessService.DeleteCategoryAsync(sku);
            if (!result.Success)
            {
                var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(HttpContext, result.StatusCode, result.Message, result.Message, _problemDetailsFactory);
                return new ObjectResult(problemDetails) { StatusCode = result.StatusCode };
            }
            return NoContent();

        }

        #endregion

        #region ItemsActions

        [HttpGet("items")]
        public async Task<ActionResult<List<ItemDto>>> GetAllItems()
        {
            var result = await _catalogBusinessService.GetAllItemsAsync();
            return Ok(result.Data);
        }

        [HttpGet("items/{sku}")]
        public async Task<ActionResult<ItemDto>> GetItemBySku(Guid sku)
        {
            var result = await _catalogBusinessService.GetItemBySkuAsync(sku);

            if (!result.Success)
            {
                var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(HttpContext, result.StatusCode, result.Message, result.Message, _problemDetailsFactory);
                return new ObjectResult(problemDetails) { StatusCode = result.StatusCode };
            }
            return Ok(result.Data);
        }

        [HttpPost("items")]
        public async Task<ActionResult<ItemDto>> CreateItem(CreateItemDto itemDto)
        {
            var result = await _catalogBusinessService.CreateItemAsync(itemDto);

            if (!result.Success)
            {
                var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(HttpContext, result.StatusCode, result.Message, result.Message, _problemDetailsFactory);
                return new ObjectResult(problemDetails) { StatusCode = result.StatusCode };
            }

            return Ok(result.Data);
        }

        [HttpPut("items/{sku}")]
        public async Task<ActionResult<CatalogItemUpdated>> UpdateItem(Guid sku, UpdateItemDto itemDto)
        {
            var result = await _catalogBusinessService.UpdateItemAsync(sku, itemDto);
            if (!result.Success)
            {
                var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(HttpContext, result.StatusCode, result.Message, result.Message, _problemDetailsFactory);
                return new ObjectResult(problemDetails) { StatusCode = result.StatusCode };
            }
            return Ok(result.Data);
        }

        [HttpDelete("items/{sku}")]
        public async Task<ActionResult> DeleteItem(Guid sku)
        {
            var result = await _catalogBusinessService.DeleteItemAsync(sku);
            if (!result.Success)
            {
                var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(HttpContext, result.StatusCode, result.Message, result.Message, _problemDetailsFactory);
                return new ObjectResult(problemDetails) { StatusCode = result.StatusCode };
            }
            return NoContent();
        }

        [HttpGet("items/search")]
        public async Task<ActionResult<List<ItemDto>>> SearchItems([FromQuery] string query)
        {
            var result = await _catalogBusinessService.SearchItemsAsync(query);
            if (!result.Success)
            {
                var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(HttpContext, result.StatusCode, result.Message, result.Message, _problemDetailsFactory);
                return new ObjectResult(problemDetails) { StatusCode = result.StatusCode };
            }
            return Ok(result.Data);
        }

        #endregion
    }
}

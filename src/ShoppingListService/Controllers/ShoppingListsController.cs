using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShoppingListService.Data;
using ShoppingListService.DTOs;

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
    }
}

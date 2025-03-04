using CatalogService.RequestHelpers;
using CatalogService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FamilyController : ControllerBase
    {
        private readonly IGraphService _graphService;
        private readonly IRequestContextService _requestContextService;
        private readonly AppConfig _appConfig;
        public FamilyController(IGraphService graphService, IRequestContextService requestContextService, AppConfig appConfig)
        {
            _graphService = graphService;
            _requestContextService = requestContextService;
            _appConfig = appConfig;
        }

        [HttpGet()]
        public async Task<IActionResult> GetFamilyMembers()
        {
            string family = _requestContextService.FamilyName;
            try
            {
                var result = await _graphService.GetFamilyUsersAsync(family);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, $"Graph API call failed: {ex.Message}");
            }
        }
    }

}


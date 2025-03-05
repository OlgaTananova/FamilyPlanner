using CatalogService.DTOs;
using CatalogService.RequestHelpers;
using CatalogService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FamilyController : ControllerBase
    {
        private readonly IGraphService _graphService;
        private readonly IRequestContextService _requestContextService;
        private readonly ProblemDetailsFactory _problemDetailsFactory;
        public FamilyController(IGraphService graphService, IRequestContextService requestContextService, ProblemDetailsFactory problemDetailsFactory)
        {
            _graphService = graphService;
            _requestContextService = requestContextService;
            _problemDetailsFactory = problemDetailsFactory;
        }

        [HttpGet()]
        public async Task<ActionResult<List<FamilyUserDto>>> GetFamilyMembers()
        {
            string family = _requestContextService.FamilyName;

            var result = await _graphService.GetFamilyUsersAsync(family);
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
    }

}


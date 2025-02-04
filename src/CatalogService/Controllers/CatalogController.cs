using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using AutoMapper.Configuration.Annotations;
using CatalogService.Data;
using CatalogService.DTOs;
using CatalogService.Entities;
using CatalogService.RequestHelpers;
using Contracts.Catalog;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class CatalogController : ControllerBase
	{
		private readonly IMapper _mapper;
		private readonly ICatalogRepository _repo;
		private readonly string _familyName;
		private readonly string _userId;
		private readonly IPublishEndpoint _publishEndpoint;
		private ILogger<CatalogController> _logger;
		private IHttpContextAccessor _httpContext;
		private readonly string _operationId;
		private ProblemDetailsFactory _problemDetailsFactory;

		public CatalogController(
		IMapper mapper, ICatalogRepository repo,
		IHttpContextAccessor httpContextAccessor,
		IPublishEndpoint publishEndpoint,
		ILogger<CatalogController> logger,
		ProblemDetailsFactory problemDetailsFactory
		)
		{
			_mapper = mapper;
			_repo = repo;
			_publishEndpoint = publishEndpoint;
			_logger = logger;
			_httpContext = httpContextAccessor;
			_problemDetailsFactory = problemDetailsFactory;

			// Centralized family and user ID extraction
			_familyName = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "family")?.Value;
			_userId = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
			_operationId = httpContextAccessor.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity.TraceId.ToString();

			if (string.IsNullOrEmpty(_familyName) || string.IsNullOrEmpty(_userId))
			{
				throw new UnauthorizedAccessException("Family or user information is missing.");
			}
		}

		#region CategoryActions

		[HttpGet("categories")]
		public async Task<ActionResult<List<CategoryDto>>> GetAllCategories()
		{
			return Ok(await _repo.GetAllCategoriesAsync(_familyName));
		}

		[HttpGet("categories/{sku}")]
		public async Task<ActionResult<CategoryDto>> GetCategoryBySku(Guid sku)
		{
			CategoryDto category = await _repo.GetCategoryBySkuAsync(sku, _familyName);
			if (category == null)
			{
				var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
						_httpContext.HttpContext,
						StatusCodes.Status404NotFound,
						"Category not found",
						$"The category with SKU {sku} was not found.",
						_problemDetailsFactory);

				return NotFound(problemDetails);
			}
			return Ok(category);
		}

		[HttpPost("categories")]
		public async Task<ActionResult<CategoryDto>> CreateCategory(
				CreateCategoryDto categoryDto)
		{

			StructuredLogger.LogInformation(_logger, HttpContext, "Create Category request received.", _userId, _familyName,
					 new Dictionary<string, object>
							{
												{ "category", categoryDto.Name }
							});
			Category existingCategory = await _repo.GetCategoryEntityByName(categoryDto.Name, _familyName);

			if (existingCategory != null)
			{
				StructuredLogger.LogWarning(_logger, HttpContext,
				"Create Category request failed: Category already exists.",
				_userId,
				_familyName,
				new Dictionary<string, object>
				{
										{ "category", categoryDto.Name },
				});
				var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
				HttpContext,
				StatusCodes.Status400BadRequest,
				"Category Already Exists",
				$"A category with the name '{categoryDto.Name}' already exists.",
				_problemDetailsFactory
		);
				return BadRequest(problemDetails);
			}

			var category = _mapper.Map<Category>(categoryDto);

			category.OwnerId = _userId;
			category.Family = _familyName;

			_repo.AddCategory(category);

			CategoryDto newCategory = _mapper.Map<CategoryDto>(category);

			await _publishEndpoint.Publish(_mapper.Map<CatalogCategoryCreated>(newCategory), context =>
					{
						context.Headers.Set("OperationId", _operationId);
						context.Headers.Set("traceId", _operationId);
						context.Headers.Set("requestId", _httpContext.HttpContext.TraceIdentifier);

					});

			var result = await _repo.SaveChangesAsync();

			if (!result)
			{
				StructuredLogger.LogError(_logger, HttpContext,
				"Create Category request failed: Database save error. Try again later.",
				_userId,
				_familyName,
				new Dictionary<string, object>
				{
										{ "category", categoryDto.Name },
				});

				var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
						HttpContext,
						StatusCodes.Status400BadRequest,
						"Database Error",
						"Could not save changes to the database.",
						_problemDetailsFactory
				);
				return BadRequest(problemDetails);

			}
			StructuredLogger.LogInformation(_logger, HttpContext,
			"Create Category request succeeded.",
			_userId,
			_familyName,
			new Dictionary<string, object>
			{
								{ "category", category.Name },
			});
			return Ok(newCategory);
		}

		[HttpPut("categories/{sku}")]
		public async Task<ActionResult<CategoryDto>> UpdateCategory(Guid sku, UpdateCategoryDto categoryDto)
		{
			StructuredLogger.LogInformation(_logger, HttpContext,
					"Update Category request received.",
					_userId,
					_familyName,
					new Dictionary<string, object>
			{
								{ "category", categoryDto.Name },
			});

			using var transaction = await _repo.BeginTransactionAsync();
			try
			{
				Category category = await _repo.GetCategoryEntityBySku(sku, _familyName);

				if (category == null)
				{
					StructuredLogger.LogWarning(_logger, HttpContext,
					"Update Category request failed: Category not found.",
					_userId,
					_familyName,
					new Dictionary<string, object>
							{
								{ "category", categoryDto.Name },
							});

					var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
							HttpContext,
							StatusCodes.Status404NotFound,
							"Category Not Found",
							$"The category with SKU {sku} was not found or you are not allowed to update it.",
							_problemDetailsFactory
					);
					return NotFound(problemDetails);
				}

				await _repo.UpdateCategoryAsync(category, categoryDto);

				CategoryDto updatedCategory = _mapper.Map<CategoryDto>(category);

				await _publishEndpoint.Publish(_mapper.Map<CatalogCategoryUpdated>(updatedCategory), context =>
					{
						context.Headers.Set("OperationId", _operationId);
						context.Headers.Set("traceId", _operationId);
						context.Headers.Set("requestId", _httpContext.HttpContext.TraceIdentifier);
					});

				bool result = await _repo.SaveChangesAsync();

				if (!result)
				{
					StructuredLogger.LogError(_logger, HttpContext,
							"Update Category request failed: Database save error.",
							_userId,
							_familyName,
							new Dictionary<string, object>
							{
																{ "category", categoryDto.Name },
							});

					var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
							HttpContext,
							StatusCodes.Status400BadRequest,
							"Database Error",
							"Could not save changes to the database while updating the category.",
							_problemDetailsFactory
					);
					return BadRequest(problemDetails);
				}

				await transaction.CommitAsync();
				StructuredLogger.LogInformation(_logger, HttpContext,
						"Update Category request succeeded.",
						_userId,
						_familyName,
						new Dictionary<string, object>
						{
												{ "category", category.Name },
						});

				return Ok(updatedCategory);

			}
			catch (Exception ex)
			{
				// Rollback the transaction on any exception
				await transaction.RollbackAsync();
				StructuredLogger.LogCritical(_logger, HttpContext,
						"Unhandled exception during Update Category request.",
						_userId,
						_familyName,
						new Dictionary<string, object>
						{
												{ "exception", ex.Message },
												{ "operationId", _operationId },
												{ "stackTrace", ex.StackTrace ?? "No StackTrace" }
						});

				var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
						HttpContext,
						StatusCodes.Status400BadRequest,
						"Internal Server Error",
						"An unexpected error occurred while updating the category. Try again later.",
						_problemDetailsFactory
				);

				return BadRequest(problemDetails);
			}

		}

		[HttpDelete("categories/{sku}")]
		public async Task<ActionResult> DeleteCategory(Guid sku)
		{
			using var transaction = await _repo.BeginTransactionAsync();
			try
			{
				StructuredLogger.LogInformation(_logger, HttpContext,
				"Delete Category request received.",
				_userId,
				_familyName,
				new Dictionary<string, object>
				{
												{ "sku", sku },
				});

				Category category = await _repo.GetCategoryEntityBySku(sku, _familyName);

				if (category == null)
				{
					StructuredLogger.LogWarning(_logger, HttpContext,
					"Delete Category request failed: Category not found.",
					_userId,
					_familyName,
					new Dictionary<string, object>
					{
														{ "sku", sku },
					});

					var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
							HttpContext,
							StatusCodes.Status404NotFound,
							"Category Not Found",
							$"The category with SKU {sku} was not found.",
							_problemDetailsFactory
					);

					return NotFound(problemDetails);
				}
				if (category.Items.Any())
				{
					StructuredLogger.LogWarning(_logger, HttpContext,
					"Delete Category request failed: Category is not empty.",
					_userId,
					_familyName,
					new Dictionary<string, object>
					{
												{ "sku", sku },
												{ "category", category.Name },
												{ "operationId", _operationId },
												{ "family", _familyName }
					});

					var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
							HttpContext,
							StatusCodes.Status400BadRequest,
							"Category Not Empty",
							$"The category '{category.Name}' is not empty and cannot be deleted.",
							_problemDetailsFactory
					);

					return BadRequest(problemDetails);
				}

				category.IsDeleted = true;

				CategoryDto deletedCategory = _mapper.Map<CategoryDto>(category);

				await _publishEndpoint.Publish(_mapper.Map<CatalogCategoryDeleted>(deletedCategory), context =>
				{
					context.Headers.Set("OperationId", _operationId);
					context.Headers.Set("traceId", _operationId);
					context.Headers.Set("requestId", _httpContext.HttpContext.TraceIdentifier);

				});

				bool result = await _repo.SaveChangesAsync();
				if (!result)
				{
					StructuredLogger.LogError(_logger, HttpContext,
					_userId,
					_familyName,
					"Delete Category request failed: Database save error.",
							new Dictionary<string, object>
							{
														{ "sku", sku },
							});

					var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
							HttpContext,
							StatusCodes.Status400BadRequest,
							"Database Error",
							"Could not save the deletion to the database.",
							_problemDetailsFactory
					);
					return BadRequest(problemDetails);
				}
					;
				await transaction.CommitAsync();
				StructuredLogger.LogInformation(_logger, HttpContext,
						"Delete Category request succeeded.",
						_userId,
						_familyName,
						new Dictionary<string, object>
						{
												{ "sku", sku }
						});
				return NoContent();
			}
			catch (Exception ex)
			{
				// Rollback the transaction on any exception
				await transaction.RollbackAsync();
				StructuredLogger.LogCritical(_logger, HttpContext,
						"Unhandled exception during Delete Category request.",
						_userId,
						_familyName,
						new Dictionary<string, object>
						{
														{ "sku", sku },
														{ "exception", ex.Message },
														{ "operationId", _operationId },
														{ "stackTrace", ex.StackTrace ?? "No StackTrace" }
						});

				var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
						HttpContext,
						StatusCodes.Status400BadRequest,
						"Internal Server Error",
						"An unexpected error occurred while deleting the category. Try again later.",
						_problemDetailsFactory
				);
				return BadRequest(problemDetails);
			}

		}

		#endregion

		#region ItemsActions

		[HttpGet("items")]
		public async Task<ActionResult<List<ItemDto>>> GetAllItems()
		{

			return Ok(await _repo.GetAllItemsAsync(_familyName));

		}

		[HttpGet("items/{sku}")]
		public async Task<ActionResult<ItemDto>> GetItemBySku(Guid sku)
		{

			var item = await _repo.GetItemBySkuAsync(sku, _familyName);

			if (item == null)
			{
				StructuredLogger.LogWarning(_logger, HttpContext,
				"Get Item request failed: Item not found.",
				_userId,
				_familyName,
				new Dictionary<string, object>
				{
								{ "sku", sku }
				});
				var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
						HttpContext,
						StatusCodes.Status404NotFound,
						"Item Not Found",
						$"The item with SKU {sku} was not found.",
						_problemDetailsFactory
						);

				return NotFound(problemDetails);
			}

			return Ok(item);
		}

		[HttpPost("items")]
		public async Task<ActionResult<ItemDto>> CreateItem(CreateItemDto itemDto)
		{
			StructuredLogger.LogInformation(_logger, HttpContext,
			"Create Item request received.",
			_userId,
			_familyName,
			new Dictionary<string, object>
			{
				{ "itemName", itemDto.Name },
				{ "categorySKU", itemDto.CategorySKU }
			});
			Item existingItem = await _repo.GetItemEntityByNameAsync(itemDto.Name, _familyName);

			if (existingItem != null)
			{
				StructuredLogger.LogWarning(_logger, HttpContext,
				"Create Item request failed: Item already exists.",
				_userId,
				_familyName,
				new Dictionary<string, object>
				{
					{ "itemName", itemDto.Name },
				});

				var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
					HttpContext,
					StatusCodes.Status400BadRequest,
					"Item Already Exists",
					$"An item with the name '{itemDto.Name}' already exists.",
					_problemDetailsFactory
				);
				return BadRequest(problemDetails);
			}

			Category category = await _repo.GetCategoryEntityBySku(itemDto.CategorySKU, _familyName);

			if (category == null)
			{
				StructuredLogger.LogWarning(_logger, HttpContext,
				"Create Item request failed: Category not found.",
				_userId,
				_familyName,
				new Dictionary<string, object>
				{
					{ "categorySKU", itemDto.CategorySKU },
					{ "itemName", itemDto.Name },
				});

				var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
					HttpContext,
					StatusCodes.Status404NotFound,
					"Category Not Found",
					$"The category with SKU {itemDto.CategorySKU} was not found.",
					_problemDetailsFactory
				);
				return NotFound(problemDetails);
			}

			Item item = _mapper.Map<Item>(itemDto);
			item.OwnerId = _userId;
			item.Family = _familyName;
			item.Category = category;
			item.CategorySKU = category.SKU;
			item.CategoryName = category.Name;

			_repo.AddItem(item);
			// Send a created item to the rabbitmq
			var newItem = _mapper.Map<ItemDto>(item);

			await _publishEndpoint.Publish(_mapper.Map<CatalogItemCreated>(newItem), context =>
			{
				context.Headers.Set("OperationId", _operationId);
				context.Headers.Set("traceId", _operationId);
				context.Headers.Set("requestId", _httpContext.HttpContext.TraceIdentifier);
			});

			StructuredLogger.LogInformation(_logger, HttpContext,
			"Create Item request is sent to the message broker.",
			_userId,
			_familyName,
			new Dictionary<string, object>
			{
				{ "itemName", itemDto.Name },
			});

			bool result = await _repo.SaveChangesAsync();

			if (!result)
			{
				StructuredLogger.LogError(_logger, HttpContext,
				"Create Item request failed: Database save error.",
				_userId,
				_familyName,
				new Dictionary<string, object>
				{
					{ "itemName", itemDto.Name },
				});

				var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
					HttpContext,
					StatusCodes.Status400BadRequest,
					"Database Error",
					"Could not save the item to the database.",
					_problemDetailsFactory
				);
				return BadRequest(problemDetails);
			}
			StructuredLogger.LogInformation(_logger, HttpContext,
			"Create Item request succeeded.",
			_userId,
			_familyName,
			new Dictionary<string, object>
			{
				{ "itemName", item.Name },
			});

			return Ok(_mapper.Map<ItemDto>(item));
		}

		[HttpPut("items/{sku}")]
		public async Task<ActionResult<CatalogItemUpdated>> UpdateItem(Guid sku, UpdateItemDto itemDto)
		{
			StructuredLogger.LogInformation(_logger, HttpContext,
				"Update Item request received.",
				_userId,
				_familyName,
				new Dictionary<string, object>
				{
					{ "sku", sku },
					{ "itemName", itemDto.Name },
					{ "categorySKU", itemDto.CategorySKU }
				});
			using var transaction = await _repo.BeginTransactionAsync();

			try
			{
				Item item = await _repo.GetItemEntityBySkuAsync(sku, _familyName);

				Category category = await _repo.GetCategoryEntityBySku(itemDto.CategorySKU, _familyName);

				if (item == null || category == null)
				{
					StructuredLogger.LogWarning(_logger, HttpContext,
						"Update Item request failed: Item or Category not found.",
						_userId,
						_familyName,
						new Dictionary<string, object>
						{
							{ "sku", sku },
							{ "itemName", itemDto.Name },
							{ "categorySKU", itemDto.CategorySKU },
						});

					var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
						HttpContext,
						StatusCodes.Status404NotFound,
						"Item or Category Not Found",
						$"The item with SKU {sku} or the category with SKU {itemDto.CategorySKU} was not found.",
						_problemDetailsFactory
						);
					return NotFound(problemDetails);
				}

				Guid previousCategorySKU = item.CategorySKU != itemDto.CategorySKU ? item.CategorySKU : itemDto.CategorySKU;

				await _repo.UpdateItemAsync(item, itemDto);

				CatalogItemUpdated catalogItemUpdated = new CatalogItemUpdated
				{
					UpdatedItem = _mapper.Map<UpdatedItem>(item),
					PreviousCategorySKU = previousCategorySKU
				};
				await _publishEndpoint.Publish(catalogItemUpdated, context =>
				{
					context.Headers.Set("OperationId", _operationId);
					context.Headers.Set("traceId", _operationId);
					context.Headers.Set("requestId", _httpContext.HttpContext.TraceIdentifier);
				});

				bool result = await _repo.SaveChangesAsync();

				if (!result)
				{
					await transaction.RollbackAsync();
					StructuredLogger.LogError(_logger, HttpContext,
						"Update Item request failed: Database save error.",
						_userId,
						_familyName,
						new Dictionary<string, object>
						{
							{ "sku", sku },
							{ "itemName", itemDto.Name },
							{ "operationId", _operationId },
							{ "family", _familyName }
						});

					var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
						HttpContext,
						StatusCodes.Status400BadRequest,
						"Database Error",
						"Could not save changes to the database while updating the item.",
						_problemDetailsFactory
					);

					return BadRequest(problemDetails);
				}
				await transaction.CommitAsync();
				return Ok(catalogItemUpdated);
			}
			catch (Exception ex)
			{
				// Rollback the transaction on any exception
				await transaction.RollbackAsync();
				StructuredLogger.LogCritical(_logger, HttpContext,
					"Unhandled exception during Update Item request.",
					_userId,
					_familyName,
					new Dictionary<string, object>
					{
						{ "sku", sku },
						{ "exception", ex.Message },
						{ "operationId", _operationId },
						{ "stackTrace", ex.StackTrace ?? "No StackTrace" }
					});

				var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
					HttpContext,
					StatusCodes.Status400BadRequest,
					"Internal Server Error",
					"An unexpected error occurred while updating the item.",
					_problemDetailsFactory
				);
				return BadRequest(problemDetails);
			}
		}

		[HttpDelete("items/{sku}")]
		public async Task<ActionResult> DeleteItem(Guid sku)
		{
			StructuredLogger.LogInformation(_logger, HttpContext,
				"Delete Item request received.",
				_userId,
				_familyName,
				new Dictionary<string, object>
					{
						{ "sku", sku },
					});

			using var transaction = await _repo.BeginTransactionAsync();
			try
			{
				Item item = await _repo.GetItemEntityBySkuAsync(sku, _familyName);

				if (item == null)
				{
					StructuredLogger.LogWarning(_logger, HttpContext,
					"Delete Item request failed: Item not found.",
					_userId,
					_familyName,
					new Dictionary<string, object>
						{
							{ "sku", sku },
						});

					var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
						HttpContext,
						StatusCodes.Status404NotFound,
						"Item Not Found",
						$"The item with SKU {sku} was not found.",
						_problemDetailsFactory
					);
					return NotFound(problemDetails);
				}

				item.IsDeleted = true;

				ItemDto deletedItem = _mapper.Map<ItemDto>(item);

				await _publishEndpoint.Publish(_mapper.Map<CatalogItemDeleted>(deletedItem), context =>
				{
					context.Headers.Set("OperationId", _operationId);
					context.Headers.Set("traceId", _operationId);
					context.Headers.Set("requestId", _httpContext.HttpContext.TraceIdentifier);
				});

				bool result = await _repo.SaveChangesAsync();
				if (!result)
				{
					StructuredLogger.LogError(_logger, HttpContext,
						"Delete Item request failed: Database save error.",
						_userId,
						_familyName,
						new Dictionary<string, object>
							{
								{ "sku", sku },
							});

					var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
							HttpContext,
							StatusCodes.Status400BadRequest,
							"Database Error",
							"Could not save the deletion to the database. Try again later.",
							_problemDetailsFactory
					);
					return BadRequest(problemDetails);
				}
				;

				await transaction.CommitAsync();

				StructuredLogger.LogInformation(_logger, HttpContext,
					"Delete Item request succeeded.",
					_userId,
					_familyName,
					new Dictionary<string, object>
					{
						{ "sku", sku },
					});
				return NoContent();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				StructuredLogger.LogCritical(_logger, HttpContext,
					"Unhandled exception during Delete Category request.",
					_userId,
					_familyName,
					new Dictionary<string, object>
					{
						{ "sku", sku },
						{ "exception", ex.Message },
						{ "operationId", _operationId },
						{ "stackTrace", ex.StackTrace ?? "No StackTrace" }
					});

				var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
					HttpContext,
					StatusCodes.Status400BadRequest,
					"Internal Server Error",
					"An unexpected error occurred while deleting the category.",
					_problemDetailsFactory
				);

				return BadRequest(problemDetails);
			}
		}

		[HttpGet("items/search")]
		public async Task<ActionResult<List<ItemDto>>> SearchItems([FromQuery] string query)
		{
			if (string.IsNullOrWhiteSpace(query))
			{
				var problemDetails = ProblemDetailsFactoryHelper.CreateProblemDetails(
				HttpContext,
				StatusCodes.Status400BadRequest,
				"Invalid Query Parameter",
				"Query parameter cannot be empty.",
				_problemDetailsFactory);

				return BadRequest(problemDetails);
			}

			var items = await _repo.SearchItemsAsync(query, _familyName);

			var catalogItemDtos = _mapper.Map<List<ItemDto>>(items);

			return Ok(catalogItemDtos);
		}

		#endregion
	}
}

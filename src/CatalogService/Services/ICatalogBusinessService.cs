using CatalogService.DTOs;
using CatalogService.RequestHelpers;
using Contracts.Catalog;

namespace CatalogService.Services;

public interface ICatalogBusinessService
{
    Task<ServiceResult<CategoryDto>> CreateCategoryAsync(CreateCategoryDto categoryDto);
    Task<ServiceResult<List<CategoryDto>>> GetAllCategoriesAsync();
    Task<ServiceResult<CategoryDto>> GetCategoryBySkuAsync(Guid sku);
    Task<ServiceResult<CategoryDto>> UpdateCategoryAsync(Guid sku, UpdateCategoryDto categoryDto);
    Task<ServiceResult<CategoryDto>> DeleteCategoryAsync(Guid sku);
    Task<ServiceResult<List<ItemDto>>> GetAllItemsAsync();
    Task<ServiceResult<ItemDto>> GetItemBySkuAsync(Guid sku);
    Task<ServiceResult<ItemDto>> CreateItemAsync(CreateItemDto itemDto);
    Task<ServiceResult<CatalogItemUpdated>> UpdateItemAsync(Guid sku, UpdateItemDto itemDto);
    Task<ServiceResult<ItemDto>> DeleteItemAsync(Guid sku);
    Task<ServiceResult<List<ItemDto>>> SearchItemsAsync(string query);
}

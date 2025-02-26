using ShoppingListService.DTOs;
using ShoppingListService.Helpers;

namespace ShoppingListService.Services;

public interface IShoppingListBusinessService
{
    Task<ServiceResult<List<CatalogItemDto>>> GetCatalogItemsAsync();
    Task<ServiceResult<List<ShoppingListDto>>> GetShoppingListsAsync();
    Task<ServiceResult<ShoppingListDto>> GetShoppingListAsync(Guid id);
    Task<ServiceResult<ShoppingListDto>> CreateShoppingListAsync(CreateShoppingListDto shoppingListDto);
    Task<ServiceResult<ShoppingListDto>> UpdateShoppingListAsync(Guid id, UpdateShoppingListDto shoppingListDto);
    Task<ServiceResult<ShoppingListDto>> DeleteShoppingListAsync(Guid id);
    Task<ServiceResult<ShoppingListDto>> CreateShoppingListItemAsync(Guid id, CreateShoppingListItemDto items);
    Task<ServiceResult<ShoppingListDto>> UpdateShoppingListItemAsync(Guid id, Guid itemId, UpdateShoppingListItemDto updateShoppingListItemDto);
    Task<ServiceResult<ShoppingListDto>> DeleteShoppingListItemAsync(Guid id, Guid itemId);
    Task<ServiceResult<List<CatalogItemDto>>> GetFrequentlyBoughtItemsAsync();
    Task<ServiceResult<List<CatalogItemDto>>> SearchCatalogItemsAsync(string query);

}

using ShoppingListService.DTOs;
using ShoppingListService.Helpers;

namespace ShoppingListService.Services;

public class ShoppingListBusinessService : IShoppingListBusinessService
{
    public Task<ServiceResult<ShoppingListDto>> CreateShoppingListAsync(CreateShoppingListDto shoppingListDto)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<ShoppingListDto>> CreateShoppingListItemAsync(Guid id, CreateShoppingListItemDto items)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<ShoppingListDto>> DeleteShoppingListAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<ShoppingListDto>> DeleteShoppingListItemAsync(Guid id, Guid itemId)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<List<CatalogItemDto>>> GetCatalogItemsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<List<CatalogItemDto>>> GetFrequentlyBoughtItemsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<ShoppingListDto>> GetShoppingList(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<List<ShoppingListDto>>> GetShoppingListsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<List<CatalogItemDto>>> SearchCatalogItemsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<ShoppingListDto>> UpdateShoppingListAsync(Guid id, UpdateShoppingListDto shoppingListDto)
    {
        throw new NotImplementedException();
    }

    public Task<ServiceResult<ShoppingListDto>> UpdateShoppingListItemAsync(Guid id, Guid itemId, UpdateShoppingListItemDto updateShoppingListItemDto)
    {
        throw new NotImplementedException();
    }
}

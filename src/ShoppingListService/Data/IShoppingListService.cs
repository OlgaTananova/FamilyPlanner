using System;
using ShoppingListService.DTOs;
using ShoppingListService.Entities;

namespace ShoppingListService.Data;

public interface IShoppingListService
{
    Task<List<CatalogItemDto>> GetCatalogItemsAsync(string family);
    Task<CatalogItem> GetCatalogItemBySKU(Guid id, string family);
    Task<ShoppingListItem> GetShoppingListItemById(Guid id, Guid shoppingListId, string family);
    void AddShoppingList(ShoppingList list);
    void AddShoppingListItem(ShoppingListItem item);
    Task UpdateShoppingList(ShoppingList list, UpdateShoppingListDto dto);
    void UpdateShoppingListItem(ShoppingListItem item, UpdateShoppingListItemDto dto);

    Task<ShoppingList> GetShoppingListById(Guid id, string family);
    Task<bool> SaveChangesAsync();
}

using System;
using ShoppingListService.DTOs;
using ShoppingListService.Entities;

namespace ShoppingListService.Data;

public interface IShoppingListService
{
    Task<List<CatalogItem>> GetCatalogItemsAsync(string family);
    Task<CatalogItem> GetCatalogItemBySKU(Guid id, string family);
    Task<ShoppingListItem> GetShoppingListItemById(Guid id, Guid shoppingListId, string family);
    Task<List<ShoppingList>> GetShoppingListsAsync(string family);
    Task<List<CatalogItem>> GetFrequentlyBoughtItemsAsync(string family);
    Task<List<CatalogItem>> AutocompleteCatalogItemsAsync(string query, string family);
    void AddShoppingList(ShoppingList list);
    void AddShoppingListItem(ShoppingListItem item);
    Task UpdateShoppingList(ShoppingList list, UpdateShoppingListDto dto);
    void UpdateShoppingListItem(ShoppingListItem item, UpdateShoppingListItemDto dto);
    void DeleteShoppingListItem(ShoppingListItem item);

    void DeleteShoppingList(ShoppingList list);

    Task<ShoppingList> GetShoppingListById(Guid id, string family);
    Task<bool> SaveChangesAsync();
}

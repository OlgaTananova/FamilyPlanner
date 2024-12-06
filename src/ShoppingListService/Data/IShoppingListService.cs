using System;
using ShoppingListService.DTOs;
using ShoppingListService.Entities;

namespace ShoppingListService.Data;

public interface IShoppingListService
{
    Task<List<CatalogItemDto>> GetCatalogItemsAsync(string family);
    void AddShoppingList();
    void AddShoppingListItem();
    Task<bool> SaveChangesAsync();
}

using System;
using CatalogService.DTOs;
using CatalogService.Entities;

namespace CatalogService.Data;

public interface ICatalogRepository
{
    Task<List<CategoryDto>> GetAllCategoriesAsync(string familyName);
    Task<List<ItemDto>> GetAllItemsAsync(string familyName);
    Task<CategoryDto> GetCategoryByIdAsync(Guid id, string familyName);
    Task<ItemDto> GetItemByIdAsync(Guid id, string familyName);
    Task<Category> GetCategoryEntityById(Guid id, string familyName, string userId);
    Task<Item> GetItemEntityByIdAsync(Guid id, string familyName, string userId);
    Task<Item> GetItemEntityByNameAsync(string name, string familyName);
    Task<Category> GetCategoryEntityByName(string name, string familyName);
    void AddCategory(Category category);
    void AddItem(Item item);

    Task<bool> SaveChangesAsync();

}

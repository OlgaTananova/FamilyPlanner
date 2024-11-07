using System;
using CatalogService.DTOs;
using CatalogService.Entities;

namespace CatalogService.Data;

public interface ICatalogRepository
{
    Task<List<CategoryDto>> GetAllCategoriesAsync();
    Task<List<ItemDto>> GetAllItemsAsync();
    Task<CategoryDto> GetCategoryByIdAsync(Guid id);
    Task<ItemDto> GetItemByIdAsync(Guid id);
    Task<Category> GetCategoryEntityById(Guid id);
    Task<Item> GetItemEntityByIdAsync(Guid id);
    Task<Item> GetItemEntityByNameAsync(string name);
    Task<Category> GetCategoryEntityByName(string name);
    void AddCategory(Category category);
    void AddItem(Item item);

    Task<bool> SaveChangesAsync();

}

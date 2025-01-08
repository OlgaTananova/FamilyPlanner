using System;
using CatalogService.DTOs;
using CatalogService.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace CatalogService.Data;

public interface ICatalogRepository
{
    Task<List<CategoryDto>> GetAllCategoriesAsync(string familyName);
    Task<List<ItemDto>> GetAllItemsAsync(string familyName);
    Task<CategoryDto> GetCategoryBySkuAsync(Guid sku, string familyName);
    Task<ItemDto> GetItemBySkuAsync(Guid sku, string familyName);
    Task<Category> GetCategoryEntityBySku(Guid sku, string familyName);
    Task<Item> GetItemEntityBySkuAsync(Guid sku, string familyName);
    Task<Item> GetItemEntityByNameAsync(string name, string familyName);
    Task<Category> GetCategoryEntityByName(string name, string familyName);
    Task UpdateItemAsync(Item item, UpdateItemDto itemDto);
    Task UpdateCategoryAsync(Category category, UpdateCategoryDto categoryDto);
    void AddCategory(Category category);
    void AddItem(Item item);
    Task<List<Item>> SearchItemsAsync(string query, string familyName);

    Task<bool> SaveChangesAsync();
    public Task<IDbContextTransaction> BeginTransactionAsync();

}

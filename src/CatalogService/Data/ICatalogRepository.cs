using System;
using CatalogService.DTOs;
using CatalogService.Entities;

namespace CatalogService.Data;

public interface ICatalogRepository
{
    Task<List<CategoryDto>> GetAllCategoriesAsync();
    Task<CategoryDto> GetCategoryAsync(Guid id);
    Task<Category> GetCategoryEntityById(Guid id);

    void AddCategory(Category category);

    void RemoveCategory(Category category);

    Task<bool> SaveChangesAsync();

}

using System;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CatalogService.DTOs;
using CatalogService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CatalogService.Data;

public class CatalogRepository : ICatalogRepository
{
    private readonly IMapper _mapper;
    private readonly CatalogDbContext _context;
    public CatalogRepository(CatalogDbContext context, IMapper mapper)
    {
        _mapper = mapper;
        _context = context;

    }
    public void AddCategory(Category category)
    {
        _context.Categories.Add(category);
    }

    public void AddItem(Item item)
    {
        _context.Items.Add(item);
    }

    public async Task<List<CategoryDto>> GetAllCategoriesAsync(string familyName)
    {
        return await _context.Categories.AsQueryable()
            .Where(c => c.Family == familyName && !c.IsDeleted)
            .Include(x => x.Items.Where(i => !i.IsDeleted))
            .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

    }

    public async Task<List<ItemDto>> GetAllItemsAsync(string familyName)
    {
        return await _context.Items.AsQueryable()
            .Where(c => c.Family == familyName && !c.IsDeleted)
           .Include(x => x.Category)
           .ProjectTo<ItemDto>(_mapper.ConfigurationProvider)
           .ToListAsync();
    }

    public async Task<CategoryDto> GetCategoryBySkuAsync(Guid sku, string familyName)
    {
        return await _context.Categories.AsQueryable()
            .Where(c => c.Family == familyName && !c.IsDeleted)
            .Include(x => x.Items.Where(i => !i.IsDeleted))
            .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(x => x.SKU == sku);

    }

    public async Task<Category> GetCategoryEntityBySku(Guid sku, string familyName)
    {
        return await _context.Categories.AsQueryable()
            .Where(c => c.Family == familyName && !c.IsDeleted)
            .Include(x => x.Items.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(x => x.SKU == sku);
    }

    public async Task<Category> GetCategoryEntityByName(string name, string familyName)
    {
        string normalizedString = name.ToLower().Trim();

        return await _context.Categories.AsQueryable()
        .FirstOrDefaultAsync(x => x.Name.ToLower() == normalizedString
        && x.Family == familyName && !x.IsDeleted);
    }

    public async Task<ItemDto> GetItemBySkuAsync(Guid sku, string familyName)
    {
        return await _context.Items.AsQueryable()
           .Where(c => !c.IsDeleted && c.Family == familyName)
           .Include(x => x.Category)
           .ProjectTo<ItemDto>(_mapper.ConfigurationProvider)
           .FirstOrDefaultAsync(x => x.SKU == sku);
    }

    public async Task<Item> GetItemEntityBySkuAsync(Guid sku, string familyName)
    {
        return await _context.Items.AsQueryable()
                .Where(c => !c.IsDeleted && c.Family == familyName)
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.SKU == sku);
    }

    public async Task<Item> GetItemEntityByNameAsync(string name, string familyName)
    {
        string normalizedString = name.ToLower().Trim();
        return await _context.Items.AsQueryable()
                .Where(c => !c.IsDeleted && c.Family == familyName)
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Name.ToLower().Trim() == normalizedString);
    }

    public async Task UpdateItemAsync(Item item, UpdateItemDto itemDto)
    {
        if (item.CategorySKU != itemDto.CategorySKU)
        {
            // Update CategorySKU and explicitly attach the new category
            item.CategorySKU = itemDto.CategorySKU;

            Category newCategory = await _context.Categories.FirstOrDefaultAsync(x => x.SKU == itemDto.CategorySKU);
            if (newCategory != null)
            {
                _context.Entry(newCategory).State = EntityState.Unchanged; // Attach new category
                item.Category = newCategory;
                item.CategoryName = newCategory.Name;
            }
        }

        // Update other properties
        if (!string.IsNullOrEmpty(itemDto.Name))
        {
            item.Name = itemDto.Name;
        }

        // Explicitly mark Item as modified
        _context.Entry(item).State = EntityState.Modified;
    }

    public async Task UpdateCategoryAsync(Category category, UpdateCategoryDto categoryDto)
    {
        category.Name = categoryDto.Name ?? category.Name;
        List<Item> items = await _context.Items.Where((i) => i.CategoryId == category.Id).ToListAsync();
        foreach (Item item in items)
        {
            item.CategoryName = categoryDto.Name ?? category.Name;
            _context.Items.Update(item);
        }
    }

    public async Task<List<Item>> SearchItemsAsync(string query, string family)
    {
        // query the database if the cache is empty
        var formattedQuery = query.Replace(" ", " | "); // Convert query into tsquery format

        var dbQuerySearchResult = await _context.Items
        .FromSqlInterpolated(
        $@"SELECT * 
        FROM ""Items""
        WHERE ""SearchVector"" @@ plainto_tsquery('english', {formattedQuery})
        OR ""Name""::text % {query} OR ""CategoryName""::text % {query}
        AND ""Family"" = {family}
        ORDER BY GREATEST(similarity(""Name"", {query}), similarity(""CategoryName"", {query})) DESC
        LIMIT 10")
        .ToListAsync();

        return dbQuerySearchResult;
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }

}
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

            var newCategory = await _context.Categories.FirstOrDefaultAsync(x => x.SKU == itemDto.CategorySKU);
            if (newCategory != null)
            {
                _context.Entry(newCategory).State = EntityState.Unchanged; // Attach new category
                item.Category = newCategory;
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

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }

}
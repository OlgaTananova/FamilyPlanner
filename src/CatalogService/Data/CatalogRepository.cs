using System;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CatalogService.DTOs;
using CatalogService.Entities;
using Microsoft.EntityFrameworkCore;

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

    public async Task<List<CategoryDto>> GetAllCategoriesAsync()
    {
        return await _context.Categories.AsQueryable()
            .Where(c => !c.IsDeleted)
            .Include(x => x.Items.Where(i => !i.IsDeleted))
            .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

    }

    public async Task<List<ItemDto>> GetAllItemsAsync()
    {
        return await _context.Items.AsQueryable()
           .Where(c => !c.IsDeleted)
           .Include(x => x.Category)
           .ProjectTo<ItemDto>(_mapper.ConfigurationProvider)
           .ToListAsync();
    }

    public async Task<CategoryDto> GetCategoryByIdAsync(Guid id)
    {
        return await _context.Categories.AsQueryable()
            .Where(c => !c.IsDeleted)
            .Include(x => x.Items.Where(i => !i.IsDeleted))
            .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(x => x.Id == id);

    }

    public async Task<Category> GetCategoryEntityById(Guid id)
    {
        return await _context.Categories.AsQueryable()
            .Where(x => !x.IsDeleted)
            .Include(x => x.Items.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Category> GetCategoryEntityByName(string name)
    {
        string normalizedString = name.ToLower().Trim();

        return await _context.Categories.AsQueryable()
        .FirstOrDefaultAsync(x => x.Name.ToLower() == normalizedString);
    }

    public async Task<ItemDto> GetItemByIdAsync(Guid id)
    {
        return await _context.Items.AsQueryable()
           .Where(c => !c.IsDeleted)
           .Include(x => x.Category)
           .ProjectTo<ItemDto>(_mapper.ConfigurationProvider)
           .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Item> GetItemEntityByIdAsync(Guid id)
    {
        return await _context.Items.AsQueryable()
                .Where(c => !c.IsDeleted)
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Item> GetItemEntityByNameAsync(string name)
    {
        string normalizedString = name.ToLower().Trim();
        return await _context.Items.AsQueryable()
                .Where(c => !c.IsDeleted)
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Name.ToLower().Trim() == normalizedString);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }

}

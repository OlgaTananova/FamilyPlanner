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

    public async Task<List<CategoryDto>> GetAllCategoriesAsync()
    {
            var categories = await _context.Categories
            .Where(c => !c.IsDeleted)
            .Include(x => x.Items.Where(i => !i.IsDeleted))
            .ToListAsync();
            return _mapper.Map<List<CategoryDto>>(categories);
    }

    public async Task<CategoryDto> GetCategoryByIdAsync(Guid id)
    {
        return await _context.Categories
            .Where(c => !c.IsDeleted)
            .Include(x => x.Items.Where(i => !i.IsDeleted))
            .ProjectTo<CategoryDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Category> GetCategoryEntityById(Guid id)
    {
        return await _context.Categories
            .Where(x => !x.IsDeleted)
            .Include(x => x.Items.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public void RemoveCategory(Category category)
    {
        
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}

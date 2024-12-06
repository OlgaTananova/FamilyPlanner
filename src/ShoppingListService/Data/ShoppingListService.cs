using System;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using ShoppingListService.DTOs;
using ShoppingListService.Entities;

namespace ShoppingListService.Data;

public class ShoppingListService : IShoppingListService
{
    private readonly ShoppingListContext _dbcontext;
    private readonly IMapper _mapper;

    public ShoppingListService(ShoppingListContext context, IMapper mapper)
    {
        _dbcontext = context;
        _mapper = mapper;
    }
    public async Task<List<CatalogItemDto>> GetCatalogItemsAsync(string family)
    {
        return await _dbcontext.CatalogItems.AsQueryable()
        .Where(x => x.Family == family && !x.IsDeleted)
        .ProjectTo<CatalogItemDto>(_mapper.ConfigurationProvider)
        .ToListAsync();
    }
    public void AddShoppingList()
    {
        throw new NotImplementedException();
    }

    public void AddShoppingListItem()
    {
        throw new NotImplementedException();
    }


    public Task<bool> SaveChangesAsync()
    {
        throw new NotImplementedException();
    }
}

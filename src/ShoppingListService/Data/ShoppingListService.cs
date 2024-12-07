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
    public void AddShoppingList(ShoppingList list)
    {
        _dbcontext.ShoppingLists.Add(list);
    }

    public void AddShoppingListItem(ShoppingListItem item)
    {
        _dbcontext.ShoppingListItems.Add(item);
    }


    public async Task<bool> SaveChangesAsync()
    {
        return await _dbcontext.SaveChangesAsync() > 0;
    }

    public async Task<ShoppingList> GetShoppingListById(Guid id, string family)
    {
        return await _dbcontext.ShoppingLists
            .Where(c => c.Family == family && !c.IsDeleted)
            .Include(x => x.Items.Where(i => !i.IsOrphaned))
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<CatalogItem> GetCatalogItemBySKU(Guid sku, string family)
    {
        return await _dbcontext.CatalogItems.FirstOrDefaultAsync(c =>
        !c.IsDeleted && c.SKU == sku && c.Family == family);
    }

    public async Task UpdateShoppingList(ShoppingList list, UpdateShoppingListDto dto)
    {

        list.SalesTax = dto.SalesTax;
        list.Heading = dto.Heading;
        list.IsArchived = dto.IsArchived;
        if (list.IsArchived)
        {
            var shoppingItems = await _dbcontext.ShoppingListItems.Include(i => i.CatalogItem).Where(x => x.ShoppingListId == list.Id).ToListAsync();
            shoppingItems.ForEach(i =>
            {
                i.CatalogItem.Count++;
                i.Status = Status.Finished;
            });
        }
        _dbcontext.ShoppingLists.Update(list);

    }

    public async Task<ShoppingListItem> GetShoppingListItemById(Guid id, Guid shoppingListId, string family)
    {
        return await _dbcontext.ShoppingListItems
        .Include(c => c.ShoppingList)
        .Include(c => c.CatalogItem)
        .FirstOrDefaultAsync(c => c.Id == id && c.Family == family && c.ShoppingListId == shoppingListId);
    }

// TODO: add validation and logic
    public void UpdateShoppingListItem(ShoppingListItem item, UpdateShoppingListItemDto dto)
    {
        item.Unit = dto.Unit;
        item.PricePerUnit = dto.PricePerUnit;
        item.Price = dto.Price;
        item.Quantity = dto.Quantity;
        item.Status = dto.Status;

        if (item.Status == Status.Finished)
        {
            item.CatalogItem.Count++;
        }
        _dbcontext.ShoppingListItems.Update(item);
    }
}

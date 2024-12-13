using System;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ShoppingListService.DTOs;
using ShoppingListService.Entities;

namespace ShoppingListService.Data;

public class ShoppingListService : IShoppingListService
{
    private readonly ShoppingListContext _dbcontext;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;

    public ShoppingListService(ShoppingListContext context, IMapper mapper, IMemoryCache cache)
    {
        _dbcontext = context;
        _mapper = mapper;
        _cache = cache;
    }
    public async Task<List<CatalogItem>> GetCatalogItemsAsync(string family)
    {
        return await _dbcontext.CatalogItems.AsQueryable()
        .Where(x => x.Family == family && !x.IsDeleted)
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

        // Update properties if they are not null
        if (!string.IsNullOrEmpty(dto.Heading))
        {
            list.Heading = dto.Heading;
        }

        if (dto.SalesTax.HasValue)
        {
            list.SalesTax = dto.SalesTax.Value;
        }

        if (dto.IsArchived.HasValue)
        {
            list.IsArchived = dto.IsArchived.Value;

            if (list.IsArchived)
            {
                var shoppingItems = await _dbcontext.ShoppingListItems
                    .Include(i => i.CatalogItem)
                    .Where(x => x.ShoppingListId == list.Id)
                    .ToListAsync();

                shoppingItems.ForEach(i =>
                {
                    i.CatalogItem.Count += (int)i.Quantity;
                    i.Status = Status.Finished;
                });
            }
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
        // Parse and update Unit
        if (!string.IsNullOrEmpty(dto.Unit) && Enum.TryParse(typeof(Units), dto.Unit, true, out var parsedUnit))
        {
            item.Unit = (Units)parsedUnit;
        }

        // Parse and update Status
        if (!string.IsNullOrEmpty(dto.Status) && Enum.TryParse(typeof(Status), dto.Status, true, out var parsedStatus))
        {
            item.Status = (Status)parsedStatus;
        }

        // Update other properties if they are not null
        if (dto.Quantity.HasValue) item.Quantity = dto.Quantity.Value;
        if (dto.PricePerUnit.HasValue) item.PricePerUnit = dto.PricePerUnit.Value;
        if (dto.Price.HasValue) item.Price = dto.Price.Value;

        // Recalculate price if necessary
        if (dto.PricePerUnit.HasValue || dto.Quantity.HasValue)
        {
            item.Price = item.PricePerUnit * item.Quantity;
        }
        else if (dto.Price.HasValue && item.Quantity >= 0)
        {
            item.PricePerUnit = item.Price / item.Quantity;
        }

        if (item.Status == Status.Finished)
        {
            item.CatalogItem.Count += (int)item.Quantity;
        }
        _dbcontext.ShoppingListItems.Update(item);
    }

    public void DeleteShoppingListItem(ShoppingListItem item)
    {
        _dbcontext.ShoppingListItems.Remove(item);

        if (item.Status == Status.Finished)
        {
            item.CatalogItem.Count -= (int)item.Quantity;
        }
    }

    public void DeleteShoppingList(ShoppingList list)
    {
        list.IsDeleted = true;
        list.Items.ForEach(x =>
        {
            x.IsOrphaned = true;
        });
        _dbcontext.ShoppingLists.Update(list);
    }

    public Task<List<ShoppingList>> GetShoppingListsAsync(string family)
    {
        return _dbcontext.ShoppingLists.Include(x => x.Items)
        .Where(x => !x.IsDeleted && x.Family == family)
        .OrderByDescending(x => x.CreatedAt)
        .ToListAsync();
    }

    public async Task<List<CatalogItem>> GetFrequentlyBoughtItemsAsync(string family)
    {
        // Define a cache key
        const string cacheKey = "FrequentlyBoughtItems";

        // Try to get the data from cache
        if (!_cache.TryGetValue(cacheKey, out List<CatalogItem> frequentlyBoughtItems))
        {
            // Data not in cache, fetch from the database
            frequentlyBoughtItems = await _dbcontext.CatalogItems
                .Where(x => x.Family == family && !x.IsDeleted)
                .OrderByDescending(c => c.Count) // Order by Count descending
                .Take(10)                        // Get the top 10 items
                .ToListAsync();

            // Set cache options
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(10)) // Cache expires after 5 minutes of inactivity
                .SetAbsoluteExpiration(TimeSpan.FromHours(1)); // Cache expires absolutely after 1 hour

            // Save data in cache
            _cache.Set(cacheKey, frequentlyBoughtItems, cacheEntryOptions);
        }

        return frequentlyBoughtItems;
    }


    public async Task<List<CatalogItem>> AutocompleteCatalogItemsAsync(string query, string family)
    {

        // Get query result from the cache
        const string cacheKeyPrefix = "Autocomplete_";
        string cacheKey = $"{cacheKeyPrefix}{query.ToLower()}";

        if (_cache.TryGetValue(cacheKey, out List<CatalogItem> cachedItems))
        {
            Console.WriteLine("Return the data from the cache");
            return cachedItems;
        }

        // query the database if the cache is empty
        var formattedQuery = query.Replace(" ", " | "); // Convert query into tsquery format

        var dbQuerySearchResult = await _dbcontext.CatalogItems
            .FromSqlInterpolated(
            $@"SELECT * 
                FROM ""CatalogItems""
                WHERE ""SearchVector"" @@ plainto_tsquery('english', {formattedQuery})
                OR ""Name"" % {query} OR ""CategoryName"" % {query}
                AND ""Family"" = {family}
                ORDER BY GREATEST(similarity(""Name"", {query}), similarity(""CategoryName"", {query})) DESC
                LIMIT 10")
            .ToListAsync();

        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(5))
            .SetAbsoluteExpiration(TimeSpan.FromHours(1));

        _cache.Set(cacheKey, dbQuerySearchResult, cacheEntryOptions);

        return dbQuerySearchResult;
    }

    public async Task<List<CatalogItem>> GetCatalogItemsBySKUsAsync(List<Guid> skus, string familyName)
    {
        return await _dbcontext.CatalogItems
            .Where(c => skus.Contains(c.SKU) && c.Family == familyName && !c.IsDeleted)
            .ToListAsync();
    }
}

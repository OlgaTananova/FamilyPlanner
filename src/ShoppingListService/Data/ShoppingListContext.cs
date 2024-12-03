using System;
using Microsoft.EntityFrameworkCore;
using ShoppingListService.Entities;

namespace ShoppingListService.Data;

public class ShoppingListContext : DbContext
{

    public ShoppingListContext(DbContextOptions options) : base(options)
    {

    }
    DbSet<CatalogItem> CatalogItems { get; set; }
    DbSet<ShoppingList> ShoppingLists { get; set; }
    DbSet<ShoppingListItem> ShoppingListItems { get; set; }

}

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Migrations;
using ShoppingListService.Entities;
using ShoppingListService.Helpers;

namespace ShoppingListService.Data;

public class ShoppingListContext : DbContext
{

    public ShoppingListContext(DbContextOptions options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShoppingList>()
            .HasIndex(sl => new { sl.OwnerId, sl.Family });

        modelBuilder.Entity<ShoppingListItem>()
            .HasIndex(sli => new { sli.ShoppingListId, sli.CatalogItemId });

    }



    public DbSet<CatalogItem> CatalogItems { get; set; }
    public DbSet<ShoppingList> ShoppingLists { get; set; }
    public DbSet<ShoppingListItem> ShoppingListItems { get; set; }

}

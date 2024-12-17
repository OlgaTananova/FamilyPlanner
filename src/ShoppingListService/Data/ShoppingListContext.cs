using System;
using MassTransit;
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
        // support for outbox pattern 
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        modelBuilder.Entity<ShoppingList>()
            .HasIndex(sl => new { sl.OwnerId, sl.Family });

        modelBuilder.Entity<ShoppingListItem>()
            .HasIndex(sli => new { sli.ShoppingListId, sli.CatalogItemId });

        modelBuilder.Entity<CatalogItem>()
        .HasGeneratedTsVectorColumn(
            p => p.SearchVector,
            "english",  // Text search config
            p => new { p.Name, p.CategoryName })  // Included properties
        .HasIndex(p => p.SearchVector)
        .HasMethod("GIN"); // Index method on the search vector (GIN or GIST)

    }



    public DbSet<CatalogItem> CatalogItems { get; set; }
    public DbSet<ShoppingList> ShoppingLists { get; set; }
    public DbSet<ShoppingListItem> ShoppingListItems { get; set; }

}

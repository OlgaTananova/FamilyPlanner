using System;
using Microsoft.EntityFrameworkCore;
using ShoppingListService.Data;
using ShoppingListService.Entities;

namespace ShoppingListService.IntegrationTests.Utils;

public class DbHelper
{
    private static readonly string _ownerId = "test-user-id";
    private static readonly string _family = "test-family";

    public static void InitDbForTests(ShoppingListContext context)
    {
        context.Database.ExecuteSqlRaw("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
        context.CatalogItems.AddRange(GetCatalogItemsForTests());
        context.SaveChanges();
    }

    public static void ReinitDbForTests(ShoppingListContext context)
    {
        // Clear all categories and related items
        context.ShoppingListItems.RemoveRange(context.ShoppingListItems);
        context.CatalogItems.RemoveRange(context.CatalogItems);
        context.ShoppingLists.RemoveRange(context.ShoppingLists);
        context.SaveChanges();

        // Reinitialize the database
        InitDbForTests(context);

    }

    private static List<CatalogItem> GetCatalogItemsForTests()
    {
        List<CatalogItem> catalogItems = new() {
            new(){
                SKU = Guid.Parse("57134ac7-5446-4377-88f4-b0f9b71bf57f"),
                Name = "Apples",
                CategorySKU = Guid.Parse("6b0eaee4-c2b8-481b-98d7-ff73b4f4f8d7"),
                CategoryName = "Veggies",
                IsDeleted = false,
                Family = _family,
                OwnerId = _ownerId,
            },
            new() {
                SKU = Guid.Parse("6b0eaee4-c2b8-481b-98d7-ff73b4f4f8d7"),
                Name = "Bananas",
                CategorySKU = Guid.Parse("6b0eaee4-c2b8-481b-98d7-ff73b4f4f8d7"),
                CategoryName = "Veggies",
                IsDeleted = false,
                Family = _family,
                OwnerId = _ownerId,
            },
            new(){
                SKU = Guid.Parse("ef7df054-c821-45ff-8633-e83066a17fed"),
                Name = "Coffee",
                CategorySKU = Guid.Parse("c93953b1-e58e-467b-9c25-8cecdc97eb2a"),
                CategoryName = "Beverages",
                IsDeleted = false,
                Family = _family,
                OwnerId = _ownerId,
            }
          };

        return catalogItems;

    }

}

using CatalogService.Data;
using CatalogService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.IntegrationTests.Utils;

public static class DbHelper
{
    private static readonly string _ownerId = "test-user-id";
    private static readonly string _family = "test-family";
    public static void InitDbForTests(CatalogDbContext context)
    {
        // Execute the raw SQL to create the extension
        context.Database.ExecuteSqlRaw("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
        context.Categories.AddRange(GetCategoriesForTest());
        context.SaveChanges();
    }
    public static void ReinitDbForTests(CatalogDbContext context)
    {
        // Clear all categories and related items
        context.Items.RemoveRange(context.Items);
        context.Categories.RemoveRange(context.Categories);
        context.SaveChanges();

        // Reinitialize the database
        InitDbForTests(context);

    }

    private static List<Category> GetCategoriesForTest()
    {
        List<Category> categories = new List<Category>() {
            new() {
        Id = Guid.Parse("02d928a0-d6cb-4686-a5dd-7c180db3868a"),
        Name = "Veggies",
        SKU = Guid.Parse("1625f681-af55-4f80-a88f-c65b666d701d"),
        OwnerId = _ownerId,
        Family = _family,
        Items = new List<Item>
        {
            new() { Id = Guid.Parse("ac23ec89-92f5-4299-9872-7dacc7c966bd"), SKU= Guid.Parse("f4f69463-467c-4875-aead-2ec4cf6d7ead"), Name = "Apples", OwnerId = _ownerId, Family=_family },
            new() { Id = Guid.NewGuid(), Name = "Bananas", OwnerId = _ownerId, Family=_family },
            new() { Id = Guid.NewGuid(), Name = "Oranges", OwnerId = _ownerId, Family=_family}
        }
    },
    new() {
        Id = new Guid(),
        Name = "Dairy",
        OwnerId = _ownerId,
        Family = _family,
        Items = new List<Item>
        {
            new() { Id = Guid.NewGuid(), Name = "Milk", OwnerId = _ownerId,Family=_family  },
            new() { Id = Guid.NewGuid(), Name = "Cheese", OwnerId = _ownerId, Family=_family  },
            new() { Id = Guid.NewGuid(), Name = "Yogurt", OwnerId = _ownerId, Family=_family }
        }
    },
    new() {
        Id = Guid.Parse("ac23ec89-92f5-4299-9872-7dacc7c966bd"),
        SKU=Guid.Parse("ac23ec89-92f5-4299-9872-7dacc7c966bd"),
        Name = "Bakery",
        OwnerId = _ownerId,
        Family=_family,
        Items = new List<Item>
        {
            new() { Id = Guid.NewGuid(), Name = "Bread", OwnerId = _ownerId, Family=_family  },
            new() { Id = Guid.NewGuid(), Name = "Bagels", OwnerId = _ownerId, Family=_family  },
            new() { Id = Guid.NewGuid(), Name = "Donuts", OwnerId = _ownerId, Family=_family  }
        }
    },
        };

        // Populate CategorySKU and CategoryName for all items
        foreach (var category in categories)
        {
            foreach (var item in category.Items)
            {
                item.CategoryId = category.Id;
                item.CategorySKU = category.SKU;
                item.CategoryName = category.Name;
            }
        }
        return categories;
    }
}

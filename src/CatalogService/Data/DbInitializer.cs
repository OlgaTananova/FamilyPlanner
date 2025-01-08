using System;
using System.Security.Cryptography;
using CatalogService.Data.Migrations;
using CatalogService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Data;

public static class DbInitializer
{
    private static readonly string _ownerId = "63e6299d-567c-49fc-a0e4-4d53e7ca1dd5";
    private static readonly string _family = "Smith";
    public static async void InitDb(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        await SeedData(scope.ServiceProvider.GetService<CatalogDbContext>());
        AddSearchingFunction(scope.ServiceProvider.GetService<CatalogDbContext>());
    }

    private static async Task AddCaterotyNamesToItems(CatalogDbContext context)
    {
        var items = context.Items.Include(i => i.Category).ToList();
        foreach (var item in items)
        {
            item.CategoryName = item.Category.Name;
            item.CategorySKU = item.Category.SKU;
        }

        await context.SaveChangesAsync();
    }

    private static void AddSearchingFunction(CatalogDbContext context)
    {
        context.Database.EnsureCreated();

        // Execute the raw SQL to create the extension
        context.Database.ExecuteSqlRaw("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
    }

    private static async Task SeedData(CatalogDbContext context)
    {
        context.Database.Migrate();

        if (context.Categories.Any())
        {
            Console.WriteLine("The database already exists.");
            return;
        }
        var categories = new List<Category>
{
    new Category
    {
        Id = Guid.NewGuid(),
        Name = "Veggies",
        OwnerId = _ownerId,
        Family = _family,
        Items = new List<Item>
        {
            new Item { Id = Guid.NewGuid(), Name = "Apples", OwnerId = _ownerId, Family=_family },
            new Item { Id = Guid.NewGuid(), Name = "Bananas", OwnerId = _ownerId, Family=_family },
            new Item { Id = Guid.NewGuid(), Name = "Oranges", OwnerId = _ownerId, Family=_family}
        }
    },
    new Category
    {
        Id = Guid.NewGuid(),
        Name = "Dairy",
        OwnerId = _ownerId,
        Family = _family,
        Items = new List<Item>
        {
            new Item { Id = Guid.NewGuid(), Name = "Milk", OwnerId = _ownerId,Family=_family  },
            new Item { Id = Guid.NewGuid(), Name = "Cheese", OwnerId = _ownerId, Family=_family  },
            new Item { Id = Guid.NewGuid(), Name = "Yogurt", OwnerId = _ownerId, Family=_family }
        }
    },
    new Category
    {
        Id = Guid.NewGuid(),
        Name = "Bakery",
        OwnerId = _ownerId,
        Family=_family,
        Items = new List<Item>
        {
            new Item { Id = Guid.NewGuid(), Name = "Bread", OwnerId = _ownerId, Family=_family  },
            new Item { Id = Guid.NewGuid(), Name = "Bagels", OwnerId = _ownerId, Family=_family  },
            new Item { Id = Guid.NewGuid(), Name = "Donuts", OwnerId = _ownerId, Family=_family  }
        }
    },
    new Category
    {
        Id = Guid.NewGuid(),
        Name = "Meat",
        OwnerId = _ownerId,
        Family=_family,
        Items = new List<Item>
        {
            new Item { Id = Guid.NewGuid(), Name = "Chicken Breast", OwnerId = _ownerId, Family=_family  },
            new Item { Id = Guid.NewGuid(), Name = "Ground Beef", OwnerId = _ownerId, Family=_family  },
            new Item { Id = Guid.NewGuid(), Name = "Pork Chops", OwnerId = _ownerId, Family=_family }
        }
    },
    new Category
    {
        Id = Guid.NewGuid(),
        Name = "Beverages",
        OwnerId = _ownerId,
        Family=_family,
        Items = new List<Item>
        {
            new Item { Id = Guid.NewGuid(), Name = "Orange Juice", OwnerId = _ownerId, Family=_family  },
            new Item { Id = Guid.NewGuid(), Name = "Soda", OwnerId = _ownerId, Family=_family  },
            new Item { Id = Guid.NewGuid(), Name = "Coffee", OwnerId = _ownerId, Family=_family  }
        }
    },
    new Category
    {
        Id = Guid.NewGuid(),
        Name = "Frozen Foods",
        OwnerId = _ownerId,
        Family=_family,
        Items = new List<Item>
        {
            new Item { Id = Guid.NewGuid(), Name = "Frozen Pizza",OwnerId = _ownerId, Family=_family  },
            new Item { Id = Guid.NewGuid(), Name = "Ice Cream", OwnerId = _ownerId, Family=_family  }
            }
    }
};
        context.AddRange(categories);

        await context.SaveChangesAsync();

    }
}


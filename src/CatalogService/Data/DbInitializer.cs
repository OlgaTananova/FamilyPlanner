using System;
using CatalogService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Data;

public static class DbInitializer
{
    public static async void InitDb(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        await SeedData(scope.ServiceProvider.GetService<CatalogDbContext>());
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
        OwnerId = "owner1",
        Items = new List<Item>
        {
            new Item { Id = Guid.NewGuid(), Name = "Apples", OwnerId = "owner1" },
            new Item { Id = Guid.NewGuid(), Name = "Bananas", OwnerId = "owner1" },
            new Item { Id = Guid.NewGuid(), Name = "Oranges", OwnerId = "owner1" }
        }
    },
    new Category
    {
        Id = Guid.NewGuid(),
        Name = "Dairy",
        OwnerId = "owner1",
        Items = new List<Item>
        {
            new Item { Id = Guid.NewGuid(), Name = "Milk", OwnerId = "owner1" },
            new Item { Id = Guid.NewGuid(), Name = "Cheese", OwnerId = "owner1" },
            new Item { Id = Guid.NewGuid(), Name = "Yogurt", OwnerId = "owner1" }
        }
    },
    new Category
    {
        Id = Guid.NewGuid(),
        Name = "Bakery",
        OwnerId = "owner1",
        Items = new List<Item>
        {
            new Item { Id = Guid.NewGuid(), Name = "Bread", OwnerId = "owner1" },
            new Item { Id = Guid.NewGuid(), Name = "Bagels", OwnerId = "owner1" },
            new Item { Id = Guid.NewGuid(), Name = "Donuts", OwnerId = "owner1" }
        }
    },
    new Category
    {
        Id = Guid.NewGuid(),
        Name = "Meat",
        OwnerId = "owner2",
        Items = new List<Item>
        {
            new Item { Id = Guid.NewGuid(), Name = "Chicken Breast", OwnerId = "owner2" },
            new Item { Id = Guid.NewGuid(), Name = "Ground Beef", OwnerId = "owner2" },
            new Item { Id = Guid.NewGuid(), Name = "Pork Chops", OwnerId = "owner2" }
        }
    },
    new Category
    {
        Id = Guid.NewGuid(),
        Name = "Beverages",
        OwnerId = "owner3",
        Items = new List<Item>
        {
            new Item { Id = Guid.NewGuid(), Name = "Orange Juice", OwnerId = "owner3" },
            new Item { Id = Guid.NewGuid(), Name = "Soda", OwnerId = "owner3" },
            new Item { Id = Guid.NewGuid(), Name = "Coffee", OwnerId = "owner3" }
        }
    },
    new Category
    {
        Id = Guid.NewGuid(),
        Name = "Frozen Foods",
        OwnerId = "owner3",
        Items = new List<Item>
        {
            new Item { Id = Guid.NewGuid(), Name = "Frozen Pizza", OwnerId = "owner3" },
            new Item { Id = Guid.NewGuid(), Name = "Ice Cream", OwnerId = "owner3" },
            new Item { Id = Guid.NewGuid(), Name = "Vegetables", OwnerId = "owner3" }
        }
    }
};
        context.AddRange(categories);

        await context.SaveChangesAsync();
    }
}

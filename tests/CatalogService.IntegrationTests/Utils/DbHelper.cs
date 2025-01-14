using System;
using CatalogService.Data;
using CatalogService.Entities;

namespace CatalogService.IntegrationTests.Utils;

public static class DbHelper
{
    private static readonly string _ownerId = "test-user-id";
    private static readonly string _family = "test-family";
    public static void InitDbForTests(CatalogDbContext context)
    {
        context.Categories.AddRange(GetCategoriesForTest());
        context.SaveChanges();
    }
    public static void ReinitDbForTests(CatalogDbContext context)
    {
        context.Categories.RemoveRange(context.Categories);
        context.SaveChanges();
        InitDbForTests(context);
    }

    private static List<Category> GetCategoriesForTest()
    {
        return new List<Category>() {
            new() {
        Id = Guid.NewGuid(),
        Name = "Veggies",
        OwnerId = _ownerId,
        Family = _family,
        Items = new List<Item>
        {
            new() { Id = Guid.NewGuid(), Name = "Apples", OwnerId = _ownerId, Family=_family },
            new() { Id = Guid.NewGuid(), Name = "Bananas", OwnerId = _ownerId, Family=_family },
            new() { Id = Guid.NewGuid(), Name = "Oranges", OwnerId = _ownerId, Family=_family}
        }
    },
    new() {
        Id = Guid.NewGuid(),
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
        Id = Guid.NewGuid(),
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
    }
}

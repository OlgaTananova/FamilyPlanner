using AutoMapper;
using CatalogService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Data;

public static class DbInitializer
{
    private static readonly string _ownerId = "bda6a5a5-5538-409a-af44-1f64d7ee4468";
    private static readonly string _family = "Smith";

    public static bool isNewDatabase;
    public static async Task InitDb(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        try
        {
            isNewDatabase = await SeedData(context);

            if (isNewDatabase)
            {
                await AddCategoryNamesToItems(context);
                AddSearchingFunction(context);
                Console.WriteLine("Database initialized.");
            }
            else
            {
                Console.WriteLine("Database already exists. Skipping initialization.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error during database initialization: {Message}", ex.Message);
        }
    }

    private static async Task AddCategoryNamesToItems(CatalogDbContext context)
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

    private static async Task<bool> SeedData(CatalogDbContext context)
    {
        context.Database.Migrate();

        if (context.Categories.Any())
        {
            Console.WriteLine("The database already exists.");
            return false;
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
        return true;

    }
}


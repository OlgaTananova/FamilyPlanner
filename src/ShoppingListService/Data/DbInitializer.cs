using System;
using Microsoft.EntityFrameworkCore;
using ShoppingListService.Entities;

namespace ShoppingListService.Data;

public static class DbInitializer
{
    private static readonly string _ownerId = "63e6299d-567c-49fc-a0e4-4d53e7ca1dd5";
    private static readonly string _family = "Smith";
    public static async void InitDb(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        await SeedData(scope.ServiceProvider.GetService<ShoppingListContext>());
    }

    private static async Task SeedData(ShoppingListContext context)
    {
        context.Database.Migrate();

        if (context.CatalogItems.Any())
        {
            Console.WriteLine("The database already exists.");
            return;
        }
        // Seed data
        var catalogItems = new List<CatalogItem>
        {
            new CatalogItem
            {
                Name = "Yogurt",
                OwnerId = _ownerId,
                Family = _family,
                IsDeleted = false,
                CategoryId = Guid.Parse("0f21a3b5-07a0-4a26-bde7-67f6c8fe7f82"),
                CategoryName = "Dairy"
            },
            new CatalogItem
            {
                Name = "Pork Chops",
                OwnerId = _ownerId,
                Family = _family,
                IsDeleted = false,
                CategoryId = Guid.Parse("5f76fb8b-9ee1-4e6d-96b0-0213d39da4cb"),
                CategoryName = "Meat"
            },
            new CatalogItem
            {
                Name = "Orange Juice",
                OwnerId = _ownerId,
                Family = _family,
                IsDeleted = false,
                CategoryId = Guid.Parse("e69b4645-8746-4483-9573-ff665a69d2d0"),
                CategoryName = "Beverages"
            },
            new CatalogItem
            {
                Name = "Coffee",
                OwnerId = _ownerId,
                Family = _family,
                IsDeleted = false,
                CategoryId = Guid.Parse("e69b4645-8746-4483-9573-ff665a69d2d0"),
                CategoryName = "Beverages"
            },
            new CatalogItem
            {
                Name = "Oranges",
                OwnerId = _ownerId,
                Family = _family,
                IsDeleted = false,
                CategoryId = Guid.Parse("451e720a-cc44-485a-8702-e8c138a9bf1c"),
                CategoryName = "Veggies"
            },
            new CatalogItem
            {
                Name = "Milk",
                OwnerId = _ownerId,
                Family = _family,
                IsDeleted = false,
                CategoryId = Guid.Parse("0f21a3b5-07a0-4a26-bde7-67f6c8fe7f82"),
                CategoryName = "Dairy"
            },
            new CatalogItem
            {
                Name = "Ground Beef",
                OwnerId = _ownerId,
                Family = _family,
                IsDeleted = false,
                CategoryId = Guid.Parse("5f76fb8b-9ee1-4e6d-96b0-0213d39da4cb"),
                CategoryName = "Meat"
            },
            new CatalogItem
            {
                Name = "Cheese",
                OwnerId = _ownerId,
                Family = _family,
                IsDeleted = false,
                CategoryId = Guid.Parse("0f21a3b5-07a0-4a26-bde7-67f6c8fe7f82"),
                CategoryName = "Dairy"
            },
            new CatalogItem
            {
                Name = "Frozen Pizza",
                OwnerId = _ownerId,
                Family = _family,
                IsDeleted = false,
                CategoryId = Guid.Parse("f1028df4-84fe-41d9-b041-baccfda00320"),
                CategoryName = "Frozen Foods"
            },
            new CatalogItem
            {
                Name = "Chicken Breast",
                OwnerId = _ownerId,
                Family = _family,
                IsDeleted = false,
                CategoryId = Guid.Parse("5f76fb8b-9ee1-4e6d-96b0-0213d39da4cb"),
                CategoryName = "Meat"
            }
        };



        context.AddRange(catalogItems);

        await context.SaveChangesAsync();

    }
}



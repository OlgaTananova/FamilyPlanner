using System;
using Microsoft.EntityFrameworkCore;
using ShoppingListService.Entities;

namespace ShoppingListService.Data;

public static class DbInitializer
{
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
    // Bakery Items
    new CatalogItem
    {
        SKU = Guid.Parse("527f2ca2-f74a-4b60-8fab-36c4ad7d2934"),
        Name = "Bread",
        CategorySKU = Guid.Parse("3f98e309-c502-4d5e-88b5-c8a9683b70ac"),
        CategoryName = "Bakery",
        IsDeleted = false,
        Family = "Smith",
        OwnerId = "63e6299d-567c-49fc-a0e4-4d53e7ca1dd5"
    },
    new CatalogItem
    {
        SKU = Guid.Parse("3033a951-c4b8-4e78-96fa-fc8b5990dfad"),
        Name = "Bagels",
        CategorySKU = Guid.Parse("3f98e309-c502-4d5e-88b5-c8a9683b70ac"),
        CategoryName = "Bakery",
        IsDeleted = false,
        Family = "Smith",
        OwnerId = "63e6299d-567c-49fc-a0e4-4d53e7ca1dd5"
    },
    new CatalogItem
    {
        SKU = Guid.Parse("85d1df9a-d4a7-4313-a3eb-c10ad83b877d"),
        Name = "Donuts",
        CategorySKU = Guid.Parse("3f98e309-c502-4d5e-88b5-c8a9683b70ac"),
        CategoryName = "Bakery",
        IsDeleted = false,
        Family = "Smith",
        OwnerId = "63e6299d-567c-49fc-a0e4-4d53e7ca1dd5"
    },
    // Veggies Items
    new CatalogItem
    {
        SKU = Guid.Parse("ddd5ab22-a017-4527-91b3-820908b29c7f"),
        Name = "Bananas",
        CategorySKU = Guid.Parse("72cf081f-f49b-4898-841e-4863bccbe54f"),
        CategoryName = "Veggies",
        IsDeleted = false,
        Family = "Smith",
        OwnerId = "63e6299d-567c-49fc-a0e4-4d53e7ca1dd5"
    },
    new CatalogItem
    {
        SKU = Guid.Parse("4a686669-e542-43c7-8fa0-4c2947736ae6"),
        Name = "Apples",
        CategorySKU = Guid.Parse("72cf081f-f49b-4898-841e-4863bccbe54f"),
        CategoryName = "Veggies",
        IsDeleted = false,
        Family = "Smith",
        OwnerId = "63e6299d-567c-49fc-a0e4-4d53e7ca1dd5"
    },
    new CatalogItem
    {
        SKU = Guid.Parse("4faf32d5-33da-4052-a984-a93fab3b9d86"),
        Name = "Oranges",
        CategorySKU = Guid.Parse("72cf081f-f49b-4898-841e-4863bccbe54f"),
        CategoryName = "Veggies",
        IsDeleted = false,
        Family = "Smith",
        OwnerId = "63e6299d-567c-49fc-a0e4-4d53e7ca1dd5"
    },
    // Meat Items
    new CatalogItem
    {
        SKU = Guid.Parse("e6c5e468-8c5b-49d0-963c-6c83f35c31ba"),
        Name = "Ground Beef",
        CategorySKU = Guid.Parse("1b0abc58-d487-44fe-8160-953c2045bd43"),
        CategoryName = "Meat",
        IsDeleted = false,
        Family = "Smith",
        OwnerId = "63e6299d-567c-49fc-a0e4-4d53e7ca1dd5"
    },
    new CatalogItem
    {
        SKU = Guid.Parse("e6120da3-e30f-4fec-8918-2adc8363ade4"),
        Name = "Chicken Breast",
        CategorySKU = Guid.Parse("1b0abc58-d487-44fe-8160-953c2045bd43"),
        CategoryName = "Meat",
        IsDeleted = false,
        Family = "Smith",
        OwnerId = "63e6299d-567c-49fc-a0e4-4d53e7ca1dd5"
    },
    new CatalogItem
    {
        SKU = Guid.Parse("4c890ef8-cb2f-4b65-9266-9134e29e0569"),
        Name = "Pork Chops",
        CategorySKU = Guid.Parse("1b0abc58-d487-44fe-8160-953c2045bd43"),
        CategoryName = "Meat",
        IsDeleted = false,
        Family = "Smith",
        OwnerId = "63e6299d-567c-49fc-a0e4-4d53e7ca1dd5"
    },
    // Frozen Foods Items
    new CatalogItem
    {
        SKU = Guid.Parse("7c9b979d-b769-48db-bc42-cd708091e802"),
        Name = "Ice Cream",
        CategorySKU = Guid.Parse("d01c2129-2788-45cd-88af-758d275cc94e"),
        CategoryName = "Frozen Foods",
        IsDeleted = false,
        Family = "Smith",
        OwnerId = "63e6299d-567c-49fc-a0e4-4d53e7ca1dd5"
    },
    new CatalogItem
    {
        SKU = Guid.Parse("a85bf144-c19c-4f30-9685-67491153c2ee"),
        Name = "Frozen Pizza",
        CategorySKU = Guid.Parse("d01c2129-2788-45cd-88af-758d275cc94e"),
        CategoryName = "Frozen Foods",
        IsDeleted = false,
        Family = "Smith",
        OwnerId = "63e6299d-567c-49fc-a0e4-4d53e7ca1dd5"
    },
    // Beverages Items
    new CatalogItem
    {
        SKU = Guid.Parse("f6a30693-18eb-402e-acd5-b9d666f2ad60"),
        Name = "Soda",
        CategorySKU = Guid.Parse("4bc4981e-43ec-443f-a53d-463b028ebb17"),
        CategoryName = "Beverages",
        IsDeleted = false,
        Family = "Smith",
        OwnerId = "63e6299d-567c-49fc-a0e4-4d53e7ca1dd5"
    },
    new CatalogItem
    {
        SKU = Guid.Parse("8b9763e0-801b-4894-901b-872bd129548b"),
        Name = "Orange Juice",
        CategorySKU = Guid.Parse("4bc4981e-43ec-443f-a53d-463b028ebb17"),
        CategoryName = "Beverages",
        IsDeleted = false,
        Family = "Smith",
        OwnerId = "63e6299d-567c-49fc-a0e4-4d53e7ca1dd5"
    },
    new CatalogItem
    {
        SKU = Guid.Parse("ad4a7d73-0411-485e-bd78-e58447ee8cc6"),
        Name = "Coffee",
        CategorySKU = Guid.Parse("4bc4981e-43ec-443f-a53d-463b028ebb17"),
        CategoryName = "Beverages",
        IsDeleted = false,
        Family = "Smith",
        OwnerId = "63e6299d-567c-49fc-a0e4-4d53e7ca1dd5"
    }
};

        context.AddRange(catalogItems);

        await context.SaveChangesAsync();

    }
}



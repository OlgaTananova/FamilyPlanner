using System;
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using ShoppingListService.Entities;

namespace ShoppingListService.Data;

public static class DbInitializer
{
    private static readonly string _family = "Smith";
    private static readonly string _ownerId = "63e6299d-567c-49fc-a0e4-4d53e7ca1dd5";
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
        SKU = Guid.Parse("028d927f-184b-4fc4-9a23-49f2336d4241"),
        Name = "Yogurt",
        CategorySKU = Guid.Parse("e5d2f9c0-32c9-48b7-aba6-0bb89e4b1a94"),
        CategoryName = "Diary",
        IsDeleted = false,
        Family = _family,
        OwnerId = _ownerId
    },
    new CatalogItem
    {
        SKU = Guid.Parse("852da8bd-e5e6-4e8f-a447-3a1375b618e3"),
        Name = "Chicken Breast",
        CategorySKU = Guid.Parse("f38b63dd-d576-4daf-8202-3c0cccc0ead6"),
        CategoryName = "Meat",
        IsDeleted = false,
        Family = _family,
        OwnerId = _ownerId
    },
      new CatalogItem
    {
        SKU = Guid.Parse("d138fdef-adf1-487c-9d0a-5e0eb5226a39"),
        Name = "Frozen Pizza",
        CategorySKU = Guid.Parse("b6fec0fa-122e-460e-b49c-0a06967ba324"),
        CategoryName = "Frozen Foods",
        IsDeleted = false,
        Family = _family,
        OwnerId = _ownerId
    },
          new CatalogItem
    {
        SKU = Guid.Parse("f61e2e05-47e8-4146-9032-93286d793251"),
        Name = "Ice Cream",
        CategorySKU = Guid.Parse("b6fec0fa-122e-460e-b49c-0a06967ba324"),
        CategoryName = "Frozen Foods",
        IsDeleted = false,
        Family = _family,
        OwnerId = _ownerId
    },
              new CatalogItem
    {
        SKU = Guid.Parse("3126be4e-c040-4e8c-a318-fed9dc3eaf7c"),
        Name = "Bananas",
        CategorySKU = Guid.Parse("558acd49-13a1-41ee-887c-88e36566a02c"),
        CategoryName = "Veggies",
        IsDeleted = false,
        Family = _family,
        OwnerId = _ownerId
    },
                  new CatalogItem
    {
        SKU = Guid.Parse("16f97254-2e8a-48b3-a117-8b80fc30971b"),
        Name = "Pork Chops",
        CategorySKU = Guid.Parse("f38b63dd-d576-4daf-8202-3c0cccc0ead6"),
        CategoryName = "Meat",
        IsDeleted = false,
        Family = _family,
        OwnerId = _ownerId
    },
                      new CatalogItem
    {
        SKU = Guid.Parse("66709923-55ba-4afc-a2b8-75ddadc916c5"),
        Name = "Bagels",
        CategorySKU = Guid.Parse("3458f4d8-6ef7-4bbb-8513-6dc1d99ccd5a"),
        CategoryName = "Bakery",
        IsDeleted = false,
        Family = _family,
        OwnerId = _ownerId
    },
                          new CatalogItem
    {
        SKU = Guid.Parse("abfd9d02-ecb9-4e4b-a25e-1c81580a8245"),
        Name = "Apples",
        CategorySKU = Guid.Parse("558acd49-13a1-41ee-887c-88e36566a02c"),
        CategoryName = "Veggies",
        IsDeleted = false,
        Family = _family,
        OwnerId = _ownerId
    },
                              new CatalogItem
    {
        SKU = Guid.Parse("e0372feb-2bbd-43f3-8d9c-0593b13fcdf2"),
        Name = "Oranges",
        CategorySKU = Guid.Parse("558acd49-13a1-41ee-887c-88e36566a02c"),
        CategoryName = "Veggies",
        IsDeleted = false,
        Family = _family,
        OwnerId = _ownerId
    },
                                      new CatalogItem
    {
        SKU = Guid.Parse("7e1cd557-10fd-4fc1-8833-91cd78bed6d3"),
        Name = "Orange Juice",
        CategorySKU = Guid.Parse("7376d09c-3bde-4fbd-a9e7-2556ddc8003b"),
        CategoryName = "Beverages",
        IsDeleted = false,
        Family = _family,
        OwnerId = _ownerId
    },
            new CatalogItem
    {
        SKU = Guid.Parse("d728def7-475d-482a-977d-9c67a3ec55a1"),
        Name = "Coffee",
        CategorySKU = Guid.Parse("7376d09c-3bde-4fbd-a9e7-2556ddc8003b"),
        CategoryName = "Beverages",
        IsDeleted = false,
        Family = _family,
        OwnerId = _ownerId
    },
        new CatalogItem
    {
        SKU = Guid.Parse("84f55f47-dd71-443f-84b1-9c32bae4bdcf"),
        Name = "Cheese",
        CategorySKU = Guid.Parse("e5d2f9c0-32c9-48b7-aba6-0bb89e4b1a94"),
        CategoryName = "Dairy",
        IsDeleted = false,
        Family = _family,
        OwnerId = _ownerId
    },
        new CatalogItem
    {
        SKU = Guid.Parse("cfccc2dc-69ae-47a2-b135-f2321dce5da4"),
        Name = "Milk",
        CategorySKU = Guid.Parse("e5d2f9c0-32c9-48b7-aba6-0bb89e4b1a94"),
        CategoryName = "Dairy",
        IsDeleted = false,
        Family = _family,
        OwnerId = _ownerId
    },
            new CatalogItem
    {
        SKU = Guid.Parse("557330e0-2aa6-411e-b872-d5969f163de7"),
        Name = "Bread",
        CategorySKU = Guid.Parse("3458f4d8-6ef7-4bbb-8513-6dc1d99ccd5a"),
        CategoryName = "Bakery",
        IsDeleted = false,
        Family = _family,
        OwnerId = _ownerId
    },
        new CatalogItem
    {
        SKU = Guid.Parse("61ca90d0-cfe1-4e43-95fb-94ff972a024e"),
        Name = "Ground Beef",
        CategorySKU = Guid.Parse("f38b63dd-d576-4daf-8202-3c0cccc0ead6"),
        CategoryName = "Meat",
        IsDeleted = false,
        Family = _family,
        OwnerId = _ownerId
    },
                    new CatalogItem
    {
        SKU = Guid.Parse("e64c3f2f-4ba3-44f8-8792-d1d2b75a9737"),
        Name = "Donuts",
        CategorySKU = Guid.Parse("3458f4d8-6ef7-4bbb-8513-6dc1d99ccd5a"),
        CategoryName = "Bakery",
        IsDeleted = false,
        Family = _family,
        OwnerId = _ownerId
    },
                        new CatalogItem
    {
        SKU = Guid.Parse("60c119cd-93a8-43d7-8b5e-81dd238b2aa6"),
        Name = "Soda",
        CategorySKU = Guid.Parse("7376d09c-3bde-4fbd-a9e7-2556ddc8003b"),
        CategoryName = "Beverages",
        IsDeleted = false,
        Family = _family,
        OwnerId = _ownerId
    },
};
        context.AddRange(catalogItems);
        await context.SaveChangesAsync();
    }
}

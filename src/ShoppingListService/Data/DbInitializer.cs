using System;
using System.Net.Sockets;
using System.Text.Json;
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
        AddSearchingFunction(scope.ServiceProvider.GetService<ShoppingListContext>());
    }


    private static void AddSearchingFunction(ShoppingListContext context)
    {
        context.Database.EnsureCreated();

        // Execute the raw SQL to create the extension
        context.Database.ExecuteSqlRaw("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
    }
    private static async Task SeedData(ShoppingListContext context)
    {
        context.Database.Migrate();

        if (context.CatalogItems.Any())
        {
            Console.WriteLine("The database already exists.");
            return;
        }

        // Path to the JSON file (adjust the path accordingly)
        var jsonFilePath = "Data/data.json";

        if (!File.Exists(jsonFilePath))
        {
            Console.WriteLine($"The JSON file {jsonFilePath} does not exist.");
            return;
        }

        // Seed data


        try
        {
            // Read and deserialize the JSON file
            var jsonData = await File.ReadAllTextAsync(jsonFilePath);
            var categories = JsonSerializer.Deserialize<List<Category>>(jsonData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });


            if (categories == null || !categories.Any())
            {
                Console.WriteLine("No data found in the JSON file.");
                return;
            }

            // Transform data into CatalogItem entities
            var catalogItems = categories
                .Where(c => c.Items != null) // Ensure there are items
                .SelectMany(c => c.Items.Select(item =>
                {
                    // Safely parse GUIDs
                    bool skuParsed = Guid.TryParse(item.Sku, out var itemSku);
                    bool categorySkuParsed = Guid.TryParse(c.Sku, out var categorySku);

                    // Return null if parsing fails
                    if (!skuParsed || !categorySkuParsed)
                        return null;
                    return new CatalogItem
                    {
                        SKU = itemSku,
                        Name = item.Name,
                        CategorySKU = categorySku,
                        CategoryName = c.Name,
                        IsDeleted = item.IsDeleted,
                        Family = _family,
                        OwnerId = _ownerId
                    };
                }))
                .ToList();

            // Add and save the data to the database
            await context.CatalogItems.AddRangeAsync(catalogItems);
            await context.SaveChangesAsync();

            Console.WriteLine("Database seeded successfully with JSON data.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding data: {ex.Message}");
        }
    }
}
// Strongly-typed C# classes for JSON deserialization
public class Category
{
    public string Id { get; set; }
    public string Sku { get; set; }
    public string Name { get; set; }
    public string OwnerId { get; set; }
    public string Family { get; set; }
    public bool IsDeleted { get; set; }
    public List<Item> Items { get; set; }
}

public class Item
{
    public string Id { get; set; }
    public string Sku { get; set; }
    public string Name { get; set; }
    public string OwnerId { get; set; }
    public string Family { get; set; }
    public bool IsDeleted { get; set; }
    public string CategoryId { get; set; }
    public string CategorySKU { get; set; }
    public string CategoryName { get; set; }
}
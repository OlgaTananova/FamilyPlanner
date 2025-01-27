using System;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ShoppingListService.Entities;

namespace ShoppingListService.Data;

public static class DbInitializer
{
    public static void AddSearchingFunction(WebApplication app)
    {
        using var serviceScope = app.Services.CreateScope();
        ShoppingListContext context = serviceScope.ServiceProvider.GetRequiredService<ShoppingListContext>();

        context.Database.Migrate();
        context.Database.EnsureCreated();

        // Execute the raw SQL to create the extension
        context.Database.ExecuteSqlRaw("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
    }
}
using System;
using CatalogService.Entities;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Data;

public class CatalogDbContext: DbContext
{
    public CatalogDbContext(DbContextOptions options) : base(options)
    {
        
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Item> Items { get; set; }
}

using System;
using CatalogService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Data;

public class CatalogDbContext : DbContext
{
    private readonly string _connectionString;
    public CatalogDbContext(DbContextOptions options, IConfiguration configuration) : base(options)
    {

        var username = configuration["PostgresUser"];
        var password = configuration["PostgresPassword"];
        var database = configuration["Database"];
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            .Replace("{PostgresUser}", username)
            .Replace("{PostgresPassword}", password)
            .Replace("{Database}", database);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // support for outbox pattern 
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        modelBuilder.Entity<Item>()
        .HasGeneratedTsVectorColumn(
            p => p.SearchVector,
            "english",  // Text search config
            p => new { p.Name, p.CategoryName})  // Included properties
        .HasIndex(p => p.SearchVector)
        .HasMethod("GIN"); // Index method on the search vector (GIN or GIST)
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseLazyLoadingProxies()
            .UseNpgsql(_connectionString);
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Item> Items { get; set; }
}

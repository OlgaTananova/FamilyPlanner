using CatalogService.Data;
using CatalogService.Entities;
using Contracts.Catalog;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Microsoft.Extensions.Configuration;

namespace CatalogService.IntegrationTests;

public class CatalogOutboxTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer =
        new PostgreSqlBuilder()
            .WithDatabase("catalog_outbox_test_db")
            .Build();

    private ServiceProvider? _serviceProvider;

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        services.AddDbContext<CatalogDbContext>(options =>
        {
            options.UseNpgsql(_postgresContainer.GetConnectionString());
        });

        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<CatalogDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });

            // We don't need RabbitMQ for this test.
            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });

        _serviceProvider = services.BuildServiceProvider();

        using var scope = _serviceProvider.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<CatalogDbContext>();

        await db.Database.MigrateAsync();

        await db.Database.ExecuteSqlRawAsync(
            "CREATE EXTENSION IF NOT EXISTS pg_trgm;");
    }

    public async Task DisposeAsync()
    {
        if (_serviceProvider is not null)
        {
            await _serviceProvider.DisposeAsync();
        }

        await _postgresContainer.DisposeAsync();
    }

    [Fact]
    public async Task SaveChanges_ShouldPersistDomainEntityAndOutboxMessage()
    {
        // Arrange
        using var scope = _serviceProvider!.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<CatalogDbContext>();

        var publishEndpoint = scope.ServiceProvider
            .GetRequiredService<IPublishEndpoint>();

        var category = new Category
        {
            Id = Guid.NewGuid(),
            SKU = Guid.NewGuid(),
            Name = "Outbox Test Category",
            OwnerId = "test-user-id",
            Family = "test-family"
        };

        db.Categories.Add(category);

        var message = new CatalogCategoryCreated
        {
            Sku = category.SKU,
            Name = category.Name,
            OwnerId = category.OwnerId,
            Family = category.Family
        };

        // Act
        await publishEndpoint.Publish(message);

        await db.SaveChangesAsync();

        // Assert - domain data was persisted
        bool categoryExists = await db.Categories
            .AnyAsync(c => c.Id == category.Id);

        Assert.True(categoryExists);

        // Assert - MassTransit persisted the outgoing message
        await using var command =
            db.Database.GetDbConnection().CreateCommand();

        command.CommandText =
            """
            SELECT COUNT(*)
            FROM "OutboxMessage";
            """;

        if (command.Connection!.State != System.Data.ConnectionState.Open)
        {
            await command.Connection.OpenAsync();
        }

        var count = Convert.ToInt64(
            await command.ExecuteScalarAsync());

        Assert.True(
            count > 0,
            "Expected MassTransit to persist an OutboxMessage.");
    }

    [Fact]
    public async Task Rollback_ShouldNotPersistDomainEntityOrOutboxMessage()
    {
        // Arrange
        Guid categoryId = Guid.NewGuid();

        using (var scope = _serviceProvider!.CreateScope())
        {
            var db = scope.ServiceProvider
                .GetRequiredService<CatalogDbContext>();

            var publishEndpoint = scope.ServiceProvider
                .GetRequiredService<IPublishEndpoint>();

            await using var transaction =
                await db.Database.BeginTransactionAsync();

            var category = new Category
            {
                Id = categoryId,
                SKU = Guid.NewGuid(),
                Name = "Rolled Back Category",
                OwnerId = "test-user-id",
                Family = "test-family"
            };

            db.Categories.Add(category);

            var message = new CatalogCategoryCreated
            {
                Sku = category.SKU,
                Name = category.Name,
                OwnerId = category.OwnerId,
                Family = category.Family
            };

            // Stage the outgoing message in the Bus Outbox
            await publishEndpoint.Publish(message);

            // Both the Category and OutboxMessage are written
            // inside the current database transaction.
            await db.SaveChangesAsync();

            // Simulate the operation ultimately failing.
            await transaction.RollbackAsync();
        }

        // Assert using a fresh DbContext so tracked entities
        using var verificationScope =
            _serviceProvider!.CreateScope();

        var verificationDb = verificationScope.ServiceProvider
            .GetRequiredService<CatalogDbContext>();

        bool categoryExists = await verificationDb.Categories
            .AnyAsync(c => c.Id == categoryId);

        Assert.False(categoryExists);

        await using var command =
            verificationDb.Database.GetDbConnection().CreateCommand();

        command.CommandText =
            """
        SELECT COUNT(*)
        FROM "OutboxMessage";
        """;

        if (command.Connection!.State !=
            System.Data.ConnectionState.Open)
        {
            await command.Connection.OpenAsync();
        }

        var outboxCount = Convert.ToInt64(
            await command.ExecuteScalarAsync());

        Assert.Equal(0, outboxCount);
    }
}

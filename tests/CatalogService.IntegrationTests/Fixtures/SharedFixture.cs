
namespace CatalogService.IntegrationTests.Fixtures;

[CollectionDefinition("Shared collection")]

// Created a single instanse of the application to be shared across multiple test classes
public class SharedFixture : ICollectionFixture<CustomWebAppFactory>
{

}

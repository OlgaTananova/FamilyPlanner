using Microsoft.Graph;

namespace CatalogService.Services;

public interface IGraphServiceClientFactory
{
    GraphServiceClient GetGraphServiceClient();
}

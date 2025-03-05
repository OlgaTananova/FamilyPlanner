
using Microsoft.Graph.Beta;

namespace CatalogService.Services;

public interface IGraphServiceClientFactory
{
    GraphServiceClient GetGraphServiceClient();
}

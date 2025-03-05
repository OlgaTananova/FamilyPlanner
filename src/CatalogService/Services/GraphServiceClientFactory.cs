using Azure.Identity;
using CatalogService.RequestHelpers;
using Microsoft.Graph.Beta;

namespace CatalogService.Services;

public class GraphServiceClientFactory : IGraphServiceClientFactory
{
    private readonly AppConfig _appConfig;
    public GraphServiceClientFactory(AppConfig appConfig)
    {
        _appConfig = appConfig;
    }
    public GraphServiceClient GetGraphServiceClient()
    {
        var scopes = new[] { "https://graph.microsoft.com/.default" };
        var tenantId = _appConfig.AzureAdB2CDomain;
        var clientId = _appConfig.AzureAdB2CClientId;
        var clientSecret = _appConfig.AzureAdB2CClientSecret;
        var clientSecretCredential = new ClientSecretCredential(
        tenantId, clientId, clientSecret);
        return new GraphServiceClient(clientSecretCredential, scopes);
    }
}

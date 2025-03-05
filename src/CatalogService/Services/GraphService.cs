using CatalogService.DTOs;
using CatalogService.RequestHelpers;
using Microsoft.Graph.Beta;

namespace CatalogService.Services;

public class GraphService : IGraphService
{
    private readonly GraphServiceClient _graphServiceClient;
    private readonly AppConfig _appConfig;
    public GraphService(IGraphServiceClientFactory graphServiceClientFactory, AppConfig appConfig)
    {
        _graphServiceClient = graphServiceClientFactory.GetGraphServiceClient();
        _appConfig = appConfig;
    }
    public async Task<ServiceResult<List<FamilyUserDto>>> GetFamilyUsersAsync(string family)
    {
        string extensionId = _appConfig.AzureAdB2CExtensionId;
        var userResponse = await _graphServiceClient.Users.GetAsync();
        var extensionPrefix = $"extension_{extensionId}_";
        var familyUsers = userResponse.Value
            .Where(user =>
                user.AdditionalData != null &&
                user.AdditionalData.ContainsKey($"{extensionPrefix}Family") &&
                user.AdditionalData.ContainsKey($"{extensionPrefix}Role") &&
                user.AdditionalData.ContainsKey($"{extensionPrefix}IsAdmin") &&
                user.AdditionalData[$"{extensionPrefix}Family"]?.ToString() == family)
            .Select(user => new FamilyUserDto
            {
                Id = user.Id,
                GivenName = user.GivenName,
                DisplayName = user.DisplayName,
                Email = user.Mail
                    ?? user.OtherMails?.FirstOrDefault()
                    ?? user.Identities?.FirstOrDefault()?.IssuerAssignedId,
                Family = user.AdditionalData[$"{extensionPrefix}Family"]?.ToString() ?? "",
                Role = user.AdditionalData[$"{extensionPrefix}Role"]?.ToString() ?? "",
                IsAdmin = bool.TryParse(
                    user.AdditionalData[$"{extensionPrefix}IsAdmin"]?.ToString(),
                    out var isAdmin) && isAdmin
            })
        .ToList();

        if (familyUsers != null)
        {
            return ServiceResult<List<FamilyUserDto>>.SuccessResult(familyUsers);
        }

        return ServiceResult<List<FamilyUserDto>>.FailureResult("Family users are not found", 404);
    }
}

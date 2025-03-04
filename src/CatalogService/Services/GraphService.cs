using AutoMapper;
using Microsoft.Graph;

namespace CatalogService.Services;

public class GraphService : IGraphService
{
    private readonly GraphServiceClient _graphServiceClient;
    private readonly IMapper _mapper;
    public GraphService(IGraphServiceClientFactory graphServiceClientFactory, IMapper mapper)
    {
        _graphServiceClient = graphServiceClientFactory.GetGraphServiceClient();
    }
    public async Task<IEnumerable<User>> GetFamilyUsersAsync(string family)
    {
        var users = await _graphServiceClient.Users
            .Request()
            .Select($"id,givenName,mail,otherMails,identities,extensions")
            .Top(999) // Adjust as needed
            .GetAsync();

        // var familyUsers = users.CurrentPage
        // .Where(user =>
        //     user.AdditionalData != null &&
        //     user.AdditionalData.TryGetValue($"extension_{_extensionIdWithoutDashes}_Family", out var familyValue) &&
        //     familyValue?.ToString() == familyName
        // )
        // .Select(user => new FamilyUserDto
        // {
        //     Id = user.Id,
        //     GivenName = user.GivenName,
        //     Family = user.AdditionalData[$"extension_{_extensionIdWithoutDashes}_Family"]?.ToString(),
        //     Role = user.AdditionalData[$"extension_{_extensionIdWithoutDashes}_Role"]?.ToString(),
        //     IsAdmin = bool.TryParse(user.AdditionalData[$"extension_{_extensionIdWithoutDashes}_IsAdmin"]?.ToString(), out var isAdmin) && isAdmin,
        //     Email = user.Mail ?? user.OtherMails?.FirstOrDefault() ?? user.Identities?.FirstOrDefault()?.Id
        // })
        // .ToList();
        return users.CurrentPage.ToList();
    }
}

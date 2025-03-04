
using Microsoft.Graph;

namespace CatalogService.Services;

public interface IGraphService
{
    Task<IEnumerable<User>> GetFamilyUsersAsync(string family);
}

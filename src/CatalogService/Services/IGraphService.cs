using CatalogService.DTOs;
using CatalogService.RequestHelpers;

namespace CatalogService.Services;

public interface IGraphService
{
    Task<ServiceResult<List<FamilyUserDto>>> GetFamilyUsersAsync(string family);
}

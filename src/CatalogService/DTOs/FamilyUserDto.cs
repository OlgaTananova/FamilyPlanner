
namespace CatalogService.DTOs;

public class FamilyUserDto
{
    public string Id { get; set; }
    public string Family { get; set; }
    public string GivenName { get; set; }
    public string DisplayName { get; set; }
    public string Role { get; set; }
    public bool IsAdmin { get; set; }
    public string Email { get; set; }
}
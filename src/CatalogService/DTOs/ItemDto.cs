using System;

namespace CatalogService.DTOs;

public class ItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string OwnerId { get; set; }
    public string Family { get; set; }
    public bool IsDeleted { get; set; }
    public Guid CategoryId { get; set; }
}

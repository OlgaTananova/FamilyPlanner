using System;

namespace CatalogService.Entities;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string OwnerId { get; set; }
    public bool IsDeleted { get; set; } =false;

    // navigation properties
    public List<Item> Items { get; set; } = new List<Item>();
    
}

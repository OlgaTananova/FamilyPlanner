using System;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.Entities;

public class Category
{
    public Guid Id { get; set; }
    [Required]
    public Guid SKU { get; set; } = Guid.NewGuid();
    [Required]
    public string Name { get; set; }
    [Required]
    public string OwnerId { get; set; }
    [Required]    
    public string Family { get; set; }
    public bool IsDeleted { get; set; } =false;

    // navigation properties
    public virtual List<Item> Items { get; set; } = new List<Item>();
    
}

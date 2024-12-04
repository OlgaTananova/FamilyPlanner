using System;
using System.ComponentModel.DataAnnotations;

namespace ShoppingListService.Entities;

public class CatalogItem
{
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; }
    public Guid CategoryId { get; set; }

    [Required]
    public string CategoryName { get; set; }
    public bool IsDeleted { get; set; } = false;

    [Required]
    public string Family { get; set; }
    [Required]
    public string OwnerId { get; set; }
}
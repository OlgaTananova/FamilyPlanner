using System.ComponentModel.DataAnnotations;
using NpgsqlTypes;

namespace ShoppingListService.Entities;

public class CatalogItem
{
    public Guid Id { get; set; }
    [Required]
    public Guid SKU { get; set; }
    [Required]
    public string Name { get; set; }
    public int Count { get; set; } = 0;

    [Required]
    public Guid CategorySKU { get; set; }
    [Required]
    public string CategoryName { get; set; }

    public bool IsDeleted { get; set; } = false;

    [Required]
    public string Family { get; set; }
    [Required]
    public string OwnerId { get; set; }

    public NpgsqlTsVector SearchVector { get; set; }
}
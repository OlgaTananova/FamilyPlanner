using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NpgsqlTypes;

namespace CatalogService.Entities;

[Table("Items")]
public class Item
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
    public bool IsDeleted { get; set; } = false;

    //navigation properties
    [Required]
    public Guid CategoryId { get; set; }
    [Required]
    public Guid CategorySKU { get; set; }
    public string CategoryName { get; set; }
    public virtual Category Category { get; set; }

    public NpgsqlTsVector SearchVector { get; set; }

}

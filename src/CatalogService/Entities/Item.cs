using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatalogService.Entities;

[Table("Items")]
public class Item
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string OwnerId { get; set; }
    public string Family { get; set; }
    public bool IsDeleted { get; set; } = false;

    //navigation properties
    public Guid CategoryId { get; set; }
    public Category Category { get; set; }

}

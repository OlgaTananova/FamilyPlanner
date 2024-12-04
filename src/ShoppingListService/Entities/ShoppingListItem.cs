using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ShoppingListService.Entities;

public enum Units
{
    pcs,
    gal,
    lb,
    oz,
    carton,
    fl_oz
}

public enum Status
{
    Pending,
    Finished
}

public class ShoppingListItem
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid ShoppingListId { get; set; }
    public Guid CatalogItemId { get; set; } // navigation property
    public CatalogItem CatalogItem { get; set; } // navigation property
    public Units Unit { get; set; } = Entities.Units.pcs;
    public decimal Quantity { get; set; } = 1.00M;
    public decimal PricePerUnit { get; set; } = 0.00M;
    public decimal Price { get; set; } = 0.00M;
    public Status Status { get; set; } = Status.Pending;
    public bool IsOrphaned { get; set; } = false;
    [Required]
    public string Family { get; set; }
    [Required]
    public string OwnerId { get; set; }
}

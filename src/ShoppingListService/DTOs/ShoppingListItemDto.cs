using System;
using System.ComponentModel.DataAnnotations;

namespace ShoppingListService.DTOs;

public class ShoppingListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid CatalogItemSKU { get; set; }
    public string CategoryName { get; set; }
    public Guid CategorySKU { get; set; }
    public Guid ShoppingListId { get; set; }
    public string Unit { get; set; }
    public decimal Quantity { get; set; }
    public decimal PricePerUnit { get; set; } = 0.00M;
    public decimal Price { get; set; } = 0.00M;
    public string Status { get; set; }
    public bool IsOrphaned { get; set; }

    public string Family { get; set; }
    public string OwnerId { get; set; }
}


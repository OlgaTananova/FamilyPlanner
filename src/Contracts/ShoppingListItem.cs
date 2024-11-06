using System;

namespace Contracts;

public class ShoppingListItem
{
    public Guid Id { get; set; }
    public decimal Quantity { get; set; } = 1.0M;
    public decimal PricePerUnit { get; set; } = 0.00M;
    public decimal Price { get; set; } = 0.00M;
    public bool IsDeleted { get; set; } = false;
}
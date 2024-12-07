using System;
using System.ComponentModel.DataAnnotations;
using ShoppingListService.Entities;

namespace ShoppingListService.DTOs;

public class UpdateShoppingListItemDto
{
    public Units Unit { get; set; }
    public decimal Quantity { get; set; }
    public decimal PricePerUnit { get; set; }
    public decimal Price { get; set; }
    public Status Status { get; set; }

}

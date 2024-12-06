using System;
using System.ComponentModel.DataAnnotations;
using ShoppingListService.Entities;

namespace ShoppingListService.DTOs;

public class UpdateShoppingListItemDto
{
    [Required]
    public Guid Id { get; set; }
    public Units Unit { get; set; }
    public decimal Quantity { get; set; }
    public decimal PricePerUnit { get; set; }
    public decimal Price { get; set; }
    public Status Status { get; set; }

}

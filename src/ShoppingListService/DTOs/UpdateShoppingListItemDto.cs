using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Cors;
using ShoppingListService.Entities;

namespace ShoppingListService.DTOs;

# nullable enable
public class UpdateShoppingListItemDto
{
    public string? Unit { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? PricePerUnit { get; set; }
    public decimal? Price { get; set; }
    public string? Status { get; set; }

}

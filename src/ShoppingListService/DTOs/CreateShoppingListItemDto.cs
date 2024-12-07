using System;
using System.ComponentModel.DataAnnotations;

namespace ShoppingListService.DTOs;

public class CreateShoppingListItemDto
{
    [Required]
    public Guid SKU { get; set; }
}
using System;
using System.ComponentModel.DataAnnotations;

namespace ShoppingListService.DTOs;

public class DeleteShoppingListItemDto
{
    [Required]
    public Guid Id { get; set; }
}

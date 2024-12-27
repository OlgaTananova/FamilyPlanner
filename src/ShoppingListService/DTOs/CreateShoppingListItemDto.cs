using System;
using System.ComponentModel.DataAnnotations;

namespace ShoppingListService.DTOs;

public class CreateShoppingListItemDto
{
    [Required]
    public List<Guid> SKUs { get; set; }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace ShoppingListService.DTOs;

public class DeleteShoppingListDto
{
    [Required]
    public Guid Id { get; set; }
}

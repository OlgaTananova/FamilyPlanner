using System;
using System.ComponentModel.DataAnnotations;

namespace ShoppingListService.DTOs;

public class UpdateShoppingListDto
{
    [Required]
    public Guid Id { get; set; }
    public string Heading { get; set; }
    public decimal SalesTax { get; set; }
    public bool IsArchived { get; set; }
}

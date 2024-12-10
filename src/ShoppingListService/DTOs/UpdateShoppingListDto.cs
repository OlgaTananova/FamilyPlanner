using System;
using System.ComponentModel.DataAnnotations;

namespace ShoppingListService.DTOs;

# nullable enable
public class UpdateShoppingListDto
{
    public string? Heading { get; set; }
    public decimal? SalesTax { get; set; }
    public bool? IsArchived { get; set; }
}

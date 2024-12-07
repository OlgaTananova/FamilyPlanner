using System;
using System.ComponentModel.DataAnnotations;

namespace ShoppingListService.DTOs;

public class UpdateShoppingListDto
{
    public string Heading { get; set; }
    public decimal SalesTax { get; set; }
    public bool IsArchived { get; set; }
}
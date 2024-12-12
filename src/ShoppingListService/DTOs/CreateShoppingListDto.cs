using System;

namespace ShoppingListService.DTOs;

# nullable enable
public class CreateShoppingListDto
{
    public string? Name { get; set; }
    public List<string>? SKUs { get; set; }
}

using System;

namespace ShoppingListService.DTOs;

# nullable enable
public class CreateShoppingListDto
{
    public string? Heading { get; set; }
    public List<Guid>? SKUs { get; set; }
}

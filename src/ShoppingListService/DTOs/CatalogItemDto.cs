using System;

namespace ShoppingListService.DTOs;

public class CatalogItemDto
{
    public Guid Id { get; set; }
    public Guid SKU { get; set; }
    public string Name { get; set; }
    public string CategoryName { get; set; }
    public Guid CategorySKU { get; set; }
    public int Count { get; set; }
    public bool IsDeleted { get; set; }
    public string OwnerId { get; set; }
    public string Family { get; set; }
}

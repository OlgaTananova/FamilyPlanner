using System;

namespace Contracts.Catalog;

public class CatalogItemSeeded
{
    public List<SeededItem> Items { get; set; }
}


public class SeededItem
{
    public Guid SKU { get; set; }
    public string Name { get; set; }
    public string OwnerId { get; set; }
    public string Family { get; set; }
    public bool IsDeleted { get; set; }
    public Guid CategorySKU { get; set; }
    public string CategoryName { get; set; }
}
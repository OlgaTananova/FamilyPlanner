using System;

namespace Contracts.Catalog;

public class CatalogItemUpdated
{
    public UpdatedItem UpdatedItem { get; set; }
    public Guid PreviousCategorySKU { get; set; }
}

public class UpdatedItem
{
    public Guid Id { get; set;}
    public Guid SKU { get; set; }
    public string Name { get; set; }
    public string OwnerId { get; set; }
    public string Family { get; set; }
    public bool IsDeleted { get; set; }
    public Guid CategoryId { get; set; }
    public Guid CategorySKU { get; set; }
    public string CategoryName { get; set; }
    }

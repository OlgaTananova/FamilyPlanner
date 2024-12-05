using System;

namespace Contracts.Catalog;

public class CatalogItemDeleted
{
    public Guid SKU { get; set; }
    public string Family { get; set; }
    public string OwnerId { get; set; }
}

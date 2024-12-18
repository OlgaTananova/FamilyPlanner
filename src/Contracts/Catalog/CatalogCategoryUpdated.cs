using System;

namespace Contracts.Catalog;

public class CatalogCategoryUpdated
{
    public Guid Sku { get; set; }
    public string Name { get; set; }
    public string Family { get; set; }
    public string OwnerId { get; set; }
}

using System;

namespace Contracts.Catalog;

public class CatalogCategoryDeleted
{
    public Guid Id { get; set; }
    public Guid Sku { get; set; }
    public string OwnerId { get; set; }
    public string Family { get; set; }

}

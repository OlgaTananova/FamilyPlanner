using System;

namespace Contracts.Catalog;

public class CatalogCategoryCreated
{
    public Guid Id { get; set; }
    public Guid CategorySKU { get; set; }
    public string Name { get; set; }
    public string Family { get; set; }
    public string OwnerId { get; set; }

}

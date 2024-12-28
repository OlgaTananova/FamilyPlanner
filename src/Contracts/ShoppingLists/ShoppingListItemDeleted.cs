using System;

namespace Contracts.ShoppingLists;

public class ShoppingListItemDeleted
{
    public Guid ShoppingListId { get; set; }
    public Guid ItemId { get; set; }
    public string Family { get; set; }
    public string OwnerId { get; set; }
}

using System;

namespace Contracts.ShoppingLists;

public class ShoppingListDeleted
{
    public Guid Id { get; set; }
    public string Family { get; set; }
    public string OwnerId { get; set; }
    public bool IsDeleted { get; set; }

}

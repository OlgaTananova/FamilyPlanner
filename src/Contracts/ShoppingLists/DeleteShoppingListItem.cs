using System;

namespace Contracts.ShoppingLists;

public class DeleteShoppingListItem
{
    public Guid Id { get; set; }
    public Guid SKU { get; set; }
}

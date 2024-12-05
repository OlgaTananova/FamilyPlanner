using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.AspNetCore.Http.Features;

namespace ShoppingListService.Entities;

public class ShoppingList
{
    public Guid Id { get; set;}
    public string Heading { get; set; } = "Shopping list";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<ShoppingListItem> Items { get; set; }
    public decimal SalesTax { get; set; } = 0.00M;
    public bool IsArchived { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    [Required]
    public string OwnerId { get; set; }
    [Required]
    public string Family { get; set; }     
}

using System;
using System.ComponentModel.DataAnnotations;

namespace ShoppingListService.DTOs;

public class ShoppingListDto
{
    public Guid Id { get; set; }
    [Required]
    public string Heading { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ShoppingListItemDto> Items { get; set; }
    public decimal SalesTax { get; set; }
    public bool IsArchived { get; set; }
    public bool IsDeleted { get; set; }
    [Required]
    public string OwnerId { get; set; }
    [Required]
    public string Family { get; set; }

}

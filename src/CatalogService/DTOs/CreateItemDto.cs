using System;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.DTOs;

public class CreateItemDto
{
    [Required]
    public string Name { get; set; }
    [Required]
    public Guid CategorySKU { get; set; }

}

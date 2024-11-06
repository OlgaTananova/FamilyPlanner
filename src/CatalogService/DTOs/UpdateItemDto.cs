using System;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.DTOs;

public class UpdateItemDto
{
    [Required]
    public string Name { get; set; }
    [Required]
    public Guid CategoryId { get; set; }
}

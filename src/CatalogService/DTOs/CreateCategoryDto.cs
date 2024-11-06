using System;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.DTOs;

public class CreateCategoryDto
{
    [Required]
    public string Name { get; set; }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace CatalogService.DTOs;

public class UpdateCategoryDto
{
    [Required]
    public string Name { get; set; }

}

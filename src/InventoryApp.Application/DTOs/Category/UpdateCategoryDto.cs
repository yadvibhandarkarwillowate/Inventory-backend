using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Application.DTOs.Category;

public class UpdateCategoryDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Description { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace InventoryApp.Application.DTOs.Supplier;

public class CreateSupplierDto
{
    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(150)]
    public string? ContactPerson { get; set; }

    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }

    [EmailAddress]
    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }
}
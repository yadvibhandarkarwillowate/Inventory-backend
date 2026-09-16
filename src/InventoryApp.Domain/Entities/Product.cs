using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryApp.Domain.Entities;

[Table("products")]
public class Product
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("brand")]
    public string? Brand { get; set; }

    [Column("model_number")]
    public string? ModelNumber { get; set; }

    [Required]
    [Column("sku")]
    public string Sku { get; set; } = string.Empty;

    [Column("barcode")]
    public string? Barcode { get; set; }

    [Column("category_id")]
    public long? CategoryId { get; set; }
    public Category? Category { get; set; }

    [Column("supplier_id")]
    public long? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    [Column("purchase_price")]
    public decimal PurchasePrice { get; set; } = 0m;

    [Column("selling_price")]
    public decimal SellingPrice { get; set; } = 0m;

    [Column("current_stock")]
    public int CurrentStock { get; set; } = 0;

    [Column("reorder_level")]
    public int ReorderLevel { get; set; } = 5;

    [Column("warranty_period")]
    public string? WarrantyPeriod { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("image_url")]
    public string? ImageUrl { get; set; }

    [Column("status")]
    public string Status { get; set; } = "ACTIVE";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
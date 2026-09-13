using InventoryApp.Domain.Enums;

namespace InventoryApp.Domain.Entities;

public class InventoryTransaction
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public Product? Product { get; set; }
    public TransactionType TransactionType { get; set; }
    public int QuantityChange { get; set; }
    public int PreviousStock { get; set; }
    public int NewStock { get; set; }
    public string? Reason { get; set; }
    public string? ReferenceNo { get; set; }
    public long CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
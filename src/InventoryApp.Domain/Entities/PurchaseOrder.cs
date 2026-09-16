using InventoryApp.Domain.Enums;

namespace InventoryApp.Domain.Entities;

public class PurchaseOrder
{
    public long Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public long SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.UtcNow;
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.PENDING;
    public decimal TotalAmount { get; set; }
    public long CreatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<PurchaseOrderItem> Items { get; set; } = new();
}

namespace InventoryApp.Domain.Entities;

public class PurchaseOrderItem
{
    public long Id { get; set; }
    public long PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
    public long ProductId { get; set; }
    public Product? Product { get; set; }
    public int OrderedQty { get; set; }
    public int ReceivedQty { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal Total { get; set; }
}

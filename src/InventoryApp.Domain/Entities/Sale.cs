namespace InventoryApp.Domain.Entities;

public class Sale
{
    public long Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

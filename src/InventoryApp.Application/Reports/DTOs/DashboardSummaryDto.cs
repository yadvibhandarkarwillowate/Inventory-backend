namespace InventoryApp.Application.Reports.DTOs;

public class DashboardSummaryDto
{
    public int TotalProducts { get; set; }
    public int LowStockCount { get; set; }
    public decimal TotalStockValue { get; set; }
    public decimal TotalSales { get; set; }
}

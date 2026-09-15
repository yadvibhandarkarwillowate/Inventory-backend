using InventoryApp.Application.Reports.DTOs;
using InventoryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Application.Reports;

public class ReportsService : IReportsService
{
    private readonly AppDbContext _dbContext;

    public ReportsService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
    {
        var totalProducts = await _dbContext.Products.CountAsync();
        
        var lowStockCount = await _dbContext.Products
            .CountAsync(p => p.CurrentStock < p.ReorderLevel);

        var totalStockValue = await _dbContext.Products
            .SumAsync(p => (decimal?)(p.CurrentStock * p.PurchasePrice)) ?? 0m;

        var totalSales = await _dbContext.Sales
            .Where(s => s.Status == "COMPLETED")
            .SumAsync(s => (decimal?)s.Total) ?? 0m;

        return new DashboardSummaryDto
        {
            TotalProducts = totalProducts,
            LowStockCount = lowStockCount,
            TotalStockValue = totalStockValue,
            TotalSales = totalSales
        };
    }
}

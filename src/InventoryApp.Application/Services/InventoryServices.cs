using InventoryApp.Application.DTOs.Inventory;
using InventoryApp.Application.Interfaces;
using InventoryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly AppDbContext _db;
    public InventoryService(AppDbContext db) => _db = db;

    public async Task<PagedResult<StockLevelDto>> GetStockLevelsAsync(StockQueryParams query)
    {
        var productsQuery = _db.Products.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            productsQuery = productsQuery.Where(p =>
                p.Name.ToLower().Contains(term) || p.Sku.ToLower().Contains(term));
        }

        if (query.LowStockOnly)
            productsQuery = productsQuery.Where(p => p.CurrentStock <= p.ReorderLevel);

        var totalCount = await productsQuery.CountAsync();

        var items = await productsQuery
            .OrderBy(p => p.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new StockLevelDto(p.Id, p.Sku, p.Name, p.CurrentStock, p.ReorderLevel, p.CurrentStock <= p.ReorderLevel))
            .ToListAsync();

        return new PagedResult<StockLevelDto>(items, totalCount, query.Page, query.PageSize);
    }
}
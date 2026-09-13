namespace InventoryApp.Application.DTOs.Inventory;

public record StockLevelDto(
    long ProductId, string Sku, string Name,
    int CurrentStock, int ReorderLevel, bool IsLowStock
);

public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);

public record StockQueryParams(
    string? Search = null, bool LowStockOnly = false, int Page = 1, int PageSize = 20
);
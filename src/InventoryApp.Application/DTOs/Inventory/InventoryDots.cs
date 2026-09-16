namespace InventoryApp.Application.DTOs.Inventory;

public record StockLevelDto(
    long ProductId, string Sku, string Name,
    int CurrentStock, int ReorderLevel, bool IsLowStock
);

public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);

public record StockQueryParams(
    string? Search = null, bool LowStockOnly = false, int Page = 1, int PageSize = 20
);

public record IssueStockRequest(long ProductId, int Quantity, string Reason, string? ReferenceNo = null);

public record StockChangeResult(long ProductId, int PreviousStock, int NewStock, int QuantityChange, long TransactionId);

public record DamagedLostRequest(long ProductId, int Quantity, string Type, string Reason);

public record AdjustStockRequest(long ProductId, int NewCount, string Reason);

public record TransactionDto(
    long Id, long ProductId, string ProductName, string Type,
    int QuantityChange, int PreviousStock, int NewStock,
    string? Reason, string? ReferenceNo, string CreatedByUsername, DateTimeOffset CreatedAt
);

public record TransactionQueryParams(
    long? ProductId = null, string? Type = null,
    DateTimeOffset? FromDate = null, DateTimeOffset? ToDate = null,
    int Page = 1, int PageSize = 20
);

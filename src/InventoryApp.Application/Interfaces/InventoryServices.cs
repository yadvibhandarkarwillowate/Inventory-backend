using InventoryApp.Application.DTOs.Inventory;

namespace InventoryApp.Application.Interfaces;

public interface IInventoryService
{
    Task<PagedResult<StockLevelDto>> GetStockLevelsAsync(StockQueryParams query);
    Task<StockChangeResult> IssueStockAsync(IssueStockRequest request, long userId);
    Task<StockChangeResult> RecordDamagedOrLostAsync(DamagedLostRequest request, long userId);
    Task<StockChangeResult> AdjustStockAsync(AdjustStockRequest request, long userId);
    Task<PagedResult<TransactionDto>> GetTransactionHistoryAsync(TransactionQueryParams query);


}


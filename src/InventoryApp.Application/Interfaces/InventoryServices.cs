using InventoryApp.Application.DTOs.Inventory;

namespace InventoryApp.Application.Interfaces;

public interface IInventoryService
{
    Task<PagedResult<StockLevelDto>> GetStockLevelsAsync(StockQueryParams query);
}
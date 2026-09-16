using InventoryApp.Application.DTOs.Inventory;
using InventoryApp.Application.DTOs.PurchaseOrder;

namespace InventoryApp.Application.Interfaces;

public interface IPurchaseOrderService
{
    Task<PurchaseOrderResponseDto> CreatePurchaseOrderAsync(CreatePurchaseOrderDto dto, long userId);
    Task<PagedResult<PurchaseOrderListDto>> GetPurchaseOrdersAsync(PurchaseOrderQueryParams query);
    Task<PurchaseOrderResponseDto?> GetPurchaseOrderByIdAsync(long id);
    Task<ReceiveStockResultDto> ReceiveStockAsync(long poId, ReceiveStockRequestDto request, long userId);
}

using InventoryApp.Application.DTOs.PurchaseOrder;

namespace InventoryApp.Application.Interfaces;

public interface ISupplierService
{
    Task<List<SupplierDto>> GetSuppliersAsync();
    Task<SupplierDto?> GetSupplierByIdAsync(long id);
    Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto dto);
}

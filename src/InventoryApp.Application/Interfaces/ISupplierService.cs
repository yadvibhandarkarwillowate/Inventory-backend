using InventoryApp.Application.DTOs.Supplier;

namespace InventoryApp.Application.Interfaces;

public interface ISupplierService
{
    Task<IEnumerable<SupplierResponseDto>> GetAllAsync();

    Task<SupplierResponseDto?> GetByIdAsync(long id);

    Task<SupplierResponseDto> CreateAsync(CreateSupplierDto dto);

    Task<bool> UpdateAsync(long id, UpdateSupplierDto dto);

    Task<bool> DeleteAsync(long id);
}
using InventoryApp.Application.DTOs.PurchaseOrder;
using InventoryApp.Application.Interfaces;
using InventoryApp.Domain.Entities;
using InventoryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Application.Services;

public class SupplierService : ISupplierService
{
    private readonly AppDbContext _db;

    public SupplierService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<SupplierDto>> GetSuppliersAsync()
    {
        return await _db.Suppliers
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SupplierDto(
                s.Id, s.Name, s.ContactPerson, s.Email, s.Phone, s.Address, s.CreatedAt
            ))
            .ToListAsync();
    }

    public async Task<SupplierDto?> GetSupplierByIdAsync(long id)
    {
        var supplier = await _db.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (supplier == null) return null;

        return new SupplierDto(
            supplier.Id, supplier.Name, supplier.ContactPerson, supplier.Email,
            supplier.Phone, supplier.Address, supplier.CreatedAt
        );
    }

    public async Task<SupplierDto> CreateSupplierAsync(CreateSupplierDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new InvalidOperationException("Supplier name is required.");

        var supplier = new Supplier
        {
            Name = dto.Name.Trim(),
            ContactPerson = dto.ContactPerson?.Trim(),
            Email = dto.Email?.Trim(),
            Phone = dto.Phone?.Trim(),
            Address = dto.Address?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync();

        return new SupplierDto(
            supplier.Id, supplier.Name, supplier.ContactPerson, supplier.Email,
            supplier.Phone, supplier.Address, supplier.CreatedAt
        );
    }
}

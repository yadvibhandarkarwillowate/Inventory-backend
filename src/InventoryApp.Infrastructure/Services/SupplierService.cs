using InventoryApp.Application.DTOs.Supplier;
using InventoryApp.Application.Interfaces;
using InventoryApp.Domain.Entities;
using InventoryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Infrastructure.Services;

public class SupplierService : ISupplierService
{
    private readonly AppDbContext _context;

    public SupplierService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SupplierResponseDto>> GetAllAsync()
    {
        return await _context.Suppliers
            .AsNoTracking()
            .Select(s => new SupplierResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                ContactPerson = s.ContactPerson,
                Phone = s.Phone,
                Email = s.Email,
                Address = s.Address,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<SupplierResponseDto?> GetByIdAsync(long id)
    {
        return await _context.Suppliers
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new SupplierResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                ContactPerson = s.ContactPerson,
                Phone = s.Phone,
                Email = s.Email,
                Address = s.Address,
                CreatedAt = s.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<SupplierResponseDto> CreateAsync(CreateSupplierDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ArgumentException("Supplier name is required.");
        }

        var supplier = new Supplier
        {
            Name = dto.Name.Trim(),
            ContactPerson = dto.ContactPerson,
            Phone = dto.Phone,
            Email = dto.Email,
            Address = dto.Address
        };

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        return new SupplierResponseDto
        {
            Id = supplier.Id,
            Name = supplier.Name,
            ContactPerson = supplier.ContactPerson,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Address = supplier.Address,
            CreatedAt = supplier.CreatedAt
        };
    }

    public async Task<bool> UpdateAsync(long id, UpdateSupplierDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ArgumentException("Supplier name is required.");
        }

        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id);

        if (supplier == null)
        {
            return false;
        }

        supplier.Name = dto.Name.Trim();
        supplier.ContactPerson = dto.ContactPerson;
        supplier.Phone = dto.Phone;
        supplier.Email = dto.Email;
        supplier.Address = dto.Address;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id);

        if (supplier == null)
        {
            return false;
        }

        // Relationship validation:
        // A supplier cannot be deleted if it is used by products
        // or purchase orders.
        var isReferencedByProducts = await _context.Database
            .SqlQuery<int>($"""
                SELECT COUNT(*) AS "Value"
                FROM products
                WHERE supplier_id = {id}
                """)
            .AnyAsync(count => count > 0);

        var isReferencedByPurchaseOrders = await _context.Database
            .SqlQuery<int>($"""
                SELECT COUNT(*) AS "Value"
                FROM purchase_orders
                WHERE supplier_id = {id}
                """)
            .AnyAsync(count => count > 0);

        if (isReferencedByProducts || isReferencedByPurchaseOrders)
        {
            return false;
        }

        _context.Suppliers.Remove(supplier);

        await _context.SaveChangesAsync();

        return true;
    }
}
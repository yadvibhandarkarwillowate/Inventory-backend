using InventoryApp.Application.DTOs.Supplier;
using InventoryApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var suppliers = await _supplierService.GetAllAsync();

        return Ok(suppliers);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var supplier = await _supplierService.GetByIdAsync(id);

        if (supplier == null)
        {
            return NotFound(new
            {
                message = "Supplier not found."
            });
        }

        return Ok(supplier);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateSupplierDto dto)
    {
        var supplier = await _supplierService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = supplier.Id },
            supplier
        );
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(
        long id,
        UpdateSupplierDto dto)
    {
        var updated = await _supplierService.UpdateAsync(id, dto);

        if (!updated)
        {
            return NotFound(new
            {
                message = "Supplier not found."
            });
        }

        return Ok(new
        {
            message = "Supplier updated successfully."
        });
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var deleted = await _supplierService.DeleteAsync(id);

        if (!deleted)
        {
            return Conflict(new
            {
                message = "Supplier not found or cannot be deleted because it is referenced by other records."
            });
        }

        return Ok(new
        {
            message = "Supplier deleted successfully."
        });
    }
}
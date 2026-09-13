using InventoryApp.Application.DTOs.Inventory;
using InventoryApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApp.API.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    public InventoryController(IInventoryService inventoryService) => _inventoryService = inventoryService;

    [HttpGet("stock")]
    public async Task<ActionResult<PagedResult<StockLevelDto>>> GetStock(
        [FromQuery] string? search, [FromQuery] bool lowStockOnly = false,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _inventoryService.GetStockLevelsAsync(new StockQueryParams(search, lowStockOnly, page, pageSize));
        return Ok(result);
    }
}
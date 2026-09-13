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


    [HttpPost("issue")]
    public async Task<ActionResult<StockChangeResult>> IssueStock(IssueStockRequest request)
    {
        try
        {
            var result = await _inventoryService.IssueStockAsync(request, GetUserId());
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("damaged-lost")]
    public async Task<ActionResult<StockChangeResult>> RecordDamagedOrLost(DamagedLostRequest request)
    {
        try
        {
            var result = await _inventoryService.RecordDamagedOrLostAsync(request, GetUserId());
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
    
    [HttpPost("adjust")]
    public async Task<ActionResult<StockChangeResult>> AdjustStock(AdjustStockRequest request)
    {
        try
        {
            var result = await _inventoryService.AdjustStockAsync(request, GetUserId());
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
}

    [HttpGet("transactions")]
    public async Task<ActionResult<PagedResult<TransactionDto>>> GetTransactions(
        [FromQuery] long? productId, [FromQuery] string? type,
        [FromQuery] DateTimeOffset? fromDate, [FromQuery] DateTimeOffset? toDate,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _inventoryService.GetTransactionHistoryAsync(
            new TransactionQueryParams(productId, type, fromDate, toDate, page, pageSize));
        return Ok(result);
    }

    // TEMP helper — replace with real JWT claim once Auth is ready
    private long GetUserId() => 1;
}
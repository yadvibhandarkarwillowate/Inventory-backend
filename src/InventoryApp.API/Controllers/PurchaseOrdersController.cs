using System.Security.Claims;
using InventoryApp.Application.DTOs.Inventory;
using InventoryApp.Application.DTOs.PurchaseOrder;
using InventoryApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryApp.API.Controllers;

[ApiController]
[Route("api/purchase-orders")]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IPurchaseOrderService _poService;

    public PurchaseOrdersController(IPurchaseOrderService poService)
    {
        _poService = poService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<PurchaseOrderListDto>>> GetPurchaseOrders(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] long? supplierId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new PurchaseOrderQueryParams(search, status, supplierId, page, pageSize);
        var result = await _poService.GetPurchaseOrdersAsync(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PurchaseOrderResponseDto>> GetPurchaseOrder(long id)
    {
        var po = await _poService.GetPurchaseOrderByIdAsync(id);
        if (po == null)
            return NotFound(new { message = $"Purchase Order with ID {id} not found." });

        return Ok(po);
    }

    [HttpPost]
    public async Task<ActionResult<PurchaseOrderResponseDto>> CreatePurchaseOrder(CreatePurchaseOrderDto dto)
    {
        try
        {
            var userId = GetUserId();
            var po = await _poService.CreatePurchaseOrderAsync(dto, userId);
            return CreatedAtAction(nameof(GetPurchaseOrder), new { id = po.Id }, po);
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

    [HttpPost("{id}/receive")]
    public async Task<ActionResult<ReceiveStockResultDto>> ReceiveStock(long id, ReceiveStockRequestDto request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _poService.ReceiveStockAsync(id, request, userId);
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

    private long GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var userId) ? userId : 1;
    }
}

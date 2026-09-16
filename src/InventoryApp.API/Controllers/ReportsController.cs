using InventoryApp.Application.Reports;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;

namespace InventoryApp.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportsService _reportsService;

    public ReportsController(IReportsService reportsService)
    {
        _reportsService = reportsService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetDashboardSummaryAsync()
    {
        try
        {
            return Ok(await _reportsService.GetDashboardSummaryAsync());
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

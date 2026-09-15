using InventoryApp.Application.Reports.DTOs;

namespace InventoryApp.Application.Reports;

public interface IReportsService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync();
}

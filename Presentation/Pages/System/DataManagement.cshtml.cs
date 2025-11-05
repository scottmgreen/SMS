using Microsoft.AspNetCore.Mvc.RazorPages;
using PDXSMS.Services;

namespace PDXSMS_Presentation.Pages.System;

/// <summary>
/// Data Management - Manage data backup, archival, and maintenance operations
/// </summary>
public class DataManagementModel : PageModel
{
    private readonly DashboardDataService _dashboardService;
    
    public DataManagementStats Overview { get; set; } = new();

    public DataManagementModel(DashboardDataService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Data Management - System Administration";
        Overview = await _dashboardService.GetDataManagementStatsAsync();
    }
}
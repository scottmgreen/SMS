using Microsoft.AspNetCore.Mvc.RazorPages;


namespace PDXSMS_Presentation.Pages.System;

/// <summary>
/// System Administration - Main Landing Page
/// Central hub for system configuration, user management, and administrative functions
/// </summary>
public class IndexModel : PageModel
{
    //private readonly DashboardDataService _dashboardService;
    
    //public SystemAdministrationStats Dashboard { get; set; } = new();

    //public IndexModel(DashboardDataService dashboardService)
    //{
    //    _dashboardService = dashboardService;
    //}

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "System Administration";
        //Dashboard = await _dashboardService.GetSystemAdministrationStatsAsync();
    }
}
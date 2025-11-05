using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using PDXSMS.Services;

namespace PDXSMS_Presentation.Pages;

/// <summary>
/// SMS Dashboard - Main entry point for the Safety Management System
/// Provides overview of system status, recent activities, and navigation to all modules
/// </summary>
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly DashboardDataService _dashboardService;

    // Properties for dashboard data
    public MainDashboardStats DashboardStats { get; set; } = new();

    public IndexModel(ILogger<IndexModel> logger, DashboardDataService dashboardService)
    {
        _logger = logger;
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // CRITICAL SECURITY: Prevent guest users from accessing the dashboard
        var isGuest = HttpContext.Session.GetString("IsGuest") == "true";
        
        if (isGuest)
        {
            _logger.LogWarning("SECURITY VIOLATION: Guest user attempting to access dashboard. Clearing session and redirecting to login.");
            HttpContext.Session.Clear();
            return RedirectToPage("/Account/Login");
        }

        ViewData["Title"] = "PDXSMS - Safety Management System Dashboard";

        // Load real dashboard statistics
        DashboardStats = await _dashboardService.GetMainDashboardStatsAsync();

        return Page();
    }
}
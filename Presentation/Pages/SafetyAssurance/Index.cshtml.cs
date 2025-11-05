using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PDXSMS_Presentation.Pages.SafetyAssurance;

/// <summary>
/// Section 2: Safety Assurance - Main Landing Page
/// Overview and navigation hub for all safety assurance activities
/// </summary>
public class IndexModel : PageModel
{
    public SafetyAssuranceDashboard Dashboard { get; set; } = new();

    public void OnGet()
    {
        ViewData["Title"] = "Section 2: Safety Assurance";
        LoadDashboardData();
    }

    private void LoadDashboardData()
    {
        Dashboard = new SafetyAssuranceDashboard
        {
            ActiveSPIs = 12,
            SPIsWithinTarget = 10,
            SPIsRequiringAttention = 2,
            UpcomingAudits = 3,
            ActiveAudits = 1,
            CompletedAuditsThisQuarter = 8,
            ReportsGeneratedThisMonth = 5,
            OverallComplianceRating = 94,
            LastAuditDate = DateTime.Now.AddDays(-12),
            NextScheduledAudit = DateTime.Now.AddDays(18)
        };
    }
}

public class SafetyAssuranceDashboard
{
    public int ActiveSPIs { get; set; }
    public int SPIsWithinTarget { get; set; }
    public int SPIsRequiringAttention { get; set; }
    public int UpcomingAudits { get; set; }
    public int ActiveAudits { get; set; }
    public int CompletedAuditsThisQuarter { get; set; }
    public int ReportsGeneratedThisMonth { get; set; }
    public int OverallComplianceRating { get; set; }
    public DateTime LastAuditDate { get; set; }
    public DateTime NextScheduledAudit { get; set; }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PDXSMS_Presentation.Pages.SafetyAssurance;

/// <summary>
/// FR-2.1.2: Safety Audits and Inspections
/// Schedule, conduct, and manage safety audits and inspections
/// </summary>
public class SafetyAuditsModel : PageModel
{
    public List<SafetyAuditSummary> UpcomingAudits { get; set; } = new();
    public List<SafetyAuditSummary> ActiveAudits { get; set; } = new();
    public List<SafetyAuditSummary> CompletedAudits { get; set; } = new();

    [BindProperty]
    public CreateAuditViewModel NewAudit { get; set; } = new();

    public void OnGet()
    {
        ViewData["Title"] = "FR-2.1.2: Safety Audits and Inspections";
        LoadAudits();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            LoadAudits();
            return Page();
        }

        // TODO: Create new audit
        TempData["SuccessMessage"] = $"Safety Audit '{NewAudit.AuditTitle}' scheduled successfully. ID: SA-2024-001";
        return RedirectToPage();
    }

    private void LoadAudits()
    {
        var allAudits = GetMockAudits();
        UpcomingAudits = allAudits.Where(a => a.Status == "Scheduled").ToList();
        ActiveAudits = allAudits.Where(a => a.Status == "In Progress").ToList();
        CompletedAudits = allAudits.Where(a => a.Status == "Completed").ToList();
    }

    private List<SafetyAuditSummary> GetMockAudits()
    {
        return new List<SafetyAuditSummary>
        {
            new() { Id = "SA-2024-001", Title = "Terminal B Security Operations Audit", AuditType = "Internal", PlannedStartDate = DateTime.Now.AddDays(7), PlannedEndDate = DateTime.Now.AddDays(14), LeadAuditor = "Sarah Johnson", Department = "Security Operations", Status = "Scheduled", Priority = "High" },
            new() { Id = "SA-2024-002", Title = "Ground Support Equipment Inspection", AuditType = "Compliance", PlannedStartDate = DateTime.Now.AddDays(14), PlannedEndDate = DateTime.Now.AddDays(16), LeadAuditor = "Mike Brown", Department = "Ground Operations", Status = "Scheduled", Priority = "Medium" },
            new() { Id = "SA-2024-003", Title = "Runway Safety Area Assessment", AuditType = "External", PlannedStartDate = DateTime.Now.AddDays(-5), PlannedEndDate = DateTime.Now.AddDays(2), LeadAuditor = "FAA Inspector", Department = "Airfield Operations", Status = "In Progress", Priority = "Critical" },
            new() { Id = "SA-2024-004", Title = "Emergency Response Procedures Review", AuditType = "Internal", PlannedStartDate = DateTime.Now.AddDays(-15), PlannedEndDate = DateTime.Now.AddDays(-10), LeadAuditor = "Tom Davis", Department = "Emergency Services", Status = "Completed", Priority = "High", CompletedDate = DateTime.Now.AddDays(-8) },
            new() { Id = "SA-2024-005", Title = "Baggage Handling System Compliance", AuditType = "Compliance", PlannedStartDate = DateTime.Now.AddDays(-20), PlannedEndDate = DateTime.Now.AddDays(-18), LeadAuditor = "Lisa Wilson", Department = "Baggage Operations", Status = "Completed", Priority = "Medium", CompletedDate = DateTime.Now.AddDays(-17) }
        };
    }
}

public class SafetyAuditSummary
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string AuditType { get; set; } = string.Empty;
    public DateTime PlannedStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public string LeadAuditor { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime? CompletedDate { get; set; }
}

public class CreateAuditViewModel
{
    public string AuditTitle { get; set; } = string.Empty;
    public string AuditType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime PlannedStartDate { get; set; } = DateTime.Now.AddDays(7);
    public DateTime PlannedEndDate { get; set; } = DateTime.Now.AddDays(14);
    public string LeadAuditor { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string AuditScope { get; set; } = string.Empty;
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PDXSMS_Presentation.Pages.SafetyPromotion;

/// <summary>
/// Section 5: Safety Communications
/// Safety bulletins, operational briefings, committee summaries, and digital updates
/// Based on Section 5 requirements for safety promotion and communication
/// </summary>
public class SafetyCommunicationsModel : PageModel
{
    public List<SafetyBulletin> RecentBulletins { get; set; } = new();
    public List<OperationalBriefing> RecentBriefings { get; set; } = new();
    public List<CommitteeSummary> RecentSummaries { get; set; } = new();
    public List<DigitalUpdate> RecentUpdates { get; set; } = new();

    [BindProperty]
    public CreateCommunicationViewModel NewCommunication { get; set; } = new();

    public void OnGet()
    {
        ViewData["Title"] = "Section 5: Safety Communications";
        LoadCommunications();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            LoadCommunications();
            return Page();
        }

        // TODO: Create new communication
        TempData["SuccessMessage"] = $"{NewCommunication.Type} created and distributed successfully.";
        return RedirectToPage();
    }

    private void LoadCommunications()
    {
        RecentBulletins = GetMockBulletins();
        RecentBriefings = GetMockBriefings();
        RecentSummaries = GetMockSummaries();
        RecentUpdates = GetMockUpdates();
    }

    private List<SafetyBulletin> GetMockBulletins()
    {
        return new List<SafetyBulletin>
        {
            new() { Id = "SB-2024-001", Title = "Winter Weather Operations Reminder", IssueDate = DateTime.Now.AddDays(-2), Status = "Active", Priority = "High" },
            new() { Id = "SB-2024-002", Title = "Updated FOD Prevention Procedures", IssueDate = DateTime.Now.AddDays(-5), Status = "Active", Priority = "Medium" },
            new() { Id = "SB-2024-003", Title = "Emergency Equipment Inspection Schedule", IssueDate = DateTime.Now.AddDays(-7), Status = "Archived", Priority = "Low" }
        };
    }

    private List<OperationalBriefing> GetMockBriefings()
    {
        return new List<OperationalBriefing>
        {
            new() { Id = "OB-2024-001", Title = "Shift Handover - Morning Operations", BriefingDate = DateTime.Now.AddHours(-2), ConductedBy = "Operations Supervisor", Audience = "Ground Crew" },
            new() { Id = "OB-2024-002", Title = "Runway Construction Update", BriefingDate = DateTime.Now.AddHours(-8), ConductedBy = "Airport Operations", Audience = "All Staff" }
        };
    }

    private List<CommitteeSummary> GetMockSummaries()
    {
        return new List<CommitteeSummary>
        {
            new() { Id = "CS-2024-001", Title = "SMS Airside Safety Committee - January 2024", SummaryDate = DateTime.Now.AddDays(-3), CommitteeName = "SMS Airside Safety Committee", KeyFindings = new[] { "Reviewed 5 hazard reports", "Approved 3 mitigation plans" } },
            new() { Id = "CS-2024-002", Title = "Rapid Review Team - Weekly Summary", SummaryDate = DateTime.Now.AddDays(-1), CommitteeName = "SMS Rapid Review Team", KeyFindings = new[] { "Processed 12 reports", "Escalated 2 high-priority hazards" } }
        };
    }

    private List<DigitalUpdate> GetMockUpdates()
    {
        return new List<DigitalUpdate>
        {
            new() { Id = "DU-2024-001", Title = "SMS Web Portal Enhancement", UpdateDate = DateTime.Now.AddDays(-1), Channel = "Web Portal", Content = "New hazard reporting features available" },
            new() { Id = "DU-2024-002", Title = "Mobile App Security Update", UpdateDate = DateTime.Now.AddDays(-4), Channel = "Mobile App", Content = "Enhanced security features and bug fixes" }
        };
    }
}

public class SafetyBulletin
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
}

public class OperationalBriefing
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime BriefingDate { get; set; }
    public string ConductedBy { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}

public class CommitteeSummary
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime SummaryDate { get; set; }
    public string CommitteeName { get; set; } = string.Empty;
    public string[] KeyFindings { get; set; } = Array.Empty<string>();
}

public class DigitalUpdate
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime UpdateDate { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class CreateCommunicationViewModel
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string Channel { get; set; } = string.Empty;
}

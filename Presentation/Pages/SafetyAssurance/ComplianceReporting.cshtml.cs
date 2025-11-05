using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PDXSMS_Presentation.Pages.SafetyAssurance;

/// <summary>
/// Compliance Reporting - Generate regulatory compliance reports and documentation
/// </summary>
public class ComplianceReportingModel : PageModel
{
    public List<ComplianceReport> Reports { get; set; } = new();
    public List<RegulatoryRequirement> Requirements { get; set; } = new();
    public ComplianceOverview Overview { get; set; } = new();

    public void OnGet()
    {
        ViewData["Title"] = "Compliance Reporting - Safety Assurance";
        LoadComplianceData();
    }

    private void LoadComplianceData()
    {
        Reports = GetMockReports();
        Requirements = GetMockRequirements();
        Overview = new ComplianceOverview
        {
            OverallComplianceRate = 94,
            ReportsGenerated = Reports.Count,
            ActiveRequirements = Requirements.Count(r => r.IsActive),
            UpcomingDeadlines = Requirements.Count(r => r.NextDeadline <= DateTime.Now.AddDays(30)),
            LastAuditDate = DateTime.Now.AddDays(-45)
        };
    }

    private List<ComplianceReport> GetMockReports()
    {
        return new List<ComplianceReport>
        {
            new() { Id = "CR-2024-001", Title = "Q1 2024 SMS Compliance Report", Type = "SMS Annual Report", Status = "Completed", GeneratedDate = DateTime.Now.AddDays(-5), DueDate = DateTime.Now.AddDays(-1), Regulation = "14 CFR Part 139" },
            new() { Id = "CR-2024-002", Title = "Safety Performance Report", Type = "Performance Report", Status = "In Progress", GeneratedDate = DateTime.Now.AddDays(-2), DueDate = DateTime.Now.AddDays(7), Regulation = "14 CFR Part 139.303" },
            new() { Id = "CR-2024-003", Title = "Emergency Response Compliance", Type = "Emergency Preparedness", Status = "Draft", GeneratedDate = DateTime.Now.AddDays(-1), DueDate = DateTime.Now.AddDays(14), Regulation = "14 CFR Part 139.325" },
            new() { Id = "CR-2024-004", Title = "Training Records Certification", Type = "Training Compliance", Status = "Completed", GeneratedDate = DateTime.Now.AddDays(-10), DueDate = DateTime.Now.AddDays(-3), Regulation = "14 CFR Part 139.303" },
            new() { Id = "CR-2024-005", Title = "Wildlife Hazard Assessment", Type = "Environmental Report", Status = "Scheduled", GeneratedDate = null, DueDate = DateTime.Now.AddDays(21), Regulation = "14 CFR Part 139.337" }
        };
    }

    private List<RegulatoryRequirement> GetMockRequirements()
    {
        return new List<RegulatoryRequirement>
        {
            new() { Id = "REQ-001", Title = "Annual SMS Report", Regulation = "14 CFR Part 139.303", Frequency = "Annual", NextDeadline = DateTime.Now.AddDays(90), IsActive = true, ComplianceLevel = 100 },
            new() { Id = "REQ-002", Title = "Emergency Response Plan Review", Regulation = "14 CFR Part 139.325", Frequency = "Annual", NextDeadline = DateTime.Now.AddDays(45), IsActive = true, ComplianceLevel = 95 },
            new() { Id = "REQ-003", Title = "Safety Training Documentation", Regulation = "14 CFR Part 139.303", Frequency = "Ongoing", NextDeadline = DateTime.Now.AddDays(15), IsActive = true, ComplianceLevel = 88 },
            new() { Id = "REQ-004", Title = "Wildlife Hazard Management", Regulation = "14 CFR Part 139.337", Frequency = "Bi-Annual", NextDeadline = DateTime.Now.AddDays(180), IsActive = true, ComplianceLevel = 100 },
            new() { Id = "REQ-005", Title = "Pavement Condition Assessment", Regulation = "14 CFR Part 139.305", Frequency = "Annual", NextDeadline = DateTime.Now.AddDays(120), IsActive = true, ComplianceLevel = 92 }
        };
    }
}

public class ComplianceReport
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? GeneratedDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Regulation { get; set; } = string.Empty;
}

public class RegulatoryRequirement
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Regulation { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public DateTime NextDeadline { get; set; }
    public bool IsActive { get; set; }
    public int ComplianceLevel { get; set; }
}

public class ComplianceOverview
{
    public int OverallComplianceRate { get; set; }
    public int ReportsGenerated { get; set; }
    public int ActiveRequirements { get; set; }
    public int UpcomingDeadlines { get; set; }
    public DateTime LastAuditDate { get; set; }
}

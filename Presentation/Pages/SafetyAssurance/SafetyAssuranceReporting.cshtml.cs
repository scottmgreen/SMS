using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PDXSMS_Presentation.Pages.SafetyAssurance;

/// <summary>
/// FR-2.1.3: Safety Assurance Reporting
/// Generate and distribute safety assurance reports
/// </summary>
public class SafetyAssuranceReportingModel : PageModel
{
    public List<SafetyAssuranceReportSummary> RecentReports { get; set; } = new();
    public List<ReportTemplate> AvailableTemplates { get; set; } = new();
    public AssuranceMetrics Metrics { get; set; } = new();

    [BindProperty]
    public GenerateReportViewModel NewReport { get; set; } = new();

    public void OnGet()
    {
        ViewData["Title"] = "FR-2.1.3: Safety Assurance Reporting";
        LoadReports();
    }

    public async Task<IActionResult> OnPostGenerateAsync()
    {
        if (!ModelState.IsValid)
        {
            LoadReports();
            return Page();
        }

        // TODO: Generate new report
        TempData["SuccessMessage"] = $"Safety Assurance Report '{NewReport.ReportTitle}' generated successfully. ID: SAR-2024-001";
        return RedirectToPage();
    }

    private void LoadReports()
    {
        RecentReports = GetMockReports();
        AvailableTemplates = GetMockTemplates();
        Metrics = GetMockMetrics();
    }

    private List<SafetyAssuranceReportSummary> GetMockReports()
    {
        return new List<SafetyAssuranceReportSummary>
        {
            new() { Id = "SAR-2024-001", Title = "Q1 2024 Safety Assurance Report", ReportType = "Quarterly", GeneratedDate = DateTime.Now.AddDays(-5), GeneratedBy = "Safety Manager", Status = "Published", Department = "SMS Office" },
            new() { Id = "SAR-2024-002", Title = "January 2024 Monthly Safety Review", ReportType = "Monthly", GeneratedDate = DateTime.Now.AddDays(-10), GeneratedBy = "Quality Assurance", Status = "Published", Department = "QA Department" },
            new() { Id = "SAR-2024-003", Title = "Runway Safety Performance Analysis", ReportType = "Ad-hoc", GeneratedDate = DateTime.Now.AddDays(-3), GeneratedBy = "Operations Manager", Status = "Draft", Department = "Airfield Operations" },
            new() { Id = "SAR-2024-004", Title = "Emergency Response Effectiveness Review", ReportType = "Semi-annual", GeneratedDate = DateTime.Now.AddDays(-15), GeneratedBy = "Emergency Coordinator", Status = "Under Review", Department = "Emergency Services" }
        };
    }

    private List<ReportTemplate> GetMockTemplates()
    {
        return new List<ReportTemplate>
        {
            new() { Id = "TPL-001", Name = "Monthly Safety Performance Report", Type = "Monthly", Description = "Standard monthly safety performance analysis" },
            new() { Id = "TPL-002", Name = "Quarterly SMS Effectiveness Report", Type = "Quarterly", Description = "Comprehensive quarterly SMS effectiveness assessment" },
            new() { Id = "TPL-003", Name = "Annual Safety Review", Type = "Annual", Description = "Complete annual safety review and planning" },
            new() { Id = "TPL-004", Name = "Incident Analysis Report", Type = "Ad-hoc", Description = "Detailed incident investigation and analysis" },
            new() { Id = "TPL-005", Name = "Audit Findings Summary", Type = "Ad-hoc", Description = "Summary of audit findings and corrective actions" }
        };
    }

    private AssuranceMetrics GetMockMetrics()
    {
        return new AssuranceMetrics
        {
            TotalReports = 23,
            PublishedReports = 18,
            DraftReports = 3,
            UnderReviewReports = 2,
            AvgGenerationTime = 4.2,
            ComplianceRating = 94
        };
    }
}

public class SafetyAssuranceReportSummary
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public DateTime GeneratedDate { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}

public class ReportTemplate
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class AssuranceMetrics
{
    public int TotalReports { get; set; }
    public int PublishedReports { get; set; }
    public int DraftReports { get; set; }
    public int UnderReviewReports { get; set; }
    public double AvgGenerationTime { get; set; }
    public int ComplianceRating { get; set; }
}

public class GenerateReportViewModel
{
    public string ReportTitle { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public string TemplateId { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; } = DateTime.Now.AddMonths(-1);
    public DateTime PeriodEnd { get; set; } = DateTime.Now;
    public string Department { get; set; } = string.Empty;
    public bool IncludeCharts { get; set; } = true;
    public bool IncludeRecommendations { get; set; } = true;
    public string DistributionList { get; set; } = string.Empty;
}

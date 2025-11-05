using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PDXSMS_Presentation.Pages.SafetyAssurance;

/// <summary>
/// FR-2.1.1: Safety Performance Monitoring
/// Establish and monitor safety performance indicators (SPIs)
/// </summary>
public class SafetyPerformanceMonitoringModel : PageModel
{
    public List<SafetyPerformanceIndicator> ActiveSPIs { get; set; } = new();
    public List<PerformanceAlert> ActiveAlerts { get; set; } = new();
    public PerformanceDashboard Dashboard { get; set; } = new();

    [BindProperty]
    public CreateSPIViewModel NewSPI { get; set; } = new();

    public void OnGet()
    {
        ViewData["Title"] = "FR-2.1.1: Safety Performance Monitoring";
        LoadPerformanceData();
    }

    public async Task<IActionResult> OnPostCreateSPIAsync()
    {
        if (!ModelState.IsValid)
        {
            LoadPerformanceData();
            return Page();
        }

        // TODO: Create new SPI
        TempData["SuccessMessage"] = $"Safety Performance Indicator '{NewSPI.IndicatorName}' created successfully.";
        return RedirectToPage();
    }

    private void LoadPerformanceData()
    {
        ActiveSPIs = GetMockSPIs();
        ActiveAlerts = GetMockAlerts();
        Dashboard = GetMockDashboard();
    }

    private List<SafetyPerformanceIndicator> GetMockSPIs()
    {
        return new List<SafetyPerformanceIndicator>
        {
            new() { Id = "SPI-001", Name = "Hazard Reports per Month", CurrentValue = 23, TargetValue = 20, ThresholdValue = 35, Status = "Normal", DataType = "Count", Frequency = "Monthly", LastUpdated = DateTime.Now.AddDays(-1) },
            new() { Id = "SPI-002", Name = "Risk Assessment Completion Rate", CurrentValue = 95, TargetValue = 100, ThresholdValue = 85, Status = "Normal", DataType = "Percentage", Frequency = "Monthly", LastUpdated = DateTime.Now.AddDays(-2) },
            new() { Id = "SPI-003", Name = "Runway Incursions per Quarter", CurrentValue = 2, TargetValue = 0, ThresholdValue = 3, Status = "Alert", DataType = "Count", Frequency = "Quarterly", LastUpdated = DateTime.Now.AddDays(-3) },
            new() { Id = "SPI-004", Name = "Mitigation Implementation Rate", CurrentValue = 87, TargetValue = 95, ThresholdValue = 80, Status = "Warning", DataType = "Percentage", Frequency = "Monthly", LastUpdated = DateTime.Now.AddDays(-1) },
            new() { Id = "SPI-005", Name = "Safety Training Completion", CurrentValue = 98, TargetValue = 100, ThresholdValue = 90, Status = "Normal", DataType = "Percentage", Frequency = "Quarterly", LastUpdated = DateTime.Now.AddDays(-5) }
        };
    }

    private List<PerformanceAlert> GetMockAlerts()
    {
        return new List<PerformanceAlert>
        {
            new() { Id = "ALT-001", SPIId = "SPI-003", Message = "Runway incursions exceeded threshold for Q1", Severity = "High", CreatedDate = DateTime.Now.AddDays(-1), Status = "Active" },
            new() { Id = "ALT-002", SPIId = "SPI-004", Message = "Mitigation implementation rate below target", Severity = "Medium", CreatedDate = DateTime.Now.AddDays(-2), Status = "Active" }
        };
    }

    private PerformanceDashboard GetMockDashboard()
    {
        return new PerformanceDashboard
        {
            TotalSPIs = 12,
            NormalStatus = 8,
            WarningStatus = 3,
            AlertStatus = 1,
            OverallPerformance = 92,
            TrendDirection = "Improving"
        };
    }
}

public class SafetyPerformanceIndicator
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal CurrentValue { get; set; }
    public decimal TargetValue { get; set; }
    public decimal ThresholdValue { get; set; }
    public string Status { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}

public class PerformanceAlert
{
    public string Id { get; set; } = string.Empty;
    public string SPIId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class PerformanceDashboard
{
    public int TotalSPIs { get; set; }
    public int NormalStatus { get; set; }
    public int WarningStatus { get; set; }
    public int AlertStatus { get; set; }
    public decimal OverallPerformance { get; set; }
    public string TrendDirection { get; set; } = string.Empty;
}

public class CreateSPIViewModel
{
    public string IndicatorName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public decimal TargetValue { get; set; }
    public decimal ThresholdValue { get; set; }
    public string ResponsiblePerson { get; set; } = string.Empty;
}

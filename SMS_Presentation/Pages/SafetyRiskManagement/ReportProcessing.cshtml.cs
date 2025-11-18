using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;

using SMS_Domain.Common;
using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS_Presentation.Pages.SafetyRiskManagement;

/// <summary>
/// Report Processing page - Enhanced CQRS implementation with Smart Enums
/// Loads Reports with related Hazards using proper status categorization
/// </summary>
public class ReportProcessingModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReportProcessingModel> _logger;

    public ReportProcessingModel(IMediator mediator, ILogger<ReportProcessingModel> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // Enhanced UI Properties with complete workflow tracking
    public ReportProcessingDashboard Dashboard { get; set; } = new();
    public List<ReportProcessingSummary> PendingValidation { get; set; } = new();
    public List<ReportProcessingSummary> PendingRiskAssessment { get; set; } = new();
    public List<ReportProcessingSummary> PendingInvestigation { get; set; } = new();
    public List<ReportProcessingSummary> PendingTRA { get; set; } = new();
    public List<ReportProcessingSummary> PendingLeadershipApproval { get; set; } = new();
    public List<ReportProcessingSummary> InMitigation { get; set; } = new();
    public List<ReportProcessingSummary> ClosedReferred { get; set; } = new();

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "Report Processing";
        await LoadReportProcessingDataAsync();
    }

    /// <summary>
    /// Enhanced data loading using CQRS to retrieve Reports with related Hazards
    /// </summary>
    private async Task LoadReportProcessingDataAsync()
    {
        try
        {
            _logger.LogInformation("Loading comprehensive report processing data using CQRS mediator");

            // Load core entities using CQRS
            var (reports, hazards) = await LoadCoreEntitiesAsync();

            if (!reports.Any())
            {
                _logger.LogWarning("No reports found - initializing empty dashboard");
                InitializeEmptyDashboard();
                return;
            }

            // Create comprehensive report summaries with related data
            var reportSummaries = CreateReportSummaries(reports, hazards);
            
            _logger.LogInformation("Created {Count} comprehensive report summaries", reportSummaries.Count);

            // Categorize reports using Smart Enum-based logic
            CategorizeReportsBySmartEnumStatus(reportSummaries);

            // Calculate enhanced dashboard metrics
            CalculateEnhancedDashboardMetrics(reportSummaries);

            _logger.LogInformation("Report processing data loaded successfully - Active: {Active}, Categories: V:{V}, RA:{RA}, I:{I}, M:{M}", 
                Dashboard.TotalActive, Dashboard.PendingValidation, Dashboard.PendingRiskAssessment, 
                Dashboard.PendingInvestigation, Dashboard.InMitigation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading comprehensive report processing data");
            InitializeEmptyDashboard();
        }
    }

    /// <summary>
    /// Load core entities through CQRS queries
    /// </summary>
    private async Task<(List<Report> reports, List<Hazard> hazards)> LoadCoreEntitiesAsync()
    {
        var reports = new List<Report>();
        var hazards = new List<Hazard>();

        try
        {
            // Get all reports
            var reportsQuery = new GetAllReportsQuery();
            var reportsResult = await _mediator.SendAsync(reportsQuery, CancellationToken.None);
            if (reportsResult.IsSuccess)
            {
                reports = reportsResult.Value ?? new List<Report>();
                _logger.LogInformation("Retrieved {Count} reports", reports.Count);
            }
            else
            {
                _logger.LogError("Failed to retrieve reports: {Error}", reportsResult.Error?.Message);
            }

            // Get all hazards
            var hazardsQuery = new GetAllHazardsQuery();
            var hazardsResult = await _mediator.SendAsync(hazardsQuery, CancellationToken.None);
            if (hazardsResult.IsSuccess)
            {
                hazards = hazardsResult.Value ?? new List<Hazard>();
                _logger.LogInformation("Retrieved {Count} hazards", hazards.Count);
            }
            else
            {
                _logger.LogError("Failed to retrieve hazards: {Error}", hazardsResult.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in core entities retrieval");
        }

        return (reports, hazards);
    }

    /// <summary>
    /// Create comprehensive report summaries with related data using advanced LINQ
    /// </summary>
    private List<ReportProcessingSummary> CreateReportSummaries(List<Report> reports, List<Hazard> hazards)
    {
        return (from report in reports
                // Left join with hazards
                join hazard in hazards on report.Code equals hazard.ReportCode into hazardGroup
                from hazard in hazardGroup.DefaultIfEmpty()
                
                select new ReportProcessingSummary
                {
                    // Core Report Data
                    ReportId = report.Code,
                    ReportDescription = report.Description,
                    ReportStage = report.Stage ?? "Initial",
                    ReportStatus = report.Status ?? "New",
                    CreatedBy = report.CreatedBy,
                    CreatedDate = report.CreatedDate ?? DateTime.UtcNow,
                    
                    // Hazard Data (if exists)
                    HazardId = hazard?.Code,
                    HazardType = hazard?.HazardType ?? "Unknown",
                    HazardDescription = hazard?.Description ?? report.Description,
                    Location = hazard?.HazardLocation?.Description ?? hazard?.LocationArea ?? "Not specified",
                    Priority = GetPriorityString(hazard?.Priority),
                    ReportedBy = hazard?.ReportedBy ?? report.CreatedBy,
                    ReportedDate = hazard?.ReportedOn ?? report.CreatedDate ?? DateTime.UtcNow,
                    IsConfidential = hazard?.IsConfidential ?? false,
                    
                    // Computed Status using Smart Enums
                    EffectiveStatus = DetermineEffectiveStatusWithSmartEnum(report, hazard),
                    ProcessStage = DetermineProcessStageWithSmartEnum(report, hazard),
                    DaysInStage = CalculateDaysInStageEnhanced(report, hazard),
                    AssignedTo = DetermineAssignedToEnhanced(report, hazard),
                    RiskScore = DetermineRiskScoreEnhanced(hazard),
                    
                    // Navigation properties
                    CanValidate = CanValidateReportEnhanced(report, hazard),
                    ValidationUrl = GetValidationUrlEnhanced(report, hazard),
                    
                    // Workflow Status Category using your specified categories
                    StatusCategory = DetermineStatusCategory(report, hazard)
                }).ToList();
    }

    /// <summary>
    /// Categorize reports using Smart Enum-based status logic
    /// </summary>
    private void CategorizeReportsBySmartEnumStatus(List<ReportProcessingSummary> reports)
    {
        PendingValidation = reports
            .Where(r => IsStatusInCategory(r.EffectiveStatus, "Validation"))
            .OrderByDescending(r => r.ReportedDate)
            .ToList();

        PendingRiskAssessment = reports
            .Where(r => IsStatusInCategory(r.EffectiveStatus, "RiskAssessment"))
            .OrderByDescending(r => r.ReportedDate)
            .ToList();

        PendingInvestigation = reports
            .Where(r => IsStatusInCategory(r.EffectiveStatus, "Investigation"))
            .OrderByDescending(r => r.ReportedDate)
            .ToList();

        PendingTRA = reports
            .Where(r => IsStatusInCategory(r.EffectiveStatus, "TRA"))
            .OrderByDescending(r => r.ReportedDate)
            .ToList();

        PendingLeadershipApproval = reports
            .Where(r => IsStatusInCategory(r.EffectiveStatus, "Leadership"))
            .OrderByDescending(r => r.ReportedDate)
            .ToList();

        InMitigation = reports
            .Where(r => IsStatusInCategory(r.EffectiveStatus, "Mitigation"))
            .OrderByDescending(r => r.ReportedDate)
            .ToList();

        ClosedReferred = reports
            .Where(r => IsStatusInCategory(r.EffectiveStatus, "Closed"))
            .OrderByDescending(r => r.ReportedDate)
            .ToList();

        _logger.LogInformation("Categorized reports - V:{V}, RA:{RA}, I:{I}, TRA:{TRA}, L:{L}, M:{M}, C:{C}",
            PendingValidation.Count, PendingRiskAssessment.Count, PendingInvestigation.Count,
            PendingTRA.Count, PendingLeadershipApproval.Count, InMitigation.Count, ClosedReferred.Count);
    }

    #region Smart Enum-Based Status Determination

    /// <summary>
    /// Determine status category using Smart Enum logic according to your requirements
    /// </summary>
    private ProcessingStatusCategory DetermineStatusCategory(Report report, Hazard? hazard)
    {
        // Use hazard status if available (more current), otherwise report status
        var effectiveStatus = hazard?.Status?.ToString() ?? report.Status ?? "New";

        return GetCategoryFromStatus(effectiveStatus);
    }

    private ProcessingStatusCategory GetCategoryFromStatus(string status)
    {
        return status switch
        {
            // Validation Category
            "Initial" or "New" or "Submitted" or "Under Review" or "ACTIVE" => ProcessingStatusCategory.Validation,
            
            // RiskAssessment Category
            "SMS Risk Assessment - In Progress" or "Risk Assessment" or "Processing" or "UNDER_REVIEW" => ProcessingStatusCategory.RiskAssessment,
            
            // Investigation Category
            "Under Investigation - Information Needed" or "Investigation Required" or "Pending Investigation" or "UNDER_INVESTIGATION" => ProcessingStatusCategory.Investigation,
            
            // TRA Category
            "TRA Required" or "Technical Assessment" => ProcessingStatusCategory.TRA,
            
            // Leadership Category
            "Leadership Review" or "Awaiting Approval" => ProcessingStatusCategory.Leadership,
            
            // Mitigation Category
            "Mitigation Planning" or "In Progress" or "Implementation" or "Tracking" => ProcessingStatusCategory.Mitigation,
            
            // Closed Category
            "Closed" or "Referred" or "Completed" or "Closed - Not SMS Risk" or "CLOSED" or "CANCELLED" => ProcessingStatusCategory.Closed,
            
            _ => ProcessingStatusCategory.Validation
        };
    }

    private bool IsStatusInCategory(string status, string category)
    {
        var statusCategory = GetCategoryFromStatus(status);
        
        return category switch
        {
            "Validation" => statusCategory == ProcessingStatusCategory.Validation,
            "RiskAssessment" => statusCategory == ProcessingStatusCategory.RiskAssessment,
            "Investigation" => statusCategory == ProcessingStatusCategory.Investigation,
            "TRA" => statusCategory == ProcessingStatusCategory.TRA,
            "Leadership" => statusCategory == ProcessingStatusCategory.Leadership,
            "Mitigation" => statusCategory == ProcessingStatusCategory.Mitigation,
            "Closed" => statusCategory == ProcessingStatusCategory.Closed,
            _ => false
        };
    }

    #endregion

    #region Helper Methods

    private string GetPriorityString(HazardPriority? priority)
    {
        return priority?.ToString() ?? "Medium";
    }

    private string DetermineEffectiveStatusWithSmartEnum(Report report, Hazard? hazard)
    {
        // Hazard status takes precedence if it exists (more current)
        return hazard?.Status?.ToString() ?? report.Status ?? "Initial";
    }

    private string DetermineProcessStageWithSmartEnum(Report report, Hazard? hazard)
    {
        var category = DetermineStatusCategory(report, hazard);
        return category.ToString();
    }

    private int CalculateDaysInStageEnhanced(Report report, Hazard? hazard)
    {
        var referenceDate = hazard?.UpdatedDate ?? hazard?.CreatedDate ?? 
                           report.UpdatedDate ?? report.CreatedDate ?? DateTime.UtcNow;
        
        return (DateTime.UtcNow - referenceDate).Days;
    }

    private string? DetermineAssignedToEnhanced(Report report, Hazard? hazard)
    {
        var category = DetermineStatusCategory(report, hazard);
        return category switch
        {
            ProcessingStatusCategory.Validation => "SMS Validation Team",
            ProcessingStatusCategory.RiskAssessment => "Risk Assessment Team",
            ProcessingStatusCategory.Investigation => "Investigation Team", 
            ProcessingStatusCategory.Mitigation => "Operations Team",
            _ => null
        };
    }

    private string? DetermineRiskScoreEnhanced(Hazard? hazard)
    {
        return hazard?.Priority?.ToString();
    }

    private bool CanValidateReportEnhanced(Report report, Hazard? hazard)
    {
        var category = DetermineStatusCategory(report, hazard);
        return category == ProcessingStatusCategory.Validation;
    }

    private string GetValidationUrlEnhanced(Report report, Hazard? hazard)
    {
        return $"/SafetyRiskManagement/ReportValidation/{report.Code}";
    }

    #endregion

    #region Dashboard and Metrics

    private void CalculateEnhancedDashboardMetrics(List<ReportProcessingSummary> reports)
    {
        var activeReports = reports.Where(r => r.StatusCategory != ProcessingStatusCategory.Closed).ToList();
        var closedThisMonth = reports.Where(r => r.StatusCategory == ProcessingStatusCategory.Closed && 
            r.ReportedDate >= DateTime.UtcNow.AddMonths(-1)).ToList();

        Dashboard = new ReportProcessingDashboard
        {
            TotalActive = activeReports.Count,
            PendingValidation = PendingValidation.Count,
            PendingRiskAssessment = PendingRiskAssessment.Count,
            PendingInvestigation = PendingInvestigation.Count,
            //PendingTRA = PendingTRA.Count,
            //PendingLeadershipApproval = PendingLeadershipApproval.Count,
            InMitigation = InMitigation.Count,
            ClosedThisMonth = closedThisMonth.Count,
            AverageProcessingTime = CalculateAverageProcessingTimeEnhanced(ClosedReferred)
        };
    }

    private double CalculateAverageProcessingTimeEnhanced(List<ReportProcessingSummary> closedReports)
    {
        if (!closedReports.Any()) return 0.0;

        var processingTimes = closedReports.Select(r => r.DaysInStage).ToList();
        return processingTimes.Average();
    }

    private void InitializeEmptyDashboard()
    {
        Dashboard = new ReportProcessingDashboard();
        PendingValidation = new List<ReportProcessingSummary>();
        PendingRiskAssessment = new List<ReportProcessingSummary>();
        PendingInvestigation = new List<ReportProcessingSummary>();
        PendingTRA = new List<ReportProcessingSummary>();
        PendingLeadershipApproval = new List<ReportProcessingSummary>();
        InMitigation = new List<ReportProcessingSummary>();
        ClosedReferred = new List<ReportProcessingSummary>();
    }

    #endregion
}

#region Supporting Types

/// <summary>
/// Processing status categories based on your requirements
/// </summary>
public enum ProcessingStatusCategory
{
    Validation,
    RiskAssessment,
    Investigation,
    TRA,
    Leadership,
    Mitigation,
    Closed
}

/// <summary>
/// Enhanced report processing summary with all related data
/// </summary>
public class ReportProcessingSummary
{
    // Core Report Data
    public string ReportId { get; set; } = string.Empty;
    public string ReportDescription { get; set; } = string.Empty;
    public string ReportStage { get; set; } = string.Empty;
    public string ReportStatus { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }

    // Hazard Data
    public string? HazardId { get; set; }
    public string HazardType { get; set; } = string.Empty;
    public string HazardDescription { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string ReportedBy { get; set; } = string.Empty;
    public DateTime ReportedDate { get; set; }
    public bool IsConfidential { get; set; }

    // Computed Status Properties
    public string EffectiveStatus { get; set; } = string.Empty;
    public string ProcessStage { get; set; } = string.Empty;
    public int DaysInStage { get; set; }
    public string? AssignedTo { get; set; }
    public string? RiskScore { get; set; }
    public ProcessingStatusCategory StatusCategory { get; set; }

    // Navigation Properties
    public bool CanValidate { get; set; }
    public string ValidationUrl { get; set; } = string.Empty;

    /// <summary>
    /// Display ID - shows HazardId if available, otherwise ReportId
    /// </summary>
    public string DisplayId => !string.IsNullOrEmpty(HazardId) ? HazardId : ReportId;

    /// <summary>
    /// Display type for UI
    /// </summary>
    public string DisplayType => !string.IsNullOrEmpty(HazardType) && HazardType != "Unknown" ? HazardType : "Report";
}

/// <summary>
/// Dashboard metrics for the enhanced report processing overview
/// </summary>
public class ReportProcessingDashboard
{
    public int TotalActive { get; set; }
    public int PendingValidation { get; set; }
    public int PendingRiskAssessment { get; set; }
    public int PendingInvestigation { get; set; }
    public int InMitigation { get; set; }
    public int ClosedThisMonth { get; set; }
    public double AverageProcessingTime { get; set; }
}

#endregion










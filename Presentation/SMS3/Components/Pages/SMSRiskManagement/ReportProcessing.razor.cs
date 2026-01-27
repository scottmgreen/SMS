using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace SMS3.Components.Pages.SMSRiskManagement;

/// <summary>
/// Code-behind for ReportProcessing page
/// Manages the comprehensive report processing workflow for all SMS reports
/// </summary>
public partial class ReportProcessing : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<ReportProcessing> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;

    // Data Properties
    private List<ReportProcessingSummary> PendingValidation { get; set; } = new();
    private List<ReportProcessingSummary> PendingRiskAssessment { get; set; } = new();
    private List<ReportProcessingSummary> PendingInvestigation { get; set; } = new();
    private List<ReportProcessingSummary> InMitigation { get; set; } = new();
    private List<ReportProcessingSummary> ClosedReferred { get; set; } = new();

    private int selectedTabIndex = 0;
    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    #region Data Loading

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;

            Logger.LogWarning("?? DEBUG: Starting LoadDataAsync()...");

            // Load core entities using CQRS - ENHANCED to include Investigations and Interviews
            var (reports, hazards, riskAssessments, reportValidations, investigations, interviews) = await LoadCoreEntitiesAsync();

            Logger.LogWarning("?? DEBUG: LoadCoreEntitiesAsync completed. Reports: {ReportCount}, Hazards: {HazardCount}, Validations: {ValidationCount}",
                reports.Count, hazards.Count, reportValidations.Count);

            if (!reports.Any())
            {
                Logger.LogWarning("? No reports found - initializing empty lists");
                InitializeEmptyLists();
                return;
            }

            // Create report summaries and categorize - ENHANCED with investigations and interviews
            var reportSummaries = CreateReportSummaries(reports, hazards, riskAssessments, reportValidations, investigations, interviews);

            Logger.LogWarning("?? DEBUG: Created {SummaryCount} report summaries", reportSummaries.Count);

            CategorizeReports(reportSummaries);

            Logger.LogWarning("? Report processing data loaded - V:{V}, RA:{RA}, I:{I}, M:{M}, C:{C}",
                PendingValidation.Count, PendingRiskAssessment.Count, PendingInvestigation.Count,
                InMitigation.Count, ClosedReferred.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "? Error loading report processing data");
            InitializeEmptyLists();
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task<(List<Report> reports, List<Hazard> hazards, List<RiskAssessment> riskAssessments, List<SMS_Domain.Entities.ReportValidation> reportValidations, List<Investigation> investigations, List<Interview> interviews)> LoadCoreEntitiesAsync()
    {
        var reports = new List<Report>();
        var hazards = new List<Hazard>();
        var riskAssessments = new List<RiskAssessment>();
        var reportValidations = new List<SMS_Domain.Entities.ReportValidation>();
        var investigations = new List<Investigation>();
        var interviews = new List<Interview>();

        try
        {
            Logger.LogWarning("?? DEBUG: Starting LoadCoreEntitiesAsync - loading reports...");

            // Get all reports
            var reportsQuery = new GetAllReportsQuery();
            var reportsResult = await Mediator.SendAsync(reportsQuery, CancellationToken.None);
            if (reportsResult.IsSuccess)
            {
                reports = reportsResult.Value ?? new List<Report>();
                Logger.LogWarning("? Successfully loaded {Count} reports from database", reports.Count);

                
            }
            else
            {
                Logger.LogError("? Failed to retrieve reports: {Error}", reportsResult.Error?.Message);
            }

            Logger.LogWarning("?? DEBUG: Loading hazards...");

            // Get all hazards
            var hazardsQuery = new GetAllHazardsQuery();
            var hazardsResult = await Mediator.SendAsync(hazardsQuery, CancellationToken.None);
            if (hazardsResult.IsSuccess)
            {
                hazards = hazardsResult.Value ?? new List<Hazard>();
                Logger.LogWarning("? Successfully loaded {Count} hazards from database", hazards.Count);
            }
            else
            {
                Logger.LogError("? Failed to retrieve hazards: {Error}", hazardsResult.Error?.Message);
            }

            Logger.LogWarning("?? DEBUG: Loading report validations...");

            // CRITICAL: Get all report validations to determine which reports have been validated
            var reportValidationsQuery = new GetAllReportValidationsQuery();
            var reportValidationsResult = await Mediator.SendAsync(reportValidationsQuery, CancellationToken.None);
            if (reportValidationsResult.IsSuccess && reportValidationsResult.Value != null)
            {
                reportValidations = reportValidationsResult.Value.ToList();
                Logger.LogWarning("? Successfully loaded {Count} report validations", reportValidations.Count);

                // Log which reports have been validated for debugging
                foreach (var validation in reportValidations.Take(3))
                {
                    Logger.LogWarning("?? Validation: Report {ReportCode} | Decision: {Decision} | Type: {Type}",
                        validation.ReportCode, validation.ValidationDecision, validation.ValidationType);
                }
            }
            else
            {
                Logger.LogWarning("?? No report validations found or failed to retrieve: {Error}", reportValidationsResult.Error?.Message);
            }

            // Get all risk assessments
            var riskAssessmentsQuery = new GetAllRiskAssessmentsQuery();
            var riskAssessmentsResult = await Mediator.SendAsync(riskAssessmentsQuery, CancellationToken.None);
            if (riskAssessmentsResult.IsSuccess)
            {
                riskAssessments = riskAssessmentsResult.Value ?? new List<RiskAssessment>();
                Logger.LogInformation("Loaded {Count} risk assessments", riskAssessments.Count);
            }
            else
            {
                Logger.LogError("Failed to retrieve risk assessments: {Error}", riskAssessmentsResult.Error?.Message);
            }

            // NEW: Get all investigations
            var investigationsQuery = new GetAllInvestigationsQuery();
            var investigationsResult = await Mediator.SendAsync(investigationsQuery, CancellationToken.None);
            if (investigationsResult.IsSuccess && investigationsResult.Value != null)
            {
                investigations = investigationsResult.Value.ToList();
                Logger.LogInformation("Loaded {Count} investigations", investigations.Count);
            }
            else
            {
                Logger.LogError("Failed to retrieve investigations: {Error}", investigationsResult.Error?.Message);
            }

            // NEW: Get all interviews
            var interviewsQuery = new GetAllInterviewsQuery();
            var interviewsResult = await Mediator.SendAsync(interviewsQuery, CancellationToken.None);
            if (interviewsResult.IsSuccess && interviewsResult.Value != null)
            {
                interviews = interviewsResult.Value.ToList();
                Logger.LogInformation("Loaded {Count} interviews", interviews.Count);
            }
            else
            {
                Logger.LogError("Failed to retrieve interviews: {Error}", interviewsResult.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "? Exception in LoadCoreEntitiesAsync");
        }

        Logger.LogWarning("?? DEBUG: LoadCoreEntitiesAsync returning - Reports: {RC}, Hazards: {HC}, Validations: {VC}",
            reports.Count, hazards.Count, reportValidations.Count);

        return (reports, hazards, riskAssessments, reportValidations, investigations, interviews);
    }

    #endregion

    #region Report Processing

    private List<ReportProcessingSummary> CreateReportSummaries(List<Report> reports, List<Hazard> hazards, List<RiskAssessment> riskAssessments, List<SMS_Domain.Entities.ReportValidation> reportValidations, List<Investigation> investigations, List<Interview> interviews)
    {
        var summaries = new List<ReportProcessingSummary>();

        Logger.LogInformation("Creating report summaries - Reports: {ReportCount}, Hazards: {HazardCount}, RiskAssessments: {AssessmentCount}, ReportValidations: {ValidationCount}, Investigations: {InvestigationCount}, Interviews: {InterviewCount}",
            reports.Count, hazards.Count, riskAssessments.Count, reportValidations.Count, investigations.Count, interviews.Count);

        foreach (var report in reports)
        {
            try
            {
                // Find matching hazard
                var hazard = hazards.FirstOrDefault(h => h.ReportCode?.Trim() == report.Code?.Trim());

                // Find matching risk assessment
                var riskAssessment = hazard != null ?
                    riskAssessments.FirstOrDefault(ra => ra.HazardCode?.Trim() == hazard.Code?.Trim() &&
                                                        ra.AssessmentType == RiskAssessmentType.Initial) :
                    null;

                // CRITICAL: Find matching report validation
                var reportValidation = reportValidations.FirstOrDefault(rv => rv.ReportCode?.Trim() == report.Code?.Trim());

                // NEW: Find matching investigation
                var investigation = hazard != null ?
                    investigations.FirstOrDefault(inv => inv.HazardCode?.Trim() == hazard.Code?.Trim()) :
                    null;

                // NEW: Find matching interviews for this investigation
                var investigationInterviews = investigation != null ?
                    interviews.Where(iv => iv.InvestigationCode?.Trim() == investigation.Code?.Trim()).ToList() :
                    new List<Interview>();

                var summary = new ReportProcessingSummary
                {
                    ReportId = report.Code ?? "Unknown",
                    ReportDescription = report.Description ?? "No description",
                    ReportStatus = report.Status ?? "New",
                    ReportStage = report.Stage ?? "New",
                    CreatedBy = report.CreatedBy ?? "Unknown",
                    CreatedDate = report.CreatedDate ?? DateTime.UtcNow,

                    HazardId = hazard?.Code,
                    HazardType = hazard?.HazardType ?? "Unknown",
                    HazardDescription = hazard?.Description ?? report.Description ?? "No description",
                    Location = hazard?.HazardLocation?.Description ?? hazard?.LocationArea ?? "Not specified",
                    Priority = GetPriorityString(hazard?.Priority),
                    ReportedBy = hazard?.ReportedBy ?? report.CreatedBy ?? report.UpdatedBy ?? "Unknown", // ENHANCED: Added fallback to UpdatedBy
                    ReportedDate = hazard?.ReportedOn ?? report.CreatedDate ?? DateTime.UtcNow,
                    IsConfidential = hazard?.IsConfidential ?? false,

                    // Risk Assessment Information
                    RiskAssessmentId = riskAssessment?.Code,
                    CurrentAssessmentStep = riskAssessment?.CurrentStep ?? 0,
                    RiskAssessmentStatus = riskAssessment?.Status?.ToString() ?? "",
                    AssessmentStage = riskAssessment?.Stage ?? "",
                    AssessmentType = riskAssessment?.RiskAssessmentCategory?.ToString() ?? "", // NEW: Technical vs Preliminary

                    // Report Validation Information - NEW
                    ReportValidationId = reportValidation?.Code,
                    ValidationType = reportValidation?.ValidationType ?? "", // NEW: Key for smart routing
                    ValidationDecision = reportValidation?.ValidationDecision ?? "",

                    // Investigation Information - NEW
                    InvestigationId = investigation?.Code,
                    InvestigationStatus = investigation?.Status?.ToString() ?? "",
                    AssignedInvestigator = investigation?.AssignedInvestigatorId,
                    InvestigationNotes = investigation?.InvestigationNotes,
                    InterviewCount = investigationInterviews.Count,

                    // Status determination - UPDATED: Include investigation
                    StatusCategory = DetermineStatusCategory(report, hazard, riskAssessment, reportValidation, investigation),
                    DaysInStage = CalculateDaysInStage(report, hazard, riskAssessment, reportValidation),
                    AssignedTo = DetermineAssignedTo(report, hazard, riskAssessment, reportValidation, investigation),
                    ValidationUrl = GetValidationUrl(report, hazard, reportValidation)
                };

                summaries.Add(summary);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating summary for report {ReportCode}", report.Code);
            }
        }

        Logger.LogInformation("Created {SummaryCount} report summaries", summaries.Count);
        return summaries;
    }

    private void CategorizeReports(List<ReportProcessingSummary> reports)
    {
        PendingValidation = reports.Where(r => r.StatusCategory == ProcessingStatusCategory.Validation).ToList();
        PendingRiskAssessment = reports.Where(r => r.StatusCategory == ProcessingStatusCategory.RiskAssessment).ToList();
        PendingInvestigation = reports.Where(r => r.StatusCategory == ProcessingStatusCategory.Investigation).ToList();
        InMitigation = reports.Where(r => r.StatusCategory == ProcessingStatusCategory.Mitigation).ToList();
        ClosedReferred = reports.Where(r => r.StatusCategory == ProcessingStatusCategory.Closed).ToList();
    }

    #endregion

    #region Helper Methods

    private ProcessingStatusCategory DetermineStatusCategory(Report report, Hazard? hazard, RiskAssessment? riskAssessment, SMS_Domain.Entities.ReportValidation? reportValidation, Investigation? investigation)
    {
        Logger.LogWarning("?? CATEGORIZING Report: {ReportCode} | HasValidation: {HasValidation} | ValidationType: {ValidationType} | ValidationDecision: {ValidationDecision} | HasRiskAssessment: {HasRA} | HasInvestigation: {HasInv} | InvStatus: {InvStatus} | InvDecision: {InvDecision}",
            report.Code, reportValidation != null, reportValidation?.ValidationType ?? "NULL", reportValidation?.ValidationDecision ?? "NULL", riskAssessment != null, investigation != null, investigation?.Status ?? "NULL", investigation?.DecisionType ?? "NULL");

        // CRITICAL DESIGN CONCEPT: 
        // 1. If Investigation exists and is active -> INVESTIGATION tab (HIGHEST PRIORITY)
        // 2. If no ReportValidation exists -> VALIDATION tab (needs initial validation)
        // 3. If ReportValidation exists but ValidationType is NULL or ValidationDecision is NULL -> VALIDATION tab (reset validation)
        // 4. If ReportValidation exists with decision but no RiskAssessment -> RISK ASSESSMENT tab (validated, needs risk assessment)  
        // 5. If RiskAssessment exists and in progress -> RISK ASSESSMENT tab (assessment in progress)
        // 6. If RiskAssessment complete -> MITIGATION tab

        // HIGHEST PRIORITY: Check for active investigation first - but exclude completed investigations that returned to validation
        if (investigation != null)
        {
            // FIXED: Use more robust status checking to handle different status formats
            var status = investigation.Status?.ToUpperInvariant() ?? "";
            var isActiveInvestigation = status == "INPROGRESS" ||
                                      status == "IN_PROGRESS" ||
                                      status == "ONHOLD" ||
                                      status == "ON_HOLD" ||
                                      status == "ASSIGNED";

            // NEW: Check if investigation completed with ReturnToValidation - SKIP active investigation logic
            if (status == "COMPLETED" &&
                string.Equals(investigation.DecisionType, "ReturnToValidation", StringComparison.OrdinalIgnoreCase))
            {
                Logger.LogWarning("? Report {ReportId} -> Investigation {InvestigationId} completed with ReturnToValidation decision, continuing with validation logic ?",
                    report.Code, investigation.Code);
                // Continue with validation logic below - do NOT return Investigation
            }
            else if (isActiveInvestigation)
            {
                Logger.LogWarning("? Report {ReportId} -> INVESTIGATION (active investigation {InvestigationId} with status '{Status}') - HIGHEST PRIORITY",
                    report.Code, investigation.Code, investigation.Status);
                return ProcessingStatusCategory.Investigation;
            }
            else if (status == "COMPLETED")
            {
                Logger.LogWarning("? Report {ReportId} -> Investigation {InvestigationId} completed with decision {Decision}, continuing with normal flow",
                    report.Code, investigation.Code, investigation.DecisionType ?? "NULL");
                // Continue with normal flow below
            }
        }

        // No validation record = needs validation
        if (reportValidation == null)
        {
            Logger.LogWarning("? Report {ReportId} -> VALIDATION (no validation record)", report.Code);
            return ProcessingStatusCategory.Validation;
        }

        // ENHANCED: Check if validation was reset (ValidationType or ValidationDecision is null) - THIS SHOULD CATCH IT!
        if (string.IsNullOrEmpty(reportValidation.ValidationType) || string.IsNullOrEmpty(reportValidation.ValidationDecision))
        {
            Logger.LogWarning("? Report {ReportId} -> VALIDATION (reset validation - ValidationType: {ValidationType}, ValidationDecision: {ValidationDecision}) ? EXPECTED PATH",
                report.Code, reportValidation.ValidationType ?? "NULL", reportValidation.ValidationDecision ?? "NULL");
            return ProcessingStatusCategory.Validation;
        }

        // Has validation with decision but no risk assessment = validated, needs risk assessment
        if (riskAssessment == null)
        {
            Logger.LogWarning("? Report {ReportId} -> RISK ASSESSMENT (validated but no assessment)", report.Code);
            return ProcessingStatusCategory.RiskAssessment;
        }

        // Has risk assessment - determine stage based on progress
        if (riskAssessment.Status == RiskAssessmentStatus.Created || riskAssessment.Status == RiskAssessmentStatus.InProgress)
        {
            // Risk assessment in progress
            var category = riskAssessment.CurrentStep switch
            {
                1 => ProcessingStatusCategory.RiskAssessment, // System Description
                2 => ProcessingStatusCategory.RiskAssessment, // Hazard Identification  
                3 => ProcessingStatusCategory.RiskAssessment, // Risk Analysis
                4 => ProcessingStatusCategory.RiskAssessment, // Risk Assessment
                5 => ProcessingStatusCategory.Mitigation,     // Risk Mitigation
                _ => ProcessingStatusCategory.RiskAssessment
            };

            Logger.LogWarning("? Report {ReportId} -> {Category} (assessment step {Step})",
                report.Code, category, riskAssessment.CurrentStep);
            return category;
        }
        else if (riskAssessment.Status == RiskAssessmentStatus.Completed)
        {
            Logger.LogWarning("? Report {ReportId} -> MITIGATION (assessment complete)", report.Code);
            return ProcessingStatusCategory.Mitigation;
        }

        // Fallback to hazard/report status for edge cases
        var effectiveStatus = hazard?.Status?.ToString() ?? report.Status ?? "New";

        Logger.LogWarning("? Report {ReportId} -> Fallback logic with status: {Status}", report.Code, effectiveStatus);

        return effectiveStatus switch
        {
            "Under Investigation - Information Needed" or "Investigation Required" or "Pending Investigation" or "UNDER_INVESTIGATION" => ProcessingStatusCategory.Investigation,
            "Mitigation Planning" or "In Progress" or "Implementation" or "Tracking" => ProcessingStatusCategory.Mitigation,
            "Closed" or "Referred" or "Completed" or "Closed - Not SMS Risk" or "CLOSED" or "CANCELLED" => ProcessingStatusCategory.Closed,
            _ => ProcessingStatusCategory.RiskAssessment
        };
    }

    private string GetPriorityString(HazardPriority? priority) => priority?.ToString() ?? "Medium";

    private int CalculateDaysInStage(Report report, Hazard? hazard, RiskAssessment? riskAssessment, SMS_Domain.Entities.ReportValidation? reportValidation)
    {
        // Use the most recent update date to calculate days in current stage
        var referenceDate = riskAssessment?.UpdatedDate ??
                           reportValidation?.UpdatedDate ??
                           hazard?.UpdatedDate ?? hazard?.CreatedDate ??
                           report.UpdatedDate ?? report.CreatedDate ?? DateTime.UtcNow;
        return (DateTime.UtcNow - referenceDate).Days;
    }

    private string? DetermineAssignedTo(Report report, Hazard? hazard, RiskAssessment? riskAssessment, SMS_Domain.Entities.ReportValidation? reportValidation, Investigation? investigation)
    {
        // NEW: Use Investigation assigned investigator if available
        if (investigation != null && !string.IsNullOrEmpty(investigation.AssignedInvestigatorId))
        {
            return investigation.AssignedInvestigatorId;
        }

        // Use RiskAssessment lead assessor if available
        if (riskAssessment != null && !string.IsNullOrEmpty(riskAssessment.LeadAssessorId))
        {
            return riskAssessment.LeadAssessorId;
        }

        // Use ReportValidation validator if available
        if (reportValidation != null && !string.IsNullOrEmpty(reportValidation.ValidatedBy))
        {
            return reportValidation.ValidatedBy;
        }

        // Fallback to category-based assignment
        var category = DetermineStatusCategory(report, hazard, riskAssessment, reportValidation, investigation);
        return category switch
        {
            ProcessingStatusCategory.Validation => "SMS Validation Team",
            ProcessingStatusCategory.RiskAssessment => "Risk Assessment Team",
            ProcessingStatusCategory.Investigation => "Investigative Team",
            ProcessingStatusCategory.Mitigation => "Mitigation Team",
            ProcessingStatusCategory.Closed => "Closed Cases Team",
            _ => "General Review Team"
        };
    }

    private string GetValidationUrl(Report report, Hazard? hazard, SMS_Domain.Entities.ReportValidation? reportValidation)
    {
        // If no validation exists, go to report validation
        if (reportValidation == null)
        {
            return $"/SMSRiskManagement/ReportValidation/{report.Code}";
        }

        // If validation exists but no risk assessment, start risk assessment
        // This would be for the risk assessment tab
        if (!string.IsNullOrEmpty(hazard?.Code))
        {
            return $"/SMSRiskManagement/TechnicalAssessment/{report.Code}/{hazard.Code}/1";
        }

        // Fallback
        return $"/SMSRiskManagement/ReportValidation/{report.Code}";
    }

    private void InitializeEmptyLists()
    {
        PendingValidation = new List<ReportProcessingSummary>();
        PendingRiskAssessment = new List<ReportProcessingSummary>();
        PendingInvestigation = new List<ReportProcessingSummary>();
        InMitigation = new List<ReportProcessingSummary>();
        ClosedReferred = new List<ReportProcessingSummary>();
    }

    #endregion

    #region Tab Rendering

    private RenderFragment RenderValidationTab()
    {
        return builder =>
        {
            if (IsLoading)
            {
                builder.OpenComponent<RadzenProgressBarCircular>(0);
                builder.AddAttribute(1, "ShowValue", false);
                builder.CloseComponent();
                return;
            }

            if (!PendingValidation.Any())
            {
                RenderEmptyState(builder, "check_circle", "No reports pending validation", "New reports will appear here for SMS risk determination");
                return;
            }

            builder.OpenComponent<RadzenDataGrid<ReportProcessingSummary>>(0);
            builder.AddAttribute(1, "Data", PendingValidation);
            builder.AddAttribute(2, "AllowSorting", true);
            builder.AddAttribute(3, "AllowPaging", true);
            builder.AddAttribute(4, "PageSize", 10);
            builder.AddAttribute(5, "Columns", (RenderFragment)(columnsBuilder =>
            {
                RenderValidationColumns(columnsBuilder);
            }));
            builder.CloseComponent();
        };
    }

    private RenderFragment RenderRiskAssessmentTab()
    {
        return builder =>
        {
            if (IsLoading)
            {
                builder.OpenComponent<RadzenProgressBarCircular>(0);
                builder.AddAttribute(1, "ShowValue", false);
                builder.CloseComponent();
                return;
            }

            if (!PendingRiskAssessment.Any())
            {
                RenderEmptyState(builder, "assessment", "No reports pending risk assessment", "Validated reports will appear here for risk assessment");
                return;
            }

            builder.OpenComponent<RadzenDataGrid<ReportProcessingSummary>>(0);
            builder.AddAttribute(1, "Data", PendingRiskAssessment);
            builder.AddAttribute(2, "AllowSorting", true);
            builder.AddAttribute(3, "AllowPaging", true);
            builder.AddAttribute(4, "PageSize", 10);
            builder.AddAttribute(5, "Columns", (RenderFragment)(columnsBuilder =>
            {
                RenderRiskAssessmentColumns(columnsBuilder);
            }));
            builder.CloseComponent();
        };
    }

    private RenderFragment RenderInvestigationTab()
    {
        return builder =>
        {
            if (IsLoading)
            {
                builder.OpenComponent<RadzenProgressBarCircular>(0);
                builder.AddAttribute(1, "ShowValue", false);
                builder.CloseComponent();
                return;
            }

            if (!PendingInvestigation.Any())
            {
                RenderEmptyState(builder, "search", "No reports requiring investigation", "Reports needing more information will appear here");
                return;
            }

            builder.OpenComponent<RadzenDataGrid<ReportProcessingSummary>>(0);
            builder.AddAttribute(1, "Data", PendingInvestigation);
            builder.AddAttribute(2, "AllowSorting", true);
            builder.AddAttribute(3, "AllowPaging", true);
            builder.AddAttribute(4, "PageSize", 10);
            builder.AddAttribute(5, "Columns", (RenderFragment)(columnsBuilder =>
            {
                RenderInvestigationColumns(columnsBuilder);
            }));
            builder.CloseComponent();
        };
    }

    private RenderFragment RenderMitigationTab()
    {
        return builder =>
        {
            if (IsLoading)
            {
                builder.OpenComponent<RadzenProgressBarCircular>(0);
                builder.AddAttribute(1, "ShowValue", false);
                builder.CloseComponent();
                return;
            }

            if (!InMitigation.Any())
            {
                RenderEmptyState(builder, "build", "No reports in mitigation phase", "Approved risk assessments will appear here");
                return;
            }

            builder.OpenComponent<RadzenDataGrid<ReportProcessingSummary>>(0);
            builder.AddAttribute(1, "Data", InMitigation);
            builder.AddAttribute(2, "AllowSorting", true);
            builder.AddAttribute(3, "AllowPaging", true);
            builder.AddAttribute(4, "PageSize", 10);
            builder.AddAttribute(5, "Columns", (RenderFragment)(columnsBuilder =>
            {
                RenderMitigationColumns(columnsBuilder);
            }));
            builder.CloseComponent();
        };
    }

    private RenderFragment RenderClosedTab()
    {
        return builder =>
        {
            if (IsLoading)
            {
                builder.OpenComponent<RadzenProgressBarCircular>(0);
                builder.AddAttribute(1, "ShowValue", false);
                builder.CloseComponent();
                return;
            }

            if (!ClosedReferred.Any())
            {
                RenderEmptyState(builder, "archive", "No recently closed reports", "Completed reports will appear here");
                return;
            }

            builder.OpenComponent<RadzenDataGrid<ReportProcessingSummary>>(0);
            builder.AddAttribute(1, "Data", ClosedReferred.Take(20));
            builder.AddAttribute(2, "AllowSorting", true);
            builder.AddAttribute(3, "AllowPaging", true);
            builder.AddAttribute(4, "PageSize", 10);
            builder.AddAttribute(5, "Columns", (RenderFragment)(columnsBuilder =>
            {
                RenderStandardColumns(columnsBuilder, false); // No actions for closed
            }));
            builder.CloseComponent();
        };
    }

    private void RenderEmptyState(RenderTreeBuilder builder, string icon, string title, string description)
    {
        builder.OpenComponent<RadzenStack>(0);
        builder.AddAttribute(1, "Orientation", Orientation.Vertical);
        builder.AddAttribute(2, "AlignItems", AlignItems.Center);
        builder.AddAttribute(3, "Gap", "1rem");
        builder.AddAttribute(4, "Style", "padding: 3rem;");
        builder.AddAttribute(5, "ChildContent", (RenderFragment)(stackBuilder =>
        {
            stackBuilder.OpenComponent<RadzenIcon>(0);
            stackBuilder.AddAttribute(1, "Icon", icon);
            stackBuilder.AddAttribute(2, "Style", "font-size: 4rem; color: var(--rz-text-disabled-color);");
            stackBuilder.CloseComponent();

            stackBuilder.OpenComponent<RadzenText>(3);
            stackBuilder.AddAttribute(4, "TextStyle", TextStyle.H6);
            stackBuilder.AddAttribute(5, "Style", "color: var(--rz-text-disabled-color);");
            stackBuilder.AddAttribute(6, "Text", title);
            stackBuilder.CloseComponent();

            stackBuilder.OpenComponent<RadzenText>(7);
            stackBuilder.AddAttribute(8, "TextStyle", TextStyle.Body1);
            stackBuilder.AddAttribute(9, "Style", "color: var(--rz-text-disabled-color);");
            stackBuilder.AddAttribute(10, "Text", description);
            stackBuilder.CloseComponent();
        }));
        builder.CloseComponent();
    }

    private void RenderValidationColumns(RenderTreeBuilder builder)
    {
        // Report ID Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(0);
        builder.AddAttribute(1, "Property", "ReportId");
        builder.AddAttribute(2, "Title", "Report ID");
        builder.AddAttribute(3, "Width", "150px");
        builder.AddAttribute(4, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenStack>(0);
                templateBuilder.AddAttribute(1, "Orientation", Orientation.Vertical);
                templateBuilder.AddAttribute(2, "Gap", "0.25rem");
                templateBuilder.AddAttribute(3, "ChildContent", (RenderFragment)(stackBuilder =>
                {
                    stackBuilder.OpenComponent<RadzenText>(0);
                    stackBuilder.AddAttribute(1, "TextStyle", TextStyle.Body1);
                    stackBuilder.AddAttribute(2, "Style", "font-weight: 600;");
                    stackBuilder.AddAttribute(3, "Text", report.ReportId);
                    stackBuilder.CloseComponent();

                    stackBuilder.OpenComponent<RadzenText>(5);
                    stackBuilder.AddAttribute(6, "TextStyle", TextStyle.Caption);
                    stackBuilder.AddAttribute(7, "Style", "color: var(--rz-warning); font-weight: 500;");
                    stackBuilder.AddAttribute(8, "Text", "Needs Validation");
                    stackBuilder.CloseComponent();
                }));
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();

        // Hazard ID Column (FIXED: This was showing "Report ID" header)
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(10);
        builder.AddAttribute(11, "Property", "HazardId");
        builder.AddAttribute(12, "Title", "Hazard ID");
        builder.AddAttribute(13, "Width", "150px");
        builder.AddAttribute(14, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenText>(0);
                templateBuilder.AddAttribute(1, "TextStyle", TextStyle.Body1);
                templateBuilder.AddAttribute(2, "Style", "font-weight: 600;");
                templateBuilder.AddAttribute(3, "Text", !string.IsNullOrEmpty(report.HazardId) ? report.HazardId : "N/A");
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();

        // Description Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(20);
        builder.AddAttribute(21, "Property", "HazardDescription");
        builder.AddAttribute(22, "Title", "Description");
        builder.AddAttribute(23, "Width", "300px");
        builder.CloseComponent();

        // Status Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(30);
        builder.AddAttribute(31, "Property", "ReportStatus");
        builder.AddAttribute(32, "Title", "Status");
        builder.AddAttribute(33, "Width", "120px");
        builder.AddAttribute(34, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenBadge>(0);
                templateBuilder.AddAttribute(1, "BadgeStyle", BadgeStyle.Base);
                templateBuilder.AddAttribute(2, "Text", report.ReportStatus);
                templateBuilder.AddAttribute(3, "Variant", Variant.Flat);
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();

        // Stage Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(40);
        builder.AddAttribute(41, "Property", "ReportStage");
        builder.AddAttribute(42, "Title", "Stage");
        builder.AddAttribute(43, "Width", "120px");
        builder.AddAttribute(44, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenBadge>(0);
                templateBuilder.AddAttribute(1, "BadgeStyle", BadgeStyle.Base);
                templateBuilder.AddAttribute(2, "Text", report.ReportStage);
                templateBuilder.AddAttribute(3, "Variant", Variant.Flat);
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();



        // Priority Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(50);
        builder.AddAttribute(51, "Property", "Priority");
        builder.AddAttribute(52, "Title", "Priority");
        builder.AddAttribute(53, "Width", "100px");
        builder.AddAttribute(54, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                var badgeStyle = report.Priority switch
                {
                    "High" => BadgeStyle.Danger,
                    "Medium" => BadgeStyle.Warning,
                    _ => BadgeStyle.Base
                };
                templateBuilder.OpenComponent<RadzenBadge>(0);
                templateBuilder.AddAttribute(1, "BadgeStyle", badgeStyle);
                templateBuilder.AddAttribute(2, "Text", report.Priority);
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();

        // Reported By Column (FIXED: Ensure proper data binding)
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(60);
        builder.AddAttribute(61, "Property", "ReportedBy");
        builder.AddAttribute(62, "Title", "Reported By");
        builder.AddAttribute(63, "Width", "150px");
        builder.AddAttribute(64, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenText>(0);
                templateBuilder.AddAttribute(1, "TextStyle", TextStyle.Body1);
                templateBuilder.AddAttribute(2, "Text", !string.IsNullOrEmpty(report.ReportedBy) ? report.ReportedBy : "Not Specified");
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();

        // Days in Stage Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(70);
        builder.AddAttribute(71, "Property", "DaysInStage");
        builder.AddAttribute(72, "Title", "Days in Stage");
        builder.AddAttribute(73, "Width", "120px");
        builder.AddAttribute(74, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                var badgeStyle = report.DaysInStage > 2 ? BadgeStyle.Warning : BadgeStyle.Secondary;
                templateBuilder.OpenComponent<RadzenBadge>(0);
                templateBuilder.AddAttribute(1, "BadgeStyle", badgeStyle);
                templateBuilder.AddAttribute(2, "Text", $"{report.DaysInStage} days");
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();

        // Actions Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(80);
        builder.AddAttribute(81, "Title", "Actions");
        builder.AddAttribute(82, "Width", "150px");
        builder.AddAttribute(83, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenButton>(0);
                templateBuilder.AddAttribute(1, "Text", "Start Validation");
                templateBuilder.AddAttribute(2, "Icon", "check_circle");
                templateBuilder.AddAttribute(3, "ButtonStyle", ButtonStyle.Success);
                templateBuilder.AddAttribute(4, "Size", ButtonSize.Small);
                templateBuilder.AddAttribute(5, "Click", EventCallback.Factory.Create<MouseEventArgs>(this,
                    (args) => Navigation.NavigateTo($"/SMSRiskManagement/ReportValidation/{report.ReportId}")));
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();
    }

    private void RenderStandardColumns(RenderTreeBuilder builder, bool includeActions = true)
    {
        // Report ID Column - Show actual Report ID for all tabs
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(5);
        builder.AddAttribute(6, "Property", "HazardId");
        builder.AddAttribute(7, "Title", "Hazard ID");
        builder.AddAttribute(8, "Width", "150px");
        builder.AddAttribute(9, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenStack>(0);
                templateBuilder.AddAttribute(1, "Orientation", Orientation.Vertical);
                templateBuilder.AddAttribute(2, "Gap", "0.25rem");
                templateBuilder.AddAttribute(3, "ChildContent", (RenderFragment)(stackBuilder =>
                {
                    stackBuilder.OpenComponent<RadzenText>(0);
                    stackBuilder.AddAttribute(1, "TextStyle", TextStyle.Body1);
                    stackBuilder.AddAttribute(2, "Style", "font-weight: 600;");
                    stackBuilder.AddAttribute(3, "Text", report.HazardId);
                    stackBuilder.CloseComponent();

                    // Show associated Hazard ID as secondary info if available
                    //if (!string.IsNullOrEmpty(report.HazardId))
                    //{
                    //    stackBuilder.OpenComponent<RadzenText>(10);
                    //    stackBuilder.AddAttribute(11, "TextStyle", TextStyle.Caption);
                    //    stackBuilder.AddAttribute(12, "Style", "color: var(--rz-text-disabled-color);");
                    //    stackBuilder.AddAttribute(13, "Text", $"Hazard: {report.HazardId}");
                    //    stackBuilder.CloseComponent();
                    //}
                }));
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();

        // Description Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(10);
        builder.AddAttribute(11, "Property", "HazardDescription");
        builder.AddAttribute(12, "Title", "Description");
        builder.AddAttribute(13, "Width", "300px");
        builder.CloseComponent();

        // Stage Column - Shows current processing stage with assessment type info
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(15);
        builder.AddAttribute(16, "Property", "ReportStatus");
        builder.AddAttribute(17, "Title", "Stage");
        builder.AddAttribute(18, "Width", "150px");
        builder.AddAttribute(19, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                var badgeStyle = report.StatusCategory switch
                {
                    ProcessingStatusCategory.Validation => BadgeStyle.Info,
                    ProcessingStatusCategory.RiskAssessment => BadgeStyle.Primary,
                    ProcessingStatusCategory.Investigation => BadgeStyle.Warning,
                    ProcessingStatusCategory.Mitigation => BadgeStyle.Secondary,
                    ProcessingStatusCategory.Closed => BadgeStyle.Success,
                    _ => BadgeStyle.Light
                };

                // Show stage with assessment type if available
                var stageText = report.StatusCategory.ToString();
                if (report.StatusCategory == ProcessingStatusCategory.RiskAssessment && !string.IsNullOrEmpty(report.ValidationType))
                {
                    stageText = $"{report.ValidationType} Assessment";
                }

                templateBuilder.OpenComponent<RadzenBadge>(0);
                templateBuilder.AddAttribute(1, "BadgeStyle", badgeStyle);
                templateBuilder.AddAttribute(2, "Text", stageText);
                templateBuilder.AddAttribute(3, "Variant", Variant.Flat);
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();

        // Priority Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(20);
        builder.AddAttribute(21, "Property", "Priority");
        builder.AddAttribute(22, "Title", "Priority");
        builder.AddAttribute(23, "Width", "100px");
        builder.AddAttribute(24, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                var badgeStyle = report.Priority switch
                {
                    "High" => BadgeStyle.Danger,
                    "Medium" => BadgeStyle.Warning,
                    _ => BadgeStyle.Info
                };
                templateBuilder.OpenComponent<RadzenBadge>(0);
                templateBuilder.AddAttribute(1, "BadgeStyle", badgeStyle);
                templateBuilder.AddAttribute(2, "Text", report.Priority);
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();

        // Reported By Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(30);
        builder.AddAttribute(31, "Property", "ReportedBy");
        builder.AddAttribute(32, "Title", "Reported By");
        builder.AddAttribute(33, "Width", "150px");
        builder.CloseComponent();

        // Days in Stage Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(40);
        builder.AddAttribute(41, "Property", "DaysInStage");
        builder.AddAttribute(42, "Title", "Days in Stage");
        builder.AddAttribute(43, "Width", "120px");
        builder.AddAttribute(44, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                var badgeStyle = report.DaysInStage > 2 ? BadgeStyle.Warning : BadgeStyle.Secondary;
                templateBuilder.OpenComponent<RadzenBadge>(0);
                templateBuilder.AddAttribute(1, "BadgeStyle", badgeStyle);
                templateBuilder.AddAttribute(2, "Text", $"{report.DaysInStage} days");
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();
    }

    private void RenderRiskAssessmentColumns(RenderTreeBuilder builder)
    {
        // Report ID Column - WITHOUT action button (moved to Actions column)
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(0);
        builder.AddAttribute(1, "Property", "ReportId");
        builder.AddAttribute(2, "Title", "Report ID");
        builder.AddAttribute(3, "Width", "200px");
        builder.AddAttribute(4, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenStack>(0);
                templateBuilder.AddAttribute(1, "Orientation", Orientation.Vertical);
                templateBuilder.AddAttribute(2, "Gap", "0.5rem");
                templateBuilder.AddAttribute(3, "ChildContent", (RenderFragment)(stackBuilder =>
                {
                    stackBuilder.OpenComponent<RadzenText>(0);
                    stackBuilder.AddAttribute(1, "TextStyle", TextStyle.Body1);
                    stackBuilder.AddAttribute(2, "Style", "font-weight: 600;");
                    stackBuilder.AddAttribute(3, "Text", report.ReportId);
                    stackBuilder.CloseComponent();

                    // Show associated Hazard ID as secondary info if available
                    //if (!string.IsNullOrEmpty(report.HazardId))
                    //{
                    //    stackBuilder.OpenComponent<RadzenText>(10);
                    //    stackBuilder.AddAttribute(11, "TextStyle", TextStyle.Caption);
                    //    stackBuilder.AddAttribute(12, "Style", "color: var(--rz-text-disabled-color);");
                    //    stackBuilder.AddAttribute(13, "Text", $"Hazard: {report.HazardId}");
                    //    stackBuilder.CloseComponent();
                    //}

                    //// NEW: Show Validation Type
                    //if (!string.IsNullOrEmpty(report.ValidationType))
                    //{
                    //    stackBuilder.OpenComponent<RadzenText>(15);
                    //    stackBuilder.AddAttribute(16, "TextStyle", TextStyle.Caption);
                    //    stackBuilder.AddAttribute(17, "Style", "color: black; font-weight: 500;");
                    //    stackBuilder.AddAttribute(18, "Text", $"Type: {report.ValidationType}");
                    //    stackBuilder.CloseComponent();
                    //}

                    //// Risk Assessment tab: Show assessment progress if available
                    //if (report.HasRiskAssessment && report.CurrentAssessmentStep > 0)
                    //{
                    //    if (report.ValidationType?.ToLower() == "technical")
                    //    {
                    //        stackBuilder.OpenComponent<RadzenText>(20);
                    //        stackBuilder.AddAttribute(21, "TextStyle", TextStyle.Caption);
                    //        stackBuilder.AddAttribute(22, "Style", "color: var(--rz-primary); font-weight: 500;");
                    //        stackBuilder.AddAttribute(23, "Text", $"Technical Assessment Step: {report.CurrentAssessmentStep}/5");
                    //        stackBuilder.CloseComponent();
                    //    }
                    //    else if (report.ValidationType?.ToLower() == "preliminary")
                    //    {
                    //        stackBuilder.OpenComponent<RadzenText>(20);
                    //        stackBuilder.AddAttribute(21, "TextStyle", TextStyle.Caption);
                    //        stackBuilder.AddAttribute(22, "Style", "color: var(--rz-success); font-weight: 500;");
                    //        stackBuilder.AddAttribute(23, "Text", "Preliminary Assessment");
                    //        stackBuilder.CloseComponent();
                    //    }
                    //}
                    //else
                    //{
                    //    stackBuilder.OpenComponent<RadzenText>(25);
                    //    stackBuilder.AddAttribute(26, "TextStyle", TextStyle.Caption);
                    //    stackBuilder.AddAttribute(27, "Style", "color: var(--rz-success); font-weight: 500;");
                    //    stackBuilder.AddAttribute(28, "Text", "Validated - Ready for Assessment");
                    //    stackBuilder.CloseComponent();
                    //}
                }));
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();

        // Standard columns (Description, Stage, Priority, Reported By, Days in Stage)
        RenderStandardColumns(builder, false); // No additional actions column in standard columns

        // Actions Column - NEW: Moved the action button to the far right
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(100);
        builder.AddAttribute(101, "Title", "Actions");
        builder.AddAttribute(102, "Width", "200px");
        builder.AddAttribute(103, "Sortable", false);
        builder.AddAttribute(104, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenButton>(0);
                templateBuilder.AddAttribute(1, "Text", report.ActionButtonText);
                templateBuilder.AddAttribute(2, "Icon", GetAssessmentIcon(report));
                templateBuilder.AddAttribute(3, "ButtonStyle", GetAssessmentButtonStyle(report));
                templateBuilder.AddAttribute(4, "Size", ButtonSize.Small);
                templateBuilder.AddAttribute(5, "Click", EventCallback.Factory.Create<MouseEventArgs>(this,
                    (args) => Navigation.NavigateTo(report.SmartValidationUrl)));
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();
    }

    private void RenderInvestigationColumns(RenderTreeBuilder builder)
    {
        // Hazard ID Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(0);
        builder.AddAttribute(1, "Property", "HazardId");
        builder.AddAttribute(2, "Title", "Hazard ID");
        builder.AddAttribute(3, "Width", "150px");
        builder.AddAttribute(4, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenStack>(0);
                templateBuilder.AddAttribute(1, "Orientation", Orientation.Vertical);
                templateBuilder.AddAttribute(2, "Gap", "0.25rem");
                templateBuilder.AddAttribute(3, "ChildContent", (RenderFragment)(stackBuilder =>
                {
                    stackBuilder.OpenComponent<RadzenText>(0);
                    stackBuilder.AddAttribute(1, "TextStyle", TextStyle.Body1);
                    stackBuilder.AddAttribute(2, "Style", "font-weight: 600;");
                    stackBuilder.AddAttribute(3, "Text", report.HazardId ?? "N/A");
                    stackBuilder.CloseComponent();

                    stackBuilder.OpenComponent<RadzenText>(5);
                    stackBuilder.AddAttribute(6, "TextStyle", TextStyle.Caption);
                    stackBuilder.AddAttribute(7, "Style", "color: var(--rz-warning); font-weight: 500;");
                    stackBuilder.AddAttribute(8, "Text", "Investigation Required");
                    stackBuilder.CloseComponent();
                }));
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();

        // Description Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(10);
        builder.AddAttribute(11, "Property", "HazardDescription");
        builder.AddAttribute(12, "Title", "Description");
        builder.AddAttribute(13, "Width", "300px");
        builder.CloseComponent();

        // Investigation Status Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(15);
        builder.AddAttribute(16, "Property", "InvestigationStatus");
        builder.AddAttribute(17, "Title", "Investigation Status");
        builder.AddAttribute(18, "Width", "150px");
        builder.AddAttribute(19, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                var badgeStyle = report.HasInvestigation ? BadgeStyle.Primary : BadgeStyle.Warning;
                var statusText = report.HasInvestigation ? report.InvestigationStatus : "Not Started";

                templateBuilder.OpenComponent<RadzenBadge>(0);
                templateBuilder.AddAttribute(1, "BadgeStyle", badgeStyle);
                templateBuilder.AddAttribute(2, "Text", statusText);
                templateBuilder.AddAttribute(3, "Variant", Variant.Flat);
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();

        // Assigned Investigator Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(20);
        builder.AddAttribute(21, "Property", "AssignedInvestigator");
        builder.AddAttribute(22, "Title", "Assigned To");
        builder.AddAttribute(23, "Width", "150px");
        builder.AddAttribute(24, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenText>(0);
                templateBuilder.AddAttribute(1, "TextStyle", TextStyle.Body1);
                templateBuilder.AddAttribute(2, "Text", report.AssignedInvestigator ?? "Not Assigned");
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();

        // Interview Count Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(25);
        builder.AddAttribute(26, "Property", "InterviewCount");
        builder.AddAttribute(27, "Title", "Interviews");
        builder.AddAttribute(28, "Width", "100px");
        builder.AddAttribute(29, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                var badgeStyle = report.InterviewCount > 0 ? BadgeStyle.Success : BadgeStyle.Light;

                templateBuilder.OpenComponent<RadzenBadge>(0);
                templateBuilder.AddAttribute(1, "BadgeStyle", badgeStyle);
                templateBuilder.AddAttribute(2, "Text", report.InterviewCount.ToString());
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();

        // Priority Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(30);
        builder.AddAttribute(31, "Property", "Priority");
        builder.AddAttribute(32, "Title", "Priority");
        builder.AddAttribute(33, "Width", "100px");
        builder.AddAttribute(34, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                var badgeStyle = report.Priority switch
                {
                    "High" => BadgeStyle.Danger,
                    "Medium" => BadgeStyle.Warning,
                    _ => BadgeStyle.Info
                };
                templateBuilder.OpenComponent<RadzenBadge>(0);
                templateBuilder.AddAttribute(1, "BadgeStyle", badgeStyle);
                templateBuilder.AddAttribute(2, "Text", report.Priority);
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();

        // Days in Stage Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(40);
        builder.AddAttribute(41, "Property", "DaysInStage");
        builder.AddAttribute(42, "Title", "Days in Stage");
        builder.AddAttribute(43, "Width", "120px");
        builder.AddAttribute(44, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                var badgeStyle = report.DaysInStage > 3 ? BadgeStyle.Danger :
                               report.DaysInStage > 1 ? BadgeStyle.Warning : BadgeStyle.Secondary;
                templateBuilder.OpenComponent<RadzenBadge>(0);
                templateBuilder.AddAttribute(1, "BadgeStyle", badgeStyle);
                templateBuilder.AddAttribute(2, "Text", $"{report.DaysInStage} days");
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();

        // Actions Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(50);
        builder.AddAttribute(51, "Title", "Actions");
        builder.AddAttribute(52, "Width", "150px");
        builder.AddAttribute(53, "Sortable", false);
        builder.AddAttribute(54, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                var buttonText = report.HasInvestigation ? "Continue Investigation" : "Start Investigation";
                var buttonIcon = report.HasInvestigation ? "edit" : "search";
                var buttonStyle = report.HasInvestigation ? ButtonStyle.Primary : ButtonStyle.Warning;

                templateBuilder.OpenComponent<RadzenButton>(0);
                templateBuilder.AddAttribute(1, "Text", buttonText);
                templateBuilder.AddAttribute(2, "Icon", buttonIcon);
                templateBuilder.AddAttribute(3, "ButtonStyle", buttonStyle);
                templateBuilder.AddAttribute(4, "Size", ButtonSize.Small);
                templateBuilder.AddAttribute(5, "Click", EventCallback.Factory.Create<MouseEventArgs>(this,
                    (args) => NavigateToInvestigation(report)));
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();
    }

    private void RenderMitigationColumns(RenderTreeBuilder builder)
    {
        // Hazard ID Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(0);
        builder.AddAttribute(1, "Property", "HazardId");
        builder.AddAttribute(2, "Title", "Hazard ID");
        builder.AddAttribute(3, "Width", "150px");
        builder.AddAttribute(4, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenStack>(0);
                templateBuilder.AddAttribute(1, "Orientation", Orientation.Vertical);
                templateBuilder.AddAttribute(2, "Gap", "0.25rem");
                templateBuilder.AddAttribute(3, "ChildContent", (RenderFragment)(stackBuilder =>
                {
                    stackBuilder.OpenComponent<RadzenText>(0);
                    stackBuilder.AddAttribute(1, "TextStyle", TextStyle.Body1);
                    stackBuilder.AddAttribute(2, "Style", "font-weight: 600;");
                    stackBuilder.AddAttribute(3, "Text", report.HazardId ?? "N/A");
                    stackBuilder.CloseComponent();

                    stackBuilder.OpenComponent<RadzenText>(5);
                    stackBuilder.AddAttribute(6, "TextStyle", TextStyle.Caption);
                    stackBuilder.AddAttribute(7, "Style", "color: var(--rz-success); font-weight: 500;");
                    stackBuilder.AddAttribute(8, "Text", "Ready for Mitigation");
                    stackBuilder.CloseComponent();
                }));
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();

        // Description Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(10);
        builder.AddAttribute(11, "Property", "HazardDescription");
        builder.AddAttribute(12, "Title", "Description");
        builder.AddAttribute(13, "Width", "300px");
        builder.CloseComponent();

        // Risk Assessment Status Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(15);
        builder.AddAttribute(16, "Property", "RiskAssessmentStatus");
        builder.AddAttribute(17, "Title", "Assessment Status");
        builder.AddAttribute(18, "Width", "150px");
        builder.AddAttribute(19, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                var badgeStyle = report.RiskAssessmentStatus?.Contains("Complete") == true ?
                    BadgeStyle.Success : BadgeStyle.Primary;
                var statusText = report.RiskAssessmentStatus ?? "Completed";

                templateBuilder.OpenComponent<RadzenBadge>(0);
                templateBuilder.AddAttribute(1, "BadgeStyle", badgeStyle);
                templateBuilder.AddAttribute(2, "Text", statusText);
                templateBuilder.AddAttribute(3, "Variant", Variant.Flat);
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();

        // Assessment Type Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(20);
        builder.AddAttribute(21, "Property", "AssessmentType");
        builder.AddAttribute(22, "Title", "Assessment Type");
        builder.AddAttribute(23, "Width", "120px");
        builder.AddAttribute(24, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                var badgeStyle = report.AssessmentType?.ToLower() == "technical" ?
                    BadgeStyle.Info : BadgeStyle.Secondary;
                var typeText = report.AssessmentType ?? "Standard";

                templateBuilder.OpenComponent<RadzenBadge>(0);
                templateBuilder.AddAttribute(1, "BadgeStyle", badgeStyle);
                templateBuilder.AddAttribute(2, "Text", typeText);
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();

        // Priority Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(30);
        builder.AddAttribute(31, "Property", "Priority");
        builder.AddAttribute(32, "Title", "Priority");
        builder.AddAttribute(33, "Width", "100px");
        builder.AddAttribute(34, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                var badgeStyle = report.Priority switch
                {
                    "High" => BadgeStyle.Danger,
                    "Medium" => BadgeStyle.Warning,
                    _ => BadgeStyle.Info
                };
                templateBuilder.OpenComponent<RadzenBadge>(0);
                templateBuilder.AddAttribute(1, "BadgeStyle", badgeStyle);
                templateBuilder.AddAttribute(2, "Text", report.Priority);
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();

        // Assigned To Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(40);
        builder.AddAttribute(41, "Property", "AssignedTo");
        builder.AddAttribute(42, "Title", "Assigned To");
        builder.AddAttribute(43, "Width", "150px");
        builder.CloseComponent();

        // Days in Stage Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(50);
        builder.AddAttribute(51, "Property", "DaysInStage");
        builder.AddAttribute(52, "Title", "Days in Stage");
        builder.AddAttribute(53, "Width", "120px");
        builder.AddAttribute(54, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                var badgeStyle = report.DaysInStage > 5 ? BadgeStyle.Danger :
                               report.DaysInStage > 2 ? BadgeStyle.Warning : BadgeStyle.Secondary;
                templateBuilder.OpenComponent<RadzenBadge>(0);
                templateBuilder.AddAttribute(1, "BadgeStyle", badgeStyle);
                templateBuilder.AddAttribute(2, "Text", $"{report.DaysInStage} days");
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();

        // Actions Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(60);
        builder.AddAttribute(61, "Title", "Actions");
        builder.AddAttribute(62, "Width", "150px");
        builder.AddAttribute(63, "Sortable", false);
        builder.AddAttribute(64, "Template", (RenderFragment<ReportProcessingSummary>)(report =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenButton>(0);
                templateBuilder.AddAttribute(1, "Text", "Manage Mitigation");
                templateBuilder.AddAttribute(2, "Icon", "build");
                templateBuilder.AddAttribute(3, "ButtonStyle", ButtonStyle.Success);
                templateBuilder.AddAttribute(4, "Size", ButtonSize.Small);
                templateBuilder.AddAttribute(5, "Click", EventCallback.Factory.Create<MouseEventArgs>(this,
                    (args) => NavigateToMitigation(report)));
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();
    }

    private string GetAssessmentIcon(ReportProcessingSummary report)
    {
        if (report.ValidationType?.ToLower() == "preliminary")
            return "speed"; // Fast/quick assessment icon
        else if (report.ValidationType?.ToLower() == "technical")
            return report.HasRiskAssessment ? "edit" : "engineering"; // Technical assessment icon
        else
            return "assessment";
    }

    private ButtonStyle GetAssessmentButtonStyle(ReportProcessingSummary report)
    {
        if (report.ValidationType?.ToLower() == "preliminary")
            return ButtonStyle.Success;
        else if (report.ValidationType?.ToLower() == "technical")
            return report.HasRiskAssessment ? ButtonStyle.Primary : ButtonStyle.Info;
        else
            return ButtonStyle.Secondary;
    }

    #endregion

    #region Navigation Methods

    private void NavigateToInvestigation(ReportProcessingSummary report)
    {
        try
        {
            string navigationUrl;

            if (report.HasInvestigation && !string.IsNullOrEmpty(report.HazardId))
            {
                // Navigate to existing investigation
                navigationUrl = $"/SMSRiskManagement/Investigations/{report.InvestigationId}/{report.HazardId}";
                Logger.LogInformation("Navigating to existing investigation: {InvestigationId} for hazard: {HazardId}",
                    report.InvestigationId, report.HazardId);
            }
            else if (!string.IsNullOrEmpty(report.HazardId))
            {
                // Navigate to investigation page with hazard (will create investigation if needed)
                navigationUrl = $"/SMSRiskManagement/Investigations/{report.HazardId}";
                Logger.LogInformation("Navigating to start investigation for hazard: {HazardId}", report.HazardId);
            }
            else
            {
                // Navigate to general investigation page
                navigationUrl = "/SMSRiskManagement/Investigations";
                Logger.LogInformation("Navigating to general investigation page for report: {ReportId}", report.ReportId);
            }

            Navigation.NavigateTo(navigationUrl);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error navigating to investigation for report: {ReportId}", report.ReportId);
            ShowErrorNotification("Error navigating to investigation page");
        }
    }

    private void NavigateToMitigation(ReportProcessingSummary report)
    {
        try
        {
            // For now, navigate to the mitigation listings page
            // Later this could be enhanced to create/edit specific mitigation records
            string navigationUrl = "/Listings/Mitigations";

            Logger.LogInformation("Navigating to mitigation management for hazard: {HazardId}", report.HazardId);
            Navigation.NavigateTo(navigationUrl);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error navigating to mitigation for report: {ReportId}", report.ReportId);
            ShowErrorNotification("Error navigating to mitigation page");
        }
    }

    private void ShowErrorNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Navigation Error",
            Detail = message,
            Duration = 5000
        });
        Logger.LogError("Error: {Message}", message);
    }

    #endregion

    #region Models

    public enum ProcessingStatusCategory
    {
        Validation,
        RiskAssessment,
        Investigation,
        Mitigation,
        Closed
    }

    public class ReportProcessingSummary
    {
        public string ReportId { get; set; } = string.Empty;
        public string ReportDescription { get; set; } = string.Empty;
        public string ReportStatus { get; set; } = string.Empty;

        public string ReportStage { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }

        public string? HazardId { get; set; }
        public string HazardType { get; set; } = string.Empty;
        public string HazardCategory { get; set; } = string.Empty;
        public string HazardDescription { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string ReportedBy { get; set; } = string.Empty;
        public DateTime ReportedDate { get; set; }
        public bool IsConfidential { get; set; }

        // ENHANCED: Risk Assessment Information
        public string? RiskAssessmentId { get; set; }
        public int CurrentAssessmentStep { get; set; } = 0;
        public string RiskAssessmentStatus { get; set; } = string.Empty;
        public string AssessmentStage { get; set; } = string.Empty;
        public string AssessmentType { get; set; } = string.Empty; // NEW: Technical vs Preliminary
        public bool HasRiskAssessment => !string.IsNullOrEmpty(RiskAssessmentId);

        // ENHANCED: Report Validation Information
        public string? ReportValidationId { get; set; }
        public string ValidationType { get; set; } = string.Empty; // NEW: Preliminary vs Technical
        public string ValidationDecision { get; set; } = string.Empty;
        public bool HasReportValidation => !string.IsNullOrEmpty(ReportValidationId);

        // NEW: Investigation Information
        public string? InvestigationId { get; set; }
        public string InvestigationStatus { get; set; } = string.Empty;
        public string? AssignedInvestigator { get; set; }
        public string? InvestigationNotes { get; set; }
        public int InterviewCount { get; set; } = 0;
        public bool HasInvestigation => !string.IsNullOrEmpty(InvestigationId);

        public ProcessingStatusCategory StatusCategory { get; set; }
        public int DaysInStage { get; set; }
        public string? AssignedTo { get; set; }
        public string ValidationUrl { get; set; } = string.Empty;

        public string DisplayId => !string.IsNullOrEmpty(HazardId) ? HazardId : ReportId;

        // ENHANCED: Smart validation URL based on validation type and assessment progress
        public string SmartValidationUrl
        {
            get
            {
                // VALIDATION TAB: Reports without ReportValidation record
                if (StatusCategory == ProcessingStatusCategory.Validation)
                {
                    return $"/SMSRiskManagement/ReportValidation/{ReportId}";
                }

                // RISK ASSESSMENT TAB: Reports with ReportValidation - route based on ValidationType
                if (StatusCategory == ProcessingStatusCategory.RiskAssessment)
                {
                    // Determine assessment type from ValidationType
                    if (ValidationType?.ToLower() == "preliminary")
                    {
                        // Preliminary Assessment - always single step
                        return $"/SMSRiskManagement/PreliminaryRiskAssessment/{ReportId}";
                    }
                    else if (ValidationType?.ToLower() == "technical")
                    {
                        // Technical Assessment - multi-step, smart navigation
                        if (HasRiskAssessment && CurrentAssessmentStep > 0)
                        {
                            // Continue to next step of existing assessment
                            var nextStep = CurrentAssessmentStep < 5 ? CurrentAssessmentStep + 1 : CurrentAssessmentStep;
                            return $"/SMSRiskManagement/TechnicalAssessment/{ReportId}/{HazardId}/{nextStep}";
                        }
                        else if (!string.IsNullOrEmpty(HazardId))
                        {
                            // Start new technical assessment
                            return $"/SMSRiskManagement/TechnicalAssessment/{ReportId}/{HazardId}/1";
                        }
                    }
                }

                // INVESTIGATION TAB: Navigate to investigation if available
                if (StatusCategory == ProcessingStatusCategory.Investigation)
                {
                    if (HasInvestigation && !string.IsNullOrEmpty(HazardId))
                    {
                        return $"/SMSRiskManagement/Investigations/{InvestigationId}/{HazardId}";
                    }
                }

                // Fallback to report validation
                return $"/SMSRiskManagement/ReportValidation/{ReportId}";
            }
        }

        // ENHANCED: Smart button text based on validation type and progress
        public string ActionButtonText
        {
            get
            {
                return StatusCategory switch
                {
                    ProcessingStatusCategory.Validation => "Start Validation",
                    ProcessingStatusCategory.RiskAssessment => GetRiskAssessmentButtonText(),
                    ProcessingStatusCategory.Investigation => HasInvestigation ? "Continue Investigation" : "Start Investigation",
                    ProcessingStatusCategory.Mitigation => "View Mitigation",
                    ProcessingStatusCategory.Closed => "View Closed",
                    _ => "Process"
                };
            }
        }

        private string GetRiskAssessmentButtonText()
        {
            // Determine button text based on ValidationType
            if (ValidationType?.ToLower() == "preliminary")
            {
                return "Start Preliminary Assessment";
            }
            else if (ValidationType?.ToLower() == "technical")
            {
                if (HasRiskAssessment && CurrentAssessmentStep > 0)
                {
                    return CurrentAssessmentStep < 5 ? $"Continue Step {CurrentAssessmentStep + 1}" : "Review Assessment";
                }
                else
                {
                    return "Start Technical Assessment";
                }
            }
            else
            {
                return "Start Risk Assessment";
            }
        }
    }

    #endregion
}
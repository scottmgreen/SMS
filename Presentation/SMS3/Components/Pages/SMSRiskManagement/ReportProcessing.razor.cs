using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using SMS_Application.Messaging.Queries;
using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Shared.Common;

namespace SMS3.Components.Pages.SMSRiskManagement;

public partial class ReportProcessing : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<ReportProcessing> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

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

            // Load core entities using CQRS - ENHANCED to include ReportValidations
            var (reports, hazards, riskAssessments, reportValidations) = await LoadCoreEntitiesAsync();

            if (!reports.Any())
            {
                Logger.LogWarning("No reports found - initializing empty lists");
                InitializeEmptyLists();
                return;
            }

            // Create report summaries and categorize - ENHANCED with report validations
            var reportSummaries = CreateReportSummaries(reports, hazards, riskAssessments, reportValidations);
            CategorizeReports(reportSummaries);

            Logger.LogInformation("Report processing data loaded - V:{V}, RA:{RA}, I:{I}, M:{M}, C:{C}",
                PendingValidation.Count, PendingRiskAssessment.Count, PendingInvestigation.Count,
                InMitigation.Count, ClosedReferred.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading report processing data");
            InitializeEmptyLists();
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task<(List<Report> reports, List<Hazard> hazards, List<RiskAssessment> riskAssessments, List<SMS_Domain.Entities.ReportValidation> reportValidations)> LoadCoreEntitiesAsync()
    {
        var reports = new List<Report>();
        var hazards = new List<Hazard>();
        var riskAssessments = new List<RiskAssessment>();
        var reportValidations = new List<SMS_Domain.Entities.ReportValidation>();

        try
        {
            // Get all reports
            var reportsQuery = new GetAllReportsQuery();
            var reportsResult = await Mediator.SendAsync(reportsQuery, CancellationToken.None);
            if (reportsResult.IsSuccess)
            {
                reports = reportsResult.Value ?? new List<Report>();
            }
            else
            {
                Logger.LogError("Failed to retrieve reports: {Error}", reportsResult.Error?.Message);
            }

            // Get all hazards
            var hazardsQuery = new GetAllHazardsQuery();
            var hazardsResult = await Mediator.SendAsync(hazardsQuery, CancellationToken.None);
            if (hazardsResult.IsSuccess)
            {
                hazards = hazardsResult.Value ?? new List<Hazard>();
            }
            else
            {
                Logger.LogError("Failed to retrieve hazards: {Error}", hazardsResult.Error?.Message);
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

            // CRITICAL: Get all report validations to determine which reports have been validated
            var reportValidationsQuery = new GetAllReportValidationsQuery();
            var reportValidationsResult = await Mediator.SendAsync(reportValidationsQuery, CancellationToken.None);
            if (reportValidationsResult.IsSuccess && reportValidationsResult.Value != null)
            {
                reportValidations = reportValidationsResult.Value.ToList();
                Logger.LogInformation("Loaded {Count} report validations", reportValidations.Count);
            }
            else
            {
                Logger.LogError("Failed to retrieve report validations: {Error}", reportValidationsResult.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in core entities retrieval");
        }

        return (reports, hazards, riskAssessments, reportValidations);
    }

    #endregion

    #region Report Processing

    private List<ReportProcessingSummary> CreateReportSummaries(List<Report> reports, List<Hazard> hazards, List<RiskAssessment> riskAssessments, List<SMS_Domain.Entities.ReportValidation> reportValidations)
    {
        var summaries = new List<ReportProcessingSummary>();

        Logger.LogInformation("Creating report summaries - Reports: {ReportCount}, Hazards: {HazardCount}, RiskAssessments: {AssessmentCount}, ReportValidations: {ValidationCount}", 
            reports.Count, hazards.Count, riskAssessments.Count, reportValidations.Count);

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
                
                var summary = new ReportProcessingSummary
                {
                    ReportId = report.Code ?? "Unknown",
                    ReportDescription = report.Description ?? "No description",
                    ReportStatus = report.Status ?? "New",
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
                    
                    // Status determination
                    StatusCategory = DetermineStatusCategory(report, hazard, riskAssessment, reportValidation),
                    DaysInStage = CalculateDaysInStage(report, hazard, riskAssessment, reportValidation),
                    AssignedTo = DetermineAssignedTo(report, hazard, riskAssessment, reportValidation),
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

    private ProcessingStatusCategory DetermineStatusCategory(Report report, Hazard? hazard, RiskAssessment? riskAssessment, SMS_Domain.Entities.ReportValidation? reportValidation)
    {
        // CRITICAL DESIGN CONCEPT: 
        // 1. If no ReportValidation exists -> VALIDATION tab (needs initial validation)
        // 2. If ReportValidation exists but no RiskAssessment -> RISK ASSESSMENT tab (validated, needs risk assessment)  
        // 3. If RiskAssessment exists and in progress -> RISK ASSESSMENT tab (assessment in progress)
        // 4. If RiskAssessment complete -> MITIGATION tab
        
        // No validation record = needs validation
        if (reportValidation == null)
        {
            Logger.LogDebug("Report {ReportId} -> VALIDATION (no validation record)", report.Code);
            return ProcessingStatusCategory.Validation;
        }

        // Has validation but no risk assessment = validated, needs risk assessment
        if (riskAssessment == null)
        {
            Logger.LogDebug("Report {ReportId} -> RISK ASSESSMENT (validated but no assessment)", report.Code);
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
            
            Logger.LogDebug("Report {ReportId} -> {Category} (assessment step {Step})", 
                report.Code, category, riskAssessment.CurrentStep);
            return category;
        }
        else if (riskAssessment.Status == RiskAssessmentStatus.Completed)
        {
            Logger.LogDebug("Report {ReportId} -> MITIGATION (assessment complete)", report.Code);
            return ProcessingStatusCategory.Mitigation;
        }

        // Fallback to hazard/report status for edge cases
        var effectiveStatus = hazard?.Status?.ToString() ?? report.Status ?? "New";
        
        Logger.LogDebug("Report {ReportId} -> Fallback logic with status: {Status}", report.Code, effectiveStatus);
        
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

    private string? DetermineAssignedTo(Report report, Hazard? hazard, RiskAssessment? riskAssessment, SMS_Domain.Entities.ReportValidation? reportValidation)
    {
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
        var category = DetermineStatusCategory(report, hazard, riskAssessment, reportValidation);
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
                RenderStandardColumns(columnsBuilder);
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
                RenderStandardColumns(columnsBuilder);
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

        // Stage Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(30);
        builder.AddAttribute(31, "Property", "ReportStatus");
        builder.AddAttribute(32, "Title", "Stage");
        builder.AddAttribute(33, "Width", "120px");
        builder.AddAttribute(34, "Template", (RenderFragment<ReportProcessingSummary>)(report => 
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenBadge>(0);
                templateBuilder.AddAttribute(1, "BadgeStyle", BadgeStyle.Info);
                templateBuilder.AddAttribute(2, "Text", "Validation");
                templateBuilder.AddAttribute(3, "Variant", Variant.Flat);
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();

        // Priority Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(40);
        builder.AddAttribute(41, "Property", "Priority");
        builder.AddAttribute(42, "Title", "Priority");
        builder.AddAttribute(43, "Width", "100px");
        builder.AddAttribute(44, "Template", (RenderFragment<ReportProcessingSummary>)(report => 
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

        // Reported By Column (FIXED: Ensure proper data binding)
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(50);
        builder.AddAttribute(51, "Property", "ReportedBy");
        builder.AddAttribute(52, "Title", "Reported By");
        builder.AddAttribute(53, "Width", "150px");
        builder.AddAttribute(54, "Template", (RenderFragment<ReportProcessingSummary>)(report => 
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenText>(0);
                templateBuilder.AddAttribute(1, "TextStyle", TextStyle.Body1);
                templateBuilder.AddAttribute(2, "Text", !string.IsNullOrEmpty(report.ReportedBy) ? report.ReportedBy : "Not Specified");
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();

        // Days in Stage Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(60);
        builder.AddAttribute(61, "Property", "DaysInStage");
        builder.AddAttribute(62, "Title", "Days in Stage");
        builder.AddAttribute(63, "Width", "120px");
        builder.AddAttribute(64, "Template", (RenderFragment<ReportProcessingSummary>)(report => 
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
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(70);
        builder.AddAttribute(71, "Title", "Actions");
        builder.AddAttribute(72, "Width", "150px");
        builder.AddAttribute(73, "Template", (RenderFragment<ReportProcessingSummary>)(report => 
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
        builder.AddAttribute(6, "Property", "ReportId");
        builder.AddAttribute(7, "Title", "Report ID");
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
                    stackBuilder.AddAttribute(3, "Text", report.ReportId);
                    stackBuilder.CloseComponent();
                    
                    // Show associated Hazard ID as secondary info if available
                    if (!string.IsNullOrEmpty(report.HazardId))
                    {
                        stackBuilder.OpenComponent<RadzenText>(10);
                        stackBuilder.AddAttribute(11, "TextStyle", TextStyle.Caption);
                        stackBuilder.AddAttribute(12, "Style", "color: var(--rz-text-disabled-color);");
                        stackBuilder.AddAttribute(13, "Text", $"Hazard: {report.HazardId}");
                        stackBuilder.CloseComponent();
                    }
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
        // Report ID Column with RISK ASSESSMENT action button - navigates to correct assessment type
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
                    if (!string.IsNullOrEmpty(report.HazardId))
                    {
                        stackBuilder.OpenComponent<RadzenText>(10);
                        stackBuilder.AddAttribute(11, "TextStyle", TextStyle.Caption);
                        stackBuilder.AddAttribute(12, "Style", "color: var(--rz-text-disabled-color);");
                        stackBuilder.AddAttribute(13, "Text", $"Hazard: {report.HazardId}");
                        stackBuilder.CloseComponent();
                    }
                    
                    // NEW: Show Validation Type
                    if (!string.IsNullOrEmpty(report.ValidationType))
                    {
                        stackBuilder.OpenComponent<RadzenText>(15);
                        stackBuilder.AddAttribute(16, "TextStyle", TextStyle.Caption);
                        stackBuilder.AddAttribute(17, "Style", "color: var(--rz-info); font-weight: 500;");
                        stackBuilder.AddAttribute(18, "Text", $"Type: {report.ValidationType}");
                        stackBuilder.CloseComponent();
                    }
                    
                    // Risk Assessment tab: Show assessment progress if available
                    if (report.HasRiskAssessment && report.CurrentAssessmentStep > 0)
                    {
                        if (report.ValidationType?.ToLower() == "technical")
                        {
                            stackBuilder.OpenComponent<RadzenText>(20);
                            stackBuilder.AddAttribute(21, "TextStyle", TextStyle.Caption);
                            stackBuilder.AddAttribute(22, "Style", "color: var(--rz-primary); font-weight: 500;");
                            stackBuilder.AddAttribute(23, "Text", $"Technical Assessment Step: {report.CurrentAssessmentStep}/5");
                            stackBuilder.CloseComponent();
                        }
                        else if (report.ValidationType?.ToLower() == "preliminary")
                        {
                            stackBuilder.OpenComponent<RadzenText>(20);
                            stackBuilder.AddAttribute(21, "TextStyle", TextStyle.Caption);
                            stackBuilder.AddAttribute(22, "Style", "color: var(--rz-success); font-weight: 500;");
                            stackBuilder.AddAttribute(23, "Text", "Preliminary Assessment");
                            stackBuilder.CloseComponent();
                        }
                    }
                    else
                    {
                        stackBuilder.OpenComponent<RadzenText>(25);
                        stackBuilder.AddAttribute(26, "TextStyle", TextStyle.Caption);
                        stackBuilder.AddAttribute(27, "Style", "color: var(--rz-success); font-weight: 500;");
                        stackBuilder.AddAttribute(28, "Text", "Validated - Ready for Assessment");
                        stackBuilder.CloseComponent();
                    }
                    
                    // ENHANCED SMART ACTION: Navigation based on ValidationType
                    stackBuilder.OpenComponent<RadzenButton>(30);
                    stackBuilder.AddAttribute(31, "Text", report.ActionButtonText);
                    stackBuilder.AddAttribute(32, "Icon", GetAssessmentIcon(report));
                    stackBuilder.AddAttribute(33, "ButtonStyle", GetAssessmentButtonStyle(report));
                    stackBuilder.AddAttribute(34, "Size", ButtonSize.Small);
                    stackBuilder.AddAttribute(35, "Click", EventCallback.Factory.Create<MouseEventArgs>(this, (args) => Navigation.NavigateTo(report.SmartValidationUrl)));
                    stackBuilder.CloseComponent();
                }));
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();

        RenderStandardColumns(builder, false); // No additional actions column
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
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        
        public string? HazardId { get; set; }
        public string HazardType { get; set; } = string.Empty;
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
                    ProcessingStatusCategory.Investigation => "View Investigation",
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
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

            // Load core entities using CQRS
            var (reports, hazards) = await LoadCoreEntitiesAsync();

            if (!reports.Any())
            {
                Logger.LogWarning("No reports found - initializing empty lists");
                InitializeEmptyLists();
                return;
            }

            // Create report summaries and categorize
            var reportSummaries = CreateReportSummaries(reports, hazards);
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

    private async Task<(List<Report> reports, List<Hazard> hazards)> LoadCoreEntitiesAsync()
    {
        var reports = new List<Report>();
        var hazards = new List<Hazard>();

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
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in core entities retrieval");
        }

        return (reports, hazards);
    }

    #endregion

    #region Report Processing

    private List<ReportProcessingSummary> CreateReportSummaries(List<Report> reports, List<Hazard> hazards)
    {
        var summaries = new List<ReportProcessingSummary>();

        foreach (var report in reports)
        {
            try
            {
                // Find matching hazard
                var hazard = hazards.FirstOrDefault(h => h.ReportCode?.Trim() == report.Code?.Trim());
                
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
                    Location = hazard?.HazardLocation?.Description ?? "Not specified",
                    Priority = GetPriorityString(hazard?.Priority),
                    ReportedBy = hazard?.ReportedBy ?? report.CreatedBy ?? "Unknown",
                    ReportedDate = hazard?.ReportedOn ?? report.CreatedDate ?? DateTime.UtcNow,
                    IsConfidential = hazard?.IsConfidential ?? false,
                    
                    StatusCategory = DetermineStatusCategory(report, hazard),
                    DaysInStage = CalculateDaysInStage(report, hazard),
                    AssignedTo = DetermineAssignedTo(report, hazard),
                    ValidationUrl = GetValidationUrl(report, hazard)
                };
                
                summaries.Add(summary);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error creating summary for report {ReportCode}", report.Code);
            }
        }

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

    private ProcessingStatusCategory DetermineStatusCategory(Report report, Hazard? hazard)
    {
        var effectiveStatus = hazard?.Status?.ToString() ?? report.Status ?? "New";

        return effectiveStatus switch
        {
            // ? FIXED: Added "Initial" and "Active" to match your actual data
            "Initial" or "Active" or "New" or "Submitted" or "Under Review" or "ACTIVE" or "Pending" => ProcessingStatusCategory.Validation,
            "SMS Risk Assessment - In Progress" or "Risk Assessment" or "Processing" or "UNDER_REVIEW" => ProcessingStatusCategory.RiskAssessment,
            "Under Investigation - Information Needed" or "Investigation Required" or "Pending Investigation" or "UNDER_INVESTIGATION" => ProcessingStatusCategory.Investigation,
            "Mitigation Planning" or "In Progress" or "Implementation" or "Tracking" => ProcessingStatusCategory.Mitigation,
            "Closed" or "Referred" or "Completed" or "Closed - Not SMS Risk" or "CLOSED" or "CANCELLED" => ProcessingStatusCategory.Closed,
            _ => ProcessingStatusCategory.Validation
        };
    }

    private string GetPriorityString(HazardPriority? priority) => priority?.ToString() ?? "Medium";

    private int CalculateDaysInStage(Report report, Hazard? hazard)
    {
        var referenceDate = hazard?.UpdatedDate ?? hazard?.CreatedDate ?? 
                           report.UpdatedDate ?? report.CreatedDate ?? DateTime.UtcNow;
        return (DateTime.UtcNow - referenceDate).Days;
    }

    private string? DetermineAssignedTo(Report report, Hazard? hazard)
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

    private string GetValidationUrl(Report report, Hazard? hazard)
    {
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
                RenderEmptyState(builder, "assessment", "No reports pending risk assessment", "Validated SMS risks will appear here");
                return;
            }

            builder.OpenComponent<RadzenDataGrid<ReportProcessingSummary>>(0);
            builder.AddAttribute(1, "Data", PendingRiskAssessment);
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
        // Report ID Column with Validate button
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
                    stackBuilder.AddAttribute(3, "Text", report.DisplayId);
                    stackBuilder.CloseComponent();
                    
                    stackBuilder.OpenComponent<RadzenButton>(4);
                    stackBuilder.AddAttribute(5, "Text", "Validate");
                    stackBuilder.AddAttribute(6, "Icon", "check_circle");
                    stackBuilder.AddAttribute(7, "ButtonStyle", ButtonStyle.Success);
                    stackBuilder.AddAttribute(8, "Size", ButtonSize.Small);
                    // ? FIXED: Use EventCallback<MouseEventArgs> instead of EventCallback
                    stackBuilder.AddAttribute(9, "Click", EventCallback.Factory.Create<MouseEventArgs>(this, (args) => Navigation.NavigateTo(report.ValidationUrl)));
                    stackBuilder.CloseComponent();
                }));
                templateBuilder.CloseComponent();
            })));
        builder.CloseComponent();

        RenderStandardColumns(builder, false); // No additional actions column
    }

    private void RenderStandardColumns(RenderTreeBuilder builder, bool includeActions = true)
    {
        // Description Column
        builder.OpenComponent<RadzenDataGridColumn<ReportProcessingSummary>>(10);
        builder.AddAttribute(11, "Property", "HazardDescription");
        builder.AddAttribute(12, "Title", "Description");
        builder.AddAttribute(13, "Width", "300px");
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
        
        public ProcessingStatusCategory StatusCategory { get; set; }
        public int DaysInStage { get; set; }
        public string? AssignedTo { get; set; }
        public string ValidationUrl { get; set; } = string.Empty;
        
        public string DisplayId => !string.IsNullOrEmpty(HazardId) ? HazardId : ReportId;
    }

    #endregion
}
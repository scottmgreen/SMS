using Microsoft.AspNetCore.Components;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Shared.Common;
using SMS3.Components.Layout;
using Radzen;
using Radzen.Blazor;

namespace SMS3.Components.Pages.SMSRiskManagement;

/// <summary>
/// Reports Management - Code-behind for Reports.razor
/// Handles all business logic and data operations for Reports listing with expandable hazard details, paging, and CRUD operations
/// </summary>
public partial class Reports : ComponentBase
{
    #region Dependencies
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<Reports> Logger { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    // **CASCADING PARAMETER**: Get authentication from MainLayout
    [CascadingParameter(Name = "AuthState")]
    public AuthenticationState? AuthState { get; set; }
    #endregion

    #region Properties
    /// <summary>
    /// List of all reports loaded from SMS backend
    /// </summary>
    public IList<Report> AllReports { get; set; } = new List<Report>();

    /// <summary>
    /// List of hazards for expanded reports (cached for performance)
    /// </summary>
    public Dictionary<string, List<Hazard>> ReportHazards { get; set; } = new Dictionary<string, List<Hazard>>();

    /// <summary>
    /// Loading state indicator
    /// </summary>
    public bool IsLoading { get; set; } = true;

    /// <summary>
    /// Error message if data loading fails
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Current user authentication state (from cascading parameter)
    /// </summary>
    public bool IsAuthenticated => AuthState?.IsAuthenticated == true;

    /// <summary>
    /// Current user display name (from cascading parameter)
    /// </summary>
    public string? CurrentUserName => AuthState?.DisplayName;

    /// <summary>
    /// Set of currently expanded report codes
    /// </summary>
    private HashSet<string> expandedReports = new();

    /// <summary>
    /// Current page size for pagination
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Available page size options
    /// </summary>
    public IList<int> PageSizeOptions { get; set; } = new List<int> { 5, 10, 15, 20, 25, 50 };

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int CurrentPage { get; set; } = 1;

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)AllReports.Count / PageSize);
    #endregion

    #region Lifecycle Methods
    /// <summary>
    /// Component initialization - Load reports data
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        await CheckAuthenticationAsync();
        await LoadReportsAsync();
    }
    #endregion

    #region Data Loading Methods
    /// <summary>
    /// Check user authentication status
    /// </summary>
    private async Task CheckAuthenticationAsync()
    {
        try
        {
            if (!IsAuthenticated)
            {
                Logger.LogWarning("Unauthorized access attempt to Reports page");
                ErrorMessage = "You must be logged in to view reports.";
            }
            else
            {
                Logger.LogInformation("Authenticated user {UserName} accessing Reports page", CurrentUserName);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error checking authentication status");
            ErrorMessage = "Authentication check failed.";
        }
    }

    /// <summary>
    /// Load all reports from SMS backend using GetAllReportsQuery
    /// </summary>
    private async Task LoadReportsAsync()
    {
        if (!IsAuthenticated)
        {
            IsLoading = false;
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            Logger.LogInformation("Loading reports for user: {UserName}", CurrentUserName);

            // Execute GetAllReportsQuery via Mediator
            var query = new GetAllReportsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                AllReports = result.Value;
                Logger.LogInformation("Successfully loaded {Count} reports", AllReports.Count);
                
                // Show success notification
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Reports Loaded",
                    Detail = $"Successfully loaded {AllReports.Count} reports",
                    Duration = 3000
                });
            }
            else
            {
                AllReports = new List<Report>();
                ErrorMessage = result.Error?.Message ?? "Failed to load reports";
                Logger.LogWarning("Failed to load reports: {Error}", ErrorMessage);
                
                // Show error notification
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Load Failed",
                    Detail = ErrorMessage,
                    Duration = 5000
                });
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception occurred while loading reports");
            AllReports = new List<Report>();
            ErrorMessage = "An unexpected error occurred while loading reports.";
            
            // Show error notification
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Error",
                Detail = "An unexpected error occurred while loading reports.",
                Duration = 5000
            });
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }
    #endregion

    #region Paging Event Handlers
    /// <summary>
    /// Handle page size change
    /// </summary>
    /// <param name="newPageSize">New page size selected</param>
    public async Task OnPageSizeChanged(object newPageSize)
    {
        if (int.TryParse(newPageSize?.ToString(), out var pageSize))
        {
            Logger.LogInformation("Page size changed from {OldSize} to {NewSize}", PageSize, pageSize);
            PageSize = pageSize;
            CurrentPage = 1; // Reset to first page when page size changes
            
            // Collapse any expanded row when changing page size
            expandedReports.Clear();
            
            StateHasChanged();
            
            // Show notification
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Info,
                Summary = "Page Size Changed",
                Detail = $"Now showing {PageSize} reports per page",
                Duration = 2000
            });
        }
    }
    #endregion

    #region CRUD Event Handlers
    /// <summary>
    /// Navigate to hazard reporting page instead of showing modal
    /// </summary>
    public void NavigateToHazardReporting()
    {
        Logger.LogInformation("Navigating to hazard reporting page");
        Navigation.NavigateTo("/SMSRiskManagement/HazardReporting");
    }

    /// <summary>
    /// Handle view report request - Show detailed report information
    /// </summary>
    /// <param name="report">Report to view</param>
    public async Task OnViewReportAsync(Report report)
    {
        Logger.LogInformation("View report requested: {ReportCode}", report.Code);

        try
        {
            // Get additional details from backend if needed
            var query = new GetReportByIdQuery(new ReportID(report.Code));
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            Report detailedReport = result.IsSuccess ? result.Value : report;

            // Build comprehensive report details
            var reportDetails = $@"
?? Report Information:

• Report Code: {detailedReport.Code}
• Name: {detailedReport.Name ?? "Not specified"}
• Status: {detailedReport.Status ?? "Not specified"}
• Stage: {detailedReport.Stage ?? "Not specified"}

?? Description:
{detailedReport.Description ?? "No description provided"}

?? Timeline:
• Created: {(detailedReport.CreatedDate?.ToString("dddd, MMMM dd, yyyy 'at' h:mm tt") ?? "N/A")}
• Created By: {detailedReport.CreatedBy ?? "System"}
• Last Updated: {(detailedReport.UpdatedDate?.ToString("dddd, MMMM dd, yyyy 'at' h:mm tt") ?? "Never")}
• Updated By: {detailedReport.UpdatedBy ?? "N/A"}

?? Associated Hazards:
{(ReportHazards.ContainsKey(report.Code) && ReportHazards[report.Code].Any() ? 
    string.Join("\n", ReportHazards[report.Code].Select(h => $"  • {h.Code} - {h.Name ?? "Unnamed Hazard"}")) : 
    "No hazards associated with this report")}";

            await DialogService.Alert(reportDetails, $"Report Details - {detailedReport.Code}");

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Info,
                Summary = "Report Viewed",
                Detail = $"Displayed details for report {report.Code}",
                Duration = 2000
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error showing report details for {ReportCode}", report.Code);

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "View Error",
                Detail = "Failed to display report details. Please try again.",
                Duration = 3000
            });
        }
    }

    /// <summary>
    /// Handle edit report request - Navigate to HazardReporting page in edit mode
    /// </summary>
    /// <param name="report">Report to edit</param>
    public async Task OnEditReportAsync(Report report)
    {
        Logger.LogInformation("Edit report requested: {ReportCode}", report.Code);

        try
        {
            // Show confirmation before navigating away
            var confirmed = await DialogService.Confirm(
                $"Edit report '{report.Code} - {report.Name}'?\n\nThis will navigate to the hazard reporting form in edit mode.", 
                "Edit Report", 
                new ConfirmOptions() 
                { 
                    OkButtonText = "Yes, Edit Report", 
                    CancelButtonText = "Cancel" 
                });

            if (confirmed == true)
            {
                // Navigate to HazardReporting page with edit parameters
                // The HazardReporting page will need to be enhanced to handle edit mode
                Navigation.NavigateTo($"/SMSRiskManagement/HazardReporting?mode=edit&reportCode={report.Code}");

                Logger.LogInformation("Navigating to edit report: {ReportCode}", report.Code);

                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Info,
                    Summary = "Navigating to Edit",
                    Detail = $"Opening {report.Code} for editing...",
                    Duration = 3000
                });
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error navigating to edit report {ReportCode}", report.Code);

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Navigation Error",
                Detail = "Failed to navigate to edit form. Please try again.",
                Duration = 3000
            });
        }
    }

    /// <summary>
    /// Handle delete report request - Show confirmation and delete via CQRS
    /// </summary>
    /// <param name="report">Report to delete</param>
    public async Task OnDeleteReportAsync(Report report)
    {
        Logger.LogInformation("Delete report requested: {ReportCode}", report.Code);

        try
        {
            // Get hazard count for this report to show in confirmation
            var hazardCount = 0;
            if (ReportHazards.ContainsKey(report.Code))
            {
                hazardCount = ReportHazards[report.Code].Count;
            }
            else
            {
                // Try to load hazards if not already cached
                await LoadHazardsForReportAsync(report.Code);
                hazardCount = ReportHazards.ContainsKey(report.Code) ? ReportHazards[report.Code].Count : 0;
            }

            // Build confirmation message with hazard warning
            var confirmationMessage = $"Are you sure you want to delete report '{report.Code}'?\n\n" +
                                    $"Report Details:\n" +
                                    $"• Name: {report.Name ?? "Unnamed Report"}\n" +
                                    $"• Status: {report.Status ?? "Unknown"}\n" +
                                    $"• Associated Hazards: {hazardCount}\n\n" +
                                    (hazardCount > 0 ? "??  WARNING: This report has associated hazards that may also be affected.\n\n" : "") +
                                    "?? This action cannot be undone!";

            // Show Radzen confirmation dialog
            var confirmed = await DialogService.Confirm(
                confirmationMessage,
                "Confirm Delete Report", 
                new ConfirmOptions() 
                { 
                    OkButtonText = "Yes, Delete Report", 
                    CancelButtonText = "Cancel",
                    AutoFocusFirstElement = false // Focus Cancel by default for safety
                });

            if (confirmed == true)
            {
                // Execute delete command via CQRS
                var deleteCommand = new DeleteReportCommand(new ReportID(report.Code));
                var result = await Mediator.SendAsync(deleteCommand, CancellationToken.None);

                if (result.IsSuccess && result.Value)
                {
                    Logger.LogInformation("Successfully deleted report: {ReportCode}", report.Code);

                    // Remove from local list
                    var reportList = AllReports.ToList();
                    reportList.RemoveAll(r => r.Code == report.Code);
                    AllReports = reportList;

                    // Clear any cached hazards for this report
                    if (ReportHazards.ContainsKey(report.Code))
                    {
                        ReportHazards.Remove(report.Code);
                    }

                    // Remove from expanded reports if it was expanded
                    expandedReports.Remove(report.Code);

                    StateHasChanged();

                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Report Deleted",
                        Detail = $"Report {report.Code} has been successfully deleted.",
                        Duration = 4000
                    });
                }
                else
                {
                    throw new InvalidOperationException(result.Error?.Message ?? "Failed to delete report");
                }
            }
            else
            {
                Logger.LogInformation("Delete report cancelled by user: {ReportCode}", report.Code);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting report: {ReportCode}", report.Code);

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Delete Failed",
                Detail = "Failed to delete the report. Please try again.",
                Duration = 5000
            });
        }
    }
    #endregion

    #region UI Event Handlers
    /// <summary>
    /// Refresh the reports list
    /// </summary>
    public async Task RefreshAsync()
    {
        // Clear expanded reports and hazard cache when refreshing
        expandedReports.Clear();
        ReportHazards.Clear();
        CurrentPage = 1; // Reset to first page
        await LoadReportsAsync();
    }

    /// <summary>
    /// Get paginated reports for current page
    /// </summary>
    /// <returns>Reports for current page</returns>
    public IEnumerable<Report> GetPaginatedReports()
    {
        return AllReports.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
    }

    /// <summary>
    /// Handle page change
    /// </summary>
    /// <param name="pageIndex">New page index (0-based)</param>
    public async Task OnPageChanged(int pageIndex)
    {
        CurrentPage = pageIndex + 1; // Convert from 0-based to 1-based
        Logger.LogInformation("Page changed to {PageNumber}", CurrentPage);
        
        // Collapse all expanded reports when changing pages for better performance
        expandedReports.Clear();
        
        StateHasChanged();
        
        // Show notification
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Info,
            Summary = "Page Changed",
            Detail = $"Showing page {CurrentPage} of {TotalPages}",
            Duration = 2000
        });
    }

    /// <summary>
    /// Handle report expansion changed
    /// </summary>
    /// <param name="report">Report being expanded/collapsed</param>
    /// <param name="expanded">True if expanding, false if collapsing</param>
    public async Task OnReportExpandedChanged(Report report, bool expanded)
    {
        Logger.LogInformation("Report {ReportCode} accordion {Action}, ExpandedReports before: {ExpandedReports}", 
            report.Code, expanded ? "expanded" : "collapsed", string.Join(", ", expandedReports));
        
        try
        {
            if (expanded)
            {
                // Add to expanded set
                expandedReports.Add(report.Code);
                Logger.LogInformation("Added {ReportCode} to expandedReports. Current count: {Count}", 
                    report.Code, expandedReports.Count);
                
                // Load hazards for this report if not already cached
                if (!ReportHazards.ContainsKey(report.Code))
                {
                    await LoadHazardsForReportAsync(report.Code);
                }
                
                // Show notification about loading hazards
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Info,
                    Summary = "Loading Hazards",
                    Detail = $"Loading hazards for report {report.Code}...",
                    Duration = 2000
                });
            }
            else
            {
                // Remove from expanded set
                expandedReports.Remove(report.Code);
                Logger.LogInformation("Removed {ReportCode} from expandedReports. Current count: {Count}", 
                    report.Code, expandedReports.Count);
            }
            
            Logger.LogInformation("ExpandedReports after: {ExpandedReports}", string.Join(", ", expandedReports));
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling report expansion for report {ReportCode}", report.Code);
            
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Error",
                Detail = "Failed to load report details.",
                Duration = 3000
            });
        }
    }

    /// <summary>
    /// Check if a report is currently expanded
    /// </summary>
    /// <param name="report">Report to check</param>
    /// <returns>True if expanded</returns>
    public bool IsReportExpanded(Report report)
    {
        var isExpanded = expandedReports.Contains(report.Code);
        Logger.LogDebug("IsReportExpanded check: Report {ReportCode}, Expanded: {IsExpanded}, ExpandedReports count: {Count}", 
            report.Code, isExpanded, expandedReports.Count);
        return isExpanded;
    }

    /// <summary>
    /// Get display text for the accordion header
    /// </summary>
    /// <param name="report">Report to get display text for</param>
    /// <returns>Formatted display text</returns>
    public string GetReportDisplayText(Report report)
    {
        return $"{report.Code} - {report.Name ?? "Unnamed Report"}";
    }
    /// <summary>
    /// Get primary hazard reported date for display
    /// </summary>
    /// <param name="report">Report to get primary hazard date for</param>
    /// <returns>Formatted date string</returns>
    public string GetPrimaryHazardDate(Report report)
    {
        try
        {
            // If hazards are cached for this report
            if (ReportHazards.TryGetValue(report.Code, out var hazards) && hazards.Any())
            {
                // Find the primary/initial hazard (first one reported)
                var primaryHazard = hazards.OrderBy(h => h.ReportedOn).FirstOrDefault();
                if (primaryHazard != null)
                {
                    return primaryHazard.ReportedOn.ToString("MM/dd/yyyy");
                }
            }
            
            // Fallback to report created date
            return report.CreatedDate?.ToString("MM/dd/yyyy") ?? "N/A";
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting primary hazard date for report {ReportCode}", report.Code);
            return report.CreatedDate?.ToString("MM/dd/yyyy") ?? "N/A";
        }
    }

    /// <summary>
    /// Truncate description text for display
    /// </summary>
    /// <param name="description">Description to truncate</param>
    /// <param name="maxLength">Maximum length</param>
    /// <returns>Truncated description</returns>
    public string TruncateDescription(string description, int maxLength)
    {
        if (string.IsNullOrEmpty(description) || description.Length <= maxLength)
        {
            return description ?? "";
        }
        
        return description.Substring(0, maxLength) + "...";
    }

    /// <summary>
    /// Load hazards for a specific report
    /// </summary>
    /// <param name="reportCode">Report code to load hazards for</param>
    private async Task LoadHazardsForReportAsync(string reportCode)
    {
        try
        {
            Logger.LogInformation("Loading hazards for report: {ReportCode}", reportCode);

            // Execute GetHazardsByReportCodeQuery via Mediator
            var query = new GetHazardsByReportCodeQuery(reportCode);
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                // Cache the hazards
                ReportHazards[reportCode] = result.Value.ToList();
                Logger.LogInformation("Successfully loaded {Count} hazards for report {ReportCode}", 
                    result.Value.Count(), reportCode);
            }
            else
            {
                // Cache empty list to avoid repeated failed requests
                ReportHazards[reportCode] = new List<Hazard>();
                Logger.LogWarning("No hazards found for report {ReportCode}: {Error}", 
                    reportCode, result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception occurred while loading hazards for report {ReportCode}", reportCode);
            // Cache empty list to avoid repeated failed requests
            ReportHazards[reportCode] = new List<Hazard>();
        }
    }

    /// <summary>
    /// Handle view hazard request from the hazard details component
    /// </summary>
    /// <param name="hazard">Hazard to view</param>
    public async Task OnViewHazard(Hazard hazard)
    {
        Logger.LogInformation("View hazard requested: {HazardCode}", hazard.Code);
        
        var hazardDetails = $@"Hazard Details:

Code: {hazard.Code}
Name: {hazard.Name ?? "Not specified"}
Type: {hazard.HazardType ?? "Not specified"}
Category: {hazard.Category ?? "Not specified"}
Description: {hazard.Description ?? "Not specified"}
Status: {hazard.Status?.Name ?? "Not specified"}
Priority: {hazard.Priority?.Name ?? "Not specified"}
Risk Level: {hazard.RiskLevel ?? "Not assessed"}
Reported By: {hazard.ReportedBy ?? "Unknown"}
Reported On: {hazard.ReportedOn:MM/dd/yyyy}
Created: {hazard.CreatedDate.Value:MM/dd/yyyy}";

        await DialogService.Alert(hazardDetails, "Hazard Information");
    }

    /// <summary>
    /// Handle edit hazard request from the hazard details component
    /// </summary>
    /// <param name="hazard">Hazard to edit</param>
    public async Task OnEditHazard(Hazard hazard)
    {
        Logger.LogInformation("Edit hazard requested: {HazardCode}", hazard.Code);
        
        var confirmed = await DialogService.Confirm(
            $"Navigate to edit hazard {hazard.Code}?", 
            "Edit Hazard", 
            new ConfirmOptions() { OkButtonText = "Yes, Edit", CancelButtonText = "Cancel" });
            
        if (confirmed == true)
        {
            // TODO: Navigate to hazard edit page when it's implemented
            // For now, show a placeholder message
            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Info,
                Summary = "Edit Hazard",
                Detail = $"Hazard editing for {hazard.Code} will be implemented in a future update.",
                Duration = 5000
            });
        }
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Get Radzen badge style for status
    /// </summary>
    public BadgeStyle GetStatusBadgeStyle(string? status)
    {
        return status?.ToLower() switch
        {
            "active" => BadgeStyle.Success,
            "pending" => BadgeStyle.Warning,
            "closed" => BadgeStyle.Secondary,
            "cancelled" => BadgeStyle.Danger,
            _ => BadgeStyle.Info
        };
    }

    /// <summary>
    /// Get Radzen badge style for stage
    /// </summary>
    public BadgeStyle GetStageBadgeStyle(string? stage)
    {
        return stage?.ToLower() switch
        {
            "validation" => BadgeStyle.Primary,
            "assessment" => BadgeStyle.Info,
            "mitigation" => BadgeStyle.Warning,
            "closure" => BadgeStyle.Success,
            _ => BadgeStyle.Secondary
        };
    }
    #endregion
}

#region Supporting Data Classes
/// <summary>
/// Data class for editing report information
/// </summary>
public class EditReportData
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Stage { get; set; } = string.Empty;
}
#endregion
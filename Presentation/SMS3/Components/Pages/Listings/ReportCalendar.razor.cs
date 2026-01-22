namespace SMS3.Components.Pages.Listings;

public partial class ReportCalendar : ComponentBase
{
    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<ReportCalendar> Logger { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    #endregion

    #region Component State
    private bool IsLoading { get; set; } = true;
    private RadzenScheduler<ReportSchedulerItem> scheduler = default!;
    private List<Report> Reports { get; set; } = new();
    private List<ReportSchedulerItem> SchedulerData { get; set; } = new();
    private Report? SelectedReport { get; set; }

    // Modal state properties (copied from ReportListing)
    public bool ShowDetailsModal { get; set; } = false;
    public List<Hazard> AssociatedHazards { get; set; } = new();
    public string LocationsWithMaps => AssociatedHazards
        .Count(h => h.HazardLocation?.Latitude.HasValue == true && h.HazardLocation?.Longitude.HasValue == true)
        .ToString();
    public int TotalFilesCount => AssociatedHazards
        .Sum(h => h.HazardFileIds?.Count ?? 0);
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        await LoadReportsAsync();
    }
    #endregion

    #region Data Loading
    private async Task LoadReportsAsync()
    {
        try
        {
            IsLoading = true;
            StateHasChanged();

            Logger.LogInformation("Loading reports for calendar display");

            var query = new GetAllReportsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                Reports = result.Value.ToList();
                Logger.LogInformation("Loaded {Count} reports for calendar", Reports.Count);

                // Convert reports to scheduler items
                SchedulerData = Reports.Select(MapReportToSchedulerItem).ToList();
            }
            else
            {
                Logger.LogError("Failed to load reports: {Error}", result.Error?.Message);
                ShowErrorNotification("Failed to load reports for calendar");
                Reports = new List<Report>();
                SchedulerData = new List<ReportSchedulerItem>();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading reports for calendar");
            ShowErrorNotification("Error loading reports");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task RefreshData()
    {
        await LoadReportsAsync();
        ShowSuccessNotification("Calendar data refreshed");
    }
    #endregion

    #region Data Mapping
    private ReportSchedulerItem MapReportToSchedulerItem(Report report)
    {
        var reportDate = report.CreatedDate ?? DateTime.Now;

        return new ReportSchedulerItem
        {
            ReportId = report.Id?.Value ?? "",
            ReportCode = report.Code ?? "Unknown",
            Text = $"{report.Code} - {GetShortDescription(report)}",
            Start = reportDate,
            End = reportDate.AddHours(1), // Default 1 hour duration for display
            ReportType = DetermineReportType(report),
            Priority = DeterminePriority(report),
            Status = report.Status ?? "Unknown",
            Reporter = report.ReportedBy ?? "Unknown",
            Description = report.Description ?? "No description available"
        };
    }

    private string GetShortDescription(Report report)
    {
        var description = report.Description ?? "No description";
        return description.Length > 50 ? $"{description[..50]}..." : description;
    }

    private ReportType DetermineReportType(Report report)
    {
        // Logic to determine report type from report data
        // Since we don't have specific type fields, use the Name or Description to infer
        var text = (report.Name + " " + report.Description).ToLower();

        if (text.Contains("incident"))
            return ReportType.Incident;
        else if (text.Contains("hazard"))
            return ReportType.Hazard;
        else if (text.Contains("observation"))
            return ReportType.Observation;
        else
            return ReportType.Other;
    }

    private Priority DeterminePriority(Report report)
    {
        // Logic to determine priority - can be enhanced based on actual report data
        // For now, return Medium as default
        return Priority.Medium;
    }
    #endregion

    #region Scheduler Event Handlers
    private async Task OnSlotSelect(SchedulerSlotSelectEventArgs args)
    {
        try
        {
            Logger.LogInformation("Slot selected: {Start} to {End}", args.Start, args.End);

            // Optional: Show dialog to create new report for selected date
            // This can be implemented later if needed
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling slot selection");
        }
    }

    private async Task OnAppointmentSelect(SchedulerAppointmentSelectEventArgs<ReportSchedulerItem> args)
    {
        try
        {
            var reportItem = args.Data;
            Logger.LogInformation("Report appointment selected: {ReportCode}", reportItem.ReportCode);

            // Navigate to report details or show popup
            await ShowReportDetails(reportItem);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling appointment selection");
        }
    }

    private void OnAppointmentRender(SchedulerAppointmentRenderEventArgs<ReportSchedulerItem> args)
    {
        try
        {
            // Customize appointment appearance based on report type
            var reportItem = args.Data;

            switch (reportItem.ReportType)
            {
                case ReportType.Incident:
                    args.Attributes["class"] = "report-incident";
                    break;
                case ReportType.Hazard:
                    args.Attributes["class"] = "report-hazard";
                    break;
                case ReportType.Observation:
                    args.Attributes["class"] = "report-observation";
                    break;
                default:
                    args.Attributes["class"] = "report-other";
                    break;
            }

            // Add tooltip with additional information
            args.Attributes["title"] = $"Report: {reportItem.ReportCode}\nType: {reportItem.ReportType}\nReporter: {reportItem.Reporter}\nStatus: {reportItem.Status}";
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error rendering appointment");
        }
    }

    private void OnSlotRender(SchedulerSlotRenderEventArgs args)
    {
        try
        {
            // Optional: Customize slot rendering if needed
            // This can be used to highlight specific dates or add visual indicators
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error rendering slot");
        }
    }
    #endregion

    #region Navigation and Actions
    private async Task GoToToday()
    {
        try
        {
            if (scheduler != null)
            {
                // Navigate the scheduler to today's date
                scheduler.CurrentDate = DateTime.Today;
                await scheduler.Reload();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error navigating to today");
        }
    }

    private async Task ShowReportDetails(ReportSchedulerItem reportItem)
    {
        Logger.LogInformation("View report details requested from calendar: {ReportCode}", reportItem.ReportCode);

        try
        {
            IsLoading = true;
            StateHasChanged();

            // Get detailed report information
            var reportQuery = new GetReportByIdQuery(new ReportID(reportItem.ReportCode));
            var reportResult = await Mediator.SendAsync(reportQuery, CancellationToken.None);

            if (reportResult.IsSuccess && reportResult.Value != null)
            {
                SelectedReport = reportResult.Value;
            }
            else
            {
                // Find the report from the loaded reports as fallback
                SelectedReport = Reports.FirstOrDefault(r => r.Code == reportItem.ReportCode);
                if (SelectedReport == null)
                {
                    ShowErrorNotification($"Report {reportItem.ReportCode} not found");
                    return;
                }
            }

            // Load associated hazards for this report
            await LoadAssociatedHazardsAsync(reportItem.ReportCode);

            // Show the details modal
            ShowDetailsModal = true;

            Logger.LogInformation("Displaying details for report: {ReportCode} with {HazardCount} hazards",
                reportItem.ReportCode, AssociatedHazards.Count);

            ShowSuccessNotification($"Report details loaded for {reportItem.ReportCode}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error showing report details for {ReportCode}", reportItem.ReportCode);
            ShowErrorNotification("Error opening report details");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }
    #endregion

    #region Statistics Methods
    private int GetReportsThisMonth()
    {
        var now = DateTime.Now;
        return Reports.Count(r => r.CreatedDate?.Year == now.Year && r.CreatedDate?.Month == now.Month);
    }

    private int GetReportsThisWeek()
    {
        var now = DateTime.Now;
        var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(7);

        return Reports.Count(r => r.CreatedDate >= startOfWeek && r.CreatedDate < endOfWeek);
    }

    private int GetReportsToday()
    {
        var today = DateTime.Today;
        return Reports.Count(r => r.CreatedDate?.Date == today);
    }
    #endregion

    #region Notification Methods
    private void ShowSuccessNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Success",
            Detail = message,
            Duration = 4000
        });
    }

    private void ShowErrorNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Error",
            Detail = message,
            Duration = 6000
        });
    }
    #endregion

    #region Modal Management Methods (copied from ReportListing)

    /// <summary>
    /// Close the details modal
    /// </summary>
    public void CloseDetailsModal()
    {
        ShowDetailsModal = false;
        SelectedReport = null;
        AssociatedHazards.Clear();
        StateHasChanged();
    }

    /// <summary>
    /// Edit report from details modal
    /// </summary>
    /// <param name="report">Report to edit</param>
    public async Task EditFromDetailsModal(Report report)
    {
        CloseDetailsModal();
        await OnEditReportAsync(report);
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
                Navigation.NavigateTo($"/SMSRiskManagement/HazardReporting?mode=edit&reportCode={report.Code}");

                Logger.LogInformation("Navigating to edit report: {ReportCode}", report.Code);

                ShowSuccessNotification($"Opening {report.Code} for editing...");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error navigating to edit report {ReportCode}", report.Code);
            ShowErrorNotification("Failed to navigate to edit form");
        }
    }

    /// <summary>
    /// Load hazards associated with a specific report
    /// </summary>
    /// <param name="reportCode">Report code to load hazards for</param>
    private async Task LoadAssociatedHazardsAsync(string reportCode)
    {
        try
        {
            Logger.LogInformation("Loading hazards for report: {ReportCode}", reportCode);

            // Try using GetHazardsByReportCodeQuery if it exists, otherwise fallback to GetAllHazardsQuery with filtering
            try
            {
                var hazardsQuery = new GetHazardsByReportCodeQuery(reportCode);
                var hazardsResult = await Mediator.SendAsync(hazardsQuery, CancellationToken.None);

                if (hazardsResult.IsSuccess && hazardsResult.Value != null)
                {
                    AssociatedHazards = hazardsResult.Value.ToList();
                    Logger.LogInformation("Loaded {Count} hazards for report {ReportCode}",
                        AssociatedHazards.Count, reportCode);
                    return;
                }
            }
            catch (Exception queryEx)
            {
                Logger.LogWarning(queryEx, "GetHazardsByReportCodeQuery not available, using fallback approach");
            }

            // Fallback: Get all hazards and filter by report code
            var allHazardsQuery = new GetAllHazardsQuery();
            var allHazardsResult = await Mediator.SendAsync(allHazardsQuery, CancellationToken.None);

            if (allHazardsResult.IsSuccess && allHazardsResult.Value != null)
            {
                // Filter hazards that are associated with this report
                AssociatedHazards = allHazardsResult.Value
                    .Where(h => h.ReportCode == reportCode)
                    .ToList();

                Logger.LogInformation("Loaded {Count} hazards for report {ReportCode} using fallback method",
                    AssociatedHazards.Count, reportCode);
            }
            else
            {
                AssociatedHazards = new List<Hazard>();
                Logger.LogWarning("No hazards found for report {ReportCode}: {Error}",
                    reportCode, allHazardsResult.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading hazards for report {ReportCode}", reportCode);
            AssociatedHazards = new List<Hazard>();
        }
    }

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

#region Supporting Classes and Enums
/// <summary>
/// Scheduler item representation of a Report for calendar display
/// </summary>
public class ReportSchedulerItem
{
    public string ReportId { get; set; } = string.Empty;
    public string ReportCode { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public ReportType ReportType { get; set; }
    public Priority Priority { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Reporter { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Report types for calendar categorization
/// </summary>
public enum ReportType
{
    Incident,
    Hazard,
    Observation,
    Other
}

/// <summary>
/// Priority levels for visual indicators
/// </summary>
public enum Priority
{
    Low,
    Medium,
    High,
    Critical
}
#endregion
using SMS3.Components.Pages.SMSAssurance.Components;
using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration.Extensions;

namespace SMS3.Components.Pages.SMSRiskManagement;

public partial class MitigationCalendar : ComponentBase
{
    #region Injected Services
    [Inject] private IMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<MitigationCalendar> _logger { get; set; } = default!;
    [Inject] private INotificationHelper _notificationHelper { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    [Inject] private ICurrentUserService CurrentUserService { get; set; } = default!;
    #endregion

    #region Component State
    private bool IsLoading { get; set; } = true;
    private bool _isLoadingData = false; // Prevent recursive loading
    private bool _handlingAppointmentClick = false; // Prevent multiple appointment clicks
    private RadzenScheduler<MitigationSchedulerItem> scheduler = default!;
    private List<Mitigation> Mitigations { get; set; } = new();
    private List<MitigationSchedulerItem> SchedulerData { get; set; } = new();
    private Mitigation? SelectedMitigation { get; set; }

    // Enhanced UI state properties
    public bool ShowDetailsModal { get; set; } = false;
    private bool showHeader = true;
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        await LoadMitigationsAsync();
    }
    #endregion

    #region Data Loading
    private async Task LoadMitigationsAsync()
    {
        // Prevent recursive calls
        if (_isLoadingData)
        {
            return;
        }

        try
        {
            _isLoadingData = true;
            IsLoading = true;
            StateHasChanged();

            _logger.LogInformation("Loading mitigations for calendar display");

            var query = new GetAllMitigationsQuery();
            var result = await _mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                Mitigations = result.Value.ToList();
                _logger.LogInformation("Loaded {Count} mitigations for calendar", Mitigations.Count);

                // Convert mitigations to scheduler items
                SchedulerData = Mitigations.Select(MapMitigationToSchedulerItem).ToList();
            }
            else
            {
                _logger.LogError("Failed to load mitigations: {Error}", result.Error?.Message);
                await _notificationHelper.ShowErrorAsync("Failed to load mitigations for calendar");
                Mitigations = new List<Mitigation>();
                SchedulerData = new List<MitigationSchedulerItem>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading mitigations for calendar");
            await _notificationHelper.ShowErrorAsync("Error loading mitigations");
        }
        finally
        {
            _isLoadingData = false;
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task RefreshData()
    {
        _logger.LogInformation("Refreshing mitigation calendar data...");
        await LoadMitigationsAsync();

        // Reload the scheduler
        if (scheduler != null)
        {
            await scheduler.Reload();
        }

        await _notificationHelper.ShowSuccessAsync("Calendar data refreshed");
        _logger.LogInformation("Mitigation calendar refresh completed");
    }
    #endregion

    #region Data Mapping
    private MitigationSchedulerItem MapMitigationToSchedulerItem(Mitigation mitigation)
    {
        return new MitigationSchedulerItem
        {
            Start = mitigation.TargetDate ?? DateTime.Today,
            End = mitigation.TargetDate?.AddDays(1) ?? DateTime.Today.AddDays(1),
            Text = $"{mitigation.Name} (Status: {mitigation.Status})",
            MitigationCode = mitigation.Code
        };
    }

    private string GetMitigationDescription(Mitigation mitigation)
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(mitigation.HazardCode))
            parts.Add($"Hazard: {mitigation.HazardCode}");

        if (!string.IsNullOrEmpty(mitigation.AssignedTo))
            parts.Add($"Assigned to: {mitigation.AssignedTo}");

        if (mitigation.Progress > 0)
            parts.Add($"Progress: {mitigation.Progress}%");

        return string.Join(" | ", parts);
    }
    #endregion

    #region Enhanced Scheduler Event Handlers
    private async Task OnSlotSelect(SchedulerSlotSelectEventArgs args)
    {
        try
        {
            _logger.LogInformation("Slot selected: {Start} to {End}", args.Start, args.End);

            // Don't create appointments in year view
            if (args.View.Text != "Year")
            {
                await ShowCreateMitigationDialog(args.Start, args.End);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling slot selection");
        }
    }

    private async Task OnAppointmentSelect(SchedulerAppointmentSelectEventArgs<MitigationSchedulerItem> args)
    {
        if (_handlingAppointmentClick || args.Data?.MitigationCode == null)
        {
            return;
        }

        try
        {
            _handlingAppointmentClick = true;
            _logger.LogInformation("Appointment selected: {MitigationCode}", args.Data.MitigationCode);

            // Find the actual mitigation from the loaded list
            SelectedMitigation = Mitigations.FirstOrDefault(m => m.Code == args.Data.MitigationCode);
            if (SelectedMitigation != null)
            {
                ShowDetailsModal = true;
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling appointment selection");
            await _notificationHelper.ShowErrorAsync("Error displaying mitigation details");
        }
        finally
        {
            _handlingAppointmentClick = false;
        }
    }

    private void OnAppointmentRender(SchedulerAppointmentRenderEventArgs<MitigationSchedulerItem> args)
    {
        try
        {
            // Customize appointment appearance based on mitigation status
            var mitigationItem = args.Data;
            var cssClasses = new List<string>();

            // Base status class
            var statusClass = mitigationItem.Status switch
            {
                "PENDING_APPROVAL" => "mitigation-pending",
                "APPROVED" => "mitigation-approved",
                "IN_PROGRESS_DUE_DATE" => "mitigation-inprogress",
                "COMPLETE" => "mitigation-complete",
                "MONITORING_HAZARD" => "mitigation-monitoring",
                "REJECTED" => "mitigation-rejected",
                _ => "mitigation-pending" // Default to pending
            };
            cssClasses.Add(statusClass);

            // Add overdue class if needed
            if (mitigationItem.IsOverdue)
            {
                cssClasses.Add("overdue");
            }

            // Add high priority class for critical mitigations
            if (mitigationItem.Priority == "High" || mitigationItem.Priority == "Critical")
            {
                cssClasses.Add("high-priority");
            }

            args.Attributes["class"] = string.Join(" ", cssClasses);

            // Set background color based on status for better visibility
            var backgroundColor = mitigationItem.Status switch
            {
                "PENDING_APPROVAL" => "#ffc107",
                "APPROVED" => "#17a2b8",
                "IN_PROGRESS_DUE_DATE" => "#007bff",
                "COMPLETE" => "#28a745",
                "MONITORING_HAZARD" => "#6c757d",
                "REJECTED" => "#dc3545",
                _ => "#ffc107" // Default to pending color
            };

            var textColor = mitigationItem.Status == "PENDING_APPROVAL" ? "#212529" : "white";
            args.Attributes["style"] = $"background: {backgroundColor}; color: {textColor};";

            // Add enhanced tooltip with additional information
            var tooltip = $"Mitigation: {mitigationItem.MitigationCode}\\n" +
                         $"Status: {mitigationItem.Status}\\n" +
                         $"Hazard: {mitigationItem.HazardCode}\\n" +
                         $"Assigned to: {mitigationItem.AssignedTo}\\n" +
                         $"Priority: {mitigationItem.Priority}\\n" +
                         $"Progress: {mitigationItem.Progress}%";

            if (mitigationItem.IsOverdue)
                tooltip += "\\n?? OVERDUE";

            args.Attributes["title"] = tooltip;

            // Add data attributes for better event handling
            args.Attributes["data-mitigation-code"] = mitigationItem.MitigationCode;
            args.Attributes["data-mitigation-id"] = mitigationItem.MitigationId;
            args.Attributes["data-status"] = mitigationItem.Status;

            // Add style to prevent text selection which can interfere with clicking
            var existingStyle = args.Attributes.ContainsKey("style") ? args.Attributes["style"] : "";
            args.Attributes["style"] = $"{existingStyle} user-select: none; -webkit-user-select: none; -moz-user-select: none;";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering appointment");
        }
    }

    private void OnSlotRender(SchedulerSlotRenderEventArgs args)
    {
        try
        {
            // Highlight today in month view
            if (args.View.Text == "Month" && args.Start.Date == DateTime.Today)
            {
                args.Attributes["class"] = "today";
                args.Attributes["style"] = "background: var(--rz-scheduler-today-background-color, rgba(33,46,97,.1));";
            }

            // Highlight working hours (9-18) in week and day views
            if ((args.View.Text == "Week" || args.View.Text == "Day") &&
                args.Start.Hour >= 9 && args.Start.Hour < 18)
            {
                args.Attributes["class"] = "business-hours";
                args.Attributes["style"] = "background: var(--rz-scheduler-highlight-background-color, rgba(255,220,40,.1));";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering slot");
        }
    }

    private async Task OnAppointmentMove(SchedulerAppointmentMoveEventArgs args)
    {
        try
        {
            var draggedAppointment = SchedulerData.FirstOrDefault(x => x == args.Appointment.Data);

            if (draggedAppointment != null)
            {
                _logger.LogInformation("AppointmentMove: Mitigation={MitigationCode} moved to {SlotDate:yyyy-MM-dd HH:mm}", 
                    draggedAppointment.MitigationCode, args.SlotDate);

                var duration = draggedAppointment.End - draggedAppointment.Start;

                if (args.SlotDate.TimeOfDay == TimeSpan.Zero)
                {
                    draggedAppointment.Start = args.SlotDate.Date.Add(draggedAppointment.Start.TimeOfDay);
                }
                else
                {
                    draggedAppointment.Start = args.SlotDate;
                }

                draggedAppointment.End = draggedAppointment.Start.Add(duration);

                // Update the actual mitigation record
                await UpdateMitigationTargetDate(draggedAppointment);

                await scheduler.Reload();
                await _notificationHelper.ShowSuccessAsync($"Mitigation {draggedAppointment.MitigationCode} rescheduled successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving appointment");
            await _notificationHelper.ShowErrorAsync("Error rescheduling mitigation");
        }
    }

    private async Task UpdateMitigationTargetDate(MitigationSchedulerItem appointmentData)
    {
        try
        {
            // Find the actual mitigation
            var mitigation = Mitigations.FirstOrDefault(m => m.Code == appointmentData.MitigationCode);
            if (mitigation != null)
            {
                // Update target date directly
                mitigation.TargetDate = appointmentData.Start;
                mitigation.UpdatedDate = DateTime.UtcNow;

                // Save via CQRS
                var updateCommand = new UpdateMitigationCommand(mitigation);
                var result = await _mediator.SendAsync(updateCommand, CancellationToken.None);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Mitigation {MitigationCode} target date updated successfully", mitigation.Code);
                }
                else
                {
                    _logger.LogError("Failed to update mitigation target date: {Error}", result.Error?.Message);
                    await _notificationHelper.ShowErrorAsync("Failed to save mitigation changes");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating mitigation target date");
        }
    }
    #endregion

    #region _navigation and Actions
    private async Task GoToToday()
    {
        try
        {
            if (scheduler != null)
            {
                scheduler.CurrentDate = DateTime.Today;
                await scheduler.Reload();
                _logger.LogInformation("Navigated to today's date");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to today");
        }
    }

    private async Task ShowCreateMitigationDialog(DateTime? selectedStart = null, DateTime? selectedEnd = null)
    {
        try
        {
            var targetDate = selectedStart ?? DateTime.Now;
            _logger.LogInformation("Opening create mitigation dialog for {TargetDate:yyyy-MM-dd}", targetDate);

            // For now, navigate to mitigation creation page
            // TODO: Implement CreateMitigationDialog similar to CreateInterviewDialog
            _navigation.NavigateToSecure("/Listings/Mitigations");
            await _notificationHelper.ShowSuccessAsync("Navigate to Mitigations page to create new mitigation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing create mitigation dialog");
            await _notificationHelper.ShowErrorAsync("Error opening create mitigation dialog");
        }
    }

    private async Task ShowMitigationDetails(MitigationSchedulerItem mitigationItem)
    {
        _logger.LogInformation("View mitigation details requested from calendar: {MitigationCode}", mitigationItem.MitigationCode);

        try
        {
            // Prevent multiple rapid calls
            if (IsLoading || ShowDetailsModal)
            {
                return;
            }

            IsLoading = true;
            StateHasChanged();

            // Add a small delay to let the scheduler settle
            await Task.Delay(50);

            // Get detailed mitigation information
            var mitigationQuery = new GetMitigationByCodeQuery(new MitigationID(mitigationItem.MitigationCode));
            var mitigationResult = await _mediator.SendAsync(mitigationQuery, CancellationToken.None);

            if (mitigationResult.IsSuccess && mitigationResult.Value != null)
            {
                SelectedMitigation = mitigationResult.Value;
            }
            else
            {
                // Find the mitigation from the loaded mitigations as fallback
                SelectedMitigation = Mitigations.FirstOrDefault(m => m.Code == mitigationItem.MitigationCode);
                if (SelectedMitigation == null)
                {
                    await _notificationHelper.ShowErrorAsync($"Mitigation {mitigationItem.MitigationCode} not found");
                    return;
                }
            }

            // Show the details modal
            ShowDetailsModal = true;

            _logger.LogInformation("Displaying details for mitigation: {MitigationCode}", mitigationItem.MitigationCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing mitigation details for {MitigationCode}", mitigationItem.MitigationCode);
            await _notificationHelper.ShowErrorAsync("Error opening mitigation details");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }
    #endregion

    #region Statistics Methods
    private int GetMitigationsThisMonth()
    {
        var now = DateTime.Now;
        return Mitigations.Count(m => m.TargetDate?.Year == now.Year && m.TargetDate?.Month == now.Month);
    }

    private int GetMitigationsThisWeek()
    {
        var now = DateTime.Now;
        var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(7);

        return Mitigations.Count(m => m.TargetDate >= startOfWeek && m.TargetDate < endOfWeek);
    }

    private int GetOverdueMitigations()
    {
        var today = DateTime.Today;
        return Mitigations.Count(m => m.TargetDate.HasValue && m.TargetDate.Value.Date < today && m.Status != "COMPLETE");
    }
    #endregion

    #region Modal Management Methods
    /// <summary>
    /// Prevent background clicks when modal is open
    /// </summary>
    private void PreventBackgroundClick()
    {
        // This method intentionally does nothing to prevent background clicks
    }

    /// <summary>
    /// Close the details modal
    /// </summary>
    public void CloseDetailsModal()
    {
        try
        {
            ShowDetailsModal = false;
            SelectedMitigation = null;
            _logger.LogInformation("Details modal closed");

            // Force a brief delay before state change to prevent focus issues
            InvokeAsync(async () =>
            {
                await Task.Delay(100);
                StateHasChanged();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing details modal");
        }
    }

    /// <summary>
    /// Edit mitigation from details modal
    /// </summary>
    /// <param name="mitigation">Mitigation to edit</param>
    public async Task EditFromDetailsModal(Mitigation mitigation)
    {
        try
        {
            _logger.LogInformation("Edit requested for mitigation: {Code}", mitigation.Code);
            CloseDetailsModal();

            // Add a small delay to ensure modal is fully closed
            await Task.Delay(200);

            // For now, navigate to mitigations listing
            // TODO: Implement EditMitigationDialog similar to EditInterviewDialog
            _navigation.NavigateToSecure("/Listings/Mitigations");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing from details modal");
            await _notificationHelper.ShowErrorAsync("Error opening edit dialog");
        }
    }
    #endregion

    #region UI Helpers
    private BadgeStyle GetStatusBadgeStyle(string status)
    {
        return status switch
        {
            "PENDING_APPROVAL" => BadgeStyle.Warning,
            "APPROVED" => BadgeStyle.Info,
            "IN_PROGRESS_DUE_DATE" => BadgeStyle.Primary,
            "COMPLETE" => BadgeStyle.Success,
            "MONITORING_HAZARD" => BadgeStyle.Light,
            "REJECTED" => BadgeStyle.Danger,
            "On Hold" => BadgeStyle.Secondary,
            _ => BadgeStyle.Light
        };
    }

    private BadgeStyle GetProgressBadgeStyle(int progress)
    {
        return progress switch
        {
            >= 100 => BadgeStyle.Success,
            >= 75 => BadgeStyle.Info,
            >= 50 => BadgeStyle.Primary,
            >= 25 => BadgeStyle.Warning,
            _ => BadgeStyle.Light
        };
    }
    #endregion
}

#region Supporting Classes
/// <summary>
/// Enhanced scheduler item representation of a Mitigation for calendar display
/// </summary>
public class MitigationSchedulerItem
{
    public string MitigationId { get; set; } = string.Empty;
    public string MitigationCode { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string Status { get; set; } = string.Empty;
    public string HazardCode { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int Progress { get; set; }
    public bool IsOverdue { get; set; }
    public string Description { get; set; } = string.Empty;
}
#endregion
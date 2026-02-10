using SMS3.Components.Pages.SMSAssurance.Components;

namespace SMS3.Components.Pages.SMSRiskManagement;

public partial class MitigationCalendar : ComponentBase
{
    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<MitigationCalendar> Logger { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private AuthenticationService AuthService { get; set; } = default!;
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

            Logger.LogInformation("Loading mitigations for calendar display");

            var query = new GetAllMitigationsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                Mitigations = result.Value.ToList();
                Logger.LogInformation("Loaded {Count} mitigations for calendar", Mitigations.Count);

                // Convert mitigations to scheduler items
                SchedulerData = Mitigations.Select(MapMitigationToSchedulerItem).ToList();
            }
            else
            {
                Logger.LogError("Failed to load mitigations: {Error}", result.Error?.Message);
                ShowErrorNotification("Failed to load mitigations for calendar");
                Mitigations = new List<Mitigation>();
                SchedulerData = new List<MitigationSchedulerItem>();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading mitigations for calendar");
            ShowErrorNotification("Error loading mitigations");
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
        Logger.LogInformation("Refreshing mitigation calendar data...");
        await LoadMitigationsAsync();

        // Reload the scheduler
        if (scheduler != null)
        {
            await scheduler.Reload();
        }

        ShowSuccessNotification("Calendar data refreshed");
        Logger.LogInformation("Mitigation calendar refresh completed");
    }
    #endregion

    #region Data Mapping
    private MitigationSchedulerItem MapMitigationToSchedulerItem(Mitigation mitigation)
    {
        var targetDate = mitigation.TargetDate ?? mitigation.CreatedDate ?? DateTime.Now;
        var completionDate = mitigation.CompletionDate;
        
        // Use completion date if available and in the past, otherwise use target date
        var startDate = completionDate ?? targetDate;
        var endDate = completionDate?.AddHours(1) ?? targetDate.AddDays(1); // Show completed items as 1 hour blocks, targets as all-day

        return new MitigationSchedulerItem
        {
            MitigationId = mitigation.Id?.Value ?? "",
            MitigationCode = mitigation.Code ?? "Unknown",
            Text = $"Mitigation: {mitigation.Code} - {mitigation.Name}",
            Start = startDate,
            End = endDate,
            Status = mitigation.Status ?? "PENDING_APPROVAL",
            HazardCode = mitigation.HazardCode ?? "Unknown",
            AssignedTo = mitigation.AssignedTo ?? "Unassigned",
            Progress = mitigation.Progress,
            IsOverdue = mitigation.TargetDate.HasValue && mitigation.TargetDate < DateTime.Today && mitigation.Status != "COMPLETE",
            Description = GetMitigationDescription(mitigation)
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
            Logger.LogInformation("Slot selected: {Start} to {End}", args.Start, args.End);

            // Don't create appointments in year view
            if (args.View.Text != "Year")
            {
                await ShowCreateMitigationDialog(args.Start, args.End);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling slot selection");
        }
    }

    private async Task OnAppointmentSelect(SchedulerAppointmentSelectEventArgs<MitigationSchedulerItem> args)
    {
        try
        {
            // Prevent multiple rapid clicks and modal interference
            if (_handlingAppointmentClick || ShowDetailsModal || IsLoading)
            {
                return;
            }

            _handlingAppointmentClick = true;
            var mitigationItem = args.Data;
            Logger.LogInformation("Mitigation appointment selected: {MitigationCode}", mitigationItem.MitigationCode);

            // Use a background task to handle the click without blocking the UI thread
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(150); // Give the scheduler time to complete its focus operations
                    await InvokeAsync(async () =>
                    {
                        await ShowMitigationDetails(mitigationItem);
                    });
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error in background appointment handling");
                    await InvokeAsync(() =>
                    {
                        ShowErrorNotification("Error opening mitigation details");
                    });
                }
                finally
                {
                    _handlingAppointmentClick = false;
                }
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling appointment selection");
            ShowErrorNotification("Error opening mitigation details");
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
            Logger.LogError(ex, "Error rendering appointment");
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
            Logger.LogError(ex, "Error rendering slot");
        }
    }

    private async Task OnAppointmentMove(SchedulerAppointmentMoveEventArgs args)
    {
        try
        {
            var draggedAppointment = SchedulerData.FirstOrDefault(x => x == args.Appointment.Data);

            if (draggedAppointment != null)
            {
                Logger.LogInformation("AppointmentMove: Mitigation={MitigationCode} moved to {SlotDate:yyyy-MM-dd HH:mm}", 
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
                ShowSuccessNotification($"Mitigation {draggedAppointment.MitigationCode} rescheduled successfully");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error moving appointment");
            ShowErrorNotification("Error rescheduling mitigation");
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
                var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

                if (result.IsSuccess)
                {
                    Logger.LogInformation("Mitigation {MitigationCode} target date updated successfully", mitigation.Code);
                }
                else
                {
                    Logger.LogError("Failed to update mitigation target date: {Error}", result.Error?.Message);
                    ShowErrorNotification("Failed to save mitigation changes");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating mitigation target date");
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
                scheduler.CurrentDate = DateTime.Today;
                await scheduler.Reload();
                Logger.LogInformation("Navigated to today's date");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error navigating to today");
        }
    }

    private async Task ShowCreateMitigationDialog(DateTime? selectedStart = null, DateTime? selectedEnd = null)
    {
        try
        {
            var targetDate = selectedStart ?? DateTime.Now;
            Logger.LogInformation("Opening create mitigation dialog for {TargetDate:yyyy-MM-dd}", targetDate);

            // For now, navigate to mitigation creation page
            // TODO: Implement CreateMitigationDialog similar to CreateInterviewDialog
            Navigation.NavigateTo("/Listings/Mitigations");
            ShowSuccessNotification("Navigate to Mitigations page to create new mitigation");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error showing create mitigation dialog");
            ShowErrorNotification("Error opening create mitigation dialog");
        }
    }

    private async Task ShowMitigationDetails(MitigationSchedulerItem mitigationItem)
    {
        Logger.LogInformation("View mitigation details requested from calendar: {MitigationCode}", mitigationItem.MitigationCode);

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
            var mitigationResult = await Mediator.SendAsync(mitigationQuery, CancellationToken.None);

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
                    ShowErrorNotification($"Mitigation {mitigationItem.MitigationCode} not found");
                    return;
                }
            }

            // Show the details modal
            ShowDetailsModal = true;

            Logger.LogInformation("Displaying details for mitigation: {MitigationCode}", mitigationItem.MitigationCode);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error showing mitigation details for {MitigationCode}", mitigationItem.MitigationCode);
            ShowErrorNotification("Error opening mitigation details");
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
            Logger.LogInformation("Details modal closed");

            // Force a brief delay before state change to prevent focus issues
            InvokeAsync(async () =>
            {
                await Task.Delay(100);
                StateHasChanged();
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error closing details modal");
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
            Logger.LogInformation("Edit requested for mitigation: {Code}", mitigation.Code);
            CloseDetailsModal();

            // Add a small delay to ensure modal is fully closed
            await Task.Delay(200);

            // For now, navigate to mitigations listing
            // TODO: Implement EditMitigationDialog similar to EditInterviewDialog
            Navigation.NavigateTo("/Listings/Mitigations");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error editing from details modal");
            ShowErrorNotification("Error opening edit dialog");
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
            _ => BadgeStyle.Secondary
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
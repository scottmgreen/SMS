using SMS3.Components.Pages.SMSAssurance.Components;

namespace SMS3.Components.Pages.SMSRiskManagement;

public partial class InterviewCalendar : ComponentBase
{
    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<InterviewCalendar> Logger { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    #endregion

    #region Component State
    private bool IsLoading { get; set; } = true;
    private bool _isLoadingData = false; // Prevent recursive loading
    private bool _handlingAppointmentClick = false; // Prevent multiple appointment clicks
    private RadzenScheduler<InterviewSchedulerItem> scheduler = default!;
    private EventConsole? console;
    private List<Interview> Interviews { get; set; } = new();
    private List<InterviewSchedulerItem> SchedulerData { get; set; } = new();
    private Interview? SelectedInterview { get; set; }

    // Enhanced UI state properties
    public bool ShowDetailsModal { get; set; } = false;
    private bool showHeader = true;
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        await LoadInterviewsAsync();
    }
    #endregion

    #region Data Loading
    private async Task LoadInterviewsAsync()
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

            Logger.LogInformation("Loading interviews for calendar display");

            var query = new GetAllInterviewsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                Interviews = result.Value.ToList();
                Logger.LogInformation("Loaded {Count} interviews for calendar", Interviews.Count);

                // Convert interviews to scheduler items
                SchedulerData = Interviews.Select(MapInterviewToSchedulerItem).ToList();
            }
            else
            {
                Logger.LogError("Failed to load interviews: {Error}", result.Error?.Message);
                ShowErrorNotification("Failed to load interviews for calendar");
                Interviews = new List<Interview>();
                SchedulerData = new List<InterviewSchedulerItem>();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading interviews for calendar");
            ShowErrorNotification("Error loading interviews");
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
        LogEvent("Refreshing calendar data...");
        await LoadInterviewsAsync();

        // Reload the scheduler
        if (scheduler != null)
        {
            await scheduler.Reload();
        }

        ShowSuccessNotification("Calendar data refreshed");
        LogEvent("Calendar refresh completed");
    }
    #endregion

    #region Data Mapping
    private InterviewSchedulerItem MapInterviewToSchedulerItem(Interview interview)
    {
        var interviewDate = interview.InterviewDate ?? interview.CreatedDate ?? DateTime.Now;
        var duration = interview.DurationMinutes ?? 60; // Default 1 hour

        return new InterviewSchedulerItem
        {
            InterviewId = interview.Id?.Value ?? "",
            InterviewCode = interview.Code ?? "Unknown",
            Text = $"Interview: {interview.Code} - {interview.PersonInterviewed}",
            Start = interviewDate,
            End = interviewDate.AddMinutes(duration),
            InterviewType = interview.Type,
            InterviewStatus = interview.Status,
            PersonInterviewed = interview.PersonInterviewed,
            InvestigationCode = interview.InvestigationCode,
            Investigator = interview.SMSInvestigatorCode ?? "Unknown",
            Location = interview.InterviewLocation ?? "TBD",
            IsConfidential = interview.IsConfidential,
            Description = GetInterviewDescription(interview)
        };
    }

    private string GetInterviewDescription(Interview interview)
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(interview.PersonInterviewedRole))
            parts.Add($"Role: {interview.PersonInterviewedRole}");

        if (!string.IsNullOrEmpty(interview.InterviewLocation))
            parts.Add($"Location: {interview.InterviewLocation}");

        if (interview.IsConfidential)
            parts.Add("CONFIDENTIAL");

        return string.Join(" | ", parts);
    }
    #endregion

    #region Enhanced Scheduler Event Handlers
    private async Task OnSlotSelect(SchedulerSlotSelectEventArgs args)
    {
        try
        {
            Logger.LogInformation("Slot selected: {Start} to {End}", args.Start, args.End);

            // Don't create appointments in year view (like Radzen example)
            if (args.View.Text != "Year")
            {
                await ShowCreateInterviewDialog(args.Start, args.End);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling slot selection");
        }
    }

    private async Task OnAppointmentSelect(SchedulerAppointmentSelectEventArgs<InterviewSchedulerItem> args)
    {
        try
        {
            // Prevent multiple rapid clicks and modal interference
            if (_handlingAppointmentClick || ShowDetailsModal || IsLoading)
            {
                return;
            }

            _handlingAppointmentClick = true;
            var interviewItem = args.Data;
            Logger.LogInformation("Interview appointment selected: {InterviewCode}", interviewItem.InterviewCode);

            // Use a background task to handle the click without blocking the UI thread
            // This prevents the scheduler from trying to manage focus on elements we're about to change
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(150); // Give the scheduler time to complete its focus operations
                    await InvokeAsync(async () =>
                    {
                        await ShowInterviewDetails(interviewItem);
                    });
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error in background appointment handling");
                    await InvokeAsync(() =>
                    {
                        ShowErrorNotification("Error opening interview details");
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
            ShowErrorNotification("Error opening interview details");
            _handlingAppointmentClick = false;
        }
    }

    private void OnAppointmentRender(SchedulerAppointmentRenderEventArgs<InterviewSchedulerItem> args)
    {
        try
        {
            // Never call StateHasChanged in AppointmentRender - would lead to infinite loop (from Radzen sample)

            // Customize appointment appearance based on interview status
            var interviewItem = args.Data;

            var cssClasses = new List<string>();

            // Base status class
            var statusClass = interviewItem.InterviewStatus.Value switch
            {
                "SCHEDULED" => "interview-scheduled",
                "IN_PROGRESS" => "interview-inprogress",
                "COMPLETED" => "interview-completed",
                "CANCELLED" => "interview-cancelled",
                _ => "interview-scheduled" // Default to scheduled
            };
            cssClasses.Add(statusClass);

            // Add confidential class if needed
            if (interviewItem.IsConfidential)
            {
                cssClasses.Add("confidential");
            }

            // Add high priority class for urgent interviews
            if (interviewItem.InterviewType.Value == "WITNESS" &&
                interviewItem.InterviewStatus.Value == "SCHEDULED" &&
                interviewItem.Start.Date == DateTime.Today)
            {
                cssClasses.Add("high-priority");
            }

            args.Attributes["class"] = string.Join(" ", cssClasses);

            // Set background color based on status for better visibility
            var backgroundColor = interviewItem.InterviewStatus.Value switch
            {
                "SCHEDULED" => "#17a2b8",
                "IN_PROGRESS" => "#ffc107",
                "COMPLETED" => "#28a745",
                "CANCELLED" => "#dc3545",
                _ => "#17a2b8" // Default to scheduled color
            };

            args.Attributes["style"] = $"background: {backgroundColor}; color: white;";

            // Add enhanced tooltip with additional information
            var tooltip = $"Interview: {interviewItem.InterviewCode}\\n" +
                         $"Person: {interviewItem.PersonInterviewed}\\n" +
                         $"Type: {interviewItem.InterviewType.Name}\\n" +
                         $"Status: {interviewItem.InterviewStatus.Name}\\n" +
                         $"Investigation: {interviewItem.InvestigationCode}\\n" +
                         $"Investigator: {interviewItem.Investigator}\\n" +
                         $"Location: {interviewItem.Location}";

            if (interviewItem.IsConfidential)
                tooltip += "\\n🔒 CONFIDENTIAL";

            args.Attributes["title"] = tooltip;

            // Add data attributes for better event handling
            args.Attributes["data-interview-code"] = interviewItem.InterviewCode;
            args.Attributes["data-interview-id"] = interviewItem.InterviewId;
            args.Attributes["data-status"] = interviewItem.InterviewStatus.Value;

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

    // NEW: Appointment move handler (from Radzen example)
    private async Task OnAppointmentMove(SchedulerAppointmentMoveEventArgs args)
    {
        try
        {
            var draggedAppointment = SchedulerData.FirstOrDefault(x => x == args.Appointment.Data);

            if (draggedAppointment != null)
            {
                LogEvent($"AppointmentMove: Interview={draggedAppointment.InterviewCode} moved to {args.SlotDate:yyyy-MM-dd HH:mm}");

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

                // Update the actual interview record
                await UpdateInterviewDateTime(draggedAppointment);

                await scheduler.Reload();
                ShowSuccessNotification($"Interview {draggedAppointment.InterviewCode} rescheduled successfully");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error moving appointment");
            ShowErrorNotification("Error rescheduling interview");
        }
    }

    private async Task UpdateInterviewDateTime(InterviewSchedulerItem appointmentData)
    {
        try
        {
            // Find the actual interview
            var interview = Interviews.FirstOrDefault(i => i.Code == appointmentData.InterviewCode);
            if (interview != null)
            {
                // Update duration if needed
                var newDuration = (int)(appointmentData.End - appointmentData.Start).TotalMinutes;

                // Use the domain method to update date/time which will handle status transitions
                var updateResult = interview.UpdateDateTime(appointmentData.Start, newDuration);

                if (updateResult.IsFailure)
                {
                    Logger.LogError("Domain validation failed for interview datetime update: {Error}", updateResult.Error?.Message);
                    ShowErrorNotification($"Cannot reschedule interview: {updateResult.Error?.Message}");
                    return;
                }

                // Save via CQRS
                var updateCommand = new UpdateInterviewCommand(interview);
                var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

                if (result.IsSuccess)
                {
                    Logger.LogInformation("Interview {InterviewCode} datetime updated successfully", interview.Code);
                    LogEvent($"Interview {interview.Code} rescheduled and status updated to: {interview.Status.Name}");
                }
                else
                {
                    Logger.LogError("Failed to update interview datetime: {Error}", result.Error?.Message);
                    ShowErrorNotification("Failed to save interview changes");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating interview datetime");
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
                LogEvent("Navigated to today's date");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error navigating to today");
            LogEvent($"Error navigating to today: {ex.Message}");
        }
    }

    // Enhanced create interview dialog with preset times
    private async Task ShowCreateInterviewDialog(DateTime? selectedStart = null, DateTime? selectedEnd = null)
    {
        try
        {
            var startTime = selectedStart ?? DateTime.Now;
            var endTime = selectedEnd ?? startTime.AddHours(1);

            LogEvent($"Opening create interview dialog for {startTime:yyyy-MM-dd HH:mm}");

            var options = new DialogOptions()
            {
                Width = "100%",
                Height = "100%",
                Resizable = false,
                Draggable = false,
                CloseDialogOnOverlayClick = true,
                CloseDialogOnEsc = true,
                ShowTitle = false,
                ShowClose = false,
                CssClass = "custom-modal-dialog"
            };

            var parameters = new Dictionary<string, object>
            {
                { "InvestigationCode", "UNKNOWN" }, // Default - user can change
                { "PresetDateTime", startTime },
                { "PresetEndDateTime", endTime }
            };

            var result = await DialogService.OpenAsync<Components.CreateInterviewDialog>(
                "", // No title since our custom modal has its own header
                parameters,
                options);

            if (result == true)
            {
                await RefreshData();
                ShowSuccessNotification("Interview scheduled successfully");
                LogEvent("New interview created successfully");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error showing create interview dialog");
            ShowErrorNotification("Error opening create interview dialog");
            LogEvent($"Error in create dialog: {ex.Message}");
        }
    }

    private async Task ShowInterviewDetails(InterviewSchedulerItem interviewItem)
    {
        Logger.LogInformation("View interview details requested from calendar: {InterviewCode}", interviewItem.InterviewCode);

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

            // Get detailed interview information
            var interviewQuery = new GetInterviewByCodeQuery(new InterviewID(interviewItem.InterviewCode));
            var interviewResult = await Mediator.SendAsync(interviewQuery, CancellationToken.None);

            if (interviewResult.IsSuccess && interviewResult.Value != null)
            {
                SelectedInterview = interviewResult.Value;
            }
            else
            {
                // Find the interview from the loaded interviews as fallback
                SelectedInterview = Interviews.FirstOrDefault(i => i.Code == interviewItem.InterviewCode);
                if (SelectedInterview == null)
                {
                    ShowErrorNotification($"Interview {interviewItem.InterviewCode} not found");
                    return;
                }
            }

            // Show the details modal
            ShowDetailsModal = true;

            Logger.LogInformation("Displaying details for interview: {InterviewCode}", interviewItem.InterviewCode);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error showing interview details for {InterviewCode}", interviewItem.InterviewCode);
            ShowErrorNotification("Error opening interview details");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }
    #endregion

    #region Statistics Methods
    private int GetInterviewsThisMonth()
    {
        var now = DateTime.Now;
        return Interviews.Count(i => i.InterviewDate?.Year == now.Year && i.InterviewDate?.Month == now.Month);
    }

    private int GetInterviewsThisWeek()
    {
        var now = DateTime.Now;
        var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(7);

        return Interviews.Count(i => i.InterviewDate >= startOfWeek && i.InterviewDate < endOfWeek);
    }

    private int GetInterviewsToday()
    {
        var today = DateTime.Today;
        return Interviews.Count(i => i.InterviewDate?.Date == today);
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
        // The modal should only be closed via the close button
    }

    /// <summary>
    /// Close the details modal
    /// </summary>
    public void CloseDetailsModal()
    {
        try
        {
            ShowDetailsModal = false;
            SelectedInterview = null;
            LogEvent("Details modal closed");

            // Force a brief delay before state change to prevent focus issues
            InvokeAsync(async () =>
            {
                await Task.Delay(100); // Small delay to let Radzen cleanup
                StateHasChanged();
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error closing details modal");
            LogEvent($"Error closing modal: {ex.Message}");
        }
    }

    /// <summary>
    /// Edit interview from details modal
    /// </summary>
    /// <param name="interview">Interview to edit</param>
    public async Task EditFromDetailsModal(Interview interview)
    {
        try
        {
            LogEvent($"Edit requested for interview: {interview.Code}");
            CloseDetailsModal();

            // Add a small delay to ensure modal is fully closed
            await Task.Delay(200);

            await OnEditInterviewAsync(interview);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error editing from details modal");
            ShowErrorNotification("Error opening edit dialog");
            LogEvent($"Error opening edit dialog: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle edit interview request - Show edit dialog
    /// </summary>
    /// <param name="interview">Interview to edit</param>
    public async Task OnEditInterviewAsync(Interview interview)
    {
        Logger.LogInformation("Edit interview requested: {InterviewCode}", interview.Code);
        LogEvent($"Opening edit dialog for: {interview.Code}");

        try
        {
            var options = new DialogOptions()
            {
                Width = "100%",
                Height = "100%",
                Resizable = false,
                Draggable = false,
                CloseDialogOnOverlayClick = true,
                CloseDialogOnEsc = true,
                ShowTitle = false,
                ShowClose = false,
                CssClass = "custom-modal-dialog"
            };

            var parameters = new Dictionary<string, object>
            {
                { "Interview", interview }
            };

            var result = await DialogService.OpenAsync<Components.EditInterviewDialog>(
                "", // No title since our custom modal has its own header
                parameters,
                options);

            if (result == true)
            {
                await RefreshData();
                ShowSuccessNotification("Interview updated successfully");
                LogEvent($"Interview {interview.Code} updated successfully");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening edit dialog for interview {InterviewCode}", interview.Code);
            ShowErrorNotification("Failed to open edit dialog");
            LogEvent($"Error opening edit dialog: {ex.Message}");
        }
    }
    #endregion

    #region UI Helpers
    private BadgeStyle GetStatusBadgeStyle(InterviewStatus status)
    {
        if (status.Equals(InterviewStatus.Scheduled))
            return BadgeStyle.Info;
        if (status.Equals(InterviewStatus.InProgress))
            return BadgeStyle.Warning;
        if (status.Equals(InterviewStatus.Completed))
            return BadgeStyle.Success;
        if (status.Equals(InterviewStatus.Cancelled))
            return BadgeStyle.Danger;

        return BadgeStyle.Light;
    }
    #endregion

    #region Event Logging (Debug Helper)
    private void LogEvent(string message)
    {
        // Use the EventConsole for consistent logging like AuditCalendar
        console?.Log(message);
    }
    #endregion
}

#region Supporting Classes and Enums
/// <summary>
/// Enhanced scheduler item representation of an Interview for calendar display
/// </summary>
public class InterviewSchedulerItem
{
    public string InterviewId { get; set; } = string.Empty;
    public string InterviewCode { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public InterviewType InterviewType { get; set; }
    public InterviewStatus InterviewStatus { get; set; }
    public string PersonInterviewed { get; set; } = string.Empty;
    public string InvestigationCode { get; set; } = string.Empty;
    public string Investigator { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsConfidential { get; set; }
    public string Description { get; set; } = string.Empty;
}
#endregion
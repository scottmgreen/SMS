
using SMS3.Components.Pages.SMSAssurance.Components;
using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.SMSAssurance;

public partial class AuditCalendar : ComponentBase
{
    #region Injected Services
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<AuditCalendar> _logger { get; set; } = default!;
    [Inject] private INotificationHelper _notificationHelper { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;
    #endregion

    #region Component State - Matching Radzen sample
    private RadzenScheduler<AuditSchedulerItem> scheduler = default!;
    private EventConsole? console;
    private bool IsLoading { get; set; } = true;
    private bool showHeader = true;
    private List<SMSAuditPlan> AuditPlans { get; set; } = new();
    private IList<AuditSchedulerItem> SchedulerData { get; set; } = new List<AuditSchedulerItem>();
    #endregion

    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        await LoadAuditPlansAsync();
    }
    #endregion

    #region Data Loading
    private async Task LoadAuditPlansAsync()
    {
        try
        {
            IsLoading = true;
            StateHasChanged();

            _logger.LogInformation("Loading audit plans for calendar display");

            var query = new GetAllSMSAuditPlansQuery();
            var result = await _mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value is not null)
            {
                AuditPlans = result.Value.ToList();
                _logger.LogInformation("Loaded {Count} audit plans for calendar", AuditPlans.Count);

                // Convert audit plans to scheduler items - matching Radzen sample pattern
                var auditItems = new List<AuditSchedulerItem>();
                foreach (var plan in AuditPlans)
                {
                    auditItems.Add(MapAuditPlanToSchedulerItem(plan));
                }
                SchedulerData = auditItems;
            }
            else
            {
                _logger.LogError("Failed to load audit plans: {Error}", result.Error?.Message);
                await _notificationHelper.ShowErrorAsync("Failed to load audit plans for calendar");
                AuditPlans = new List<SMSAuditPlan>();
                SchedulerData = new List<AuditSchedulerItem>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading audit plans for calendar");
            await _notificationHelper.ShowErrorAsync("Error loading audit plans");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task RefreshData()
    {
        console?.Log("Refreshing calendar data...");
        await LoadAuditPlansAsync();

        // Reload the scheduler
        if (scheduler is not null)
        {
            await scheduler.Reload();
        }

        await _notificationHelper.ShowSuccessAsync("Calendar data refreshed");
        console?.Log("Calendar refresh completed");
    }
    #endregion

    #region Data Mapping
    private AuditSchedulerItem MapAuditPlanToSchedulerItem(SMSAuditPlan auditPlan)
    {
        var startDate = auditPlan.PlannedStartDate;
        var endDate = auditPlan.PlannedEndDate;

        // For multi-day audits, show the full span
        // For single-day audits, use the estimated duration or default to 8 hours
        if (startDate.Date == endDate.Date && auditPlan.EstimatedDurationHours > 0)
        {
            endDate = startDate.AddHours(auditPlan.EstimatedDurationHours);
        }

        return new AuditSchedulerItem
        {
            AuditPlanId = auditPlan.Id?.Value ?? "",
            AuditPlanCode = auditPlan.Code ?? "Unknown",
            Text = $"{auditPlan.AuditType}: {auditPlan.Name}",
            Start = startDate,
            End = endDate,
            AuditType = auditPlan.AuditType,
            Status = auditPlan.Status,
            Priority = auditPlan.Priority,
            LeadAuditor = auditPlan.LeadAuditor,
            ResponsibleDepartment = auditPlan.ResponsibleDepartment,
            EstimatedHours = auditPlan.EstimatedDurationHours,
            RequiresApproval = auditPlan.RequiresApproval,
            Description = GetAuditDescription(auditPlan)
        };
    }

    private string GetAuditDescription(SMSAuditPlan auditPlan)
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(auditPlan.LeadAuditor))
            parts.Add($"Lead: {auditPlan.LeadAuditor}");

        if (!string.IsNullOrEmpty(auditPlan.ResponsibleDepartment))
            parts.Add($"Dept: {auditPlan.ResponsibleDepartment}");

        if (auditPlan.EstimatedDurationHours > 0)
            parts.Add($"{auditPlan.EstimatedDurationHours}h");

        return string.Join(" | ", parts);
    }
    #endregion

    #region Scheduler Event Handlers - Matching Radzen sample
    void OnSlotRender(SchedulerSlotRenderEventArgs args)
    {
        try
        {
            // Highlight today in month view
            if (args.View.Text == "Month" && args.Start.Date == DateTime.Today)
            {
                args.Attributes["style"] = "background: var(--rz-scheduler-highlight-background-color, rgba(255,220,40,.2));";
            }

            // Highlight working hours (9-17) in week and day views
            if ((args.View.Text == "Week" || args.View.Text == "Day") && args.Start.Hour > 8 && args.Start.Hour < 18)
            {
                args.Attributes["style"] = "background: var(--rz-scheduler-highlight-background-color, rgba(255,220,40,.2));";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering slot");
            console?.Log($"Error in SlotRender: {ex.Message}");
        }
    }

    async Task OnSlotSelect(SchedulerSlotSelectEventArgs args)
    {
        try
        {
            console?.Log($"SlotSelect: Start={args.Start:yyyy-MM-dd HH:mm} End={args.End:yyyy-MM-dd HH:mm}");
            _logger.LogInformation("Slot selected: {Start} to {End}", args.Start, args.End);

            // Don't create appointments in year view (like Radzen sample)
            if (args.View.Text != "Year")
            {
                var newAuditPlan = new SMSAuditPlan(new SMSAuditPlanID("AP-0000"), "CURRENT_USER")
                {
                    PlannedStartDate = args.Start,
                    PlannedEndDate = args.End
                };

                var data = await _dialogService.OpenAsync<AuditPlanDialog>("Add Audit Plan",
                    new Dictionary<string, object>
                    {
                        { "AuditPlan", newAuditPlan },
                        { "IsNew", true }
                    },
                    new DialogOptions() { Width = "1200px", Height = "900px", Resizable = true });

                if (data == true)
                {
                    await LoadAuditPlansAsync();
                    // Either call the Reload method or reassign the Data property of the Scheduler
                    await scheduler.Reload();
                    await _notificationHelper.ShowSuccessAsync("Audit plan created successfully");
                    console?.Log("New audit plan created successfully");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling slot selection");
            console?.Log($"Error in SlotSelect: {ex.Message}");
        }
    }

    async Task OnAppointmentSelect(SchedulerAppointmentSelectEventArgs<AuditSchedulerItem> args)
    {
        try
        {
            console?.Log($"AppointmentSelect: AuditPlan={args.Data.AuditPlanCode}");
            _logger.LogInformation("Audit appointment selected: {AuditCode}", args.Data.AuditPlanCode);

            // Find the actual audit plan
            var auditPlan = AuditPlans.FirstOrDefault(a => a.Code == args.Data.AuditPlanCode);
            if (auditPlan is not null)
            {
                // Create a copy for editing like Radzen sample
                var copy = new SMSAuditPlan(new SMSAuditPlanID(auditPlan.Code), auditPlan.CreatedBy ?? "System")
                {
                    Code = auditPlan.Code,
                    Name = auditPlan.Name,
                    Description = auditPlan.Description,
                    AuditType = auditPlan.AuditType,
                    Scope = auditPlan.Scope,
                    Objectives = auditPlan.Objectives,
                    PlannedStartDate = auditPlan.PlannedStartDate,
                    PlannedEndDate = auditPlan.PlannedEndDate,
                    LeadAuditor = auditPlan.LeadAuditor,
                    ResponsibleDepartment = auditPlan.ResponsibleDepartment,
                    Status = auditPlan.Status,
                    Priority = auditPlan.Priority,
                    EstimatedDurationHours = auditPlan.EstimatedDurationHours,
                    RequiresApproval = auditPlan.RequiresApproval,
                    ApprovedBy = auditPlan.ApprovedBy,
                    ApprovedDate = auditPlan.ApprovedDate,
                    CreatedDate = auditPlan.CreatedDate
                };

                var data = await _dialogService.OpenAsync<AuditPlanDialog>("Edit Audit Plan",
                    new Dictionary<string, object>
                    {
                        { "AuditPlan", copy },
                        { "IsNew", false }
                    },
                    new DialogOptions() { Width = "1200px", Height = "900px", Resizable = true });

                if (data == true)
                {
                    // Reload the data and scheduler
                    await LoadAuditPlansAsync();
                    await scheduler.Reload();
                    await _notificationHelper.ShowSuccessAsync("Audit plan updated successfully");
                    console?.Log($"Audit plan {auditPlan.Code} updated successfully");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling appointment selection");
            await _notificationHelper.ShowErrorAsync("Error opening audit plan details");
            console?.Log($"Error in AppointmentSelect: {ex.Message}");
        }
    }

    void OnAppointmentRender(SchedulerAppointmentRenderEventArgs<AuditSchedulerItem> args)
    {
        try
        {
            // Never call StateHasChanged in AppointmentRender - would lead to infinite loop (from Radzen sample)

            // Customize appointment appearance based on audit status
            var auditItem = args.Data;
            var backgroundColor = auditItem.Status?.ToLower() switch
            {
                "draft" => "#6c757d",
                "approved" => "#28a745",
                "scheduled" => "#17a2b8",
                "in progress" => "#007bff",
                "completed" => "#28a745",
                "overdue" => "#dc3545",
                _ => "#6c757d"
            };

            args.Attributes["style"] = $"background: {backgroundColor}";

            // Add enhanced tooltip
            var tooltip = $"Audit Plan: {auditItem.AuditPlanCode}\\n" +
                         $"Type: {auditItem.AuditType}\\n" +
                         $"Status: {auditItem.Status}\\n" +
                         $"Priority: {auditItem.Priority}\\n" +
                         $"Lead Auditor: {auditItem.LeadAuditor}\\n" +
                         $"Department: {auditItem.ResponsibleDepartment}";

            args.Attributes["title"] = tooltip;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rendering appointment");
            console?.Log($"Error in AppointmentRender: {ex.Message}");
        }
    }

    async Task OnAppointmentMove(SchedulerAppointmentMoveEventArgs args)
    {
        try
        {
            var draggedAppointment = SchedulerData.FirstOrDefault(x => x == args.Appointment.Data);

            if (draggedAppointment is not null)
            {
                console?.Log($"AppointmentMove: AuditPlan={draggedAppointment.AuditPlanCode} moved to {args.SlotDate:yyyy-MM-dd HH:mm}");

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

                // Update the actual audit plan record
                await UpdateAuditPlanDateTime(draggedAppointment);

                await scheduler.Reload();
                await _notificationHelper.ShowSuccessAsync($"Audit plan {draggedAppointment.AuditPlanCode} rescheduled successfully");
                console?.Log($"Audit plan {draggedAppointment.AuditPlanCode} rescheduled successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving appointment");
            console?.Log($"Error in AppointmentMove: {ex.Message}");
            await _notificationHelper.ShowErrorAsync("Error rescheduling audit plan");
        }
    }

    private async Task UpdateAuditPlanDateTime(AuditSchedulerItem appointmentData)
    {
        try
        {
            // Find the actual audit plan
            var auditPlan = AuditPlans.FirstOrDefault(a => a.Code == appointmentData.AuditPlanCode);
            if (auditPlan is not null)
            {
                // Update the planned dates
                auditPlan.PlannedStartDate = appointmentData.Start;
                auditPlan.PlannedEndDate = appointmentData.End;

                // Update metadata
                auditPlan.UpdatedBy = "CURRENT_USER";
                auditPlan.UpdatedDate = DateTime.UtcNow;

                // Save via CQRS
                var updateCommand = new UpdateSMSAuditPlanCommand(auditPlan);
                var result = await _mediator.SendAsync(updateCommand, CancellationToken.None);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Audit plan {AuditCode} datetime updated successfully", auditPlan.Code);
                    console?.Log($"Audit plan {auditPlan.Code} updated in database");
                }
                else
                {
                    _logger.LogError("Failed to update audit plan datetime: {Error}", result.Error?.Message);
                    console?.Log($"Error updating audit plan: {result.Error?.Message}");
                    await _notificationHelper.ShowErrorAsync("Failed to save audit plan changes");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating audit plan datetime");
            console?.Log($"Exception updating audit plan: {ex.Message}");
        }
    }
    #endregion

    #region Navigation and Actions
    private async Task GoToToday()
    {
        try
        {
            if (scheduler is not null)
            {
                scheduler.CurrentDate = DateTime.Today;
                await scheduler.Reload();
                console?.Log("Navigated to today's date");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to today");
            console?.Log($"Error navigating to today: {ex.Message}");
        }
    }

    // Create new audit plan
    private async Task CreateNewAuditPlan()
    {
        try
        {
            var startDate = DateTime.Today.AddDays(30);
            var endDate = startDate.AddDays(1);

            console?.Log($"Opening create audit plan dialog for {startDate:yyyy-MM-dd}");

            var newAuditPlan = new SMSAuditPlan(new SMSAuditPlanID("AP-0000"), "CURRENT_USER")
            {
                PlannedStartDate = startDate,
                PlannedEndDate = endDate
            };

            var result = await _dialogService.OpenAsync<AuditPlanDialog>("Create Audit Plan",
                new Dictionary<string, object>()
                {
                    { "AuditPlan", newAuditPlan },
                    { "IsNew", true }
                },
                new DialogOptions() { Width = "1200px", Height = "900px", Resizable = true });

            if (result == true)
            {
                await LoadAuditPlansAsync();
                await scheduler.Reload();
                await _notificationHelper.ShowSuccessAsync("Audit plan created successfully");
                console?.Log("New audit plan created successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing create audit plan dialog");
            await _notificationHelper.ShowErrorAsync("Error opening create audit plan dialog");
            console?.Log($"Error in create dialog: {ex.Message}");
        }
    }
    #endregion

    #region Statistics Methods
    private int GetAuditsThisMonth()
    {
        var now = DateTime.Now;
        return AuditPlans.Count(a =>
            (a.PlannedStartDate.Year == now.Year && a.PlannedStartDate.Month == now.Month) ||
            (a.PlannedEndDate.Year == now.Year && a.PlannedEndDate.Month == now.Month));
    }

    private int GetAuditsThisWeek()
    {
        var now = DateTime.Now;
        var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(7);

        return AuditPlans.Count(a =>
            (a.PlannedStartDate >= startOfWeek && a.PlannedStartDate < endOfWeek) ||
            (a.PlannedEndDate >= startOfWeek && a.PlannedEndDate < endOfWeek) ||
            (a.PlannedStartDate <= startOfWeek && a.PlannedEndDate >= endOfWeek));
    }

    private int GetAuditsToday()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        return AuditPlans.Count(a =>
            (a.PlannedStartDate.Date <= today && a.PlannedEndDate.Date >= today) ||
            (a.PlannedStartDate >= today && a.PlannedStartDate < tomorrow));
    }
    #endregion
}

#region Supporting Classes and Enums
/// <summary>
/// Enhanced scheduler item representation of an Audit Plan for calendar display
/// </summary>
public class AuditSchedulerItem
{
    public string AuditPlanId { get; set; } = string.Empty;
    public string AuditPlanCode { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string AuditType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string LeadAuditor { get; set; } = string.Empty;
    public string ResponsibleDepartment { get; set; } = string.Empty;
    public int EstimatedHours { get; set; }
    public bool RequiresApproval { get; set; }
    public string Description { get; set; } = string.Empty;
}
#endregion
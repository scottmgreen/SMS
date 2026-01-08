using Microsoft.AspNetCore.Components;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Shared.Common;
using Radzen;
using Radzen.Blazor;

namespace SMS3.Components.Pages.SMSAssurance.Components;

public partial class AuditDialog : ComponentBase
{
    #region Parameters
    [Parameter] public SMSAudit Audit { get; set; }
    [Parameter] public bool IsNew { get; set; } = true;
    #endregion

    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<AuditDialog> Logger { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    #endregion

    #region Form State
    private bool IsSubmitting { get; set; } = false;
    private bool IsValid { get; set; } = true;
    private string ValidationMessage { get; set; } = string.Empty;
    
    // Form Data
    private string? Code { get; set; }
    private string? Name { get; set; }
    private string? Description { get; set; }
    private string? AuditPlanCode { get; set; }
    private string? AuditType { get; set; }
    private string? Scope { get; set; }
    private string? Objectives { get; set; }
    private string? ResponsibleDepartment { get; set; }
    private string? LeadAuditor { get; set; }
    private string? AuditorTeam { get; set; }
    private string? ContactPerson { get; set; }
    private string? AuditLocation { get; set; }
    private DateTime? ScheduledStartDate { get; set; }
    private DateTime? ScheduledEndDate { get; set; }
    private DateTime? ActualStartDate { get; set; }
    private DateTime? ActualEndDate { get; set; }
    private string? Priority { get; set; }
    private string? Status { get; set; }
    private string? ExecutiveSummary { get; set; }
    private string? Notes { get; set; }
    #endregion

    #region Options
    public List<string> AuditTypeOptions { get; } = new()
    {
        "Internal", "External", "Regulatory", "Management Review", "Process Audit", "Compliance", "Self-Assessment"
    };

    public List<string> DepartmentOptions { get; } = new()
    {
        "Airport Operations", "Security", "Maintenance", "Ground Handling", "Air Traffic Control", 
        "Safety", "Administration", "Emergency Services", "Environmental"
    };

    public List<string> StatusOptions { get; } = new()
    {
        "Scheduled", "In Progress", "Completed", "Cancelled", "On Hold"
    };

    public List<string> PriorityOptions { get; } = new()
    {
        "Critical", "High", "Medium", "Low"
    };
    #endregion

    #region Lifecycle Methods
    protected override void OnInitialized()
    {
        InitializeFormData();
    }

    private void InitializeFormData()
    {
        if (Audit != null)
        {
            Code = Audit.Code;
            Name = Audit.Name;
            Description = Audit.Description;
            AuditPlanCode = Audit.AuditPlanCode;
            AuditType = Audit.AuditType;
            Scope = Audit.Scope;
            Objectives = Audit.Objectives;
            ResponsibleDepartment = Audit.ResponsibleDepartment;
            LeadAuditor = Audit.LeadAuditor;
            AuditorTeam = Audit.AuditorTeam;
            ContactPerson = Audit.ContactPerson;
            AuditLocation = Audit.AuditLocation;
            ScheduledStartDate = Audit.ScheduledStartDate;
            ScheduledEndDate = Audit.ScheduledEndDate;
            ActualStartDate = Audit.ActualStartDate;
            ActualEndDate = Audit.ActualEndDate;
            Priority = Audit.Priority;
            Status = Audit.Status;
            ExecutiveSummary = Audit.ExecutiveSummary;
            Notes = Audit.Notes;
        }

        if (IsNew)
        {
            Status = "Scheduled";
            Priority = "Medium";
            if (!ScheduledStartDate.HasValue)
                ScheduledStartDate = DateTime.Today.AddDays(7);
            if (!ScheduledEndDate.HasValue)
                ScheduledEndDate = ScheduledStartDate?.AddDays(5);
        }
    }
    #endregion

    #region Form Validation
    private bool ValidateForm()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Code))
            errors.Add("Audit code is required");

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Audit name is required");

        if (string.IsNullOrWhiteSpace(AuditType))
            errors.Add("Audit type is required");

        if (string.IsNullOrWhiteSpace(Scope))
            errors.Add("Scope is required");

        if (string.IsNullOrWhiteSpace(ResponsibleDepartment))
            errors.Add("Responsible department is required");

        if (string.IsNullOrWhiteSpace(LeadAuditor))
            errors.Add("Lead auditor is required");

        if (!ScheduledStartDate.HasValue)
            errors.Add("Scheduled start date is required");

        if (!ScheduledEndDate.HasValue)
            errors.Add("Scheduled end date is required");

        if (ScheduledStartDate.HasValue && ScheduledEndDate.HasValue && ScheduledEndDate <= ScheduledStartDate)
            errors.Add("Scheduled end date must be after start date");

        if (ActualStartDate.HasValue && ActualEndDate.HasValue && ActualEndDate <= ActualStartDate)
            errors.Add("Actual end date must be after actual start date");

        if (Status == "In Progress" && !ActualStartDate.HasValue)
            errors.Add("Actual start date is required when status is 'In Progress'");

        if (Status == "Completed" && (!ActualStartDate.HasValue || !ActualEndDate.HasValue))
            errors.Add("Both actual start and end dates are required when status is 'Completed'");

        if (errors.Any())
        {
            ValidationMessage = string.Join("; ", errors);
            IsValid = false;
            return false;
        }

        ValidationMessage = string.Empty;
        IsValid = true;
        return true;
    }
    #endregion

    #region Event Handlers
    private async Task SaveAudit()
    {
        if (!ValidateForm()) return;

        try
        {
            IsSubmitting = true;
            StateHasChanged();

            if (IsNew)
            {
                var command = new CreateSMSAuditCommand(
                    code: Code!,
                    name: Name!,
                    description: Description,
                    auditPlanCode: AuditPlanCode,
                    auditType: AuditType!,
                    scope: Scope!,
                    objectives: Objectives,
                    scheduledStartDate: ScheduledStartDate!.Value,
                    scheduledEndDate: ScheduledEndDate!.Value,
                    actualStartDate: ActualStartDate,
                    actualEndDate: ActualEndDate,
                    leadAuditor: LeadAuditor!,
                    auditorTeam: AuditorTeam,
                    responsibleDepartment: ResponsibleDepartment!,
                    contactPerson: ContactPerson,
                    auditLocation: AuditLocation,
                    status: Status!,
                    priority: Priority!,
                    executiveSummary: ExecutiveSummary,
                    notes: Notes,
                    createdBy: "CURRENT_USER"
                );

                var result = await Mediator.SendAsync(command, CancellationToken.None);

                if (result.IsSuccess)
                {
                    Logger.LogInformation("Audit created successfully: {Code}", Code);
                    ShowSuccessNotification("Audit created successfully");
                    DialogService.Close(true);
                }
                else
                {
                    Logger.LogError("Failed to create audit: {Error}", result.Error?.Message);
                    ShowErrorNotification($"Failed to create audit: {result.Error?.Message}");
                }
            }
            else
            {
                var command = new UpdateSMSAuditCommand(
                    //id: Audit.Id!.Value,
                    code: Code!,
                    name: Name!,
                    description: Description,
                    auditPlanCode: AuditPlanCode,
                    auditType: AuditType!,
                    scope: Scope!,
                    objectives: Objectives,
                    scheduledStartDate: ScheduledStartDate!.Value,
                    scheduledEndDate: ScheduledEndDate!.Value,
                    actualStartDate: ActualStartDate,
                    actualEndDate: ActualEndDate,
                    leadAuditor: LeadAuditor!,
                    auditorTeam: AuditorTeam,
                    responsibleDepartment: ResponsibleDepartment!,
                    contactPerson: ContactPerson,
                    auditLocation: AuditLocation,
                    status: Status!,
                    priority: Priority!,
                    executiveSummary: ExecutiveSummary,
                    notes: Notes,
                    updatedBy: "CURRENT_USER",
                    updatedDate: DateTime.Now
                );

                var result = await Mediator.SendAsync(command, CancellationToken.None);

                if (result.IsSuccess)
                {
                    Logger.LogInformation("Audit updated successfully: {Code}", Code);
                    ShowSuccessNotification("Audit updated successfully");
                    DialogService.Close(true);
                }
                else
                {
                    Logger.LogError("Failed to update audit: {Error}", result.Error?.Message);
                    ShowErrorNotification($"Failed to update audit: {result.Error?.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving audit");
            ShowErrorNotification("Error saving audit");
        }
        finally
        {
            IsSubmitting = false;
            StateHasChanged();
        }
    }

    private void Cancel()
    {
        DialogService.Close(false);
    }

    private void OnScheduledStartDateChanged(DateTime? value)
    {
        ScheduledStartDate = value;
        
        // Automatically adjust end date if start date changes
        if (value.HasValue && (!ScheduledEndDate.HasValue || ScheduledEndDate.Value <= value.Value))
        {
            ScheduledEndDate = value.Value.AddDays(5);
        }
        
        StateHasChanged();
    }

    private void OnScheduledEndDateChanged(DateTime? value)
    {
        ScheduledEndDate = value;
        StateHasChanged();
    }

    private void OnActualStartDateChanged(DateTime? value)
    {
        ActualStartDate = value;
        StateHasChanged();
    }

    private void OnActualEndDateChanged(DateTime? value)
    {
        ActualEndDate = value;
        StateHasChanged();
    }

    private void OnStatusChanged(object args)
    {
        Status = args?.ToString();
        
        // Set actual dates based on status
        if (Status == "In Progress" && !ActualStartDate.HasValue)
        {
            ActualStartDate = DateTime.Now;
        }
        else if (Status == "Completed" && !ActualEndDate.HasValue)
        {
            ActualEndDate = DateTime.Now;
        }
        
        StateHasChanged();
    }
    #endregion

    #region Helper Methods
    private void GenerateCode()
    {
        if (string.IsNullOrWhiteSpace(Code) && !string.IsNullOrWhiteSpace(AuditType))
        {
            var prefix = AuditType switch
            {
                "Internal" => "AUD-INT",
                "External" => "AUD-EXT", 
                "Regulatory" => "AUD-REG",
                "Management Review" => "AUD-MGT",
                "Process Audit" => "AUD-PRC",
                "Compliance" => "AUD-CMP",
                "Self-Assessment" => "AUD-SA",
                _ => "AUD"
            };

            var year = DateTime.Now.Year.ToString()[2..];
            var month = DateTime.Now.Month.ToString("D2");
            var random = new Random().Next(100, 999);
            
            Code = $"{prefix}-{year}{month}-{random}";
            StateHasChanged();
        }
    }

    private bool ShowActualDates => Status == "In Progress" || Status == "Completed";
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
}
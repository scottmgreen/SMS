using Microsoft.AspNetCore.Components;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Shared.Common;
using Radzen;
using Radzen.Blazor;

namespace SMS3.Components.Pages.SMSAssurance.Components;

public partial class AuditPlanDialog : ComponentBase
{
    #region Parameters
    [Parameter] public SMSAuditPlan AuditPlan { get; set; } = new(new SMSAuditPlanID("TEMP"), "SYSTEM");
    [Parameter] public bool IsNew { get; set; } = true;
    #endregion

    #region Injected Services
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<AuditPlanDialog> Logger { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    #endregion

    #region Form State
    private bool IsSubmitting { get; set; } = false;
    private bool IsValid { get; set; } = true;
    private string ValidationMessage { get; set; } = string.Empty;
    private int selectedTabIndex = 0;
    
    // Form Data
    private string? Code { get; set; }
    private string? Name { get; set; }
    private string? Description { get; set; }
    private string? AuditType { get; set; }
    private string? Scope { get; set; }
    private string? Objectives { get; set; }
    private string? ResponsibleDepartment { get; set; }
    private string? LeadAuditor { get; set; }
    private string? AuditorTeam { get; set; }
    private DateTime? PlannedStartDate { get; set; }
    private DateTime? PlannedEndDate { get; set; }
    private int? EstimatedHours { get; set; }
    private string? Priority { get; set; }
    private string? Status { get; set; }
    private string? Notes { get; set; }
    private string? RegulatoryRequirements { get; set; }
    private string? Resources { get; set; }
    private string? Deliverables { get; set; }
    private string? SuccessCriteria { get; set; }
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
        "Draft", "Under Review", "Approved", "Scheduled", "Cancelled"
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
        if (AuditPlan != null)
        {
            Code = AuditPlan.Code;
            Name = AuditPlan.Name;
            Description = AuditPlan.Description;
            AuditType = AuditPlan.AuditType;
            Scope = AuditPlan.AuditScope; // Use AuditScope from entity
            Objectives = AuditPlan.AuditObjectives; // Use AuditObjectives from entity
            ResponsibleDepartment = AuditPlan.ResponsibleDepartment;
            LeadAuditor = AuditPlan.LeadAuditor;
            AuditorTeam = AuditPlan.AuditorTeam;
            PlannedStartDate = AuditPlan.PlannedStartDate;
            PlannedEndDate = AuditPlan.PlannedEndDate;
            EstimatedHours = AuditPlan.EstimatedDurationHours; // Use EstimatedDurationHours from entity
            Priority = AuditPlan.Priority;
            Status = AuditPlan.Status;
            Notes = AuditPlan.Notes;
            RegulatoryRequirements = AuditPlan.AuditCriteria; // Map to AuditCriteria
            Resources = AuditPlan.RequiredDocuments; // Map to RequiredDocuments
            Deliverables = AuditPlan.SpecialRequirements; // Map to SpecialRequirements (best fit)
            SuccessCriteria = AuditPlan.RiskAreas; // Map to RiskAreas (closest fit)
        }

        if (IsNew)
        {
            Status = "Draft";
            PlannedStartDate = DateTime.Today.AddDays(30);
            PlannedEndDate = DateTime.Today.AddDays(37); // Week-long audit by default
            Priority = "Medium";
            EstimatedHours = 8; // Default 8 hours
        }
    }
    #endregion

    #region Form Validation
    private bool ValidateForm()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Code))
            errors.Add("Audit plan code is required");

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Audit plan name is required");

        if (string.IsNullOrWhiteSpace(AuditType))
            errors.Add("Audit type is required");

        if (string.IsNullOrWhiteSpace(Scope))
            errors.Add("Scope is required");

        if (string.IsNullOrWhiteSpace(ResponsibleDepartment))
            errors.Add("Responsible department is required");

        if (string.IsNullOrWhiteSpace(LeadAuditor))
            errors.Add("Lead auditor is required");

        if (!PlannedStartDate.HasValue)
            errors.Add("Planned start date is required");

        if (!PlannedEndDate.HasValue)
            errors.Add("Planned end date is required");

        if (PlannedStartDate.HasValue && PlannedEndDate.HasValue && PlannedEndDate <= PlannedStartDate)
            errors.Add("Planned end date must be after start date");

        if (PlannedStartDate.HasValue && PlannedStartDate.Value < DateTime.Today)
            errors.Add("Planned start date cannot be in the past");

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
    private async Task SaveAuditPlan()
    {
        if (!ValidateForm()) return;

        try
        {
            IsSubmitting = true;
            StateHasChanged();

            if (IsNew)
            {
                var command = new CreateSMSAuditPlanCommand(
                    code: Code!,
                    name: Name!,
                    description: Description,
                    auditType: AuditType!,
                    auditScope: Scope!, // Maps to AuditScope on entity
                    auditObjectives: Objectives, // Maps to AuditObjectives on entity
                    responsibleDepartment: ResponsibleDepartment!,
                    leadAuditor: LeadAuditor!,
                    auditorTeam: AuditorTeam,
                    plannedStartDate: PlannedStartDate!.Value,
                    plannedEndDate: PlannedEndDate!.Value,
                    estimatedHours: EstimatedHours,
                    priority: Priority!,
                    status: Status!,
                    notes: Notes,
                    regulatoryRequirements: RegulatoryRequirements, // Maps to AuditCriteria on entity
                    resources: Resources, // Maps to RequiredDocuments on entity
                    deliverables: Deliverables, // Maps to SpecialRequirements on entity
                    successCriteria: SuccessCriteria, // Maps to RiskAreas on entity
                    createdBy: "CURRENT_USER"
                );

                var result = await Mediator.SendAsync(command, CancellationToken.None);

                if (result.IsSuccess)
                {
                    Logger.LogInformation("Audit plan created successfully: {Code}", Code);
                    ShowSuccessNotification("Audit plan created successfully");
                    DialogService.Close(true);
                }
                else
                {
                    Logger.LogError("Failed to create audit plan: {Error}", result.Error?.Message);
                    ShowErrorNotification($"Failed to create audit plan: {result.Error?.Message}");
                }
            }
            else
            {
                // TODO: UpdateSMSAuditPlanCommand expects Guid but entity uses string Code
                // For now, show an error message until the command is fixed
                //ShowErrorNotification("Edit functionality is not yet implemented - command/entity ID mismatch");
                //return;


                var command = new UpdateSMSAuditPlanCommand(
                     // Use Code instead of database Id
                    auditPlanCode: Code!,
                    name: Name!,
                    description: Description,
                    auditType: AuditType!,
                    auditScope: Scope!,
                    auditObjectives: Objectives,
                    responsibleDepartment: ResponsibleDepartment!,
                    leadAuditor: LeadAuditor!,
                    auditorTeam: AuditorTeam,
                    plannedStartDate: PlannedStartDate!.Value,
                    plannedEndDate: PlannedEndDate!.Value,
                    estimatedHours: EstimatedHours,
                    priority: Priority!,
                    status: Status!,
                    notes: Notes,
                    regulatoryRequirements: RegulatoryRequirements,
                    resources: Resources,
                    deliverables: Deliverables,
                    successCriteria: SuccessCriteria,
                    updatedBy: "CURRENT_USER",
                    updatedDate: DateTime.Now
                );

                var result = await Mediator.SendAsync(command, CancellationToken.None);

                if (result.IsSuccess)
                {
                    Logger.LogInformation("Audit plan updated successfully: {Code}", Code);
                    ShowSuccessNotification("Audit plan updated successfully");
                    DialogService.Close(true);
                }
                else
                {
                    Logger.LogError("Failed to update audit plan: {Error}", result.Error?.Message);
                    ShowErrorNotification($"Failed to update audit plan: {result.Error?.Message}");
                }

            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving audit plan");
            ShowErrorNotification("Error saving audit plan");
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

    private void OnStartDateChanged(DateTime? value)
    {
        PlannedStartDate = value;
        
        // Automatically adjust end date if start date changes
        if (value.HasValue && (!PlannedEndDate.HasValue || PlannedEndDate.Value <= value.Value))
        {
            PlannedEndDate = value.Value.AddDays(7);
        }
        
        StateHasChanged();
    }

    private void OnEndDateChanged(DateTime? value)
    {
        PlannedEndDate = value;
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
                "Internal" => "INT",
                "External" => "EXT", 
                "Regulatory" => "REG",
                "Management Review" => "MGT",
                "Process Audit" => "PRC",
                "Compliance" => "CMP",
                "Self-Assessment" => "SA",
                _ => "AUD"
            };

            var year = DateTime.Now.Year.ToString()[2..];
            var month = DateTime.Now.Month.ToString("D2");
            var random = new Random().Next(100, 999);
            
            Code = $"{prefix}-{year}{month}-{random}";
            StateHasChanged();
        }
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
}
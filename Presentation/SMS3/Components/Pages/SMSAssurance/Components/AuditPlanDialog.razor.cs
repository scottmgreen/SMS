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
    
    // Approval Workflow Fields - ADDED
    private bool RequiresApproval { get; set; } = true;
    private string? ApprovedBy { get; set; }
    private DateTime? ApprovedDate { get; set; }
    private string? ApprovalNotes { get; set; }
    
    // Additional Entity Fields
    private string? ContactPerson { get; set; }
    private string? RecurrencePattern { get; set; }
    private DateTime? NextScheduledDate { get; set; }
    
    // UI Mapping Fields (for backward compatibility)
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
        "Draft", "Under Review", "Approved", "Active", "Completed", "Cancelled"
    };

    public List<string> PriorityOptions { get; } = new()
    {
        "Critical", "High", "Medium", "Low"
    };
    
    public List<string> RecurrencePatternOptions { get; } = new()
    {
        "None", "Annual", "Semi-Annual", "Quarterly", "Monthly"
    };
    #endregion

    #region Lifecycle Methods
    protected override void OnInitialized()
    {
        Logger.LogInformation("DEBUG: OnInitialized called - IsNew: {IsNew}, AuditPlan.Status: {Status}", IsNew, AuditPlan?.Status);
        InitializeFormData();
    }

    private void InitializeFormData()
    {
        Logger.LogInformation("DEBUG: InitializeFormData called - AuditPlan.Status: {Status}", AuditPlan?.Status);
        
        if (AuditPlan != null)
        {
            Code = AuditPlan.Code;
            Name = AuditPlan.Name;
            Description = AuditPlan.Description;
            AuditType = AuditPlan.AuditType;
            Scope = AuditPlan.Scope;
            Objectives = AuditPlan.Objectives;
            ResponsibleDepartment = AuditPlan.ResponsibleDepartment;
            LeadAuditor = AuditPlan.LeadAuditor;
            AuditorTeam = AuditPlan.AuditorTeam;
            PlannedStartDate = AuditPlan.PlannedStartDate;
            PlannedEndDate = AuditPlan.PlannedEndDate;
            EstimatedHours = AuditPlan.EstimatedDurationHours;
            Priority = AuditPlan.Priority;
            Status = AuditPlan.Status;
            Notes = AuditPlan.Notes;
            
            // Approval Workflow Fields - ADDED
            RequiresApproval = AuditPlan.RequiresApproval;
            ApprovedBy = AuditPlan.ApprovedBy;
            ApprovedDate = AuditPlan.ApprovedDate;
            ApprovalNotes = AuditPlan.ApprovalNotes;
            
            // Additional Fields
            ContactPerson = AuditPlan.ContactPerson;
            RecurrencePattern = AuditPlan.RecurrencePattern;
            NextScheduledDate = AuditPlan.NextScheduledDate;
            
            // UI Mapping Fields (for tabs)
            RegulatoryRequirements = AuditPlan.AuditCriteria;
            Resources = AuditPlan.RequiredDocuments;
            Deliverables = AuditPlan.SpecialRequirements;
            SuccessCriteria = AuditPlan.RiskAreas;
        }

        if (IsNew)
        {
            Status = "Draft";
            PlannedStartDate = DateTime.Today.AddDays(30);
            PlannedEndDate = DateTime.Today.AddDays(37);
            Priority = "Medium";
            EstimatedHours = 8;
            RequiresApproval = true;
            RecurrencePattern = "None";
        }
        
        Logger.LogInformation("DEBUG: InitializeFormData completed - Status set to: {Status}", Status);
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

        // Make scope and objectives optional for now to debug the issue
        // if (string.IsNullOrWhiteSpace(Scope))
        //     errors.Add("Scope is required");

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

            // DEBUG: Log current form values
            Logger.LogInformation("DEBUG SaveAuditPlan: Status = {Status}", Status);
            Logger.LogInformation("DEBUG SaveAuditPlan: Scope = {Scope}", Scope);
            Logger.LogInformation("DEBUG SaveAuditPlan: Objectives = {Objectives}", Objectives);
            Logger.LogInformation("DEBUG SaveAuditPlan: ApprovedBy = {ApprovedBy}", ApprovedBy);
            Logger.LogInformation("DEBUG SaveAuditPlan: ApprovedDate = {ApprovedDate}", ApprovedDate);

            // Create or update the audit plan entity with proper property mapping
            SMSAuditPlan auditPlan;
            
            if (IsNew)
            {
                auditPlan = new SMSAuditPlan(new SMSAuditPlanID(Code!), "CURRENT_USER");
            }
            else
            {
                // Create a copy or clone the existing audit plan instead of using the reference
                auditPlan = new SMSAuditPlan(new SMSAuditPlanID(AuditPlan.Code), AuditPlan.CreatedBy);
                // Copy over the original timestamps and metadata
                auditPlan.CreatedDate = AuditPlan.CreatedDate;
            }

            // Map form fields to entity properties - COMPLETE MAPPING
            auditPlan.Code = Code!;
            auditPlan.Name = Name!;
            auditPlan.Description = Description ?? string.Empty;
            auditPlan.AuditType = AuditType!;
            auditPlan.Scope = Scope ?? string.Empty;
            auditPlan.Objectives = Objectives ?? string.Empty;
            auditPlan.AuditScope = Scope ?? string.Empty; // Compatibility
            auditPlan.AuditObjectives = Objectives ?? string.Empty; // Compatibility
            auditPlan.ResponsibleDepartment = ResponsibleDepartment!;
            auditPlan.LeadAuditor = LeadAuditor!;
            auditPlan.AuditorTeam = AuditorTeam ?? string.Empty;
            auditPlan.PlannedStartDate = PlannedStartDate!.Value;
            auditPlan.PlannedEndDate = PlannedEndDate!.Value;
            auditPlan.EstimatedDurationHours = EstimatedHours ?? 8;
            auditPlan.ExpectedDurationHours = EstimatedHours ?? 8; // Compatibility
            auditPlan.Priority = Priority ?? "Medium";
            auditPlan.Status = Status!;
            auditPlan.Notes = Notes ?? string.Empty;
            
            // Approval Workflow Fields - ADDED
            auditPlan.RequiresApproval = RequiresApproval;
            auditPlan.ApprovedBy = ApprovedBy ?? string.Empty;
            auditPlan.ApprovedDate = ApprovedDate;
            auditPlan.ApprovalNotes = ApprovalNotes ?? string.Empty;
            
            // Additional Fields
            auditPlan.ContactPerson = ContactPerson ?? string.Empty;
            auditPlan.RecurrencePattern = RecurrencePattern ?? "None";
            auditPlan.NextScheduledDate = NextScheduledDate;
            
            // Map UI fields to entity properties
            auditPlan.AuditCriteria = RegulatoryRequirements ?? string.Empty;
            auditPlan.RequiredDocuments = Resources ?? string.Empty;
            auditPlan.SpecialRequirements = Deliverables ?? string.Empty;
            auditPlan.RiskAreas = SuccessCriteria ?? string.Empty;
            
            // Set audit metadata
            auditPlan.UpdatedBy = "CURRENT_USER";
            auditPlan.UpdatedDate = DateTime.UtcNow;

            // DEBUG: Log entity values after mapping
            Logger.LogInformation("DEBUG Entity: Status = {Status}", auditPlan.Status);
            Logger.LogInformation("DEBUG Entity: Scope = {Scope}", auditPlan.Scope);
            Logger.LogInformation("DEBUG Entity: ApprovedBy = {ApprovedBy}", auditPlan.ApprovedBy);

            if (IsNew)
            {
                var command = new CreateSMSAuditPlanCommand(auditPlan);
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
                var command = new UpdateSMSAuditPlanCommand(auditPlan);
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
    
    private void OnStatusChanged(string value)
    {
        if (value != Status)
        {
            Status = value;
            
            // Handle approval workflow
            if (value == "Approved")
            {
                // If status is set to Approved, RequiresApproval should be true
                if (!RequiresApproval)
                {
                    RequiresApproval = true;
                }
                
                // Auto-populate approval fields if empty
                if (string.IsNullOrEmpty(ApprovedBy))
                {
                    ApprovedBy = "CURRENT_USER"; // Replace with actual current user
                    ApprovedDate = DateTime.UtcNow;
                }
            }
            else if (value != "Approved")
            {
                // Clear approval fields if status is changed from approved
                ApprovedBy = string.Empty;
                ApprovedDate = null;
                ApprovalNotes = string.Empty;
            }
            
            StateHasChanged();
        }
    }
    
    private void OnRequiresApprovalChanged(bool value)
    {
        RequiresApproval = value;
        
        // If approval is not required and status is approved, clear approval fields
        if (!value && Status == "Approved")
        {
            ApprovedBy = string.Empty;
            ApprovedDate = null;
            ApprovalNotes = string.Empty;
            // Also change status back to a non-approved state
            Status = "Draft";
        }
        
        // Trigger re-render to show/hide the Approval Workflow tab
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
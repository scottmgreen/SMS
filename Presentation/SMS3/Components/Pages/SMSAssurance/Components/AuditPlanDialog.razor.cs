

using SMS_Application.Interfaces;
using SMS_Domain.Enums;
using SMS_Domain.Events;

using SMS_Shared.Configuration;

using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.SMSAssurance.Components;

public partial class AuditPlanDialog : ComponentBase
{
    #region Parameters
    [Parameter] public SMSAuditPlan AuditPlan { get; set; } = new(new SMSAuditPlanID("TEMP"), string.Empty);
    [Parameter] public bool IsNew { get; set; } = true;
    #endregion

    #region Injected Services
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<AuditPlanDialog> _logger { get; set; } = default!;
    [Inject] private IBaseEventBus _eventBus { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;
    [Inject] private ICurrentUserService _currentUserService { get; set; } = default!;
    #endregion

    #region Form State
    private bool _isSubmitting { get; set; } = false;
    private bool _isValid { get; set; } = true;
    private bool _isReadOnly => !IsNew && AuditPlan?.Status == "Completed";
    private string _validationMessage { get; set; } = string.Empty;
    private int _selectedTabIndex = 0;

    // Form Data
    private string? _code { get; set; }
    private string? _name { get; set; }
    private string? _description { get; set; }
    private string? _auditType { get; set; }
    private string? _scope { get; set; }
    private string? _objectives { get; set; }
    private string? _responsibleDepartment { get; set; }
    private string? _leadAuditor { get; set; }
    private string? _auditorTeam { get; set; }
    private DateTime? _plannedStartDate { get; set; }
    private DateTime? _plannedEndDate { get; set; }
    private int? _estimatedHours { get; set; }
    private string? _priority { get; set; }
    private string? _status { get; set; }
    private string? _notes { get; set; }

    // Approval Workflow Fields - ADDED
    private bool _requiresApproval { get; set; } = true;
    private string? _approvedBy { get; set; }
    private DateTime? _approvedDate { get; set; }
    private string? _approvalNotes { get; set; }

    // Additional Entity Fields
    private string? _contactPerson { get; set; }
    private string? _recurrencePattern { get; set; }
    private DateTime? _nextScheduledDate { get; set; }

    // UI Mapping Fields (for backward compatibility)
    private string? _regulatoryRequirements { get; set; }
    private string? _resources { get; set; }
    private string? _deliverables { get; set; }
    private string? _successCriteria { get; set; }
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
        _logger.LogInformation("DEBUG: OnInitialized called - IsNew: {IsNew}, AuditPlan.Status: {_status}", IsNew, AuditPlan?.Status);
        InitializeFormData();
    }

    private void InitializeFormData()
    {
        _logger.LogInformation("DEBUG: InitializeFormData called - AuditPlan.Status: {_status}", AuditPlan?.Status);

        if (AuditPlan is not null)
        {
            _code = AuditPlan.Code;
            _name = AuditPlan.Name;
            _description = AuditPlan.Description;
            _auditType = AuditPlan.AuditType;
            _scope = AuditPlan.Scope;
            _objectives = AuditPlan.Objectives;
            _responsibleDepartment = AuditPlan.ResponsibleDepartment;
            _leadAuditor = AuditPlan.LeadAuditor;
            _auditorTeam = AuditPlan.AuditorTeam;
            _plannedStartDate = AuditPlan.PlannedStartDate;
            _plannedEndDate = AuditPlan.PlannedEndDate;
            _estimatedHours = AuditPlan.EstimatedDurationHours;
            _priority = AuditPlan.Priority;
            _status = AuditPlan.Status;
            _notes = AuditPlan.Notes;

            // Approval Workflow Fields - ADDED
            _requiresApproval = AuditPlan.RequiresApproval;
            _approvedBy = AuditPlan.ApprovedBy;
            _approvedDate = AuditPlan.ApprovedDate;
            _approvalNotes = AuditPlan.ApprovalNotes;

            // Additional Fields
            _contactPerson = AuditPlan.ContactPerson;
            _recurrencePattern = AuditPlan.RecurrencePattern;
            _nextScheduledDate = AuditPlan.NextScheduledDate;

            // UI Mapping Fields (for tabs)
            _regulatoryRequirements = AuditPlan.AuditCriteria;
            _resources = AuditPlan.RequiredDocuments;
            _deliverables = AuditPlan.SpecialRequirements;
            _successCriteria = AuditPlan.RiskAreas;
        }

        if (IsNew)
        {
            _status = "Draft";
            _plannedStartDate = DateTime.Today.AddDays(30);
            _plannedEndDate = DateTime.Today.AddDays(37);
            _priority = "Medium";
            _estimatedHours = 8;
            _requiresApproval = true;
            _recurrencePattern = "None";
        }

        _logger.LogInformation("DEBUG: InitializeFormData completed - _status set to: {_status}", _status);
    }
    #endregion

    #region Form Validation
    private bool ValidateForm()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(_code))
            errors.Add("Audit plan code is required");

        if (string.IsNullOrWhiteSpace(_name))
            errors.Add("Audit plan name is required");

        if (string.IsNullOrWhiteSpace(_auditType))
            errors.Add("Audit type is required");

        // Make scope and objectives optional for now to debug the issue
        // if (string.IsNullOrWhiteSpace(_scope))
        //     errors.Add("_scope is required");

        if (string.IsNullOrWhiteSpace(_responsibleDepartment))
            errors.Add("Responsible department is required");

        if (string.IsNullOrWhiteSpace(_leadAuditor))
            errors.Add("Lead auditor is required");

        if (!_plannedStartDate.HasValue)
            errors.Add("Planned start date is required");

        if (!_plannedEndDate.HasValue)
            errors.Add("Planned end date is required");

        if (_plannedStartDate.HasValue && _plannedEndDate.HasValue && _plannedEndDate <= _plannedStartDate)
            errors.Add("Planned end date must be after start date");

        if (_plannedStartDate.HasValue && _plannedStartDate.Value < DateTime.Today)
            errors.Add("Planned start date cannot be in the past");

        if (errors.Any())
        {
            _validationMessage = string.Join("; ", errors);
            _isValid = false;
            return false;
        }

        _validationMessage = string.Empty;
        _isValid = true;
        return true;
    }
    #endregion

    #region Event Handlers
    private async Task SaveAuditPlan()
    {
        if (!ValidateForm()) return;

        try
        {
            _isSubmitting = true;
            StateHasChanged();


            // Create or update the audit plan entity with proper property mapping
            SMSAuditPlan auditPlan;

            if (IsNew)
            {
                auditPlan = new SMSAuditPlan(new SMSAuditPlanID(_code!), _currentUserService.UserCode);
            }
            else
            {
                // Create a copy or clone the existing audit plan instead of using the reference
                auditPlan = new SMSAuditPlan(new SMSAuditPlanID(AuditPlan.Code), AuditPlan.CreatedBy ?? _currentUserService.UserCode);
                // Copy over the original timestamps and metadata
                auditPlan.CreatedDate = AuditPlan.CreatedDate;
            }

            // Map form fields to entity properties - COMPLETE MAPPING
            auditPlan.Code = _code!;
            auditPlan.Name = _name!;
            auditPlan.Description = _description ?? string.Empty;
            auditPlan.AuditType = _auditType!;
            auditPlan.Scope = _scope ?? string.Empty;
            auditPlan.Objectives = _objectives ?? string.Empty;
            auditPlan.AuditScope = _scope ?? string.Empty; // Compatibility
            auditPlan.AuditObjectives = _objectives ?? string.Empty; // Compatibility
            auditPlan.ResponsibleDepartment = _responsibleDepartment!;
            auditPlan.LeadAuditor = _leadAuditor!;
            auditPlan.AuditorTeam = _auditorTeam ?? string.Empty;
            auditPlan.PlannedStartDate = _plannedStartDate!.Value;
            auditPlan.PlannedEndDate = _plannedEndDate!.Value;
            auditPlan.EstimatedDurationHours = _estimatedHours ?? 8;
            auditPlan.ExpectedDurationHours = _estimatedHours ?? 8; // Compatibility
            auditPlan.Priority = _priority ?? "Medium";
            auditPlan.Status = _status!;
            auditPlan.Notes = _notes ?? string.Empty;

            // Approval Workflow Fields - ADDED
            auditPlan.RequiresApproval = _requiresApproval;
            auditPlan.ApprovedBy = _approvedBy ?? string.Empty;
            auditPlan.ApprovedDate = _approvedDate;
            auditPlan.ApprovalNotes = _approvalNotes ?? string.Empty;

            // Additional Fields
            auditPlan.ContactPerson = _contactPerson ?? string.Empty;
            auditPlan.RecurrencePattern = _recurrencePattern ?? "None";
            auditPlan.NextScheduledDate = _nextScheduledDate;

            // Map UI fields to entity properties
            auditPlan.AuditCriteria = _regulatoryRequirements ?? string.Empty;
            auditPlan.RequiredDocuments = _resources ?? string.Empty;
            auditPlan.SpecialRequirements = _deliverables ?? string.Empty;
            auditPlan.RiskAreas = _successCriteria ?? string.Empty;

            // Set audit metadata
            auditPlan.UpdatedBy = "CURRENT_USER";
            auditPlan.UpdatedDate = DateTime.UtcNow;

            // DEBUG: Log entity values after mapping
            _logger.LogInformation("DEBUG Entity: _status = {_status}", auditPlan.Status);
            _logger.LogInformation("DEBUG Entity: _scope = {_scope}", auditPlan.Scope);
            _logger.LogInformation("DEBUG Entity: _approvedBy = {_approvedBy}", auditPlan.ApprovedBy);

            if (IsNew)
            {
                var command = new CreateSMSAuditPlanCommand(auditPlan);
                var result = await _mediator.SendAsync(command, CancellationToken.None);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Audit plan created successfully: {_code}", _code);
                    await ShowSuccessAsyncNotification("Audit plan created successfully");
                    _dialogService.Close(true);
                }
                else
                {
                    _logger.LogError("Failed to create audit plan: {Error}", result.Error?.Message);
                    await ShowErrorAsyncNotification($"Failed to create audit plan: {result.Error?.Message}");
                }
            }
            else
            {
                var command = new UpdateSMSAuditPlanCommand(auditPlan);
                var result = await _mediator.SendAsync(command, CancellationToken.None);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Audit plan updated successfully: {_code}", _code);
                    await ShowSuccessAsyncNotification("Audit plan updated successfully");
                    _dialogService.Close(true);
                }
                else
                {
                    _logger.LogError("Failed to update audit plan: {Error}", result.Error?.Message);
                    await ShowErrorAsyncNotification($"Failed to update audit plan: {result.Error?.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving audit plan");
            await ShowErrorAsyncNotification("Error saving audit plan");
        }
        finally
        {
            _isSubmitting = false;
            StateHasChanged();
        }
    }

    private void Cancel()
    {
        _dialogService.Close(false);
    }

    private void OnStartDateChanged(DateTime? value)
    {
        _plannedStartDate = value;

        // Automatically adjust end date if start date changes
        if (value.HasValue && (!_plannedEndDate.HasValue || _plannedEndDate.Value <= value.Value))
        {
            _plannedEndDate = value.Value.AddDays(7);
        }

        // Calculate estimated hours based on duration
        CalculateEstimatedHours();

        StateHasChanged();
    }

    private void OnEndDateChanged(DateTime? value)
    {
        _plannedEndDate = value;

        // Calculate estimated hours based on duration
        CalculateEstimatedHours();

        StateHasChanged();
    }

    private void CalculateEstimatedHours()
    {
        if (_plannedStartDate.HasValue && _plannedEndDate.HasValue && _plannedEndDate.Value > _plannedStartDate.Value)
        {
            var duration = _plannedEndDate.Value - _plannedStartDate.Value;
            var totalDays = (int)duration.TotalDays;

            // Calculate estimated hours based on audit type and duration
            var baseHoursPerDay = _auditType switch
            {
                "Internal" => 6, // 6 hours per day for internal audits
                "External" => 8, // 8 hours per day for external audits  
                "Management Review" => 4, // 4 hours per day for management reviews
                "Compliance" => 7, // 7 hours per day for compliance audits
                "Follow-up" => 3, // 3 hours per day for follow-up audits
                _ => 6 // Default to 6 hours per day
            };

            // Calculate total estimated hours
            var calculatedHours = totalDays * baseHoursPerDay;

            // Apply reasonable bounds (minimum 2 hours, maximum 200 hours)
            _estimatedHours = Math.Max(2, Math.Min(200, calculatedHours));

            _logger.LogInformation("Calculated _estimatedHours: {Hours} for {Days} days of {_auditType} audit",
                _estimatedHours, totalDays, _auditType);
        }
    }

    private void OnStatusChanged(string value)
    {
        if (value != _status)
        {
            _status = value;

            // Handle approval workflow
            if (value == "Approved")
            {
                // If status is set to Approved, _requiresApproval should be true
                if (!_requiresApproval)
                {
                    _requiresApproval = true;
                }

                // Auto-populate approval fields if empty
                if (string.IsNullOrEmpty(_approvedBy))
                {
                    _approvedBy = "CURRENT_USER"; // Replace with actual current user
                    _approvedDate = DateTime.UtcNow;
                }
            }
            else if (value != "Approved")
            {
                // Clear approval fields if status is changed from approved
                _approvedBy = string.Empty;
                _approvedDate = null;
                _approvalNotes = string.Empty;
            }

            StateHasChanged();
        }
    }

    private void OnRequiresApprovalChanged(bool value)
    {
        _requiresApproval = value;

        // If approval is not required and status is approved, clear approval fields
        if (!value && _status == "Approved")
        {
            _approvedBy = string.Empty;
            _approvedDate = null;
            _approvalNotes = string.Empty;
            // Also change status back to a non-approved state
            _status = "Draft";
        }

        // Trigger re-render to show/hide the Approval Workflow tab
        StateHasChanged();
    }

    private void OnAuditTypeChanged(string value)
    {
        _auditType = value;

        // Auto-generate code if it's empty and we have an audit type
        if (string.IsNullOrWhiteSpace(_code) && !string.IsNullOrWhiteSpace(_auditType))
        {
            GenerateCode();
        }

        // Recalculate estimated hours based on new audit type
        CalculateEstimatedHours();

        StateHasChanged();
    }
    #endregion

    #region Helper Methods
    private void GenerateCode()
    {
        if (string.IsNullOrWhiteSpace(_code) && !string.IsNullOrWhiteSpace(_auditType))
        {
            var prefix = _auditType switch
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

            _code = $"{prefix}-{year}{month}-{random}";
            StateHasChanged();
        }
    }
    #endregion

    #region Notification Methods (EventBus-Driven)
    private async Task ShowSuccessAsyncNotification(string message)
    {
        await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", message));
    }

    private async Task ShowErrorAsyncNotification(string message)
    {
        await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", message));
    }
    #endregion
}


using SMS_Domain.Enums;
using SMS3.Components.Shared.UIHelpers;
using Radzen;
using SMS_Domain.Entities;

namespace SMS3.Components.Pages.SMSAssurance.Components;

public partial class AuditDialog : ComponentBase
{
    #region Parameters
    [Parameter] public SMSAudit Audit { get; set; } = default!;
    [Parameter] public bool IsNew { get; set; } = true;
    #endregion

    #region Injected Services
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<AuditDialog> _logger { get; set; } = default!;
    [Inject] private INotificationHelper _notificationHelper { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;
    #endregion

    #region Form State
    private bool _isSubmitting { get; set; } = false;
    private bool _isValid { get; set; } = true;
    private string _validationMessage { get; set; } = string.Empty;

    // Form Data
    private string? _code { get; set; }
    private string? _name { get; set; }
    private string? _description { get; set; }
    private string? _auditPlanCode { get; set; }
    private string? _auditType { get; set; }
    private string? _scope { get; set; }
    private string? _objectives { get; set; }
    private string? _responsibleDepartment { get; set; }
    private string? _leadAuditor { get; set; }
    private string? _auditorTeam { get; set; }
    private string? _contactPerson { get; set; }
    private string? _auditLocation { get; set; }
    private DateTime? _scheduledStartDate { get; set; }
    private DateTime? _scheduledEndDate { get; set; }
    private DateTime? _actualStartDate { get; set; }
    private DateTime? _actualEndDate { get; set; }
    private string? _priority { get; set; }
    private string? _status { get; set; }
    private string? _executiveSummary { get; set; }
    private string? _notes { get; set; }
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
        if (Audit is not null)
        {
            _code = Audit.Code;
            _name = Audit.Name;
            _description = Audit.Description;
            _auditPlanCode = Audit.AuditPlanCode;
            _auditType = Audit.AuditType;
            _scope = Audit.Scope;
            _objectives = Audit.Objectives;
            _responsibleDepartment = Audit.ResponsibleDepartment;
            _leadAuditor = Audit.LeadAuditor;
            _auditorTeam = Audit.AuditorTeam;
            _contactPerson = Audit.ContactPerson;
            _auditLocation = Audit.AuditLocation;
            _scheduledStartDate = Audit.ScheduledStartDate;
            _scheduledEndDate = Audit.ScheduledEndDate;
            _actualStartDate = Audit.ActualStartDate;
            _actualEndDate = Audit.ActualEndDate;
            _priority = Audit.Priority;
            _status = Audit.Status;
            _executiveSummary = Audit.ExecutiveSummary;
            _notes = Audit.Notes;
        }

        if (IsNew)
        {
            _status = "Scheduled";
            _priority = "Medium";
            if (!_scheduledStartDate.HasValue)
                _scheduledStartDate = DateTime.Today.AddDays(7);
            if (!_scheduledEndDate.HasValue)
                _scheduledEndDate = _scheduledStartDate?.AddDays(5);
        }
    }
    #endregion

    #region Form Validation
    private bool ValidateForm()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(_code))
            errors.Add("Audit code is required");

        if (string.IsNullOrWhiteSpace(_name))
            errors.Add("Audit name is required");

        if (string.IsNullOrWhiteSpace(_auditType))
            errors.Add("Audit type is required");

        if (string.IsNullOrWhiteSpace(_scope))
            errors.Add("_scope is required");

        if (string.IsNullOrWhiteSpace(_responsibleDepartment))
            errors.Add("Responsible department is required");

        if (string.IsNullOrWhiteSpace(_leadAuditor))
            errors.Add("Lead auditor is required");

        if (!_scheduledStartDate.HasValue)
            errors.Add("Scheduled start date is required");

        if (!_scheduledEndDate.HasValue)
            errors.Add("Scheduled end date is required");

        if (_scheduledStartDate.HasValue && _scheduledEndDate.HasValue && _scheduledEndDate <= _scheduledStartDate)
            errors.Add("Scheduled end date must be after start date");

        if (_actualStartDate.HasValue && _actualEndDate.HasValue && _actualEndDate <= _actualStartDate)
            errors.Add("Actual end date must be after actual start date");

        if (_status == "In Progress" && !_actualStartDate.HasValue)
            errors.Add("Actual start date is required when status is 'In Progress'");

        if (_status == "Completed" && (!_actualStartDate.HasValue || !_actualEndDate.HasValue))
            errors.Add("Both actual start and end dates are required when status is 'Completed'");

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
    private async Task SaveAudit()
    {
        if (!ValidateForm()) return;

        try
        {
            _isSubmitting = true;
            StateHasChanged();

            if (IsNew)
            {
                var command = new CreateSMSAuditCommand(
                    code: _code!,
                    name: _name!,
                    description: _description,
                    auditPlanCode: _auditPlanCode,
                    auditType: _auditType!,
                    scope: _scope!,
                    objectives: _objectives,
                    scheduledStartDate: _scheduledStartDate!.Value,
                    scheduledEndDate: _scheduledEndDate!.Value,
                    actualStartDate: _actualStartDate,
                    actualEndDate: _actualEndDate,
                    leadAuditor: _leadAuditor!,
                    auditorTeam: _auditorTeam,
                    responsibleDepartment: _responsibleDepartment!,
                    contactPerson: _contactPerson,
                    auditLocation: _auditLocation,
                    status: _status!,
                    priority: _priority!,
                    executiveSummary: _executiveSummary,
                    notes: _notes,
                    createdBy: "CURRENT_USER"
                );

                var result = await _mediator.SendAsync(command, CancellationToken.None);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Audit created successfully: {_code}", _code);
                    await _notificationHelper.ShowSuccessAsync("Audit created successfully");
                    _dialogService.Close(true);
                }
                else
                {
                    _logger.LogError("Failed to create audit: {Error}", result.Error?.Message);
                    await _notificationHelper.ShowErrorAsync($"Failed to create audit: {result.Error?.Message}");
                }
            }
            else
            {
                // Update the existing audit entity with form data
                Audit.Code = _code!;
                Audit.Name = _name!;
                Audit.Description = _description ?? string.Empty;
                Audit.AuditPlanCode = _auditPlanCode ?? string.Empty;
                Audit.AuditType = _auditType!;
                Audit.Scope = _scope!;
                Audit.Objectives = _objectives ?? string.Empty;
                Audit.ResponsibleDepartment = _responsibleDepartment!;
                Audit.LeadAuditor = _leadAuditor!;
                Audit.AuditorTeam = _auditorTeam ?? string.Empty;
                Audit.ContactPerson = _contactPerson ?? string.Empty;
                Audit.AuditLocation = _auditLocation ?? string.Empty;
                Audit.ScheduledStartDate = _scheduledStartDate!.Value;
                Audit.ScheduledEndDate = _scheduledEndDate!.Value;
                Audit.ActualStartDate = _actualStartDate;
                Audit.ActualEndDate = _actualEndDate;
                Audit.Priority = _priority!;
                Audit.Status = _status!;
                Audit.ExecutiveSummary = _executiveSummary ?? string.Empty;
                Audit.Notes = _notes ?? string.Empty;
                Audit.UpdatedBy = "CURRENT_USER";
                Audit.UpdatedDate = DateTime.UtcNow;

                var command = new UpdateSMSAuditCommand(Audit);
                var result = await _mediator.SendAsync(command, CancellationToken.None);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Audit updated successfully: {_code}", _code);
                    await _notificationHelper.ShowSuccessAsync("Audit updated successfully");
                    _dialogService.Close(true);
                }
                else
                {
                    _logger.LogError("Failed to update audit: {Error}", result.Error?.Message);
                    await _notificationHelper.ShowErrorAsync($"Failed to update audit: {result.Error?.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving audit");
            await _notificationHelper.ShowErrorAsync("Error saving audit");
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

    private void OnScheduledStartDateChanged(DateTime? value)
    {
        _scheduledStartDate = value;

        // Automatically adjust end date if start date changes
        if (value.HasValue && (!_scheduledEndDate.HasValue || _scheduledEndDate.Value <= value.Value))
        {
            _scheduledEndDate = value.Value.AddDays(5);
        }

        StateHasChanged();
    }

    private void OnScheduledEndDateChanged(DateTime? value)
    {
        _scheduledEndDate = value;
        StateHasChanged();
    }

    private void OnActualStartDateChanged(DateTime? value)
    {
        _actualStartDate = value;
        StateHasChanged();
    }

    private void OnActualEndDateChanged(DateTime? value)
    {
        _actualEndDate = value;
        StateHasChanged();
    }

    private void OnStatusChanged(object args)
    {
        _status = args?.ToString();

        // Set actual dates based on status
        if (_status == "In Progress" && !_actualStartDate.HasValue)
        {
            _actualStartDate = DateTime.Now;
        }
        else if (_status == "Completed" && !_actualEndDate.HasValue)
        {
            _actualEndDate = DateTime.Now;
        }

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

            _code = $"{prefix}-{year}{month}-{random}";
            StateHasChanged();
        }
    }

    private bool _showActualDates => _status == "In Progress" || _status == "Completed";
    #endregion
}


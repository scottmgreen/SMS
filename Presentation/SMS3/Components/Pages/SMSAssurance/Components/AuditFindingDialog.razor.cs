using SMS_Domain.Enums;
using SMS3.Components.Shared.UIHelpers;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen.Blazor;
using SMS_Application.Commands;
using SMS_Domain.Entities;
using SMS_Shared.Common;


namespace SMS3.Components.Pages.SMSAssurance.Components;

public partial class AuditFindingDialog : ComponentBase
{
    #region Parameters
    [Parameter] public string AuditCode { get; set; } = string.Empty;
    [Parameter] public SMSAuditFinding? Finding { get; set; }
    [Parameter] public bool IsNew { get; set; } = true;
    #endregion

    #region Injected Services
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<AuditFindingDialog> _logger { get; set; } = default!;
    [Inject] private INotificationHelper _notificationHelper { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;
    #endregion

    #region Component State
    private bool IsSubmitting { get; set; } = false;
    private FindingViewModel ViewModel { get; set; } = new();
    #endregion

    #region Lifecycle Methods
    protected override void OnInitialized()
    {
        if (IsNew)
        {
            ViewModel = new FindingViewModel
            {
                DiscoveredDate = DateTime.Today,
                TargetResolutionDate = DateTime.Today.AddDays(30),
                Severity = "Minor",
                Category = "Process",
                Status = "Open"
            };
        }
        else if (Finding is not null)
        {
            ViewModel = new FindingViewModel
            {
                Code = Finding.Code!,
                Title = Finding.Title!,
                Description = Finding.Description!,
                Severity = Finding.Severity!,
                Category = Finding.Category!,
                Status = Finding.Status!,
                DiscoveredDate = Finding.DiscoveredDate,
                TargetResolutionDate = Finding.TargetResolutionDate,
                ActualResolutionDate = Finding.ActualResolutionDate,
                ResponsiblePerson = Finding.ResponsiblePerson,
                CorrectiveAction = Finding.CorrectiveAction,
                RootCauseAnalysis = Finding.RootCauseAnalysis,
                VerificationRequired = Finding.VerificationRequired,
                VerifiedBy = Finding.VerifiedBy,
                VerificationDate = Finding.VerificationDate,
                Notes = Finding.Notes
            };
        }
    }
    #endregion

    #region Event Handlers
    private async Task OnSubmit()
    {
        try
        {
            IsSubmitting = true;
            StateHasChanged();

            if (IsNew)
            {
                await CreateFinding();
            }
            else
            {
                await UpdateFinding();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting finding");
            await _notificationHelper.ShowErrorAsync("Error saving finding");
        }
        finally
        {
            IsSubmitting = false;
            StateHasChanged();
        }
    }

    private void OnCancel()
    {
        _dialogService.Close(null);
    }
    #endregion

    #region CRUD Operations
    private async Task CreateFinding()
    {
        if (!await ValidateForm()) return;

        try
        {
            // Use the existing CreateSMSAuditFindingCommand structure that matches the command handler
            var command = new CreateSMSAuditFindingCommand(
                AuditCode,
                ViewModel.Description,  // findingDescription
                ViewModel.Severity,     // severity
                ViewModel.Category,     // findingType
                "",                     // affectedArea - could map from additional field
                "",                     // requirementReference - could map from additional field
                "",                     // evidenceDescription - could map from additional field
                "CURRENT_USER"          // createdBy
            );

            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                await _notificationHelper.ShowSuccessAsync("Finding created successfully");
                _dialogService.Close(result.Value);
            }
            else
            {
                await _notificationHelper.ShowErrorAsync($"Failed to create finding: {result.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating finding");
            await _notificationHelper.ShowErrorAsync("Error creating finding");
        }
    }

    private async Task UpdateFinding()
    {
        if (!await ValidateForm()) return;

        try
        {
            // Use the existing UpdateSMSAuditFindingCommand structure
            var command = new UpdateSMSAuditFindingCommand(
                ViewModel.Code,         // findingCode
                ViewModel.Description,  // findingDescription
                ViewModel.Severity,     // severity
                ViewModel.Category,     // findingType
                "",                     // affectedArea
                "",                     // requirementReference
                "",                     // evidenceDescription
                ViewModel.RootCauseAnalysis ?? "", // rootCause
                "CURRENT_USER"          // updatedBy
            );

            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                await _notificationHelper.ShowSuccessAsync("Finding updated successfully");
                _dialogService.Close(result.Value);
            }
            else
            {
                await _notificationHelper.ShowErrorAsync($"Failed to update finding: {result.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating finding");
            await _notificationHelper.ShowErrorAsync("Error updating finding");
        }
    }
    #endregion

    #region Validation
    private async Task<bool> ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(ViewModel.Title))
        {
            await _notificationHelper.ShowErrorAsync("Finding title is required");
            return false;
        }

        if (string.IsNullOrWhiteSpace(ViewModel.Description))
        {
            await _notificationHelper.ShowErrorAsync("Finding description is required");
            return false;
        }

        if (ViewModel.TargetResolutionDate.HasValue &&
            ViewModel.TargetResolutionDate.Value < ViewModel.DiscoveredDate)
        {
            await _notificationHelper.ShowErrorAsync("Target resolution date cannot be before discovered date");
            return false;
        }

        return true;
    }
    #endregion

private RadzenTemplateForm<FindingViewModel>? findingForm;
    private readonly Variant variant = Variant.Outlined;

    public class FindingViewModel
    {
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = "Minor";
        public string Category { get; set; } = "Process";
        public string Status { get; set; } = "Open";
        public DateTime DiscoveredDate { get; set; } = DateTime.Today;
        public DateTime? TargetResolutionDate { get; set; }
        public DateTime? ActualResolutionDate { get; set; }
        public string? ResponsiblePerson { get; set; }
        public string? CorrectiveAction { get; set; }
        public string? RootCauseAnalysis { get; set; }
        public bool VerificationRequired { get; set; } = false;
        public string? VerifiedBy { get; set; }
        public DateTime? VerificationDate { get; set; }
        public string? Notes { get; set; }
    }

    public List<string> SeverityOptions { get; } = new()
    {
        "Critical", "Major", "Minor", "Observation"
    };

    public List<string> CategoryOptions { get; } = new()
    {
        "Process", "Documentation", "Training", "Equipment", "Facility", "Personnel", "Compliance", "Safety"
    };

    public List<string> StatusOptions { get; } = new()
    {
        "Open", "In Progress", "Pending Verification", "Closed", "Cancelled"
    };
}

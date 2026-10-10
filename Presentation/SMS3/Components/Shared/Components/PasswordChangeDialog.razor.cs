using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using SMS_Domain.ValueObjects;
using SMS_Shared.Common;
using SMS_Application.Commands;
using SMS_Application.Interfaces;
namespace SMS3.Components.Shared.Components;

public partial class PasswordChangeDialog
{

[Parameter] public bool IsVisible { get; set; }
    [Parameter] public string UserCode { get; set; } = string.Empty;
    [Parameter] public string UserDisplayName { get; set; } = string.Empty;
    [Parameter] public string UserType { get; set; } = string.Empty; // "Application", "Organizational", "Stakeholder"
    [Parameter] public EventCallback OnPasswordChanged { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }

    private string NewPassword { get; set; } = string.Empty;
    private string ConfirmPassword { get; set; } = string.Empty;
    private string PasswordMismatchError { get; set; } = string.Empty;
    private string ErrorMessage { get; set; } = string.Empty;
    private string SuccessMessage { get; set; } = string.Empty;
    private bool IsSaving { get; set; }
    
    // Use Domain ValueObject validation
    private Result<Password>? ValidationResult { get; set; }

    private bool IsFormValid => 
        !string.IsNullOrWhiteSpace(NewPassword) && 
        !string.IsNullOrWhiteSpace(ConfirmPassword) && 
        NewPassword == ConfirmPassword &&
        ValidationResult != null && 
        ValidationResult.IsSuccess;

    protected override void OnParametersSet()
    {
        if (!IsVisible)
        {
            // Reset form when modal is hidden
            ResetForm();
        }
    }

    private void ResetForm()
    {
        NewPassword = string.Empty;
        ConfirmPassword = string.Empty;
        PasswordMismatchError = string.Empty;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
        ValidationResult = null;
        IsSaving = false;
    }

    private async Task UpdatePassword()
    {
        if (!IsFormValid)
        {
            return;
        }

        try
        {
            IsSaving = true;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;
            StateHasChanged();

            // Smart command execution based on UserType
            var result = await ExecutePasswordUpdateCommand();

            if (result.IsSuccess)
            {
                SuccessMessage = $"Password updated successfully for {UserDisplayName}!";
                StateHasChanged();

                // Wait a moment to show success message, then close
                await Task.Delay(1500);
                await NotifyPasswordChanged();
                await Cancel();
            }
            else
            {
                ErrorMessage = result.Error?.Message ?? "Failed to update password.";
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred while updating the password: {ex.Message}";
            StateHasChanged();
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private async Task<Result<bool>> ExecutePasswordUpdateCommand()
    {
        var userTypeEnum = SMS_Domain.Enums.SMSUserType.FromValue(UserType.ToUpperInvariant());
        if (userTypeEnum == null)
        {
            return Result<bool>.Failure<bool>(new Error("InvalidUserType", $"Unknown user type: {UserType}"));
        }

        if (userTypeEnum == SMS_Domain.Enums.SMSUserType.Application)
            return await ExecuteApplicationUserPasswordUpdate();
        if (userTypeEnum == SMS_Domain.Enums.SMSUserType.Organizational)
            return await ExecuteOrganizationalUserPasswordUpdate();
        if (userTypeEnum == SMS_Domain.Enums.SMSUserType.Stakeholder)
            return await ExecuteStakeholderUserPasswordUpdate();

        return Result<bool>.Failure<bool>(new Error("InvalidUserType", $"Unknown user type: {UserType}"));
    }

    private async Task<Result<bool>> ExecuteApplicationUserPasswordUpdate()
    {
        var command = new UpdateSMSApplicationUserPasswordCommand(UserCode, NewPassword);
        return await Mediator.SendAsync(command, CancellationToken.None);
    }

    private async Task<Result<bool>> ExecuteOrganizationalUserPasswordUpdate()
    {
        var command = new UpdateSMSOrganizationalUserPasswordCommand(UserCode, NewPassword);
        return await Mediator.SendAsync(command, CancellationToken.None);
    }

    private async Task<Result<bool>> ExecuteStakeholderUserPasswordUpdate()
    {
        var command = new UpdateSMSStakeholderUserPasswordCommand(UserCode, NewPassword);
        return await Mediator.SendAsync(command, CancellationToken.None);
    }

    private async Task NotifyPasswordChanged()
    {
        await OnPasswordChanged.InvokeAsync();
    }

    private async Task Cancel()
    {
        ResetForm();
        await OnCancel.InvokeAsync();
    }

    private void ValidatePassword(ChangeEventArgs e)
    {
        var password = e.Value?.ToString() ?? string.Empty;
        NewPassword = password;
        
        // Use Domain Password ValueObject validation
        ValidationResult = Password.Create(password);
        
        // Check password match if confirm password is filled
        ValidatePasswordMatch();
        
        StateHasChanged();
    }

    private void ValidatePasswordMatch(ChangeEventArgs? e = null)
    {
        if (e != null)
        {
            ConfirmPassword = e.Value?.ToString() ?? string.Empty;
        }

        PasswordMismatchError = string.Empty;

        if (!string.IsNullOrWhiteSpace(NewPassword) && !string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            if (NewPassword != ConfirmPassword)
            {
                PasswordMismatchError = "Passwords do not match.";
            }
        }

        StateHasChanged();
    }
}



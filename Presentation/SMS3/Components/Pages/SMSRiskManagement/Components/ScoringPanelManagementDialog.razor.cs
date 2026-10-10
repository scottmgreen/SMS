using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
namespace SMS3.Components.Pages.SMSRiskManagement.Components;

public partial class ScoringPanelManagementDialog
{

[Parameter] public string HazardCode { get; set; } = string.Empty;
    [Parameter] public string HazardDescription { get; set; } = string.Empty;
    [Parameter] public List<SMSStakeholderUser> AvailableStakeholders { get; set; } = new();
    [Parameter] public List<string> SelectedStakeholderCodes { get; set; } = new();

    [Inject] private DialogService DialogService { get; set; } = default!;

    private List<string> WorkingSelectedCodes = new();

    protected override void OnInitialized()
    {
        // Create a copy of the selected codes to work with
        WorkingSelectedCodes = SelectedStakeholderCodes?.ToList() ?? new List<string>();
        
        // Debug logging
        Console.WriteLine($"ScoringPanelManagementDialog initialized with {SelectedStakeholderCodes?.Count ?? 0} pre-selected codes");
        Console.WriteLine($"Working codes: {string.Join(", ", WorkingSelectedCodes)}");
    }

    private bool IsStakeholderSelected(string stakeholderCode)
    {
        var isSelected = WorkingSelectedCodes.Any(code => 
            string.Equals(code?.Trim(), stakeholderCode?.Trim(), StringComparison.OrdinalIgnoreCase));
        Console.WriteLine($"Checking if {stakeholderCode} is selected: {isSelected}");
        return isSelected;
    }

    private void ToggleStakeholder(string stakeholderCode, bool isSelected)
    {
        Console.WriteLine($"Toggling {stakeholderCode} to {isSelected}");
        
        // Use case-insensitive comparison and trim whitespace
        var existingCode = WorkingSelectedCodes.FirstOrDefault(code => 
            string.Equals(code?.Trim(), stakeholderCode?.Trim(), StringComparison.OrdinalIgnoreCase));
        
        if (isSelected && existingCode == null)
        {
            WorkingSelectedCodes.Add(stakeholderCode?.Trim() ?? string.Empty);
            Console.WriteLine($"Added {stakeholderCode}. Working codes now: {string.Join(", ", WorkingSelectedCodes)}");
        }
        else if (!isSelected && existingCode != null)
        {
            WorkingSelectedCodes.Remove(existingCode);
            Console.WriteLine($"Removed {stakeholderCode}. Working codes now: {string.Join(", ", WorkingSelectedCodes)}");
        }
        
        StateHasChanged(); // Force UI refresh
    }

    private void CloseDialog()
    {
        DialogService.Close();
    }

    private void CloseIfClickedOutside(Microsoft.AspNetCore.Components.Web.MouseEventArgs e)
    {
        // This method will be used to close when clicking outside the dialog
        // For now, we'll just close the dialog
        CloseDialog();
    }

    private void SaveChanges()
    {
        Console.WriteLine($"Saving changes with {WorkingSelectedCodes.Count} selected codes: {string.Join(", ", WorkingSelectedCodes)}");
        DialogService.Close(WorkingSelectedCodes);
    }

    private static string GetOrganizationDisplayName(string? organizationCode)
    {
        if (string.IsNullOrWhiteSpace(organizationCode))
        {
            return string.Empty;
        }

        var organization = SMSOrganization.FromValue(organizationCode.Trim());
        return organization?.Name ?? organizationCode.Trim();
    }

    private static string GetStakeholderDisplayLabel(SMSStakeholderUser stakeholder)
    {
        if (string.IsNullOrWhiteSpace(stakeholder?.Title))
        {
            return stakeholder?.DisplayName ?? string.Empty;
        }

        var titleDisplay = GetJobTitleDisplayName(stakeholder.Title);
        return $"{stakeholder.DisplayName} - {titleDisplay}";
    }

    private static string GetJobTitleDisplayName(string? titleValue)
    {
        if (string.IsNullOrWhiteSpace(titleValue))
        {
            return string.Empty;
        }

        var jobTitle = SMSJobTitle.FromValue(titleValue.Trim());
        return jobTitle?.Name ?? titleValue.Trim();
    }
}



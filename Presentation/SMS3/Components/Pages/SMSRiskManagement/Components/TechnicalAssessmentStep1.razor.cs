using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using SMS3.Components.Pages.SMSRiskManagement;
using SMS3.Components.Pages.SMSRiskManagement.Models;
using SMS3.Components.Shared;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using global::System.Text.Json;
namespace SMS3.Components.Pages.SMSRiskManagement.Components;

public partial class TechnicalAssessmentStep1
{

[Parameter] public Step1Model Step1 { get; set; } = new();
    [Parameter] public EventCallback<Step1Model> Step1Changed { get; set; }
    [Parameter] public List<SMSApplicationUser> AvailableAssessors { get; set; } = new();
    [Parameter] public List<SMSStakeholderGroup> StakeholderGroups { get; set; } = new();
    [Parameter] public List<SMSStakeholderUser> AvailableStakeholders { get; set; } = new();

    protected override void OnInitialized()
    {
        // Ensure Step1 and its collections are initialized
        if (Step1 == null)
        {
            Step1 = new Step1Model();
        }
        
        if (Step1.SelectedStakeholderGroupIds == null)
        {
            Step1.SelectedStakeholderGroupIds = new List<string>();
        }
        
        if (Step1.SelectedIndividualStakeholderIds == null)
        {
            Step1.SelectedIndividualStakeholderIds = new List<string>();
        }
    }

    private static bool IsInvalid(string? value, int minLength)
    {
        return string.IsNullOrWhiteSpace(value) || value.Trim().Length < minLength;
    }

    private bool IsLeadAssessorInvalid()
    {
        return string.IsNullOrWhiteSpace(Step1?.LeadAssessor);
    }

    private string GetFieldValidation(string? text, int minLength)
    {
        var length = text?.Trim().Length ?? 0;
        
        if (length == 0)
            return $"0/{minLength} characters";
        else if (length < minLength)
            return $"{length}/{minLength} characters (minimum {minLength} required)";
        else
            return $"{length} characters ✓";
    }

    private string GetLeadAssessorValidation()
    {
        if (string.IsNullOrEmpty(Step1.LeadAssessor))
        {
            return "Please select a lead assessor";
        }

        var selectedAssessor = AvailableAssessors?.FirstOrDefault(a => a.Code == Step1.LeadAssessor);
        if (!ReferenceEquals(selectedAssessor, null))
        {
            return $"Selected: {selectedAssessor.DisplayName} ✓";
        }

        return "Invalid assessor selection";
    }

    private async Task OnLeadAssessorChanged(object value)
    {
        if (value != null)
        {
            Step1.LeadAssessor = value.ToString() ?? string.Empty;
            await Step1Changed.InvokeAsync(Step1);
        }
    }

    private async Task OnStep1FieldChanged(ChangeEventArgs _)
    {
        await Step1Changed.InvokeAsync(Step1);
    }

    private async Task AddStakeholderGroup(string groupCode)
    {
        if (!Step1.SelectedStakeholderGroupIds.Contains(groupCode))
        {
            Step1.SelectedStakeholderGroupIds.Add(groupCode);
            UpdateStakeholderGroupsString();
            await Step1Changed.InvokeAsync(Step1);
        }
    }

    private async Task RemoveStakeholderGroup(string groupCode)
    {
        if (Step1.SelectedStakeholderGroupIds.Contains(groupCode))
        {
            Step1.SelectedStakeholderGroupIds.Remove(groupCode);
            UpdateStakeholderGroupsString();
            await Step1Changed.InvokeAsync(Step1);
        }
    }

    private async Task AddIndividualStakeholder(string stakeholderCode)
    {
        if (!Step1.SelectedIndividualStakeholderIds.Contains(stakeholderCode))
        {
            Step1.SelectedIndividualStakeholderIds.Add(stakeholderCode);
            UpdateIndividualStakeholdersString();
            await Step1Changed.InvokeAsync(Step1);
        }
    }

    private async Task RemoveIndividualStakeholder(string stakeholderCode)
    {
        if (Step1.SelectedIndividualStakeholderIds.Contains(stakeholderCode))
        {
            Step1.SelectedIndividualStakeholderIds.Remove(stakeholderCode);
            UpdateIndividualStakeholdersString();
            await Step1Changed.InvokeAsync(Step1);
        }
    }

    private void UpdateStakeholderGroupsString()
    {
        if (Step1?.SelectedStakeholderGroupIds != null && StakeholderGroups != null)
        {
            var groupNames = Step1.SelectedStakeholderGroupIds
                .Select(id => StakeholderGroups?.FirstOrDefault(g => g.Code == id)?.Name)
                .Where(name => !string.IsNullOrEmpty(name));
            
            Step1.StakeholderGroups = string.Join(", ", groupNames!);
        }
    }

    private void UpdateIndividualStakeholdersString()
    {
        if (Step1?.SelectedIndividualStakeholderIds != null && AvailableStakeholders != null)
        {
            var stakeholderNames = Step1.SelectedIndividualStakeholderIds
                .Select(id => AvailableStakeholders?.FirstOrDefault(s => s.Code == id)?.DisplayName)
                .Where(name => !string.IsNullOrEmpty(name));
            
            Step1.SelectedIndividualStakeholders = string.Join(", ", stakeholderNames!);
        }
    }
}



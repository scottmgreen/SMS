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

public partial class TechnicalAssessmentStep2
{

[Parameter] public Step1Model Step1 { get; set; } = new();
    [Parameter] public Step2Model Step2 { get; set; } = new();
    [Parameter] public EventCallback<Step2Model> Step2Changed { get; set; }
    [Parameter] public List<Hazard> Step2Hazards { get; set; } = new();
    [Parameter] public EventCallback<Hazard> OnHazardAdded { get; set; }
    [Parameter] public EventCallback<Hazard> OnHazardUpdated { get; set; }
    [Parameter] public EventCallback<Hazard> OnHazardDeleted { get; set; }
    [Parameter] public string? ReportId { get; set; }
    [Parameter] public string? InitialHazardId { get; set; }
    [Parameter] public string? RiskAssessmentId { get; set; }
    [Inject] private ILogger<TechnicalAssessmentStep2>? Logger { get; set; }

    private bool ShowAddHazardDialog { get; set; } = false;
    private bool IsEditMode { get; set; } = false; // NEW: Track if we're in edit mode
    private Hazard? EditingHazard { get; set; } = null; // NEW: Hazard being edited
    private bool _showDescriptionModal = false;
    private string _selectedDescription = string.Empty;
    private string _selectedHazardId = string.Empty;

    private void ShowDescriptionDialog(Hazard hazard)
    {
        _selectedDescription = hazard.Description ?? "No description available";
        _selectedHazardId = hazard.Code;
        _showDescriptionModal = true;
    }

    private void CloseDescriptionModal()
    {
        _showDescriptionModal = false;
        _selectedDescription = string.Empty;
        _selectedHazardId = string.Empty;
    }

    private void OpenAddHazardDialog()
    {
        IsEditMode = false;
        EditingHazard = null;
        ShowAddHazardDialog = true;
    }

    private async Task OnModalVisibilityChanged(bool isVisible)
    {
        ShowAddHazardDialog = isVisible;
        
        // Reset edit mode when dialog closes
        if (!isVisible)
        {
            IsEditMode = false;
            EditingHazard = null;
        }
        
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Determine if a hazard is the initial hazard that should not be deleted
    /// </summary>
    private bool IsInitialHazard(Hazard hazard)
    {
        // Check if this is the initial hazard based on:
        // 1. HazardId parameter match
        // 2. If it's the first/primary hazard
        // 3. If it has special indicators

        if (!string.IsNullOrEmpty(InitialHazardId) && hazard.Code == InitialHazardId)
        {
            return true;
        }

        return false;
    }

    private async Task EditHazard(Hazard hazard)
    {
        try
        {
            IsEditMode = true;
            EditingHazard = hazard;
            ShowAddHazardDialog = true;
            
            await InvokeAsync(StateHasChanged);
            
            Logger?.LogInformation("Opening edit dialog for hazard: {HazardCode}", hazard.Code);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error opening edit dialog for hazard: {HazardCode}", hazard.Code);
        }
    }

    private async Task DeleteHazard(Hazard hazard)
    {
        if (IsInitialHazard(hazard))
        {
            // Should not reach here due to UI logic, but safety check
            return;
        }

        try
        {
            // Invoke the parent's delete callback
            await OnHazardDeleted.InvokeAsync(hazard);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting hazard {hazard.Code}: {ex.Message}");
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        // Force UI refresh when AvailableHazards parameter changes
        await InvokeAsync(StateHasChanged);
    }
}



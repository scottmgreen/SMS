using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using SMS_Application.Common;
using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
namespace SMS3.Components.Pages.SMSAssurance.Components;

public partial class SPIEditDialog
{

[Inject] private DialogService DialogService { get; set; } = default!;

    [Parameter] public SafetyPerformanceIndicator SPI { get; set; } = default!;
    [Parameter] public bool IsEditMode { get; set; } = false;
    [Parameter] public List<SMSSafetyPerformanceIndicatorType> AvailableTypes { get; set; } = new();
    [Parameter] public List<SPIStatus> AvailableStatuses { get; set; } = new();
    [Parameter] public List<SPIMeasurementFrequency> AvailableFrequencies { get; set; } = new();
    [Parameter] public List<string> AvailableDepartments { get; set; } = new();
    [Inject] private ICurrentUserService _currentUserService { get; set; } = default!;

    private string SelectedStatus { get; set; } = string.Empty;
    private string SelectedIndicatorType { get; set; } = string.Empty;
    private string SelectedFrequency { get; set; } = string.Empty;
    private bool IsSaving { get; set; } = false;

    private bool IsValid => 
        !string.IsNullOrWhiteSpace(SPI?.Name) &&
        !string.IsNullOrWhiteSpace(SelectedIndicatorType);

    protected override void OnInitialized()
    {
        if (SPI is not null)
        {
            SelectedStatus = SPI.Status?.Value ?? SPIStatus.Active.Value;
            SelectedIndicatorType = SPI.IndicatorType?.Value ?? string.Empty;
            SelectedFrequency = SPI.MeasurementFrequency?.Value ?? SPIMeasurementFrequency.Monthly.Value;
        }
    }

    private async Task SaveSPI()
    {
        if (!IsValid) return;

        try
        {
            IsSaving = true;
            
            // Update SPI with selected values
            var selectedStatusEnum = AvailableStatuses.FirstOrDefault(s => s.Value == SelectedStatus) ?? SPIStatus.Active;
            var selectedTypeEnum = AvailableTypes.FirstOrDefault(t => t.Value == SelectedIndicatorType);
            var selectedFrequencyEnum = AvailableFrequencies.FirstOrDefault(f => f.Value == SelectedFrequency) ?? SPIMeasurementFrequency.Monthly;

            if (selectedTypeEnum != null)
            {
                SPI.UpdateStatus(selectedStatusEnum, _currentUserService.UserCode);
                SPI.MeasurementFrequency = selectedFrequencyEnum;
                
                DialogService.Close(SPI);
            }
        }
        finally
        {
            IsSaving = false;
        }
    }

    // Computed properties for dropdown display
    private List<SPITypeDisplayItem> GetTypeDisplayItems() => AvailableTypes
        .Select(t => new SPITypeDisplayItem 
        { 
            Value = t.Value, 
            DisplayText = $"{t.Name} - {t.Description}" 
        }).ToList();

    /// <summary>
    /// Gets the standard data source options for consistency across dialogs
    /// </summary>
    private List<string> GetDataSourceOptions()
    {
        return SPIConstants.SPIDataSources.GetAll();
    }

    public class SPITypeDisplayItem
    {
        public string Value { get; set; } = string.Empty;
        public string DisplayText { get; set; } = string.Empty;
    }
}



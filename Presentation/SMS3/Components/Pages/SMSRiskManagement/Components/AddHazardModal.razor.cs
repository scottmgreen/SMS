using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Domain.ValueObjects;
using SMS_Application.Messaging.Commands;
using SMS_Application.Interfaces;

namespace SMS3.Components.Pages.SMSRiskManagement.Components;

public partial class AddHazardModal : ComponentBase
{
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public EventCallback<Hazard> OnHazardAdded { get; set; }
    [Parameter] public string? ReportId { get; set; }

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private IMediator Mediator { get; set; } = default!;

    // Form properties
    private string NewHazardDescription { get; set; } = string.Empty;
    private string NewHazardCategory { get; set; } = string.Empty;
    private string NewHazardFiveM { get; set; } = string.Empty;

    // Location properties
    private bool ShowMapModal { get; set; } = false;
    private decimal SelectedLatitude { get; set; } = 0;
    private decimal SelectedLongitude { get; set; } = 0;
    private string LocationDescription { get; set; } = string.Empty;
    private SMS_Domain.Entities.HazardLocation? SelectedGeoLocation { get; set; }

    // Location computed properties
    private bool HasGeoLocation => SelectedGeoLocation != null;
    private bool HasValidCoordinates => SelectedLatitude != 0 && SelectedLongitude != 0;
    private string GeoLocationDisplay => HasValidCoordinates ? $"{SelectedLatitude:F6}, {SelectedLongitude:F6}" : "";
    private string LocationDisplayText => HasGeoLocation && !string.IsNullOrEmpty(SelectedGeoLocation?.Description) 
        ? SelectedGeoLocation.Description 
        : HasValidCoordinates 
            ? GeoLocationDisplay 
            : "No location selected";

    // Form validation
    private bool IsFormValid => 
        !string.IsNullOrWhiteSpace(NewHazardDescription?.Trim()) && 
        NewHazardDescription.Trim().Length >= 10 &&
        !string.IsNullOrWhiteSpace(NewHazardCategory) && 
        !string.IsNullOrWhiteSpace(NewHazardFiveM) &&
        HasGeoLocation;

    private List<string> HazardCategories = new()
    {
        "Aircraft Operations",
        "Ground Operations", 
        "Security Operations",
        "Weather Related",
        "Equipment Failure",
        "Human Factors",
        "Environmental",
        "Procedural",
        "Communication",
        "Infrastructure",
        "Other"
    };

    private List<FiveMOption> FiveMOptions = new()
    {
        new() { Value = "MAN", Text = "Man (Human)" },
        new() { Value = "MACHINE", Text = "Machine (Equipment)" },
        new() { Value = "METHOD", Text = "Method (Procedures)" },
        new() { Value = "MATERIAL", Text = "Material (Resources)" },
        new() { Value = "MILIEU", Text = "Milieu (Environment)" }
    };

    private async Task CloseModal()
    {
        IsVisible = false;
        await IsVisibleChanged.InvokeAsync(IsVisible);
        ResetForm();
    }

    private async Task AddHazard()
    {
        if (!IsFormValid) return;

        try
        {
            // Create hazard
            HazardID hazardID = new HazardID("HZ-0000");
            Hazard hazard = new Hazard(hazardID);

            // Set properties
            hazard.Code = "HZ-0000";
            hazard.Name = NewHazardDescription.Trim(); 
            hazard.Description = NewHazardDescription.Trim();
            hazard.Category = NewHazardCategory;
            hazard.ReportCode = ReportId ?? ""; 
            hazard.ReportedBy = "Technical Assessment User"; 
            hazard.ReportingDepartment = "Technical Assessment";
            hazard.HazardType = NewHazardCategory;
            hazard.FiveMComponent = GetFiveMComponent(NewHazardFiveM);
            hazard.IsConfidential = false;
            hazard.IsAnonymous = false;
            hazard.Status = HazardStatus.Active; 
            hazard.Priority = HazardPriority.Medium; 
            hazard.ReportedOn = DateTime.UtcNow;

            // First, create the hazard
            var createHazardCommand = new CreateHazardCommand(hazard);
            var hazardResult = await Mediator.SendAsync(createHazardCommand, CancellationToken.None);

            if (!hazardResult.IsSuccess)
            {
                // Handle error silently for now
                return;
            }

            var createdHazard = hazardResult.Value;

            // Now create and save the HazardLocation if we have location data
            if (HasGeoLocation && SelectedGeoLocation != null)
            {
                // Update the location with the actual hazard code
                SelectedGeoLocation.HazardCode = createdHazard.Code;
                SelectedGeoLocation.Code = $"HL-{createdHazard.Code}-{DateTime.UtcNow:yyyyMMdd}";

                // Save the HazardLocation to database
                var createLocationCommand = new CreateHazardLocationCommand(SelectedGeoLocation);
                var locationResult = await Mediator.SendAsync(createLocationCommand, CancellationToken.None);

                if (locationResult.IsSuccess)
                {
                    var createdLocation = locationResult.Value;
                    
                    // Only update the display text for backward compatibility
                    // Don't update hazard references to avoid duplicate creation
                    createdHazard.LocationArea = LocationDisplayText;
                    
                    // Note: We don't set createdHazard.HazardLocation or createdHazard.Location here
                    // because that would trigger EF to create another HazardLocation entity.
                    // The relationship is established via the HazardCode field in the HazardLocation.
                }
            }

            // Invoke callback with the created hazard
            await OnHazardAdded.InvokeAsync(createdHazard);
            await CloseModal();
        }
        catch (Exception)
        {
            // Handle error silently
        }
    }

    private void ResetForm()
    {
        NewHazardDescription = string.Empty;
        NewHazardCategory = string.Empty;
        NewHazardFiveM = string.Empty;
        SelectedGeoLocation = null;
        SelectedLatitude = 0;
        SelectedLongitude = 0;
        LocationDescription = string.Empty;
    }

    private FiveMComponent? GetFiveMComponent(string? value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        
        return value switch
        {
            "MAN" => FiveMComponent.Man,
            "MACHINE" => FiveMComponent.Machine,
            "METHOD" => FiveMComponent.Method,
            "MATERIAL" => FiveMComponent.Material,
            "MILIEU" => FiveMComponent.Milieu,
            _ => null
        };
    }

    #region Map Functionality

    private async Task OpenMapSelector()
    {
        ShowMapModal = true;
        StateHasChanged();

        await Task.Delay(100);
        await InitializeMap();
    }

    private void CloseMapSelector()
    {
        ShowMapModal = false;
        StateHasChanged();
    }

    private void ClearMapSelection()
    {
        SelectedLatitude = 0;
        SelectedLongitude = 0;
        LocationDescription = string.Empty;
        SelectedGeoLocation = null;
        StateHasChanged();
    }

    private void UseSelectedLocation()
    {
        if (HasValidCoordinates)
        {
            SelectedGeoLocation = new SMS_Domain.Entities.HazardLocation()
            {
                Latitude = SelectedLatitude,
                Longitude = SelectedLongitude,
                Description = !string.IsNullOrEmpty(LocationDescription) 
                    ? LocationDescription 
                    : $"Location at {SelectedLatitude:F6}, {SelectedLongitude:F6}",
                HazardCode = "HZ-0000", // Will be updated when hazard is created
                Code = "HL-0000", // Will be updated when created
                IsValid = true,
                DateSelected = DateTime.UtcNow
            };
        }
        CloseMapSelector();
    }

    private async Task InitializeMap()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("initializeHazardLocationMap", "hazardLocationMap", 
                DotNetObjectReference.Create(this), 45.5898, -122.5951, 12);
        }
        catch (Exception)
        {
            // Handle JS error silently
        }
    }

    [JSInvokable]
    public async Task OnLocationSelectedFromMap(double latitude, double longitude)
    {
        SelectedLatitude = (decimal)latitude;
        SelectedLongitude = (decimal)longitude;
        await InvokeAsync(StateHasChanged);
    }

    #endregion

    public class FiveMOption
    {
        public string Value { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}
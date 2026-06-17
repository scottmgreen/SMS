using Microsoft.JSInterop;

using SMS_Domain.Entities;
using SMS3.Components.Pages.SMSRiskManagement.Models;
using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.SMSRiskManagement.Components;

public partial class AddHazardDialog : ComponentBase, IDisposable
{
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public EventCallback<Hazard> OnHazardAdded { get; set; }
    [Parameter] public EventCallback<Hazard> OnHazardUpdated { get; set; }
    [Parameter] public string? ReportId { get; set; }
    [Parameter] public Hazard? EditingHazard { get; set; }
    [Parameter] public bool IsEditMode { get; set; } = false;

    [Parameter] public string RiskAssessmentId { get; set; } = string.Empty;

    [Inject] ICurrentUserService _currentUserService { get; set; } = default!;
    [Inject] private IJSRuntime _jsRuntime { get; set; } = default!;
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<AddHazardDialog>? _logger { get; set; }

    // Form properties
    private string NewHazardDescription { get; set; } = string.Empty;
    private string NewHazardCategory { get; set; } = string.Empty;
    private string NewHazardType { get; set; } = string.Empty;

    // ENHANCED: Add busy state to prevent double submissions
    private bool IsSubmitting { get; set; } = false;

    // Dropdown data
    private List<DropdownOption> HazardCategoryOptions { get; set; } = new();
    private List<DropdownOption> HazardTypeOptions { get; set; } = new();

    // Location properties - EXACTLY like HazardReporting
    private bool _showMapModal { get; set; } = false;
    public decimal SelectedLatitude { get; set; }
    public decimal SelectedLongitude { get; set; }
    public string LocationDescription { get; set; } = string.Empty;
    public HazardLocation SelectedGeoLocation { get; set; } = new();

    // Location computed properties - EXACTLY like HazardReporting
    public string SelectedLatitudeText => SelectedLatitude != 0 ? SelectedLatitude.ToString("F6") : "";
    public string SelectedLongitudeText => SelectedLongitude != 0 ? SelectedLongitude.ToString("F6") : "";
    public string LocationDisplayText => HasGeoLocation ? GetSelectedLocationText() : "No location selected";
    public bool HasGeoLocation =>
        SelectedGeoLocation is not null &&
        SelectedGeoLocation.Latitude.HasValue &&
        SelectedGeoLocation.Longitude.HasValue &&
        SelectedGeoLocation.IsValidated;
    public bool HasValidCoordinates => SelectedLatitude != 0 && SelectedLongitude != 0;
    public string GeoLocationDisplay => HasGeoLocation ? $"Lat: {SelectedGeoLocation.Latitude:F6}, Lng: {SelectedGeoLocation.Longitude:F6}" : "No coordinates selected";

    // JavaScript module reference - EXACTLY like HazardReporting
    private IJSObjectReference? _mapModule;
    private DotNetObjectReference<AddHazardDialog>? _dotNetRef;

    // Airport coordinates - EXACTLY like HazardReporting
    private double _airportCenterLatitude => 45.5898;
    private double _airportCenterLongitude => -122.5951;
    private int _defaultZoomLevel => 20;

    // Form validation - UPDATED to be more lenient for debugging
    private bool IsFormValid =>
        !string.IsNullOrWhiteSpace(NewHazardDescription?.Trim()) &&
        NewHazardDescription.Trim().Length >= 10 &&
        !string.IsNullOrWhiteSpace(NewHazardCategory) &&
        !string.Equals(NewHazardCategory, HazardCategory.Default.Value, StringComparison.OrdinalIgnoreCase) &&
        !string.IsNullOrWhiteSpace(NewHazardType) &&
        !string.Equals(NewHazardType, HazardType.Default.Value, StringComparison.OrdinalIgnoreCase) && HasGeoLocation && !IsSubmitting;

    private bool IsHazardCategoryDefault =>
        string.IsNullOrWhiteSpace(NewHazardCategory) ||
        string.Equals(NewHazardCategory, HazardCategory.Default.Value, StringComparison.OrdinalIgnoreCase);

    private bool IsHazardTypeDefault =>
        string.IsNullOrWhiteSpace(NewHazardType) ||
        string.Equals(NewHazardType, HazardType.Default.Value, StringComparison.OrdinalIgnoreCase);

    private bool IsHazardDescriptionInvalid =>
        string.IsNullOrWhiteSpace(NewHazardDescription?.Trim()) ||
        NewHazardDescription.Trim().Length < 10;

    private bool IsHazardLocationInvalid => !HasGeoLocation;
        

    // UI computed properties for edit mode
    private string _modalTitle => IsEditMode ? "Edit Hazard" : "Add New Hazard";
    private string ActionButtonText => IsSubmitting 
        ? (IsEditMode ? "Updating..." : "Adding...") 
        : (IsEditMode ? "Update Hazard" : "Add Hazard");
    private string ActionButtonIcon => IsSubmitting 
        ? "hourglass_empty" 
        : (IsEditMode ? "edit" : "add");

    protected override void OnInitialized()
    {
        InitializeDropdownOptions();
        
        // Ensure SelectedGeoLocation is properly initialized
        if (SelectedGeoLocation is null)
        {
            SelectedGeoLocation = new HazardLocation
            {
                IsValid = false,
                IsValidated = false
            };
        }
        
        // Create DotNet reference for JavaScript callbacks - EXACTLY like HazardReporting
        _dotNetRef = DotNetObjectReference.Create(this);
        
        _logger?.LogInformation("AddHazardDialog initialized. SelectedGeoLocation.IsValid: {IsValid}", SelectedGeoLocation.IsValid);
    }

    protected override async Task OnParametersSetAsync()
    {
        // When parameters change, check if we need to populate edit form
        if (IsEditMode && EditingHazard is not null && IsVisible)
        {
            await PopulateFormForEdit();
        }
        else if (!IsEditMode)
        {
            ResetForm();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                // Initialize JavaScript mapping module only once - EXACTLY like HazardReporting
                _mapModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "/js/hazard-map.js");
                _logger?.LogInformation("Map module loaded successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Could not load JavaScript map module");
            }
        }
    }

    // Dispose method - EXACTLY like HazardReporting
    public void Dispose()
    {
        _mapModule?.DisposeAsync();
        _dotNetRef?.Dispose();
    }

    /// <summary>
    /// Populate the form fields with data from the hazard being edited
    /// </summary>
    private async Task PopulateFormForEdit()
    {
        if (EditingHazard is null) return;

        _logger?.LogInformation("Populating form for editing hazard: {HazardCode}", EditingHazard.Code);

        // Populate basic fields
        NewHazardDescription = EditingHazard.Description ?? string.Empty;
        NewHazardCategory = EditingHazard.HazardCategory ?? string.Empty;
        NewHazardType = EditingHazard.HazardType ?? string.Empty;

        // Load hazard types for the category if we have one
        if (!string.IsNullOrEmpty(NewHazardCategory))
        {
            await LoadHazardTypesForCategory(NewHazardCategory);
        }

        // Populate location data if available - EXACTLY like HazardReporting
        if (EditingHazard.HazardLocation is not null)
        {
            var location = EditingHazard.HazardLocation;
            SelectedLatitude = location.Latitude ?? 0;
            SelectedLongitude = location.Longitude ?? 0;
            LocationDescription = location.Description ?? string.Empty;

            SelectedGeoLocation = new HazardLocation
            {
                Latitude = location.Latitude ?? 0,
                Longitude = location.Longitude ?? 0,
                Description = location.Description,
                IsValidated = true,
                DateSelected = DateTime.UtcNow
            };

            _logger?.LogInformation("Populated location data for editing: {Lat}, {Lng}", 
                SelectedLatitude, SelectedLongitude);
        }

        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Get display text for selected location - EXACTLY like HazardReporting
    /// </summary>
    private string GetSelectedLocationText()
    {
        if (!HasGeoLocation) return "No location selected";

        var lat = SelectedGeoLocation.Latitude;
        var lng = SelectedGeoLocation.Longitude;
        return $"Lat: {lat:F6}, Lng: {lng:F6} - {SelectedGeoLocation.Description}";
    }

    /// <summary>
    /// Initialize dropdown options using Smart Enums (same as HazardReporting)
    /// </summary>
    private void InitializeDropdownOptions()
    {
        // Hazard category options from Smart Enum
        HazardCategoryOptions = HazardCategory.GetAllValues()
            .Where(hc => !string.Equals(hc.Value, HazardCategory.Default.Value, StringComparison.OrdinalIgnoreCase))
            .Select(hc => new DropdownOption(hc.Value, hc.Name))
            .ToList();

        // Initially empty hazard types (will be populated when category is selected)
        HazardTypeOptions = new List<DropdownOption>();
    }

    /// <summary>
    /// Handle hazard category selection change (same logic as HazardReporting)
    /// </summary>
    public async Task OnHazardCategoryChanged(string? categoryValue)
    {
        _logger?.LogInformation("Hazard category changed to: {Category}", categoryValue);

        NewHazardCategory = categoryValue ?? string.Empty;

        if (string.Equals(NewHazardCategory, HazardCategory.Default.Value, StringComparison.OrdinalIgnoreCase))
        {
            NewHazardCategory = string.Empty;
        }

        // Clear selected hazard type when category changes (unless we're in edit mode and repopulating)
        if (!IsEditMode || string.IsNullOrEmpty(EditingHazard?.HazardType))
        {
            NewHazardType = string.Empty;
        }

        // Load hazard types for the new category
        await LoadHazardTypesForCategory(categoryValue ?? "");
    }

    /// <summary>
    /// Load hazard types for a specific category (same logic as HazardReporting)
    /// </summary>
    private async Task LoadHazardTypesForCategory(string categoryValue)
    {
        if (string.IsNullOrEmpty(categoryValue))
        {
            HazardTypeOptions.Clear();
            return;
        }

        var category = HazardCategory.FromValue(categoryValue);
        if (category is not null)
        {
            // Filter hazard types by selected category
            HazardTypeOptions = HazardType.GetByCategory(category)
                .Where(ht => !string.Equals(ht.Value, HazardType.Default.Value, StringComparison.OrdinalIgnoreCase))
                .Select(ht => new DropdownOption(ht.Value, ht.Name))
                .ToList();

            _logger?.LogInformation("Loaded {Count} hazard types for category: {Category}",
                HazardTypeOptions.Count, category.Name);
        }
        else
        {
            HazardTypeOptions.Clear();
            _logger?.LogWarning("Category not found: {CategoryValue}", categoryValue);
        }

        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Handle hazard type selection change
    /// </summary>
    public async Task OnHazardTypeChanged(string? hazardTypeValue)
    {
        NewHazardType = hazardTypeValue ?? string.Empty;

        if (string.Equals(NewHazardType, HazardType.Default.Value, StringComparison.OrdinalIgnoreCase))
        {
            NewHazardType = string.Empty;
        }

        if (!string.IsNullOrEmpty(hazardTypeValue))
        {
            var hazardType = HazardType.FromValue(hazardTypeValue);
            if (hazardType is not null)
            {
                _logger?.LogInformation("Hazard type changed to: {HazardType}", hazardType.Name);
            }
        }

        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Get guidance text for selected hazard type (same as HazardReporting)
    /// </summary>
    public string GetHazardTypeGuidance(string? hazardTypeValue)
    {
        if (string.IsNullOrEmpty(hazardTypeValue))
            return string.Empty;

        var hazardType = HazardType.FromValue(hazardTypeValue);
        return hazardType?.GuidanceText ?? string.Empty;
    }

    /// <summary>
    /// Check if regulatory reporting is required for selected hazard type (same as HazardReporting)
    /// </summary>
    public bool RequiresRegulatoryReporting(string? hazardTypeValue)
    {
        if (string.IsNullOrEmpty(hazardTypeValue))
            return false;

        var hazardType = HazardType.FromValue(hazardTypeValue);
        return hazardType?.RequiresRegulatoryReporting ?? false;
    }

    /// <summary>
    /// Get hazard category description for display (same as HazardReporting)
    /// </summary>
    public string GetHazardCategoryDescription(string? categoryValue)
    {
        if (string.IsNullOrEmpty(categoryValue))
            return string.Empty;

        var category = HazardCategory.FromValue(categoryValue);
        return category?.Description ?? string.Empty;
    }

    private async Task CloseModal()
    {
        IsVisible = false;
        await IsVisibleChanged.InvokeAsync(IsVisible);
        ResetForm();
    }

    private async Task AddHazard()
    {
        if (!IsFormValid || IsSubmitting) return;

        try
        {
            // CRITICAL: Set submitting state to prevent duplicate submissions
            IsSubmitting = true;
            StateHasChanged();

            if (IsEditMode)
            {
                await UpdateHazard();
            }
            else
            {
                await CreateNewHazard();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Exception during hazard {Action}", IsEditMode ? "update" : "creation");
        }
        finally
        {
            // CRITICAL: Always reset submitting state
            IsSubmitting = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Update an existing hazard - Using HazardReporting logic
    /// </summary>
    private async Task UpdateHazard()
    {
        if (EditingHazard is null)
        {
            _logger?.LogError("Cannot update hazard: EditingHazard is null");
            return;
        }

        _logger?.LogInformation("Starting hazard update for: {HazardCode}", EditingHazard.Code);

        // Update the existing hazard properties
        EditingHazard.Name = NewHazardDescription.Trim();
        EditingHazard.Description = NewHazardDescription.Trim();
        EditingHazard.HazardCategory = NewHazardCategory;
        EditingHazard.HazardType = NewHazardType;
        EditingHazard.IsInitialHazard = false;
        EditingHazard.UpdatedDate = DateTime.UtcNow;
        EditingHazard.UpdatedBy = _currentUserService?.UserDisplayName;
        await UpdateHazardLocationForHazard(EditingHazard);


        // Update the hazard
        var updateHazardCommand = new UpdateHazardCommand(EditingHazard);
        var hazardResult = await _mediator.SendAsync(updateHazardCommand, CancellationToken.None);

        if (!hazardResult.IsSuccess)
        {
            _logger?.LogError("Failed to update hazard: {Error}", hazardResult.Error?.Message);
            return;
        }

        var updatedHazard = hazardResult.Value;

        // Handle location updates - EXACTLY like HazardReporting

        await UpdateHazardLocation(updatedHazard);



        _logger?.LogInformation("Successfully updated hazard: {HazardCode} with Category: {Category}, Type: {Type}", 
            updatedHazard?.Code, NewHazardCategory, NewHazardType);

        // Invoke callback with the updated hazard
        await OnHazardUpdated.InvokeAsync(updatedHazard);
        await CloseModal();

        _logger?.LogInformation("Hazard update process completed for: {HazardCode}", updatedHazard?.Code);
    }

    /// <summary>
    /// Create a new hazard - Using HazardReporting logic
    /// </summary>
    private async Task CreateNewHazard()
    {
        
        _logger?.LogInformation("Starting hazard creation with description: {Description}, Category: {Category}, Type: {Type}", NewHazardDescription?.Trim(), NewHazardCategory, NewHazardType);

        // Create hazard
        HazardID hazardID = new HazardID("HZ-0000");
        Hazard hazard = new Hazard(hazardID);

        // Set properties using the selected category and type
        hazard.Code = "HZ-0000";
        hazard.Name = $"{NewHazardCategory} - {NewHazardType}";
        hazard.Description = NewHazardDescription?.Trim() ?? "";
        hazard.HazardCategory = NewHazardCategory;
        hazard.HazardType = NewHazardType;
        hazard.IsInitialHazard = false;
        hazard.ReportCode = ReportId ?? "";
        
        hazard.Status = HazardStatus.InitialRiskAssessment;
        
        hazard.CreatedBy = _currentUserService?.UserDisplayName;
        hazard.CreatedDate = DateTime.UtcNow;
        // Handle location for new hazard - EXACTLY like HazardReporting
        await UpdateHazardLocationForHazard(hazard);

        // Create the hazard
        var createHazardCommand = new CreateHazardCommand(hazard);
        var hazardResult = await _mediator.SendAsync(createHazardCommand, CancellationToken.None);

        if (!hazardResult.IsSuccess)
        {
            _logger?.LogError("Failed to create hazard: {Error}", hazardResult.Error?.Message);
            return;
        }

        var createdHazard = hazardResult.Value;
        _logger?.LogInformation("Successfully created hazard: {HazardCode} with Category: {Category}, Type: {Type}", createdHazard.Code, NewHazardCategory, NewHazardType);

        //Create RiskAnalysis 
        
        
        RiskAnalysis riskAnalysis = new RiskAnalysis(new RiskAnalysisID("RA-0000"));
        riskAnalysis.HazardCode = createdHazard.Code;
        riskAnalysis.RiskAssessmentCode = RiskAssessmentId ?? "";
        var createRiskAnalysis = new CreateRiskAnalysisCommand(riskAnalysis);
        var riskAnalysisResult = await _mediator.SendAsync(createRiskAnalysis, CancellationToken.None);
        if (!riskAnalysisResult.IsSuccess)
        {
            _logger?.LogError("Failed to get RiskAssessment for Hazard: {Error}", hazardResult.Error?.Message);
            return;
        }


        // Create the HazardLocation after hazard is created
        if (HasGeoLocation)
        {
            await CreateHazardLocation(createdHazard);
        }

        



        // Invoke callback with the created hazard
        await OnHazardAdded.InvokeAsync(createdHazard);
        await CloseModal();

        _logger?.LogInformation("Hazard creation process completed for: {HazardCode}", createdHazard?.Code);
    }

    /// <summary>
    /// Update hazard location fields - EXACTLY like HazardReporting logic
    /// </summary>
    private async Task UpdateHazardLocationForHazard(Hazard hazard)
    {
        // Handle geographic location if provided - EXACTLY like HazardReporting
        if (HasGeoLocation)
        {
            // Set coordinate information in hazard fields for backward compatibility
            hazard.LocationArea = $"Lat: {SelectedGeoLocation.Latitude:F6}, Lng: {SelectedGeoLocation.Longitude:F6}";
            if (!string.IsNullOrEmpty(SelectedGeoLocation.Description))
            {
                hazard.LocationSubArea = SelectedGeoLocation.Description;
            }
        }
    }

    /// <summary>
    /// Create a new hazard location - EXACTLY like HazardReporting logic
    /// </summary>
    private async Task CreateHazardLocation(Hazard hazard)
    {
        if (!HasGeoLocation) return;

        try
        {
            var hazardLocationCode = $"HL-0000";
            var newHazardLocation = new SMS_Domain.Entities.HazardLocation(new HazardLocationID(hazardLocationCode))
            {
                Code = hazardLocationCode,
                HazardCode = hazard.Code,
                Latitude = SelectedGeoLocation.Latitude,
                Longitude = SelectedGeoLocation.Longitude,
                Description = SelectedGeoLocation.Description ?? "Map selected location",
                CreatedBy = _currentUserService?.UserDisplayName,
                CreatedDate = DateTime.UtcNow,
                IsValidated = true,
                IsValid = true
            };

            // Create the new HazardLocation
            var createLocationCommand = new CreateHazardLocationCommand(newHazardLocation);
            var locationCreateResult = await _mediator.SendAsync(createLocationCommand, CancellationToken.None);

            if (locationCreateResult.IsSuccess)
            {
                var createdLocation = locationCreateResult.Value;
                hazard.HazardLocation = createdLocation;

                // Set coordinate information in hazard fields for backward compatibility - EXACTLY like HazardReporting
                hazard.LocationArea = $"Lat: {SelectedGeoLocation.Latitude:F6}, Lng: {SelectedGeoLocation.Longitude:F6}";
                if (!string.IsNullOrEmpty(SelectedGeoLocation.Description))
                {
                    hazard.LocationSubArea = SelectedGeoLocation.Description;
                }

                _logger?.LogInformation("Successfully created hazard location: {LocationCode}", createdLocation?.Code);
            }
            else
            {
                _logger?.LogWarning("Failed to create hazard location: {Error}", locationCreateResult.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to create/update hazard location, but continuing with hazard update");

            // Set location in hazard fields as fallback - EXACTLY like HazardReporting
            hazard.LocationArea = $"Lat: {SelectedGeoLocation.Latitude:F6}, Lng: {SelectedGeoLocation.Longitude:F6}";
            if (!string.IsNullOrEmpty(SelectedGeoLocation.Description))
            {
                hazard.LocationSubArea = SelectedGeoLocation.Description;
            }
        }
    }

    /// <summary>
    /// Update an existing hazard location - EXACTLY like HazardReporting logic
    /// </summary>
    private async Task UpdateHazardLocation(Hazard hazard)
    {
        if (!HasGeoLocation) return;

        try
        {
            // Check if HazardLocation already exists for this hazard
            var hazardLocationResult = await _mediator.SendAsync(new GetHazardLocationsByHazardCodeQuery(hazard.Code), CancellationToken.None);

            HazardLocation? hazardLocation = null;

            if (hazardLocationResult.IsSuccess && hazardLocationResult.Value.Any())
            {
                // UPDATE existing HazardLocation
                hazardLocation = hazardLocationResult.Value.FirstOrDefault();
                if (hazardLocation is not null)
                {
                    hazardLocation.HazardCode = hazard.Code;
                    hazardLocation.Latitude = SelectedGeoLocation.Latitude;
                    hazardLocation.Longitude = SelectedGeoLocation.Longitude;
                    hazardLocation.Description = SelectedGeoLocation.Description ?? "Map selected location";
                    hazardLocation.IsValidated = true;
                    hazardLocation.UpdatedDate = DateTime.UtcNow;
                    hazardLocation.UpdatedBy = _currentUserService?.UserDisplayName;
                    hazard.HazardLocation = hazardLocation;

                    var locationUpdateResult = await _mediator.SendAsync(
                        new UpdateHazardLocationCommand(hazardLocation), CancellationToken.None);

                    if (locationUpdateResult.IsSuccess)
                    {
                        _logger?.LogInformation("Successfully updated hazard location: {LocationCode}", hazardLocation.Code);
                      }
                }
            }
            else
            {
                // CREATE new HazardLocation if none exists
                await CreateHazardLocation(hazard);
                return;
            }

            // Set coordinate information in hazard fields for backward compatibility - EXACTLY like HazardReporting
            hazard.LocationArea = $"Lat: {SelectedGeoLocation.Latitude:F6}, Lng: {SelectedGeoLocation.Longitude:F6}";
            if (!string.IsNullOrEmpty(SelectedGeoLocation.Description))
            {
                hazard.LocationSubArea = SelectedGeoLocation.Description;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to update hazard location, but continuing with hazard update");

            // Set location in hazard fields as fallback - EXACTLY like HazardReporting
            hazard.LocationArea = $"Lat: {SelectedGeoLocation.Latitude:F6}, Lng: {SelectedGeoLocation.Longitude:F6}";
            if (!string.IsNullOrEmpty(SelectedGeoLocation.Description))
            {
                hazard.LocationSubArea = SelectedGeoLocation.Description;
            }
        }
    }

    private void ResetForm()
    {
        NewHazardDescription = string.Empty;
        NewHazardCategory = string.Empty;
        NewHazardType = string.Empty;
        
        // Reset location properties - EXACTLY like HazardReporting
        SelectedLatitude = 0;
        SelectedLongitude = 0;
        LocationDescription = string.Empty;
        SelectedGeoLocation = new HazardLocation
        {
            IsValid = false,
            IsValidated = false
        };
        _showMapModal = false;
        
        // Reset dropdown options
        HazardTypeOptions.Clear();
    }

    #region Map Functionality - EXACTLY like HazardReporting

    /// <summary>
    /// Open map selector modal - EXACTLY like HazardReporting
    /// </summary>
    public async Task OpenMapSelector()
    {
        _showMapModal = true;
        StateHasChanged();

        // Give DOM time to render the modal
        await Task.Delay(300);

        // Always try to initialize the map when modal opens - EXACTLY like HazardReporting
        if (_mapModule is not null)
        {
            try
            {
                // Always reinitialize the map since the DOM element is recreated
                await _mapModule.InvokeVoidAsync("initializeMap",_airportCenterLatitude, _airportCenterLongitude, _defaultZoomLevel, _dotNetRef);

                _logger?.LogInformation("Map reinitialized for modal opening");

                // Restore existing location if we have one
                if (HasGeoLocation)
                {
                    await Task.Delay(100); // Give map time to initialize

                    var restoreLatitude = (double)(SelectedGeoLocation.Latitude ?? SelectedLatitude);
                    var restoreLongitude = (double)(SelectedGeoLocation.Longitude ?? SelectedLongitude);

                    // Use airport defaults if we still don't have usable coordinates.
                    if (restoreLatitude == 0 && restoreLongitude == 0)
                    {
                        restoreLatitude = _airportCenterLatitude;
                        restoreLongitude = _airportCenterLongitude;
                    }

                    await _mapModule.InvokeVoidAsync("setLocationFromCoordinates", restoreLatitude, restoreLongitude, SelectedGeoLocation.Description ?? string.Empty);

                    // Update the form fields to match the restored location
                    SelectedLatitude = SelectedGeoLocation.Latitude ?? 0;
                    SelectedLongitude = SelectedGeoLocation.Longitude ?? 0;
                    LocationDescription = SelectedGeoLocation.Description ?? "";

                    _logger?.LogInformation("Existing location restored: {Lat}, {Lng}",SelectedGeoLocation.Latitude, SelectedGeoLocation.Longitude);

                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error initializing map in OpenMapSelector");
            }
        }
    }

    /// <summary>
    /// Close map selector modal - EXACTLY like HazardReporting
    /// </summary>
    public void CloseMapSelector()
    {
        _showMapModal = false;
        StateHasChanged();
    }

    /// <summary>
    /// Use selected location from map - EXACTLY like HazardReporting
    /// </summary>
    public async Task UseSelectedLocation()
    {
        if (!HasValidCoordinates)
        {
            _logger?.LogWarning("UseSelectedLocation called but no valid coordinates: Lat={Lat}, Lng={Lng}", 
                SelectedLatitude, SelectedLongitude);
            return;
        }

        // Set the geolocation data - EXACTLY like HazardReporting
        SelectedGeoLocation = new HazardLocation
        {
            Latitude = SelectedLatitude,
            Longitude = SelectedLongitude,
            Description = LocationDescription,
            IsValid = true,
            IsValidated = true,
            DateSelected = DateTime.UtcNow
        };

        // IMPORTANT: Verify that the location is now considered valid
        _logger?.LogInformation("Location set - Lat: {Lat}, Lng: {Lng}, IsValid: {IsValid}, HasGeoLocation: {HasGeoLocation}", 
            SelectedGeoLocation.Latitude, SelectedGeoLocation.Longitude, SelectedGeoLocation.IsValid, HasGeoLocation);

        _showMapModal = false;
        StateHasChanged();

        _logger?.LogInformation("Location successfully set: {GeoLocationDisplay}", GeoLocationDisplay);
    }

    /// <summary>
    /// Clear map selection - EXACTLY like HazardReporting
    /// </summary>
    public async Task ClearMapSelection()
    {
        SelectedLatitude = 0;
        SelectedLongitude = 0;
        LocationDescription = string.Empty;
        SelectedGeoLocation = new HazardLocation
        {
            IsValid = false,
            IsValidated = false
        };

        if (_mapModule is not null)
        {
            try
            {
                await _mapModule.InvokeVoidAsync("clearSelection");
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error clearing map selection");
            }
        }

        StateHasChanged();

        _logger?.LogInformation("Map selection cleared");
    }

    /// <summary>
    /// JavaScript callback for map location selection - EXACTLY like HazardReporting
    /// </summary>
    [JSInvokable]
    public async Task OnMapLocationSelected(double latitude, double longitude, string description)
    {
        SelectedLatitude = (decimal)latitude;
        SelectedLongitude = (decimal)longitude;
        LocationDescription = description; // This will set the description automatically!

        // Validate that we received proper coordinates
        if (latitude == 0 && longitude == 0)
        {
            _logger?.LogWarning("Received zero coordinates from map click");
        }

        _logger?.LogInformation("Map location received from JS: Lat={Lat}, Lng={Lng}, Description={Desc}", 
            latitude, longitude, description);

        await InvokeAsync(StateHasChanged);

        _logger?.LogInformation("Map location selected: {Lat}, {Lng} - HasValidCoordinates: {HasValid}", 
            latitude, longitude, HasValidCoordinates);
    }

    #endregion
}
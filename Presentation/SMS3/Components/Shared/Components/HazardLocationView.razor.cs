using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SMS_Application.Interfaces;
using SMS_Application.Queries;
using SMS_Domain.Entities;
namespace SMS3.Components.Shared.Components;

public partial class HazardLocationView
{

#region Parameters
    /// <summary>
    /// The hazard entity containing location information
    /// </summary>
    [Parameter] public Hazard? Hazard { get; set; }

    /// <summary>
    /// Hazard code used to resolve location data when HazardLocationData is not supplied
    /// </summary>
    [Parameter] public string? HazardCode { get; set; }

    /// <summary>
    /// Optional pre-loaded HazardLocation (used when caller already has the location)
    /// </summary>
    [Parameter] public HazardLocation? HazardLocationData { get; set; }

    /// <summary>
    /// Optional fallback location description
    /// </summary>
    [Parameter] public string? FallbackLocationDescription { get; set; }

    /// <summary>
    /// Optional fallback location area
    /// </summary>
    [Parameter] public string? FallbackLocationArea { get; set; }

    /// <summary>
    /// Optional fallback location sub-area
    /// </summary>
    [Parameter] public string? FallbackLocationSubArea { get; set; }

    /// <summary>
    /// Map title (optional)
    /// </summary>
    [Parameter] public string? Title { get; set; }

    /// <summary>
    /// Optional subtitle rendered in the map header
    /// </summary>
    [Parameter] public string? HeaderSubtitle { get; set; }

    /// <summary>
    /// Map height (default: 300px)
    /// </summary>
    [Parameter] public string MapHeight { get; set; } = "300px";

    /// <summary>
    /// Show coordinate information (default: true)
    /// </summary>
    [Parameter] public bool ShowCoordinates { get; set; } = true;

    /// <summary>
    /// Show location details panel (default: true)
    /// </summary>
    [Parameter] public bool ShowLocationDetails { get; set; } = true;

    /// <summary>
    /// Show zoom controls (default: true)
    /// </summary>
    [Parameter] public bool ShowZoomControls { get; set; } = true;

    /// <summary>
    /// Show layer controls (default: true)
    /// </summary>
    [Parameter] public bool ShowLayerControls { get; set; } = true;

    /// <summary>
    /// Initial zoom level (default: 15)
    /// </summary>
    [Parameter] public int ZoomLevel { get; set; } = 20;

    /// <summary>
    /// Show close button in map footer
    /// </summary>
    [Parameter] public bool ShowCloseButton { get; set; } = false;

    /// <summary>
    /// Show internal map component header
    /// </summary>
    [Parameter] public bool ShowHeader { get; set; } = true;

    /// <summary>
    /// Close action callback
    /// </summary>
    [Parameter] public EventCallback OnClose { get; set; }
    #endregion

    #region Private Properties
    private bool IsLoading { get; set; } = true;
    private HazardLocation? LoadedHazardLocation { get; set; }
    #endregion

    #region Lifecycle Methods
    protected override async Task OnParametersSetAsync()
    {
        await RefreshLocationAsync();
    }
    #endregion

    #region Data Loading Methods
    private async Task RefreshLocationAsync()
    {
        if (HazardLocationData is not null)
        {
            IsLoading = false;
            LoadedHazardLocation = HazardLocationData;
            return;
        }

        var hazardCode = ResolveHazardCode();
        if (!string.IsNullOrEmpty(hazardCode))
        {
            await LoadHazardLocationAsync(hazardCode);
            return;
        }

        IsLoading = false;
        LoadedHazardLocation = null;
    }

    /// <summary>
    /// Load HazardLocation using proper CQRS pattern
    /// </summary>
    private async Task LoadHazardLocationAsync(string hazardCode)
    {
        if (string.IsNullOrEmpty(hazardCode))
        {
            IsLoading = false;
            return;
        }

        try
        {
            IsLoading = true;

            Logger.LogInformation("Loading HazardLocation for hazard code: {HazardCode}", hazardCode);

            // Use CQRS query to get HazardLocation entities
            var query = new GetHazardLocationsByHazardCodeQuery(hazardCode);
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value?.Any() == true)
            {
                // Get the most recent/primary location
                LoadedHazardLocation = result.Value
                    .OrderByDescending(l => l.CreatedDate)
                    .FirstOrDefault();

                Logger.LogInformation("Successfully loaded HazardLocation: {LocationCode} for hazard: {HazardCode}", 
                    LoadedHazardLocation?.Code, hazardCode);
            }
            else
            {
                Logger.LogInformation("No HazardLocation found for hazard code: {HazardCode}", hazardCode);
                LoadedHazardLocation = null;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading HazardLocation for hazard code: {HazardCode}", hazardCode);
            LoadedHazardLocation = null;
        }
        finally
        {
            IsLoading = false;
        }
    }
    #endregion

    #region Helper Methods
    private string? ResolveHazardCode()
    {
        if (!string.IsNullOrWhiteSpace(Hazard?.Code))
        {
            return Hazard.Code;
        }

        if (!string.IsNullOrWhiteSpace(HazardCode))
        {
            return HazardCode;
        }

        return null;
    }

    private string? ResolveLocationArea()
    {
        if (!string.IsNullOrWhiteSpace(Hazard?.LocationArea))
        {
            return Hazard.LocationArea;
        }

        if (!string.IsNullOrWhiteSpace(FallbackLocationArea))
        {
            return FallbackLocationArea;
        }

        return null;
    }

    private string? ResolveLocationSubArea()
    {
        if (!string.IsNullOrWhiteSpace(Hazard?.LocationSubArea))
        {
            return Hazard.LocationSubArea;
        }

        if (!string.IsNullOrWhiteSpace(FallbackLocationSubArea))
        {
            return FallbackLocationSubArea;
        }

        return null;
    }

    private string? GetLocationId()
    {
        if (!string.IsNullOrWhiteSpace(LoadedHazardLocation?.Code))
        {
            return LoadedHazardLocation.Code;
        }

        return null;
    }

    /// <summary>
    /// Get latitude from loaded HazardLocation or fallback to parsed value
    /// </summary>
    private decimal? GetLatitude()
    {
        // Primary: Use loaded HazardLocation
        if (LoadedHazardLocation?.Latitude.HasValue == true)
        {
            return LoadedHazardLocation.Latitude.Value;
        }

        // Fallback: Parse from Hazard.LocationArea for backward compatibility
        var locationArea = ResolveLocationArea();
        if (locationArea is not null)
        {
            var latMatch = Regex.Match(locationArea, @"Lat:\s*(-?\d+\.?\d*)", RegexOptions.IgnoreCase);
            if (latMatch.Success && decimal.TryParse(latMatch.Groups[1].Value, out var lat))
            {
                return lat;
            }
        }

        return null;
    }

    /// <summary>
    /// Get longitude from loaded HazardLocation or fallback to parsed value
    /// </summary>
    private decimal? GetLongitude()
    {
        // Primary: Use loaded HazardLocation
        if (LoadedHazardLocation?.Longitude.HasValue == true)
        {
            return LoadedHazardLocation.Longitude.Value;
        }

        // Fallback: Parse from Hazard.LocationArea for backward compatibility
        var locationArea = ResolveLocationArea();
        if (locationArea is not null)
        {
            var lngMatch = Regex.Match(locationArea, @"Lng:\s*(-?\d+\.?\d*)", RegexOptions.IgnoreCase);
            if (lngMatch.Success && decimal.TryParse(lngMatch.Groups[1].Value, out var lng))
            {
                return lng;
            }
        }

        return null;
    }

    /// <summary>
    /// Get location description from loaded HazardLocation or fallback
    /// </summary>
    private string? GetLocationDescription()
    {
        // Primary: Use loaded HazardLocation description
        if (!string.IsNullOrEmpty(LoadedHazardLocation?.Description))
        {
            return LoadedHazardLocation.Description;
        }

        // Fallback: Use Hazard fields
        var locationSubArea = ResolveLocationSubArea();
        var locationArea = ResolveLocationArea();

        if (GetLatitude().HasValue && GetLongitude().HasValue)
        {
            return locationSubArea ?? FallbackLocationDescription ?? "Map location";
        }

        return locationSubArea ?? FallbackLocationDescription ?? locationArea ?? "Location not specified";
    }

    /// <summary>
    /// Get location area information - using Hazard fields since HazardLocation doesn't have this
    /// </summary>
    private string? GetLocationArea()
    {
        // HazardLocation entity doesn't have LocationArea, so use Hazard fields/fallback inputs
        return ResolveLocationArea();
    }

    /// <summary>
    /// Get location sub-area information - using Hazard fields since HazardLocation doesn't have this
    /// </summary>
    private string? GetLocationSubArea()
    {
        // HazardLocation entity doesn't have LocationSubArea, so use Hazard fields/fallback inputs
        return ResolveLocationSubArea();
    }
    #endregion
}



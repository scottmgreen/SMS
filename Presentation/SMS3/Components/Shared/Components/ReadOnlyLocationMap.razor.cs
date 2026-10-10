using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using Microsoft.JSInterop;
namespace SMS3.Components.Shared.Components;

public partial class ReadOnlyLocationMap
{

#region Parameters
    /// <summary>
    /// Latitude coordinate
    /// </summary>
    [Parameter] public decimal? Latitude { get; set; }

    /// <summary>
    /// Longitude coordinate  
    /// </summary>
    [Parameter] public decimal? Longitude { get; set; }

    /// <summary>
    /// Location identifier/code
    /// </summary>
    [Parameter] public string? LocationId { get; set; }

    /// <summary>
    /// Location description/title
    /// </summary>
    [Parameter] public string? LocationDescription { get; set; }

    /// <summary>
    /// Location area (general area)
    /// </summary>
    [Parameter] public string? LocationArea { get; set; }

    /// <summary>
    /// Location sub-area (specific area)
    /// </summary>
    [Parameter] public string? LocationSubArea { get; set; }

    /// <summary>
    /// Map title
    /// </summary>
    [Parameter] public string? Title { get; set; }

    /// <summary>
    /// Map height (default: 300px)
    /// </summary>
    [Parameter] public string MapHeight { get; set; } = "300px";

    /// <summary>
    /// Show coordinate information
    /// </summary>
    [Parameter] public bool ShowCoordinates { get; set; } = true;

    /// <summary>
    /// Show location details panel
    /// </summary>
    [Parameter] public bool ShowLocationDetails { get; set; } = true;

    /// <summary>
    /// Show zoom controls
    /// </summary>
    [Parameter] public bool ShowZoomControls { get; set; } = true;

    /// <summary>
    /// Show layer controls (street/satellite)
    /// </summary>
    [Parameter] public bool ShowLayerControls { get; set; } = false;

    /// <summary>
    /// Initial zoom level
    /// </summary>
    [Parameter] public int ZoomLevel { get; set; } = 15;
    #endregion

    #region Private Properties
    private IJSObjectReference? _mapModule;
    private string _mapContainerId = $"readonly-map-{Guid.NewGuid():N}";
    private string MapContainerId => _mapContainerId;
    
    private bool HasCoordinates => Latitude.HasValue && Longitude.HasValue && 
                                  Latitude.Value != 0 && Longitude.Value != 0;
    
    private bool HasValidLocation => HasCoordinates || !string.IsNullOrEmpty(LocationDescription) || 
                                   !string.IsNullOrEmpty(LocationArea) || !string.IsNullOrEmpty(LocationSubArea);

    private string CoordinateDisplay => HasCoordinates ? 
        $"{Latitude:F6}, {Longitude:F6}" : "No coordinates";
    #endregion

    #region Lifecycle Methods
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Logger.LogInformation("ReadOnlyLocationMap - FirstRender: HasCoordinates={HasCoordinates}, Lat={Lat}, Lng={Lng}, HasValidLocation={HasValidLocation}", 
                HasCoordinates, Latitude, Longitude, HasValidLocation);
                
            if (HasCoordinates)
            {
                await InitializeMapAsync();
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_mapModule != null)
        {
            try
            {
                await _mapModule.InvokeVoidAsync("destroyMap", MapContainerId);
                await _mapModule.DisposeAsync();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Error disposing map module");
            }
        }
    }
    #endregion

    #region Map Methods
    private async Task InitializeMapAsync()
    {
        if (!HasCoordinates) 
        {
            Logger.LogWarning("InitializeMapAsync skipped - No coordinates available");
            return;
        }

        try
        {
            Logger.LogInformation("Starting map initialization for container: {ContainerId}, Lat: {Lat}, Lng: {Lng}", 
                MapContainerId, Latitude, Longitude);

            // Load the map JavaScript module
            _mapModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "/js/readonly-map.js");
            Logger.LogInformation("JavaScript module loaded successfully");
            
            // Give DOM time to render
            await Task.Delay(100);
            
            // Initialize read-only map
            await _mapModule.InvokeVoidAsync("initializeReadOnlyMap", 
                MapContainerId,
                (double)Latitude!.Value,
                (double)Longitude!.Value, 
                ZoomLevel,
                LocationDescription ?? "Hazard Location",
                ShowZoomControls,
                ShowLayerControls);
                
            Logger.LogInformation("Read-only map initialized successfully for container: {ContainerId}, Lat: {Lat}, Lng: {Lng}", 
                MapContainerId, Latitude, Longitude);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to initialize read-only map for container: {ContainerId}", MapContainerId);
        }
    }
    #endregion

    #region Support Classes
    /// <summary>
    /// Record for representing a read-only map location
    /// </summary>
    public record ReadOnlyMapLocation
    {
        public decimal? Latitude { get; init; }
        public decimal? Longitude { get; init; }
        public string? Description { get; init; }
        public string? Area { get; init; }
        public string? SubArea { get; init; }
        
        public bool HasCoordinates => Latitude.HasValue && Longitude.HasValue && 
                                     Latitude.Value != 0 && Longitude.Value != 0;
    }
    #endregion
}



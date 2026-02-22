using Microsoft.AspNetCore.Components;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Application.Messaging.Queries;
using SMS_Application.Interfaces;
using SMS3.Components.Shared.UIHelpers;
using Radzen;
using Radzen.Blazor;
using System.Linq.Expressions;

namespace SMS3.Components.Pages.Listings;

public partial class HazardLocationListing : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<HazardLocationListing> Logger { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;

    private RadzenDataGrid<HazardLocation>? locationsGrid;
    private IEnumerable<HazardLocation> locations = new List<HazardLocation>();
    private int totalCount;
    private bool isLoading = false;
    private string BasicTextStyle = "font-size:smaller;font-weight: 600";
    protected override async Task OnInitializedAsync()
    {
        await LoadInitialData();
    }

    private async Task LoadInitialData()
    {
        try
        {
            var query = new GetAllHazardLocationsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                locations = result.Value;
                totalCount = locations.Count();
                Logger.LogInformation("Loaded {Count} hazard locations", totalCount);
            }
            else
            {
                ShowErrorNotification("Failed to load hazard locations");
                Logger.LogError("Failed to load hazard locations: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading hazard locations");
            ShowErrorNotification("Error loading hazard locations");
        }
    }

    private async Task LoadData(LoadDataArgs args)
    {
        try
        {
            isLoading = true;
            StateHasChanged();

            await LoadInitialData();

            var query = locations.AsQueryable();

            if (!string.IsNullOrEmpty(args.OrderBy))
            {
                query = args.OrderBy.Contains("desc")
                    ? query.OrderByDescending(GetPropertyExpression(args.OrderBy.Replace(" desc", "")))
                    : query.OrderBy(GetPropertyExpression(args.OrderBy));
            }

            if (args.Skip.HasValue)
            {
                query = query.Skip(args.Skip.Value);
            }

            if (args.Top.HasValue)
            {
                query = query.Take(args.Top.Value);
            }

            locations = query.ToList();
            totalCount = locations.Count();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in LoadData");
            ShowErrorNotification("Error loading data");
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Show location map in modal dialog
    /// </summary>
    private async Task ViewLocationMap(HazardLocation location)
    {
        try
        {
            Logger.LogInformation("Opening location map for hazard location: {LocationCode}", location.Code);

            if (!HasValidCoordinates(location))
            {
                ShowErrorNotification("This location does not have valid coordinates to display on the map.");
                return;
            }

            // First, get the associated Hazard entity since HazardLocationDisplay expects a Hazard parameter
            var hazard = await GetHazardForLocation(location);
            
            if (hazard == null)
            {
                ShowErrorNotification($"Could not find associated hazard for location {location.Code}");
                return;
            }

            var title = $"📍 {location.Code} - Location Map";
            var subtitle = !string.IsNullOrEmpty(location.Description) 
                ? location.Description 
                : $"Hazard: {location.HazardCode}";

            // Open modal dialog with HazardLocationDisplay component
            await DialogService.OpenAsync(title,
                ds => 
                {
                    var content = new RenderFragment(builder =>
                    {
                        // Add subtitle
                        if (!string.IsNullOrEmpty(subtitle))
                        {
                            builder.OpenElement(0, "div");
                            builder.AddAttribute(1, "class", "mb-3 text-muted");
                            builder.AddAttribute(2, "style", "font-size: 0.9rem;");
                            builder.AddContent(3, subtitle);
                            builder.CloseElement();
                        }

                        // Add HazardLocationDisplay component
                        builder.OpenComponent<SMS3.Components.Shared.HazardLocationDisplay>(10);
                        builder.AddAttribute(11, "Hazard", hazard);
                        builder.AddAttribute(12, "Title", "");
                        builder.AddAttribute(13, "MapHeight", "400px");
                        builder.AddAttribute(14, "ShowCoordinates", true);
                        builder.AddAttribute(15, "ShowLocationDetails", true);
                        builder.AddAttribute(16, "ShowZoomControls", true);
                        builder.AddAttribute(17, "ShowLayerControls", false);
                        builder.AddAttribute(18, "ZoomLevel", 18);
                        builder.CloseComponent();

                        // Add location details
                        builder.OpenElement(20, "div");
                        builder.AddAttribute(21, "class", "mt-3 p-3 bg-light rounded");
                        builder.AddAttribute(22, "style", "border-left: 4px solid #007bff;");
                        
                        builder.OpenElement(23, "h6");
                        builder.AddAttribute(24, "class", "text-primary mb-2");
                        builder.AddContent(25, "📋 Location Details");
                        builder.CloseElement();
                        
                        builder.OpenElement(26, "div");
                        builder.AddAttribute(27, "class", "row");
                        
                        // Left column
                        builder.OpenElement(28, "div");
                        builder.AddAttribute(29, "class", "col-md-6");
                        
                        builder.AddMarkupContent(30, $"<strong>Code:</strong> {location.Code}<br/>");
                        builder.AddMarkupContent(31, $"<strong>Hazard:</strong> {location.HazardCode}<br/>");
                        if (location.Latitude.HasValue && location.Longitude.HasValue)
                        {
                            builder.AddMarkupContent(32, $"<strong>Coordinates:</strong> {location.Latitude:F6}, {location.Longitude:F6}<br/>");
                        }
                        
                        builder.CloseElement(); // col-md-6
                        
                        // Right column
                        builder.OpenElement(35, "div");
                        builder.AddAttribute(36, "class", "col-md-6");
                        
                        builder.AddMarkupContent(37, $"<strong>Created:</strong> {location.CreatedDate:yyyy-MM-dd HH:mm}<br/>");
                        builder.AddMarkupContent(38, $"<strong>Created By:</strong> {location.CreatedBy ?? "Unknown"}<br/>");
                        if (!string.IsNullOrEmpty(location.Description))
                        {
                            builder.AddMarkupContent(39, $"<strong>Description:</strong> {location.Description}");
                        }
                        
                        builder.CloseElement(); // col-md-6
                        builder.CloseElement(); // row
                        builder.CloseElement(); // details container
                    });
                    
                    return content;
                },
                new DialogOptions() 
                { 
                    Width = "1200px", 
                    Height = "900px", 
                    Resizable = true, 
                    Draggable = true,
                    CloseDialogOnOverlayClick = false
                });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening location map for {LocationCode}", location.Code);
            ShowErrorNotification("Error opening location map");
        }
    }

    /// <summary>
    /// Get the associated Hazard entity for a HazardLocation
    /// </summary>
    private async Task<Hazard?> GetHazardForLocation(HazardLocation location)
    {
        try
        {
            if (string.IsNullOrEmpty(location.HazardCode))
                return null;

            var hazardQuery = new GetHazardByCodeQuery(new HazardID(location.HazardCode));
            var hazardResult = await Mediator.SendAsync(hazardQuery, CancellationToken.None);

            if (hazardResult.IsSuccess && hazardResult.Value != null)
            {
                return hazardResult.Value;
            }

            Logger.LogWarning("Could not find hazard {HazardCode} for location {LocationCode}", location.HazardCode, location.Code);
            return null;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading hazard {HazardCode} for location {LocationCode}", location.HazardCode, location.Code);
            return null;
        }
    }

    /// <summary>
    /// Check if a HazardLocation has valid coordinates for mapping
    /// </summary>
    private bool HasValidCoordinates(HazardLocation location)
    {
        return location.Latitude.HasValue && location.Longitude.HasValue &&
               location.Latitude.Value != 0 && location.Longitude.Value != 0;
    }

    private static Expression<Func<HazardLocation, object>> GetPropertyExpression(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(HazardLocation), "x");
        var property = Expression.Property(parameter, propertyName);
        var conversion = Expression.Convert(property, typeof(object));
        return Expression.Lambda<Func<HazardLocation, object>>(conversion, parameter);
    }

    private void ShowActions(HazardLocation location)
    {
        Logger.LogInformation("Actions requested for hazard location: {Code}", location.Code);
    }

    private void ShowErrorNotification(string message)
    {
        NotificationHelper.ShowError(NotificationService, message);
    }
}
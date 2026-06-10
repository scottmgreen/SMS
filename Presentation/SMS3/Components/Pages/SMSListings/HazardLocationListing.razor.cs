using System.Linq.Expressions;

using Microsoft.AspNetCore.Components;

using Radzen;
using Radzen.Blazor;

using SMS_Application.Interfaces;
using SMS_Application.Queries;

using SMS_Domain.Entities;
using SMS_Domain.Events;
using SMS_Domain.Enums;
using SMS_Domain.Errors;
using SMS_Domain.ValueObjects;

using SMS_Shared.Configuration;

using SMS3.Components.Shared;
using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.SMSListings;

/// <summary>
/// Hazard Location Listing Component - Enhanced with full CRUD operations and advanced filtering
/// Provides comprehensive data grid listing with view details, edit navigation, and delete functionality
/// </summary>
public partial class HazardLocationListing : ComponentBase
{
    private string BasicTextStyle = "font-size:smaller;font-weight: 600";

    #region Dependencies
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<HazardLocationListing> _logger { get; set; } = default!;
    [Inject] private IBaseEventBus _eventBus { get; set; } = default!;
 
    [Inject] private DialogService _dialogService { get; set; } = default!;
    #endregion

    #region Properties
    private RadzenDataGrid<HazardLocation>? _locationsGrid;
    private IEnumerable<HazardLocation> _locations = new List<HazardLocation>();
    private List<HazardLocation> _allLocations = new List<HazardLocation>(); // Store all locations for client-side filtering
    private int _totalCount;
    private bool _isLoading = false;
    #endregion
    
    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        await LoadInitialData();
    }
    #endregion

    #region Data Loading Methods
    private async Task LoadInitialData()
    {
        try
        {
            _isLoading = true;
            StateHasChanged();

            _logger.LogInformation("Loading hazard locations for listing view");

            var query = new GetAllHazardLocationsQuery();
            var result = await _mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value is not null)
            {
                _allLocations = result.Value.ToList(); // Store all locations for filtering/sorting
                _locations = _allLocations; // Initially show all locations
                _totalCount = _allLocations.Count();
                _logger.LogInformation("Loaded {Count} hazard locations for listing", _totalCount);

                // Show success notification if we have data
                if (_totalCount > 0)
                {
                    await ShowSuccessAsyncNotification($"Successfully loaded {_totalCount} hazard locations");
                }
                else
                {
                    await ShowInfoAsyncNotification("No hazard locations found");
                }
            }
            else
            {
                // Initialize with empty lists to prevent null reference issues
                _allLocations = new List<HazardLocation>();
                _locations = _allLocations;
                _totalCount = 0;
                
                await ShowErrorAsyncNotification("Failed to load hazard locations");
                _logger.LogError("Failed to load hazard locations: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            // Ensure we always have valid collections even if an error occurs
            _allLocations = new List<HazardLocation>();
            _locations = _allLocations;
            _totalCount = 0;
            
            _logger.LogError(ex, "Error loading hazard locations");
            await ShowErrorAsyncNotification($"Error loading hazard locations: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task LoadData(LoadDataArgs args)
    {
        try
        {
            _isLoading = true;
            StateHasChanged();

            _logger.LogInformation("LoadData called with Skip: {Skip}, Top: {Top}, OrderBy: {OrderBy}, Filter: {Filter}", 
                args.Skip, args.Top, args.OrderBy, args.Filter);

            // If we don't have all locations yet, load them first
            if (_allLocations is null || !_allLocations.Any())
            {
                _logger.LogInformation("No locations cached, loading initial data");
                await LoadInitialData();
                return;
            }

            // Start with all locations
            var query = _allLocations.AsQueryable();
            _logger.LogInformation("Starting with {Count} total locations", query.Count());

            // Apply filtering
            if (!string.IsNullOrEmpty(args.Filter))
            {
                _logger.LogInformation("Applying filter: {Filter}", args.Filter);
                query = ApplyFiltering(query, args);
                _logger.LogInformation("After filtering: {Count} locations", query.Count());
            }

            // Get total count after filtering but before paging
            _totalCount = query.Count();

            // Apply sorting
            if (!string.IsNullOrEmpty(args.OrderBy))
            {
                _logger.LogInformation("Applying sorting: {OrderBy}", args.OrderBy);
                query = ApplySorting(query, args.OrderBy);
                _logger.LogInformation("Sorting applied successfully");
            }
            else
            {
                // Default sorting by CreatedDate descending
                _logger.LogInformation("Applying default sort by CreatedDate");
                query = query.OrderByDescending(l => l.CreatedDate ?? DateTime.MinValue);
            }

            // Apply paging
            if (args.Skip.HasValue && args.Skip > 0)
            {
                _logger.LogInformation("Applying skip: {Skip}", args.Skip);
                query = query.Skip(args.Skip.Value);
            }

            if (args.Top.HasValue && args.Top > 0)
            {
                _logger.LogInformation("Applying take: {Top}", args.Top);
                query = query.Take(args.Top.Value);
            }

            _locations = query.ToList();

            _logger.LogInformation("Applied filtering/sorting/paging. Showing {Count} of {Total} locations", 
                _locations.Count(), _totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in LoadData with args: Skip={Skip}, Top={Top}, OrderBy={OrderBy}, Filter={Filter}", 
                args.Skip, args.Top, args.OrderBy, args.Filter);
            await ShowErrorAsyncNotification($"Error loading data: {ex.Message}");
            
            // Fallback to show all data without filtering/sorting
            try
            {
                _locations = _allLocations ?? new List<HazardLocation>();
                _totalCount = _locations.Count();
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "Error in LoadData fallback");
                _locations = new List<HazardLocation>();
                _totalCount = 0;
            }
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Apply filtering based on Radzen DataGrid filter arguments
    /// </summary>
    private IQueryable<HazardLocation> ApplyFiltering(IQueryable<HazardLocation> query, LoadDataArgs args)
    {
        try
        {
            _logger.LogInformation("ApplyFiltering called with Filter: {Filter}, Filters count: {FilterCount}", 
                args.Filter, args.Filters?.Count() ?? 0);

            // Handle simple string filter (when user types in the general filter)
            if (!string.IsNullOrEmpty(args.Filter) && !args.Filter.Contains("("))
            {
                var filterValue = args.Filter.ToLower();
                _logger.LogInformation("Applying simple string filter: {FilterValue}", filterValue);
                
                query = query.Where(l => 
                    (!string.IsNullOrEmpty(l.Code) && l.Code.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(l.HazardCode) && l.HazardCode.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(l.Description) && l.Description.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(l.CreatedBy) && l.CreatedBy.ToLower().Contains(filterValue)) ||
                    (l.Latitude.HasValue && l.Latitude.ToString()!.Contains(filterValue)) ||
                    (l.Longitude.HasValue && l.Longitude.ToString()!.Contains(filterValue))
                );
                return query;
            }

            // Handle advanced column-specific filters
            if (args.Filters is not null && args.Filters.Any())
            {
                _logger.LogInformation("Applying {Count} advanced filters", args.Filters.Count());
                
                foreach (var filter in args.Filters)
                {
                    var columnName = filter.Property?.ToLower();
                    var filterValue = filter.FilterValue?.ToString()?.ToLower();
                    var filterOperator = filter.FilterOperator;

                    _logger.LogInformation("Processing filter - Column: {Column}, Value: {Value}, Operator: {Operator}", 
                        columnName, filterValue, filterOperator);

                    if (string.IsNullOrEmpty(filterValue)) continue;

                    switch (columnName)
                    {
                        case "code":
                            query = ApplyStringFilter(query, l => l.Code, filterValue, filterOperator);
                            break;
                        case "hazardcode":
                            query = ApplyStringFilter(query, l => l.HazardCode, filterValue, filterOperator);
                            break;
                        case "description":
                            query = ApplyStringFilter(query, l => l.Description, filterValue, filterOperator);
                            break;
                        case "createdby":
                            query = ApplyStringFilter(query, l => l.CreatedBy, filterValue, filterOperator);
                            break;
                        case "latitude":
                            if (decimal.TryParse(filter.FilterValue?.ToString(), out var latValue))
                            {
                                query = ApplyDecimalFilter(query, l => l.Latitude, latValue, filterOperator);
                            }
                            break;
                        case "longitude":
                            if (decimal.TryParse(filter.FilterValue?.ToString(), out var lonValue))
                            {
                                query = ApplyDecimalFilter(query, l => l.Longitude, lonValue, filterOperator);
                            }
                            break;
                        case "createddate":
                            if (DateTime.TryParse(filter.FilterValue?.ToString(), out var createdDateValue))
                            {
                                query = ApplyDateFilter(query, l => l.CreatedDate, createdDateValue, filterOperator);
                            }
                            break;
                        case "dateselected":
                            if (DateTime.TryParse(filter.FilterValue?.ToString(), out var selectedDateValue))
                            {
                                query = ApplyDateFilter(query, l => l.DateSelected, selectedDateValue, filterOperator);
                            }
                            break;
                        default:
                            _logger.LogWarning("Unknown filter column: {ColumnName}", columnName);
                            break;
                    }
                }
            }

            return query;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying filters - Filter: {Filter}, Filters: {@Filters}", 
                args.Filter, args.Filters?.Select(f => new { f.Property, f.FilterValue, f.FilterOperator }));
            return query; // Return unfiltered query if filtering fails
        }
    }

    /// <summary>
    /// Apply string-based filtering with different operators
    /// </summary>
    private IQueryable<HazardLocation> ApplyStringFilter(IQueryable<HazardLocation> query, Expression<Func<HazardLocation, string?>> propertySelector, string filterValue, FilterOperator filterOperator)
    {
        return filterOperator switch
        {
            FilterOperator.Contains => query.Where(CombineExpressions(propertySelector, value => !string.IsNullOrEmpty(value) && value.ToLower().Contains(filterValue))),
            FilterOperator.StartsWith => query.Where(CombineExpressions(propertySelector, value => !string.IsNullOrEmpty(value) && value.ToLower().StartsWith(filterValue))),
            FilterOperator.EndsWith => query.Where(CombineExpressions(propertySelector, value => !string.IsNullOrEmpty(value) && value.ToLower().EndsWith(filterValue))),
            FilterOperator.Equals => query.Where(CombineExpressions(propertySelector, value => !string.IsNullOrEmpty(value) && value.ToLower() == filterValue)),
            FilterOperator.NotEquals => query.Where(CombineExpressions(propertySelector, value => string.IsNullOrEmpty(value) || value.ToLower() != filterValue)),
            _ => query.Where(CombineExpressions(propertySelector, value => !string.IsNullOrEmpty(value) && value.ToLower().Contains(filterValue)))
        };
    }

    /// <summary>
    /// Apply decimal-based filtering (for Latitude, Longitude, etc.)
    /// </summary>
    private IQueryable<HazardLocation> ApplyDecimalFilter(IQueryable<HazardLocation> query, Expression<Func<HazardLocation, decimal?>> propertySelector, decimal filterValue, FilterOperator filterOperator)
    {
        return filterOperator switch
        {
            FilterOperator.Equals => query.Where(CombineExpressions(propertySelector, value => value.HasValue && value.Value == filterValue)),
            FilterOperator.NotEquals => query.Where(CombineExpressions(propertySelector, value => !value.HasValue || value.Value != filterValue)),
            FilterOperator.LessThan => query.Where(CombineExpressions(propertySelector, value => value.HasValue && value.Value < filterValue)),
            FilterOperator.LessThanOrEquals => query.Where(CombineExpressions(propertySelector, value => value.HasValue && value.Value <= filterValue)),
            FilterOperator.GreaterThan => query.Where(CombineExpressions(propertySelector, value => value.HasValue && value.Value > filterValue)),
            FilterOperator.GreaterThanOrEquals => query.Where(CombineExpressions(propertySelector, value => value.HasValue && value.Value >= filterValue)),
            _ => query.Where(CombineExpressions(propertySelector, value => value.HasValue && value.Value == filterValue))
        };
    }

    /// <summary>
    /// Apply date-based filtering with different operators
    /// </summary>
    private IQueryable<HazardLocation> ApplyDateFilter(IQueryable<HazardLocation> query, Expression<Func<HazardLocation, DateTime?>> propertySelector, DateTime filterValue, FilterOperator filterOperator)
    {
        return filterOperator switch
        {
            FilterOperator.Equals => query.Where(CombineExpressions(propertySelector, value => value.HasValue && value.Value.Date == filterValue.Date)),
            FilterOperator.NotEquals => query.Where(CombineExpressions(propertySelector, value => !value.HasValue || value.Value.Date != filterValue.Date)),
            FilterOperator.LessThan => query.Where(CombineExpressions(propertySelector, value => value.HasValue && value.Value.Date < filterValue.Date)),
            FilterOperator.LessThanOrEquals => query.Where(CombineExpressions(propertySelector, value => value.HasValue && value.Value.Date <= filterValue.Date)),
            FilterOperator.GreaterThan => query.Where(CombineExpressions(propertySelector, value => value.HasValue && value.Value.Date > filterValue.Date)),
            FilterOperator.GreaterThanOrEquals => query.Where(CombineExpressions(propertySelector, value => value.HasValue && value.Value.Date >= filterValue.Date)),
            _ => query.Where(CombineExpressions(propertySelector, value => value.HasValue && value.Value.Date == filterValue.Date))
        };
    }

    /// <summary>
    /// Apply date-based filtering for non-nullable DateTime
    /// </summary>
    private IQueryable<HazardLocation> ApplyDateFilter(IQueryable<HazardLocation> query, Expression<Func<HazardLocation, DateTime>> propertySelector, DateTime filterValue, FilterOperator filterOperator)
    {
        return filterOperator switch
        {
            FilterOperator.Equals => query.Where(CombineExpressions(propertySelector, value => value.Date == filterValue.Date)),
            FilterOperator.NotEquals => query.Where(CombineExpressions(propertySelector, value => value.Date != filterValue.Date)),
            FilterOperator.LessThan => query.Where(CombineExpressions(propertySelector, value => value.Date < filterValue.Date)),
            FilterOperator.LessThanOrEquals => query.Where(CombineExpressions(propertySelector, value => value.Date <= filterValue.Date)),
            FilterOperator.GreaterThan => query.Where(CombineExpressions(propertySelector, value => value.Date > filterValue.Date)),
            FilterOperator.GreaterThanOrEquals => query.Where(CombineExpressions(propertySelector, value => value.Date >= filterValue.Date)),
            _ => query.Where(CombineExpressions(propertySelector, value => value.Date == filterValue.Date))
        };
    }

    /// <summary>
    /// Combine property selector with condition expression
    /// </summary>
    private Expression<Func<HazardLocation, bool>> CombineExpressions<T>(Expression<Func<HazardLocation, T>> propertySelector, Expression<Func<T, bool>> condition)
    {
        var parameter = propertySelector.Parameters[0];
        var property = propertySelector.Body;
        var conditionBody = condition.Body;
        var conditionParameter = condition.Parameters[0];

        // Replace the condition parameter with the property expression
        var visitor = new ParameterReplacementVisitor(conditionParameter, property);
        var newConditionBody = visitor.Visit(conditionBody);

        return Expression.Lambda<Func<HazardLocation, bool>>(newConditionBody, parameter);
    }

    /// <summary>
    /// Apply sorting based on OrderBy parameter from Radzen DataGrid
    /// </summary>
    private IQueryable<HazardLocation> ApplySorting(IQueryable<HazardLocation> query, string orderBy)
    {
        try
        {
            if (string.IsNullOrEmpty(orderBy)) return query;

            var parts = orderBy.Split(' ');
            var propertyName = parts[0].ToLower();
            var isDescending = parts.Length > 1 && parts[1].ToLower() == "desc";

            _logger.LogInformation("Applying sorting: Property={PropertyName}, Descending={IsDescending}", propertyName, isDescending);

            return propertyName switch
            {
                "code" => isDescending ? query.OrderByDescending(l => l.Code ?? "") : query.OrderBy(l => l.Code ?? ""),
                "hazardcode" => isDescending ? query.OrderByDescending(l => l.HazardCode ?? "") : query.OrderBy(l => l.HazardCode ?? ""),
                "description" => isDescending ? query.OrderByDescending(l => l.Description ?? "") : query.OrderBy(l => l.Description ?? ""),
                "createdby" => isDescending ? query.OrderByDescending(l => l.CreatedBy ?? "") : query.OrderBy(l => l.CreatedBy ?? ""),
                "latitude" => isDescending ? query.OrderByDescending(l => l.Latitude) : query.OrderBy(l => l.Latitude),
                "longitude" => isDescending ? query.OrderByDescending(l => l.Longitude) : query.OrderBy(l => l.Longitude),
                "createddate" => isDescending ? query.OrderByDescending(l => l.CreatedDate) : query.OrderBy(l => l.CreatedDate),
                "updateddate" => isDescending ? query.OrderByDescending(l => l.UpdatedDate) : query.OrderBy(l => l.UpdatedDate),
                "dateselected" => isDescending ? query.OrderByDescending(l => l.DateSelected) : query.OrderBy(l => l.DateSelected),
                _ => query.OrderByDescending(l => l.CreatedDate ?? DateTime.MinValue) // Default sort with null handling
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying sorting for OrderBy: {OrderBy}", orderBy);
            return query.OrderByDescending(l => l.CreatedDate ?? DateTime.MinValue); // Fallback to default sort
        }
    }
    #endregion

    #region Helper Methods - Keep existing functionality but improved
    private static Expression<Func<HazardLocation, object>> GetPropertyExpression(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(HazardLocation), "x");
        var property = Expression.Property(parameter, propertyName);
        var conversion = Expression.Convert(property, typeof(object));
        return Expression.Lambda<Func<HazardLocation, object>>(conversion, parameter);
    }

    /// <summary>
    /// Check if a HazardLocation has valid coordinates for mapping
    /// </summary>
    public bool HasValidCoordinates(HazardLocation location)
    {
        return location.Latitude.HasValue && location.Longitude.HasValue &&
               location.Latitude.Value != 0 && location.Longitude.Value != 0;
    }

    /// <summary>
    /// Shows error notification to user
    /// </summary>
    private async Task ShowErrorAsyncNotification(string message)
    {
        await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", message));
    }

    /// <summary>
    /// Shows success notification to user
    /// </summary>
    private async Task ShowSuccessAsyncNotification(string message)
    {
        await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", message));
    }

    /// <summary>
    /// Shows info notification to user
    /// </summary>
    private async Task ShowInfoAsyncNotification(string message)
    {
        await _eventBus.PublishUIEventAsync(UINotificationEvent.Info("Information", message));
    }
    #endregion

    #region Action Methods - Enhanced with better error handling
    /// <summary>
    /// Show location map in modal dialog
    /// </summary>
    private async Task ViewLocationMap(HazardLocation location)
    {
        try
        {
            _logger.LogInformation("Opening location map for hazard location: {LocationCode}", location.Code);

            if (!HasValidCoordinates(location))
            {
                await ShowErrorAsyncNotification("This location does not have valid coordinates to display on the map.");
                return;
            }

            // First, get the associated Hazard entity since HazardLocationDisplay expects a Hazard parameter
            var hazard = await GetHazardForLocation(location);
            
            if (hazard is null)
            {
                await ShowErrorAsyncNotification($"Could not find associated hazard for location {location.Code}");
                return;
            }

            var title = $"{location.Code} - Location Map";
            var subtitle = !string.IsNullOrEmpty(location.Description) 
                ? location.Description 
                : $"Hazard: {location.HazardCode}";

            // Open modal dialog with HazardLocationDisplay component
            await _dialogService.OpenAsync(title,
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
                        builder.AddContent(25, "Location Details");
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

            await ShowInfoAsyncNotification($"Opened location map for {location.Code}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening location map for {LocationCode}", location.Code);
            await ShowErrorAsyncNotification("Error opening location map");
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
            var hazardResult = await _mediator.SendAsync(hazardQuery, CancellationToken.None);

            if (hazardResult.IsSuccess && hazardResult.Value is not null)
            {
                return hazardResult.Value;
            }

            _logger.LogWarning("Could not find hazard {HazardCode} for location {LocationCode}", location.HazardCode, location.Code);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading hazard {HazardCode} for location {LocationCode}", location.HazardCode, location.Code);
            return null;
        }
    }

    private void ShowActions(HazardLocation location)
    {
        _logger.LogInformation("Actions requested for hazard location: {Code}", location.Code);
    }
    #endregion
}


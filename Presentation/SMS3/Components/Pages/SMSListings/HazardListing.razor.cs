using System.Linq.Expressions;

using SMS_Domain.Entities;

using Radzen;

using SMS_Application.Queries;

using SMS_Domain.Enums;
using SMS_Domain.Errors;
using SMS_Domain.Events;

using SMS_Shared.Configuration;

using SMS3.Components.Shared;
using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration.Extensions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Rendering;

namespace SMS3.Components.Pages.SMSListings;

/// <summary>
/// Hazard Listing Component - Enhanced with full CRUD operations
/// Provides comprehensive data grid listing with view details, edit navigation, and delete functionality
/// </summary>
public partial class HazardListing : ComponentBase
{
    private string _basicTextStyle = "font-size:smaller;font-weight: 600";

    #region Dependencies
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<HazardListing> _logger { get; set; } = default!;
    [Inject] private IBaseEventBus _eventBus { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;

    #endregion

    #region Properties
    private RadzenDataGrid<Hazard>? _hazardsGrid;
    private IEnumerable<Hazard> _hazards = new List<Hazard>();
    private List<Hazard> _allHazards = new List<Hazard>(); // Store all hazards for client-side filtering
    private HashSet<string> _riskRegistryOnlyReportCodes = new(StringComparer.OrdinalIgnoreCase);
    private int _totalCount;
    private bool _isLoading = false;

    private bool _showDescriptionModal = false;
    private string _selectedDescription = string.Empty;
    private string _selectedHazardId = string.Empty;
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

            _logger.LogInformation("Loading hazards for listing view");

            var query = new GetAllHazardsQuery();
            var result = await _mediator.SendAsync(query, CancellationToken.None);

            var reportsQuery = new GetAllReportsQuery();
            var reportsResult = await _mediator.SendAsync(reportsQuery, CancellationToken.None);
            if (reportsResult.IsSuccess && reportsResult.Value is not null)
            {
                _riskRegistryOnlyReportCodes = reportsResult.Value
                    .Where(r => IsRiskRegistryOnlyStatus(r.Status))
                    .Select(r => (r.Code ?? string.Empty).Trim())
                    .Where(code => !string.IsNullOrWhiteSpace(code))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                _riskRegistryOnlyReportCodes.Clear();
            }

            if (result.IsSuccess && result.Value is not null)
            {
                _allHazards = result.Value; // Store all hazards for filtering/sorting
                _hazards = _allHazards; // Initially show all hazards
                _totalCount = _allHazards.Count();
                _logger.LogInformation("Loaded {Count} hazards for listing", _totalCount);

                await ShowSuccessAsyncNotification($"Successfully loaded {_totalCount} hazards");
               
            }
            else
            {
                await ShowErrorAsyncNotification("Failed to load hazards");
                _logger.LogError("Failed to load hazards: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading hazards");
            await ShowErrorAsyncNotification("Error loading hazards");
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

            _logger.LogInformation("LoadData called with Skip: {Skip}, Top: {Top}, OrderBy: {OrderBy}, Filters: {FiltersCount}", 
                args.Skip, args.Top, args.OrderBy, args.Filters?.Count() ?? 0);

            // If we don't have all hazards yet, load them first
            if (_allHazards is null || !_allHazards.Any())
            {
                await LoadInitialData();
                return;
            }

            // Start with all hazards
            var query = _allHazards.AsQueryable();

            // Apply filtering
            if (args.Filters is not null && args.Filters.Any())
            {
                query = ApplyFiltering(query, args);
            }

            // Get total count after filtering but before paging
            _totalCount = query.Count();

            // Apply sorting
            if (!string.IsNullOrEmpty(args.OrderBy))
            {
                query = ApplySorting(query, args.OrderBy);
            }
            else
            {
                // Default sorting by CreatedDate descending
                query = query.OrderByDescending(h => h.CreatedDate);
            }

            // Apply paging
            if (args.Skip.HasValue && args.Skip > 0)
            {
                query = query.Skip(args.Skip.Value);
            }

            if (args.Top.HasValue && args.Top > 0)
            {
                query = query.Take(args.Top.Value);
            }

            _hazards = query.ToList();

            _logger.LogInformation("Applied filtering/sorting/paging. Showing {Count} of {Total} hazards", 
                _hazards.Count(), _totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in LoadData");
            await ShowErrorAsyncNotification("Error loading data");
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
    private IQueryable<Hazard> ApplyFiltering(IQueryable<Hazard> query, LoadDataArgs args)
    {
        try
        {
            // Handle advanced column-specific filters
            if (args.Filters is not null && args.Filters.Any())
            {
                foreach (var filter in args.Filters)
                {
                    var columnName = filter.Property?.ToLower();
                    var filterValue = filter.FilterValue?.ToString()?.ToLower();
                    var filterOperator = filter.FilterOperator;

                    if (string.IsNullOrEmpty(filterValue)) continue;

                    switch (columnName)
                    {
                        case "reportcode":
                            query = ApplyStringFilter(query, h => h.ReportCode, filterValue, filterOperator);
                            break;
                        case "code":
                            query = ApplyStringFilter(query, h => h.Code, filterValue, filterOperator);
                            break;
                        case "name":
                            query = ApplyStringFilter(query, h => h.Name, filterValue, filterOperator);
                            break;
                        case "hazardtitle":
                            query = ApplyStringFilter(query, h => h.HazardTitle, filterValue, filterOperator);
                            break;
                        case "description":
                            query = ApplyStringFilter(query, h => h.Description, filterValue, filterOperator);
                            break;
                        case "hazardcategory":
                            query = ApplyStringFilter(query, h => h.HazardCategory, filterValue, filterOperator);
                            break;
                        case "hazardrisklevel":
                            query = ApplyEnumFilter(query, h => h.HazardRiskLevel.Value, filterValue, filterOperator);
                            break;
                        case "locationarea":
                            query = ApplyStringFilter(query, h => h.LocationArea, filterValue, filterOperator);
                            break;
                        case "createddate":
                            if (DateTime.TryParse(filter.FilterValue?.ToString(), out var dateValue))
                            {
                                query = ApplyDateFilter(query, h => h.CreatedDate, dateValue, filterOperator);
                            }
                            break;
                    }
                }
            }

            return query;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying filters");
            return query; // Return unfiltered query if filtering fails
        }
    }

    /// <summary>
    /// Apply string-based filtering with different operators
    /// </summary>
    private IQueryable<Hazard> ApplyStringFilter(IQueryable<Hazard> query, Expression<Func<Hazard, string?>> propertySelector, string filterValue, FilterOperator filterOperator)
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
    /// Apply enum-based filtering (for RiskLevel, etc.)
    /// </summary>
    private IQueryable<Hazard> ApplyEnumFilter(IQueryable<Hazard> query, Expression<Func<Hazard, string?>> propertySelector, string filterValue, FilterOperator filterOperator)
    {
        return filterOperator switch
        {
            FilterOperator.Equals => query.Where(CombineExpressions(propertySelector, value => !string.IsNullOrEmpty(value) && value.ToLower() == filterValue)),
            FilterOperator.NotEquals => query.Where(CombineExpressions(propertySelector, value => string.IsNullOrEmpty(value) || value.ToLower() != filterValue)),
            FilterOperator.Contains => query.Where(CombineExpressions(propertySelector, value => !string.IsNullOrEmpty(value) && value.ToLower().Contains(filterValue))),
            _ => query.Where(CombineExpressions(propertySelector, value => !string.IsNullOrEmpty(value) && value.ToLower() == filterValue))
        };
    }

    /// <summary>
    /// Apply date-based filtering with different operators
    /// </summary>
    private IQueryable<Hazard> ApplyDateFilter(IQueryable<Hazard> query, Expression<Func<Hazard, DateTime?>> propertySelector, DateTime filterValue, FilterOperator filterOperator)
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
    /// Combine property selector with condition expression
    /// </summary>
    private Expression<Func<Hazard, bool>> CombineExpressions<T>(Expression<Func<Hazard, T>> propertySelector, Expression<Func<T, bool>> condition)
    {
        var parameter = propertySelector.Parameters[0];
        var property = propertySelector.Body;
        var conditionBody = condition.Body;
        var conditionParameter = condition.Parameters[0];

        // Replace the condition parameter with the property expression
        var visitor = new ParameterReplacementVisitor(conditionParameter, property);
        var newConditionBody = visitor.Visit(conditionBody);

        return Expression.Lambda<Func<Hazard, bool>>(newConditionBody, parameter);
    }

    /// <summary>
    /// Apply sorting based on OrderBy parameter from Radzen DataGrid
    /// </summary>
    private IQueryable<Hazard> ApplySorting(IQueryable<Hazard> query, string orderBy)
    {
        try
        {
            if (string.IsNullOrEmpty(orderBy)) return query;

            var (propertyName, isDescending) = ParseOrderBy(orderBy);
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                return query.OrderByDescending(h => h.CreatedDate);
            }

            propertyName = propertyName.ToLower();

            return propertyName switch
            {
                "reportcode" => isDescending ? query.OrderByDescending(h => h.ReportCode) : query.OrderBy(h => h.ReportCode),
                "code" => isDescending ? query.OrderByDescending(h => h.Code) : query.OrderBy(h => h.Code),
                "name" => isDescending ? query.OrderByDescending(h => h.Name) : query.OrderBy(h => h.Name),
                "hazardtitle" => isDescending ? query.OrderByDescending(h => h.HazardTitle) : query.OrderBy(h => h.HazardTitle),
                "description" => isDescending ? query.OrderByDescending(h => h.Description) : query.OrderBy(h => h.Description),
                "hazardcategory" => isDescending ? query.OrderByDescending(h => h.HazardCategory) : query.OrderBy(h => h.HazardCategory),
                "hazardrisklevel" => isDescending ? query.OrderByDescending(h => h.HazardRiskLevel.Value) : query.OrderBy(h => h.HazardRiskLevel.Value),
                "isinitialhazard" => isDescending ? query.OrderByDescending(h => h.IsInitialHazard) : query.OrderBy(h => h.IsInitialHazard),
                "locationarea" => isDescending ? query.OrderByDescending(h => h.LocationArea) : query.OrderBy(h => h.LocationArea),
                "createddate" => isDescending ? query.OrderByDescending(h => h.CreatedDate) : query.OrderBy(h => h.CreatedDate),
                "updateddate" => isDescending ? query.OrderByDescending(h => h.UpdatedDate) : query.OrderBy(h => h.UpdatedDate),
                _ => query.OrderByDescending(h => h.CreatedDate) // Default sort
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying sorting for OrderBy: {OrderBy}", orderBy);
            return query.OrderByDescending(h => h.CreatedDate); // Fallback to default sort
        }
    }

    private static (string propertyName, bool isDescending) ParseOrderBy(string orderBy)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
        {
            return (string.Empty, false);
        }

        var parts = orderBy.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var propertyName = parts[0];
        var isDescending = parts.Length > 1 && string.Equals(parts[1], "desc", StringComparison.OrdinalIgnoreCase);
        return (propertyName, isDescending);
    }
    #endregion

    #region Helper Methods - Keep existing functionality
    private static Expression<Func<Hazard, object>> GetPropertyExpression(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(Hazard), "x");
        var property = Expression.Property(parameter, propertyName);
        var conversion = Expression.Convert(property, typeof(object));
        return Expression.Lambda<Func<Hazard, object>>(conversion, parameter);
    }

    private bool IsRiskRegistryOnlyHazard(Hazard hazard)
    {
        var reportCode = hazard.ReportCode?.Trim();
        return !string.IsNullOrWhiteSpace(reportCode) && _riskRegistryOnlyReportCodes.Contains(reportCode);
    }

    private static bool IsRiskRegistryOnlyStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return false;
        }

        var normalizedStatus = status.Trim();
        return string.Equals(normalizedStatus, ReportStatus.RiskRegistryOnly, StringComparison.OrdinalIgnoreCase)
               || string.Equals(normalizedStatus, "RISK_REGISTRY_ONLY", StringComparison.OrdinalIgnoreCase);
    }

    private void ShowActions(Hazard hazard)
    {
        _logger.LogInformation("Actions requested for hazard: {Code}", hazard.Code);
    }

    /// <summary>
    /// Shows error notification to user
    /// </summary>
    private async Task ShowErrorAsyncNotification(string message)
    {
        await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", message, 7000));
    }

    /// <summary>
    /// Shows success notification to user
    /// </summary>
    private async Task ShowSuccessAsyncNotification(string message)
    {
        await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", message, 5000));
    }
    #endregion

    #region CRUD Action Methods - Keep existing placeholder methods

    /// <summary>
    /// Handle view hazard details - Show comprehensive read-only modal
    /// </summary>
    /// <param name="hazard">Hazard to view</param>
    public async Task OnViewHazardAsync(Hazard hazard)
    {
        _logger.LogInformation("View hazard details requested: {HazardCode}", hazard.Code);

        try
        {
            _isLoading = true;
            StateHasChanged();

            // TODO: Implement hazard details modal when ready
            await ShowSuccessAsyncNotification($"View details for hazard {hazard.Code} - Feature coming soon!");
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading hazard details for {HazardCode}", hazard.Code);
            await ShowErrorAsyncNotification("Failed to load hazard details");
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Handle edit hazard request - Navigate to Hazard page in edit mode
    /// </summary>
    /// <param name="hazard">Hazard to edit</param>
    public async Task OnEditHazardAsync(Hazard hazard)
    {
        _logger.LogInformation("Edit hazard requested: {HazardCode}", hazard.Code);

        try
        {
            var confirmed = await _dialogService.Confirm(
                $"Edit hazard '{hazard.Code} - {hazard.Name}'?\n\nThis will navigate to the hazard form in edit mode.",
                "Edit Hazard",
                new ConfirmOptions()
                {
                    OkButtonText = "Yes, Edit Hazard",
                    CancelButtonText = "Cancel"
                });

            if (confirmed == true)
            {
                // Navigate to HazardReporting page with the hazard code as route parameter
                _navigation.NavigateToSecure($"/SMSRiskManagement/HazardReporting/{hazard.Code}?returnTo=hazard-listing");
                _logger.LogInformation("Navigating to edit hazard: {HazardCode}", hazard.Code);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to edit hazard {HazardCode}", hazard.Code);
            await ShowErrorAsyncNotification("Failed to navigate to edit form");
        }
    }

    /// <summary>
    /// Handle delete hazard request - Show confirmation and delete via CQRS
    /// </summary>
    /// <param name="hazard">Hazard to delete</param>
    public async Task OnDeleteHazardAsync(Hazard hazard)
    {
        _logger.LogInformation("Delete hazard requested: {HazardCode}", hazard.Code);

        try
        {
            var confirmationMessage = $"Are you sure you want to delete hazard '{hazard.Code}'?\n\n" +
                                    $"Hazard Details:\n" +
                                    $"• Name: {hazard.Name ?? "Unnamed Hazard"}\n" +
                                    $"• Category: {hazard.HazardCategory ?? "Unknown"}\n" +
                                    $"• Risk Level: {hazard.HazardRiskLevel.Value ?? "Unknown"}\n\n" +
                                    "WARNING: This hazard has associated details that may also be affected.\n\n" +
                                    "This action cannot be undone!";

            var confirmed = await _dialogService.Confirm(
                confirmationMessage, 
                "Confirm Delete Hazard",
                new ConfirmOptions()
                {
                    OkButtonText = "Yes, Delete Hazard",
                    CancelButtonText = "Cancel",
                    AutoFocusFirstElement = false
                });

            if (confirmed == true)
            {
                // TODO: Implement delete command when ready
                await ShowSuccessAsyncNotification($"Delete hazard {hazard.Code} - Command coming soon!");
                _logger.LogInformation("Delete confirmed for hazard: {HazardCode}", hazard.Code);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting hazard: {HazardCode}", hazard.Code);
            await ShowErrorAsyncNotification("Failed to delete the hazard");
        }
    }
    #endregion

    private void RenderHazardDescriptionColumn(RenderTreeBuilder builder, bool includeActions = true)
    {
        builder.OpenComponent<RadzenDataGridColumn<Hazard>>(10);
        builder.AddAttribute(11, "Title", "Description");
        builder.AddAttribute(12, "Width", "100px");
        builder.AddAttribute(13, "Sortable", false);
        builder.AddAttribute(14, "Template", (RenderFragment<Hazard>)(hazard =>
            (templateBuilder =>
            {
                templateBuilder.OpenComponent<RadzenButton>(0);
                templateBuilder.AddAttribute(1, "Text", "Description");
                templateBuilder.AddAttribute(2, "Icon", "description");
                templateBuilder.AddAttribute(3, "ButtonStyle", ButtonStyle.Base);
                templateBuilder.AddAttribute(4, "Variant", Variant.Text);
                templateBuilder.AddAttribute(5, "Size", ButtonSize.ExtraSmall);
                templateBuilder.AddAttribute(6, "Title", "Click to view full description");
                templateBuilder.AddAttribute(7, "Class", "description-button");
                templateBuilder.AddAttribute(8, "Click", EventCallback.Factory.Create<MouseEventArgs>(this,
                    (args) => ShowDescriptionDialog(hazard)));
                templateBuilder.CloseComponent();
            }
             )));
        builder.CloseComponent();
    }

    private async Task ShowDescriptionDialog(Hazard hazard)
    {
        try
        {
            _selectedDescription = hazard.Description ?? "No description available";
            _selectedHazardId = hazard.Code ?? "Unknown";
            _showDescriptionModal = true;
            StateHasChanged();

            _logger.LogInformation("Showing description modal for hazard {HazardId}", hazard.Code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing description modal for hazard {HazardId}", hazard.Code);
            await ShowErrorAsyncNotification("Error showing description details");
        }
    }

    private void CloseDescriptionModal()
    {
        _showDescriptionModal = false;
        _selectedDescription = string.Empty;
        _selectedHazardId = string.Empty;
        StateHasChanged();
    }
}


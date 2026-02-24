using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Application.Messaging.Queries;
using SMS3.Components.Shared.UIHelpers;
using SMS3.Components.Shared;
using SMS_Domain.Errors;
using System.Linq.Expressions;
using Radzen;

namespace SMS3.Components.Pages.Listings;

/// <summary>
/// Hazard Listing Component - Enhanced with full CRUD operations
/// Provides comprehensive data grid listing with view details, edit navigation, and delete functionality
/// </summary>
public partial class HazardListing : ComponentBase
{
    private string BasicTextStyle = "font-size:smaller;font-weight: 600";

    #region Dependencies
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<HazardListing> Logger { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private AuthenticationService AuthService { get; set; } = default!;
    #endregion

    #region Properties
    private RadzenDataGrid<Hazard>? hazardsGrid;
    private IEnumerable<Hazard> hazards = new List<Hazard>();
    private List<Hazard> allHazards = new List<Hazard>(); // Store all hazards for client-side filtering
    private int totalCount;
    private bool isLoading = false;
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
            isLoading = true;
            StateHasChanged();

            Logger.LogInformation("Loading hazards for listing view");

            var query = new GetAllHazardsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                allHazards = result.Value; // Store all hazards for filtering/sorting
                hazards = allHazards; // Initially show all hazards
                totalCount = allHazards.Count();
                Logger.LogInformation("Loaded {Count} hazards for listing", totalCount);

                ShowSuccessNotification($"Successfully loaded {totalCount} hazards");
               
            }
            else
            {
                ShowErrorNotification("Failed to load hazards");
                Logger.LogError("Failed to load hazards: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading hazards");
            ShowErrorNotification("Error loading hazards");
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private async Task LoadData(LoadDataArgs args)
    {
        try
        {
            isLoading = true;
            StateHasChanged();

            Logger.LogInformation("LoadData called with Skip: {Skip}, Top: {Top}, OrderBy: {OrderBy}, Filter: {Filter}", 
                args.Skip, args.Top, args.OrderBy, args.Filter);

            // If we don't have all hazards yet, load them first
            if (allHazards == null || !allHazards.Any())
            {
                await LoadInitialData();
                return;
            }

            // Start with all hazards
            var query = allHazards.AsQueryable();

            // Apply filtering
            if (!string.IsNullOrEmpty(args.Filter))
            {
                query = ApplyFiltering(query, args);
            }

            // Get total count after filtering but before paging
            totalCount = query.Count();

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

            hazards = query.ToList();

            Logger.LogInformation("Applied filtering/sorting/paging. Showing {Count} of {Total} hazards", 
                hazards.Count(), totalCount);
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
    /// Apply filtering based on Radzen DataGrid filter arguments
    /// </summary>
    private IQueryable<Hazard> ApplyFiltering(IQueryable<Hazard> query, LoadDataArgs args)
    {
        try
        {
            // Handle simple string filter (when user types in the general filter)
            if (!string.IsNullOrEmpty(args.Filter) && !args.Filter.Contains("("))
            {
                var filterValue = args.Filter.ToLower();
                query = query.Where(h => 
                    (!string.IsNullOrEmpty(h.Code) && h.Code.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(h.Name) && h.Name.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(h.Description) && h.Description.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(h.ReportCode) && h.ReportCode.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(h.HazardCategory) && h.HazardCategory.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(h.LocationArea) && h.LocationArea.ToLower().Contains(filterValue))
                );
                return query;
            }

            // Handle advanced column-specific filters
            if (args.Filters != null && args.Filters.Any())
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
            Logger.LogError(ex, "Error applying filters");
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

            var parts = orderBy.Split(' ');
            var propertyName = parts[0].ToLower();
            var isDescending = parts.Length > 1 && parts[1].ToLower() == "desc";

            return propertyName switch
            {
                "reportcode" => isDescending ? query.OrderByDescending(h => h.ReportCode) : query.OrderBy(h => h.ReportCode),
                "code" => isDescending ? query.OrderByDescending(h => h.Code) : query.OrderBy(h => h.Code),
                "name" => isDescending ? query.OrderByDescending(h => h.Name) : query.OrderBy(h => h.Name),
                "description" => isDescending ? query.OrderByDescending(h => h.Description) : query.OrderBy(h => h.Description),
                "hazardcategory" => isDescending ? query.OrderByDescending(h => h.HazardCategory) : query.OrderBy(h => h.HazardCategory),
                "hazardrisklevel" => isDescending ? query.OrderByDescending(h => h.HazardRiskLevel.Value) : query.OrderBy(h => h.HazardRiskLevel.Value),
                "locationarea" => isDescending ? query.OrderByDescending(h => h.LocationArea) : query.OrderBy(h => h.LocationArea),
                "createddate" => isDescending ? query.OrderByDescending(h => h.CreatedDate) : query.OrderBy(h => h.CreatedDate),
                "updateddate" => isDescending ? query.OrderByDescending(h => h.UpdatedDate) : query.OrderBy(h => h.UpdatedDate),
                _ => query.OrderByDescending(h => h.CreatedDate) // Default sort
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error applying sorting for OrderBy: {OrderBy}", orderBy);
            return query.OrderByDescending(h => h.CreatedDate); // Fallback to default sort
        }
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

    private void ShowActions(Hazard hazard)
    {
        Logger.LogInformation("Actions requested for hazard: {Code}", hazard.Code);
    }

    /// <summary>
    /// Shows error notification to user
    /// </summary>
    private void ShowErrorNotification(string message)
    {
        NotificationHelper.ShowError(NotificationService, message, 7000);
    }

    /// <summary>
    /// Shows success notification to user
    /// </summary>
    private void ShowSuccessNotification(string message)
    {
        NotificationHelper.ShowSuccess(NotificationService, message, 5000);
    }
    #endregion

    #region CRUD Action Methods - Keep existing placeholder methods

    /// <summary>
    /// Handle view hazard details - Show comprehensive read-only modal
    /// </summary>
    /// <param name="hazard">Hazard to view</param>
    public async Task OnViewHazardAsync(Hazard hazard)
    {
        Logger.LogInformation("View hazard details requested: {HazardCode}", hazard.Code);

        try
        {
            isLoading = true;
            StateHasChanged();

            // TODO: Implement hazard details modal when ready
            ShowSuccessNotification($"View details for hazard {hazard.Code} - Feature coming soon!");
            
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading hazard details for {HazardCode}", hazard.Code);
            ShowErrorNotification("Failed to load hazard details");
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Handle edit hazard request - Navigate to Hazard page in edit mode
    /// </summary>
    /// <param name="hazard">Hazard to edit</param>
    public async Task OnEditHazardAsync(Hazard hazard)
    {
        Logger.LogInformation("Edit hazard requested: {HazardCode}", hazard.Code);

        try
        {
            var confirmed = await DialogService.Confirm(
                $"Edit hazard '{hazard.Code} - {hazard.Name}'?\n\nThis will navigate to the hazard form in edit mode.",
                "Edit Hazard",
                new ConfirmOptions()
                {
                    OkButtonText = "Yes, Edit Hazard",
                    CancelButtonText = "Cancel"
                });

            if (confirmed == true)
            {
                // TODO: Implement navigation to hazard edit form
                ShowSuccessNotification($"Edit hazard {hazard.Code} - Navigation coming soon!");
                Logger.LogInformation("Edit confirmed for hazard: {HazardCode}", hazard.Code);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error navigating to edit hazard {HazardCode}", hazard.Code);
            ShowErrorNotification("Failed to navigate to edit form");
        }
    }

    /// <summary>
    /// Handle delete hazard request - Show confirmation and delete via CQRS
    /// </summary>
    /// <param name="hazard">Hazard to delete</param>
    public async Task OnDeleteHazardAsync(Hazard hazard)
    {
        Logger.LogInformation("Delete hazard requested: {HazardCode}", hazard.Code);

        try
        {
            var confirmationMessage = $"Are you sure you want to delete hazard '{hazard.Code}'?\n\n" +
                                    $"Hazard Details:\n" +
                                    $"• Name: {hazard.Name ?? "Unnamed Hazard"}\n" +
                                    $"• Category: {hazard.HazardCategory ?? "Unknown"}\n" +
                                    $"• Risk Level: {hazard.HazardRiskLevel.Value ?? "Unknown"}\n\n" +
                                    "?? WARNING: This hazard has associated details that may also be affected.\n\n" +
                                    "? This action cannot be undone!";

            var confirmed = await DialogService.Confirm(
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
                ShowSuccessNotification($"Delete hazard {hazard.Code} - Command coming soon!");
                Logger.LogInformation("Delete confirmed for hazard: {HazardCode}", hazard.Code);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting hazard: {HazardCode}", hazard.Code);
            ShowErrorNotification("Failed to delete the hazard");
        }
    }
    #endregion
}
using System.Linq.Expressions;

using SMS_Domain.Entities;

using Radzen;

using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;

using SMS_Domain.Enums;
using SMS_Domain.Errors;
using SMS_Domain.ValueObjects;

using SMS_Shared.Configuration;

using SMS3.Components.Shared;
using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration.Extensions;

namespace SMS3.Components.Pages.SMSListing;

/// <summary>
/// Investigation Listing Component - Enhanced with full CRUD operations and advanced filtering
/// Provides comprehensive data grid listing with view details, edit navigation, and delete functionality
/// </summary>
public partial class InvestigationListing : ComponentBase
{
    private string BasicTextStyle = "font-size:smaller;font-weight: 600";

    #region Dependencies
    [Inject] private IMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<InvestigationListing> _logger { get; set; } = default!;
    [Inject] private INotificationHelper  _notificationHelper { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    #endregion

    #region Properties
    private RadzenDataGrid<Investigation>? investigationsGrid;
    private IEnumerable<Investigation> investigations = new List<Investigation>();
    private List<Investigation> allInvestigations = new List<Investigation>(); // Store all investigations for client-side filtering
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

            _logger.LogInformation("Loading investigations for listing view");

            var query = new GetAllInvestigationsQuery();
            var result = await _mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                allInvestigations = result.Value.ToList(); // Store all investigations for filtering/sorting
                investigations = allInvestigations; // Initially show all investigations
                totalCount = allInvestigations.Count();
                _logger.LogInformation("Loaded {Count} investigations for listing", totalCount);

                // Only show success notification if we have data
                if (totalCount > 0)
                {
                    await _notificationHelper.ShowSuccessAsync($"Successfully loaded {totalCount} investigations");

                    if (totalCount == 0)
                    {
                        await _notificationHelper.ShowInfoAsync("No investigations found");
                    }
                }
                else
                {
                    await _notificationHelper.ShowErrorAsync("Failed to load investigations");
                    _logger.LogError("Failed to load investigations: {Error}", result.Error?.Message);
                }
            }
            else
            {
                await _notificationHelper.ShowErrorAsync("Failed to load investigations");
                _logger.LogError("Failed to load investigations: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading investigations");
            await _notificationHelper.ShowErrorAsync($"Error loading investigations: {ex.Message}");
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

            _logger.LogInformation("LoadData called with Skip: {Skip}, Top: {Top}, OrderBy: {OrderBy}, Filter: {Filter}", 
                args.Skip, args.Top, args.OrderBy, args.Filter);

            // If we don't have all investigations yet, load them first
            if (allInvestigations == null || !allInvestigations.Any())
            {
                _logger.LogInformation("No investigations cached, loading initial data");
                await LoadInitialData();
                return;
            }

            // Start with all investigations
            var query = allInvestigations.AsQueryable();
            _logger.LogInformation("Starting with {Count} total investigations", query.Count());

            // Apply filtering
            if (!string.IsNullOrEmpty(args.Filter))
            {
                _logger.LogInformation("Applying filter: {Filter}", args.Filter);
                query = ApplyFiltering(query, args);
                _logger.LogInformation("After filtering: {Count} investigations", query.Count());
            }

            // Get total count after filtering but before paging
            totalCount = query.Count();

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
                query = query.OrderByDescending(i => i.CreatedDate ?? DateTime.MinValue);
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

            investigations = query.ToList();

            _logger.LogInformation("Applied filtering/sorting/paging. Showing {Count} of {Total} investigations", 
                investigations.Count(), totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in LoadData with args: Skip={Skip}, Top={Top}, OrderBy={OrderBy}, Filter={Filter}", 
                args.Skip, args.Top, args.OrderBy, args.Filter);
            await _notificationHelper.ShowErrorAsync($"Error loading data: {ex.Message}");
            
            // Fallback to show all data without filtering/sorting
            try
            {
                investigations = allInvestigations ?? new List<Investigation>();
                totalCount = investigations.Count();
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "Error in LoadData fallback");
                investigations = new List<Investigation>();
                totalCount = 0;
            }
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
    private IQueryable<Investigation> ApplyFiltering(IQueryable<Investigation> query, LoadDataArgs args)
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
                
                query = query.Where(i => 
                    (!string.IsNullOrEmpty(i.Code) && i.Code.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(i.ReportCode) && i.ReportCode.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(i.HazardCode) && i.HazardCode.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(i.InvestigationNotes) && i.InvestigationNotes.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(i.AssignedInvestigatorId) && i.AssignedInvestigatorId.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(i.CreatedBy) && i.CreatedBy.ToLower().Contains(filterValue))
                );
                return query;
            }

            // Handle advanced column-specific filters
            if (args.Filters != null && args.Filters.Any())
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
                        case "reportcode":
                            query = ApplyStringFilter(query, i => i.ReportCode, filterValue, filterOperator);
                            break;
                        case "hazardcode":
                            query = ApplyStringFilter(query, i => i.HazardCode, filterValue, filterOperator);
                            break;
                        case "code":
                            query = ApplyStringFilter(query, i => i.Code, filterValue, filterOperator);
                            break;
                        case "investigationnotes":
                            query = ApplyStringFilter(query, i => i.InvestigationNotes, filterValue, filterOperator);
                            break;
                        case "assignedinvestigatorid":
                            query = ApplyStringFilter(query, i => i.AssignedInvestigatorId, filterValue, filterOperator);
                            break;
                        case "createdby":
                            query = ApplyStringFilter(query, i => i.CreatedBy, filterValue, filterOperator);
                            break;
                        case "status":
                            query = ApplyEnumFilter(query, i => i.Status.ToString(), filterValue, filterOperator);
                            break;
                        case "completeddate":
                            if (DateTime.TryParse(filter.FilterValue?.ToString(), out var completedDateValue))
                            {
                                query = ApplyDateFilter(query, i => i.CompletedDate, completedDateValue, filterOperator);
                            }
                            break;
                        case "createddate":
                            if (DateTime.TryParse(filter.FilterValue?.ToString(), out var createdDateValue))
                            {
                                query = ApplyDateFilter(query, i => i.CreatedDate, createdDateValue, filterOperator);
                            }
                            break;
                        case "decisiondate":
                            if (DateTime.TryParse(filter.FilterValue?.ToString(), out var decisionDateValue))
                            {
                                query = ApplyDateFilter(query, i => i.DecisionDate, decisionDateValue, filterOperator);
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
    private IQueryable<Investigation> ApplyStringFilter(IQueryable<Investigation> query, Expression<Func<Investigation, string?>> propertySelector, string filterValue, FilterOperator filterOperator)
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
    /// Apply enum-based filtering (for Status, etc.)
    /// </summary>
    private IQueryable<Investigation> ApplyEnumFilter(IQueryable<Investigation> query, Expression<Func<Investigation, string?>> propertySelector, string filterValue, FilterOperator filterOperator)
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
    private IQueryable<Investigation> ApplyDateFilter(IQueryable<Investigation> query, Expression<Func<Investigation, DateTime?>> propertySelector, DateTime filterValue, FilterOperator filterOperator)
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
    private Expression<Func<Investigation, bool>> CombineExpressions<T>(Expression<Func<Investigation, T>> propertySelector, Expression<Func<T, bool>> condition)
    {
        var parameter = propertySelector.Parameters[0];
        var property = propertySelector.Body;
        var conditionBody = condition.Body;
        var conditionParameter = condition.Parameters[0];

        // Replace the condition parameter with the property expression
        var visitor = new ParameterReplacementVisitor(conditionParameter, property);
        var newConditionBody = visitor.Visit(conditionBody);

        return Expression.Lambda<Func<Investigation, bool>>(newConditionBody, parameter);
    }

    /// <summary>
    /// Apply sorting based on OrderBy parameter from Radzen DataGrid
    /// </summary>
    private IQueryable<Investigation> ApplySorting(IQueryable<Investigation> query, string orderBy)
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
                "reportcode" => isDescending ? query.OrderByDescending(i => i.ReportCode ?? "") : query.OrderBy(i => i.ReportCode ?? ""),
                "hazardcode" => isDescending ? query.OrderByDescending(i => i.HazardCode ?? "") : query.OrderBy(i => i.HazardCode ?? ""),
                "code" => isDescending ? query.OrderByDescending(i => i.Code ?? "") : query.OrderBy(i => i.Code ?? ""),
                "investigationnotes" => isDescending ? query.OrderByDescending(i => i.InvestigationNotes ?? "") : query.OrderBy(i => i.InvestigationNotes ?? ""),
                "assignedinvestigatorid" => isDescending ? query.OrderByDescending(i => i.AssignedInvestigatorId ?? "") : query.OrderBy(i => i.AssignedInvestigatorId ?? ""),
                "createdby" => isDescending ? query.OrderByDescending(i => i.CreatedBy ?? "") : query.OrderBy(i => i.CreatedBy ?? ""),
                "status" => isDescending ? query.OrderByDescending(i => i.Status.ToString()) : query.OrderBy(i => i.Status.ToString()),
                "completeddate" => isDescending ? query.OrderByDescending(i => i.CompletedDate) : query.OrderBy(i => i.CompletedDate),
                "createddate" => isDescending ? query.OrderByDescending(i => i.CreatedDate) : query.OrderBy(i => i.CreatedDate),
                "updateddate" => isDescending ? query.OrderByDescending(i => i.UpdatedDate) : query.OrderBy(i => i.UpdatedDate),
                "decisiondate" => isDescending ? query.OrderByDescending(i => i.DecisionDate) : query.OrderBy(i => i.DecisionDate),
                _ => query.OrderByDescending(i => i.CreatedDate ?? DateTime.MinValue) // Default sort with null handling
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying sorting for OrderBy: {OrderBy}", orderBy);
            return query.OrderByDescending(i => i.CreatedDate ?? DateTime.MinValue); // Fallback to default sort
        }
    }
    #endregion

    #region Action Methods - Enhanced with better error handling
    private void ShowActions(Investigation investigation)
    {
        _logger.LogInformation("Actions requested for investigation: {Code}", investigation.Code);
    }

    private async Task ViewInvestigation(Investigation investigation)
    {
        try
        {
            if (investigation == null) return;

            // Navigate to Investigation with HazardCode if available
            var navigationUrl = string.IsNullOrWhiteSpace(investigation.HazardCode)
                ? $"/SMSRiskManagement/Investigations/{investigation.Code}"
                : $"/SMSRiskManagement/Investigations/{investigation.Code}/{investigation.HazardCode}";

            _logger.LogInformation("Navigating to investigation: {Code} with URL: {Url}", investigation.Code, navigationUrl);
            _navigation.NavigateToSecure(navigationUrl);
            await _notificationHelper.ShowInfoAsync($"Opening investigation {investigation.Code}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error viewing investigation {Code}", investigation?.Code);
            await _notificationHelper.ShowErrorAsync("Error opening investigation");
        }
    }

    private async Task EditInvestigation(Investigation investigation)
    {
        try
        {
            if (investigation == null) return;

            // Navigate to Investigation edit mode with HazardCode if available
            var navigationUrl = string.IsNullOrWhiteSpace(investigation.HazardCode)
                ? $"/SMSRiskManagement/Investigations/{investigation.Code}"
                : $"/SMSRiskManagement/Investigations/{investigation.Code}/{investigation.HazardCode}";

            _logger.LogInformation("Navigating to edit investigation: {Code} with URL: {Url}", investigation.Code, navigationUrl);
            _navigation.NavigateToSecure(navigationUrl);
            await _notificationHelper.ShowInfoAsync($"Opening investigation editor for {investigation.Code}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing investigation {Code}", investigation?.Code);
            await _notificationHelper.ShowErrorAsync("Error opening investigation editor");
        }
    }
    #endregion
}

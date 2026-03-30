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

namespace SMS3.Components.Pages.Listings;

/// <summary>
/// Risk Assessment Listing Component - Enhanced with full CRUD operations and advanced filtering
/// Provides comprehensive data grid listing with view details, edit navigation, and delete functionality
/// </summary>
public partial class RiskAssessmentListing : ComponentBase
{
    private string BasicTextStyle = "font-size:smaller;font-weight: 600";

    #region Dependencies
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<RiskAssessmentListing> Logger { get; set; } = default!;
    

    [Inject] private INotificationHelper  NotificationHelper { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    #endregion

    #region Properties
    private RadzenDataGrid<RiskAssessment>? assessmentsGrid;
    private IEnumerable<RiskAssessment> assessments = new List<RiskAssessment>();
    private List<RiskAssessment> allAssessments = new List<RiskAssessment>(); // Store all assessments for client-side filtering
    private int totalCount;
    private bool isLoading = false;
    private bool ShowViewDialog = false;
    private RiskAssessment? SelectedAssessment = null;
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

            Logger.LogInformation("Loading risk assessments for listing view");

            var query = new GetAllRiskAssessmentsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                allAssessments = result.Value.ToList(); // Ensure it's a concrete list
                assessments = allAssessments; // Initially show all assessments
                totalCount = allAssessments.Count();
                Logger.LogInformation("Loaded {Count} risk assessments for listing", totalCount);

                // Only show success notification if we have data
                if (totalCount > 0)
                {
                    await NotificationHelper.ShowSuccessAsync($"Successfully loaded {totalCount} risk assessments");
                }
                else
                {
                    await NotificationHelper.ShowInfoAsync("No risk assessments found");
                }
            }
            else
            {
                // Initialize with empty lists to prevent null reference issues
                allAssessments = new List<RiskAssessment>();
                assessments = allAssessments;
                totalCount = 0;
                
                await NotificationHelper.ShowErrorAsync("Failed to load risk assessments");
                Logger.LogError("Failed to load risk assessments: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            // Ensure we always have valid collections even if an error occurs
            allAssessments = new List<RiskAssessment>();
            assessments = allAssessments;
            totalCount = 0;
            
            Logger.LogError(ex, "Error loading risk assessments");
            await NotificationHelper.ShowErrorAsync($"Error loading risk assessments: {ex.Message}");
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

            // If we don't have all assessments yet, load them first
            if (allAssessments == null || !allAssessments.Any())
            {
                Logger.LogInformation("No assessments cached, loading initial data");
                await LoadInitialData();
                return;
            }

            // Start with all assessments
            var query = allAssessments.AsQueryable();
            Logger.LogInformation("Starting with {Count} total assessments", query.Count());

            // Apply filtering
            if (!string.IsNullOrEmpty(args.Filter))
            {
                Logger.LogInformation("Applying filter: {Filter}", args.Filter);
                query = ApplyFiltering(query, args);
                Logger.LogInformation("After filtering: {Count} assessments", query.Count());
            }

            // Get total count after filtering but before paging
            totalCount = query.Count();

            // Apply sorting
            if (!string.IsNullOrEmpty(args.OrderBy))
            {
                Logger.LogInformation("Applying sorting: {OrderBy}", args.OrderBy);
                query = ApplySorting(query, args.OrderBy);
                Logger.LogInformation("Sorting applied successfully");
            }
            else
            {
                // Default sorting by CreatedDate descending
                Logger.LogInformation("Applying default sort by CreatedDate");
                query = query.OrderByDescending(a => a.CreatedDate ?? DateTime.MinValue);
            }

            // Apply paging
            if (args.Skip.HasValue && args.Skip > 0)
            {
                Logger.LogInformation("Applying skip: {Skip}", args.Skip);
                query = query.Skip(args.Skip.Value);
            }

            if (args.Top.HasValue && args.Top > 0)
            {
                Logger.LogInformation("Applying take: {Top}", args.Top);
                query = query.Take(args.Top.Value);
            }

            assessments = query.ToList();

            Logger.LogInformation("Applied filtering/sorting/paging. Showing {Count} of {Total} assessments", 
                assessments.Count(), totalCount);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in LoadData with args: Skip={Skip}, Top={Top}, OrderBy={OrderBy}, Filter={Filter}", 
                args.Skip, args.Top, args.OrderBy, args.Filter);
            await NotificationHelper.ShowErrorAsync($"Error loading data: {ex.Message}");
            
            // Fallback to show all data without filtering/sorting
            try
            {
                assessments = allAssessments ?? new List<RiskAssessment>();
                totalCount = assessments.Count();
            }
            catch (Exception fallbackEx)
            {
                Logger.LogError(fallbackEx, "Error in LoadData fallback");
                assessments = new List<RiskAssessment>();
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
    private IQueryable<RiskAssessment> ApplyFiltering(IQueryable<RiskAssessment> query, LoadDataArgs args)
    {
        try
        {
            Logger.LogInformation("ApplyFiltering called with Filter: {Filter}, Filters count: {FilterCount}", 
                args.Filter, args.Filters?.Count() ?? 0);

            // Handle simple string filter (when user types in the general filter)
            if (!string.IsNullOrEmpty(args.Filter) && !args.Filter.Contains("("))
            {
                var filterValue = args.Filter.ToLower();
                Logger.LogInformation("Applying simple string filter: {FilterValue}", filterValue);
                
                query = query.Where(a => 
                    (!string.IsNullOrEmpty(a.Code) && a.Code.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(a.Name) && a.Name.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(a.Description) && a.Description.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(a.ReportCode) && a.ReportCode.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(a.HazardCode) && a.HazardCode.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(a.LeadAssessorId) && a.LeadAssessorId.ToLower().Contains(filterValue))
                );
                return query;
            }

            // Handle advanced column-specific filters
            if (args.Filters != null && args.Filters.Any())
            {
                Logger.LogInformation("Applying {Count} advanced filters", args.Filters.Count());
                
                foreach (var filter in args.Filters)
                {
                    var columnName = filter.Property?.ToLower();
                    var filterValue = filter.FilterValue?.ToString()?.ToLower();
                    var filterOperator = filter.FilterOperator;

                    Logger.LogInformation("Processing filter - Column: {Column}, Value: {Value}, Operator: {Operator}", 
                        columnName, filterValue, filterOperator);

                    if (string.IsNullOrEmpty(filterValue)) continue;

                    switch (columnName)
                    {
                        case "reportcode":
                            query = ApplyStringFilter(query, a => a.ReportCode, filterValue, filterOperator);
                            break;
                        case "code":
                            query = ApplyStringFilter(query, a => a.Code, filterValue, filterOperator);
                            break;
                        case "name":
                            query = ApplyStringFilter(query, a => a.Name, filterValue, filterOperator);
                            break;
                        case "description":
                            query = ApplyStringFilter(query, a => a.Description, filterValue, filterOperator);
                            break;
                        case "hazardcode":
                            query = ApplyStringFilter(query, a => a.HazardCode, filterValue, filterOperator);
                            break;
                        case "leadassessorid":
                            query = ApplyStringFilter(query, a => a.LeadAssessorId, filterValue, filterOperator);
                            break;
                        case "status":
                            query = ApplyEnumFilter(query, a => a.Status.ToString(), filterValue, filterOperator);
                            break;
                        case "stage":
                            query = ApplyEnumFilter(query, a => a.Stage.ToString(), filterValue, filterOperator);
                            break;
                        case "assessmenttype":
                            query = ApplyEnumFilter(query, a => a.AssessmentType.ToString(), filterValue, filterOperator);
                            break;
                        case "riskassessmentcategory":
                            query = ApplyEnumFilter(query, a => a.RiskAssessmentCategory.ToString(), filterValue, filterOperator);
                            break;
                        case "currentstep":
                            if (int.TryParse(filter.FilterValue?.ToString(), out var stepValue))
                            {
                                query = ApplyNumericFilter(query, a => a.CurrentStep, stepValue, filterOperator);
                            }
                            break;
                        case "completeddate":
                            if (DateTime.TryParse(filter.FilterValue?.ToString(), out var dateValue))
                            {
                                query = ApplyDateFilter(query, a => a.CompletedDate, dateValue, filterOperator);
                            }
                            break;
                        case "createddate":
                            if (DateTime.TryParse(filter.FilterValue?.ToString(), out var createdDateValue))
                            {
                                query = ApplyDateFilter(query, a => a.CreatedDate, createdDateValue, filterOperator);
                            }
                            break;
                        default:
                            Logger.LogWarning("Unknown filter column: {ColumnName}", columnName);
                            break;
                    }
                }
            }

            return query;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error applying filters - Filter: {Filter}, Filters: {@Filters}", 
                args.Filter, args.Filters?.Select(f => new { f.Property, f.FilterValue, f.FilterOperator }));
            return query; // Return unfiltered query if filtering fails
        }
    }

    /// <summary>
    /// Apply string-based filtering with different operators
    /// </summary>
    private IQueryable<RiskAssessment> ApplyStringFilter(IQueryable<RiskAssessment> query, Expression<Func<RiskAssessment, string?>> propertySelector, string filterValue, FilterOperator filterOperator)
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
    /// Apply enum-based filtering (for Status, Stage, etc.)
    /// </summary>
    private IQueryable<RiskAssessment> ApplyEnumFilter(IQueryable<RiskAssessment> query, Expression<Func<RiskAssessment, string?>> propertySelector, string filterValue, FilterOperator filterOperator)
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
    /// Apply numeric filtering (for CurrentStep, etc.)
    /// </summary>
    private IQueryable<RiskAssessment> ApplyNumericFilter(IQueryable<RiskAssessment> query, Expression<Func<RiskAssessment, int>> propertySelector, int filterValue, FilterOperator filterOperator)
    {
        return filterOperator switch
        {
            FilterOperator.Equals => query.Where(CombineExpressions(propertySelector, value => value == filterValue)),
            FilterOperator.NotEquals => query.Where(CombineExpressions(propertySelector, value => value != filterValue)),
            FilterOperator.LessThan => query.Where(CombineExpressions(propertySelector, value => value < filterValue)),
            FilterOperator.LessThanOrEquals => query.Where(CombineExpressions(propertySelector, value => value <= filterValue)),
            FilterOperator.GreaterThan => query.Where(CombineExpressions(propertySelector, value => value > filterValue)),
            FilterOperator.GreaterThanOrEquals => query.Where(CombineExpressions(propertySelector, value => value >= filterValue)),
            _ => query.Where(CombineExpressions(propertySelector, value => value == filterValue))
        };
    }

    /// <summary>
    /// Apply date-based filtering with different operators
    /// </summary>
    private IQueryable<RiskAssessment> ApplyDateFilter(IQueryable<RiskAssessment> query, Expression<Func<RiskAssessment, DateTime?>> propertySelector, DateTime filterValue, FilterOperator filterOperator)
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
    private Expression<Func<RiskAssessment, bool>> CombineExpressions<T>(Expression<Func<RiskAssessment, T>> propertySelector, Expression<Func<T, bool>> condition)
    {
        var parameter = propertySelector.Parameters[0];
        var property = propertySelector.Body;
        var conditionBody = condition.Body;
        var conditionParameter = condition.Parameters[0];

        // Replace the condition parameter with the property expression
        var visitor = new ParameterReplacementVisitor(conditionParameter, property);
        var newConditionBody = visitor.Visit(conditionBody);

        return Expression.Lambda<Func<RiskAssessment, bool>>(newConditionBody, parameter);
    }

    /// <summary>
    /// Apply sorting based on OrderBy parameter from Radzen DataGrid
    /// </summary>
    private IQueryable<RiskAssessment> ApplySorting(IQueryable<RiskAssessment> query, string orderBy)
    {
        try
        {
            if (string.IsNullOrEmpty(orderBy)) return query;

            var parts = orderBy.Split(' ');
            var propertyName = parts[0].ToLower();
            var isDescending = parts.Length > 1 && parts[1].ToLower() == "desc";

            Logger.LogInformation("Applying sorting: Property={PropertyName}, Descending={IsDescending}", propertyName, isDescending);

            return propertyName switch
            {
                "reportcode" => isDescending ? query.OrderByDescending(a => a.ReportCode ?? "") : query.OrderBy(a => a.ReportCode ?? ""),
                "code" => isDescending ? query.OrderByDescending(a => a.Code ?? "") : query.OrderBy(a => a.Code ?? ""),
                "name" => isDescending ? query.OrderByDescending(a => a.Name ?? "") : query.OrderBy(a => a.Name ?? ""),
                "hazardcode" => isDescending ? query.OrderByDescending(a => a.HazardCode ?? "") : query.OrderBy(a => a.HazardCode ?? ""),
                "leadassessorid" => isDescending ? query.OrderByDescending(a => a.LeadAssessorId ?? "") : query.OrderBy(a => a.LeadAssessorId ?? ""),
                "status" => isDescending ? query.OrderByDescending(a => a.Status.ToString()) : query.OrderBy(a => a.Status.ToString()),
                "stage" => isDescending ? query.OrderByDescending(a => a.Stage.ToString()) : query.OrderBy(a => a.Stage.ToString()),
                "assessmenttype" => isDescending ? query.OrderByDescending(a => a.AssessmentType.ToString()) : query.OrderBy(a => a.AssessmentType.ToString()),
                "riskassessmentcategory" => isDescending ? query.OrderByDescending(a => a.RiskAssessmentCategory.ToString()) : query.OrderBy(a => a.RiskAssessmentCategory.ToString()),
                "currentstep" => isDescending ? query.OrderByDescending(a => a.CurrentStep) : query.OrderBy(a => a.CurrentStep),
                "completeddate" => isDescending ? query.OrderByDescending(a => a.CompletedDate) : query.OrderBy(a => a.CompletedDate),
                "createddate" => isDescending ? query.OrderByDescending(a => a.CreatedDate) : query.OrderBy(a => a.CreatedDate),
                "updateddate" => isDescending ? query.OrderByDescending(a => a.UpdatedDate) : query.OrderBy(a => a.UpdatedDate),
                _ => query.OrderByDescending(a => a.CreatedDate ?? DateTime.MinValue) // Default sort with null handling
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error applying sorting for OrderBy: {OrderBy}", orderBy);
            return query.OrderByDescending(a => a.CreatedDate ?? DateTime.MinValue); // Fallback to default sort
        }
    }
    #endregion

    #region Helper Methods - Keep existing functionality but improved
    private static Expression<Func<RiskAssessment, object>> GetPropertyExpression(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(RiskAssessment), "x");
        var property = Expression.Property(parameter, propertyName);
        var conversion = Expression.Convert(property, typeof(object));
        return Expression.Lambda<Func<RiskAssessment, object>>(conversion, parameter);
    }

    private BadgeStyle GetStatusBadgeStyle(string status)
    {
        return status switch
        {
            "Completed" => BadgeStyle.Success,
            "InProgress" => BadgeStyle.Info,
            "Approved" => BadgeStyle.Primary,
            "OnHold" => BadgeStyle.Warning,
            "Cancelled" => BadgeStyle.Danger,
            "Draft" => BadgeStyle.Secondary,
            _ => BadgeStyle.Secondary
        };
    }
    #endregion

    #region CRUD Action Methods - Enhanced with better error handling

    private async Task ViewAssessment(RiskAssessment assessment)
    {
        try
        {
            Logger.LogInformation("Viewing risk assessment: {Code}", assessment.Code);
            SelectedAssessment = assessment;
            ShowViewDialog = true;
            StateHasChanged();
            await NotificationHelper.ShowInfoAsync($"Viewing details for assessment {assessment.Code}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error viewing risk assessment {Code}", assessment.Code);
            await NotificationHelper.ShowErrorAsync("Error viewing risk assessment");
        }
    }

    private async Task EditAssessment(RiskAssessment assessment)
    {
        try
        {
            Logger.LogInformation("Editing risk assessment: {Code}", assessment.Code);
            await NavigateToTechnicalAssessment(assessment);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error editing risk assessment {Code}", assessment.Code);
            await NotificationHelper.ShowErrorAsync("Error opening risk assessment editor");
        }
    }
    
    private async Task NavigateToTechnicalAssessment(RiskAssessment assessment)
    {
        try
        {
            // ✅ PROPER WAY: Get ReportCode via HazardCode using CQRS
            var reportCode = await GetReportCodeFromAssessmentAsync(assessment);

            if (string.IsNullOrEmpty(reportCode))
            {
                Logger.LogWarning("Could not determine ReportCode for assessment {AssessmentCode}", assessment.Code);
                await NotificationHelper.ShowErrorAsync("Could not determine report code for this assessment");
                return;
            }

            var navigationUrl = $"/SMSRiskManagement/TechnicalAssessment/{reportCode}/{assessment.HazardCode}/{assessment.CurrentStep}";

            Logger.LogInformation("Navigating to Technical Assessment: {Url}", navigationUrl);
            Navigation.NavigateToSecure(navigationUrl);
            await NotificationHelper.ShowInfoAsync($"Opening technical assessment for {assessment.Code}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error navigating to Technical Assessment for {AssessmentCode}", assessment.Code);
            await NotificationHelper.ShowErrorAsync("Failed to navigate to Technical Assessment");
        }
    }
    
    private async Task<string?> GetReportCodeFromAssessmentAsync(RiskAssessment assessment)
    {
        try
        {
            // Use the HazardCode from the assessment to get the proper ReportCode
            if (string.IsNullOrEmpty(assessment.HazardCode))
            {
                Logger.LogWarning("Assessment {AssessmentCode} has no HazardCode", assessment.Code);
                return null;
            }

            Logger.LogInformation("Getting ReportCode via HazardCode {HazardCode} from assessment {AssessmentCode}",
                assessment.HazardCode, assessment.Code);

            var hazardQuery = new GetHazardByCodeQuery(new HazardID(assessment.HazardCode));
            var hazardResult = await Mediator.SendAsync(hazardQuery, CancellationToken.None);

            if (hazardResult.IsSuccess && hazardResult.Value != null)
            {
                var reportCode = hazardResult.Value.ReportCode;
                Logger.LogInformation("Found ReportCode {ReportCode} for HazardCode {HazardCode}",
                    reportCode, assessment.HazardCode);
                return reportCode;
            }
            else
            {
                Logger.LogWarning("Failed to load Hazard {HazardCode}: {Error}",
                    assessment.HazardCode, hazardResult.Error?.Message ?? "Unknown error");
                return null;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error getting ReportCode from assessment {AssessmentCode} via HazardCode {HazardCode}",
                assessment.Code, assessment.HazardCode);
            return null;
        }
    }

    private async Task DeleteAssessment(RiskAssessment assessment)
    {
        try
        {
            var confirmResult = await DialogService.Confirm(
                message: $"Are you sure you want to delete risk assessment '{assessment.Name}' ({assessment.Code})?\n\nThis action cannot be undone.",
                title: "Confirm Deletion",
                options: new ConfirmOptions
                {
                    OkButtonText = "Yes, Delete",
                    CancelButtonText = "Cancel",
                    Width = "400px"
                });

            if (confirmResult == true)
            {
                Logger.LogInformation("Deleting risk assessment: {Code}", assessment.Code);

                var deleteCommand = new DeleteRiskAssessmentCommand(new RiskAssessmentID(assessment.Id.Value));
                var result = await Mediator.SendAsync(deleteCommand, CancellationToken.None);

                if (result.IsSuccess)
                {
                    await NotificationHelper.ShowSuccessAsync($"Risk assessment '{assessment.Name}' deleted successfully");
                    Logger.LogInformation("Successfully deleted risk assessment: {Code}", assessment.Code);

                    // Refresh the data grid by reloading initial data
                    await LoadInitialData();
                    StateHasChanged();
                }
                else
                {
                    await NotificationHelper.ShowErrorAsync($"Failed to delete risk assessment: {result.Error?.Message}");
                    Logger.LogError("Failed to delete risk assessment {Code}: {Error}", assessment.Code, result.Error?.Message);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting risk assessment {Code}", assessment.Code);
            await NotificationHelper.ShowErrorAsync("Error deleting risk assessment");
        }
    }
    #endregion
}
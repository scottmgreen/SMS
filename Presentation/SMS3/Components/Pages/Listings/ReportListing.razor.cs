using System.Linq.Expressions;

using Radzen;

using SMS_Application.Messaging.Queries;

using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Domain.Errors;

using SMS_Shared.Configuration;

using SMS3.Components.Shared;
using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration.Extensions;

namespace SMS3.Components.Pages.Listings;

/// <summary>
/// Report Listing Component - Enhanced with full CRUD operations
/// Provides comprehensive data grid listing with view details, edit navigation, and delete functionality
/// </summary>
public partial class ReportListing : ComponentBase
{
    private string BasicTextStyle = "font-size:smaller;font-weight: 600";

    #region Dependencies
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<ReportListing> Logger { get; set; } = default!;
    
    [Inject] private INotificationHelper  NotificationHelper { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private ICurrentUserService CurrentUserService { get; set; } = default!;
    #endregion

    #region Properties
    private RadzenDataGrid<Report>? reportsGrid;
    private IEnumerable<Report> reports = new List<Report>();
    private List<Report> allReports = new List<Report>(); // Store all reports for client-side filtering
    private int totalCount;
    private bool isLoading = false;

    // Custom confirmation modal properties
    private bool showResetConfirmModal = false;
    private string resetConfirmationMessage = string.Empty;
    private Report? reportToReset = null;

    /// <summary>
    /// Show details modal flag
    /// </summary>
    public bool ShowDetailsModal { get; set; } = false;

    /// <summary>
    /// Currently selected report for details view
    /// </summary>
    public Report? SelectedReport { get; set; }

    /// <summary>
    /// Associated hazards for the selected report
    /// </summary>
    public List<Hazard> AssociatedHazards { get; set; } = new();

    /// <summary>
    /// Count of locations with map data
    /// </summary>
    public string LocationsWithMaps => AssociatedHazards
        .Count(h => h.HazardLocation?.Latitude.HasValue == true && h.HazardLocation?.Longitude.HasValue == true)
        .ToString();

    /// <summary>
    /// Total count of files across all hazards
    /// </summary>
    public int TotalFilesCount => AssociatedHazards
        .Sum(h => h.HazardFileIds?.Count ?? 0);
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

            Logger.LogInformation("Loading reports for listing view");

            var query = new GetAllReportsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                allReports = result.Value; // Store all reports for filtering/sorting
                reports = allReports; // Initially show all reports
                totalCount = allReports.Count();
                Logger.LogInformation("Loaded {Count} reports for listing", totalCount);

                await NotificationHelper.ShowSuccessAsync($"Successfully loaded {totalCount} reports");
               
            }
            else
            {
                await NotificationHelper.ShowErrorAsync("Failed to load reports");
                Logger.LogError("Failed to load reports: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading reports");
            await NotificationHelper.ShowErrorAsync("Error loading reports");
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

            // If we don't have all reports yet, load them first
            if (allReports == null || !allReports.Any())
            {
                await LoadInitialData();
                return;
            }

            // Start with all reports
            var query = allReports.AsQueryable();

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
                query = query.OrderByDescending(r => r.CreatedDate);
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

            reports = query.ToList();

            Logger.LogInformation("Applied filtering/sorting/paging. Showing {Count} of {Total} reports", 
                reports.Count(), totalCount);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in LoadData");
            await NotificationHelper.ShowErrorAsync("Error loading data");
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
    private IQueryable<Report> ApplyFiltering(IQueryable<Report> query, LoadDataArgs args)
    {
        try
        {
            // Handle simple string filter (when user types in the general filter)
            if (!string.IsNullOrEmpty(args.Filter) && !args.Filter.Contains("("))
            {
                var filterValue = args.Filter.ToLower();
                query = query.Where(r => 
                    (!string.IsNullOrEmpty(r.Code) && r.Code.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(r.Name) && r.Name.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(r.Description) && r.Description.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(r.Status) && r.Status.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(r.SubmittedBy) && r.SubmittedBy.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(r.SubmittingDepartment) && r.SubmittingDepartment.ToLower().Contains(filterValue))
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
                        case "code":
                            query = ApplyStringFilter(query, r => r.Code, filterValue, filterOperator);
                            break;
                        case "name":
                            query = ApplyStringFilter(query, r => r.Name, filterValue, filterOperator);
                            break;
                        case "description":
                            query = ApplyStringFilter(query, r => r.Description, filterValue, filterOperator);
                            break;
                        case "status":
                            query = ApplyStringFilter(query, r => r.Status, filterValue, filterOperator);
                            break;
                        case "submittedby":
                            query = ApplyStringFilter(query, r => r.SubmittedBy, filterValue, filterOperator);
                            break;
                        case "submittingdepartment":
                            query = ApplyStringFilter(query, r => r.SubmittingDepartment, filterValue, filterOperator);
                            break;
                        case "submitteddate":
                            if (DateTime.TryParse(filter.FilterValue?.ToString(), out var dateValue))
                            {
                                query = ApplyDateFilter(query, r => r.SubmittedDate, dateValue, filterOperator);
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
    private IQueryable<Report> ApplyStringFilter(IQueryable<Report> query, Expression<Func<Report, string?>> propertySelector, string filterValue, FilterOperator filterOperator)
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
    /// Apply date-based filtering with different operators
    /// </summary>
    private IQueryable<Report> ApplyDateFilter(IQueryable<Report> query, Expression<Func<Report, DateTime?>> propertySelector, DateTime filterValue, FilterOperator filterOperator)
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
    private Expression<Func<Report, bool>> CombineExpressions<T>(Expression<Func<Report, T>> propertySelector, Expression<Func<T, bool>> condition)
    {
        var parameter = propertySelector.Parameters[0];
        var property = propertySelector.Body;
        var conditionBody = condition.Body;
        var conditionParameter = condition.Parameters[0];

        // Replace the condition parameter with the property expression
        var visitor = new ParameterReplacementVisitor(conditionParameter, property);
        var newConditionBody = visitor.Visit(conditionBody);

        return Expression.Lambda<Func<Report, bool>>(newConditionBody, parameter);
    }

    /// <summary>
    /// Apply sorting based on OrderBy parameter from Radzen DataGrid
    /// </summary>
    private IQueryable<Report> ApplySorting(IQueryable<Report> query, string orderBy)
    {
        try
        {
            if (string.IsNullOrEmpty(orderBy)) return query;

            var parts = orderBy.Split(' ');
            var propertyName = parts[0].ToLower();
            var isDescending = parts.Length > 1 && parts[1].ToLower() == "desc";

            return propertyName switch
            {
                "code" => isDescending ? query.OrderByDescending(r => r.Code) : query.OrderBy(r => r.Code),
                "name" => isDescending ? query.OrderByDescending(r => r.Name) : query.OrderBy(r => r.Name),
                "description" => isDescending ? query.OrderByDescending(r => r.Description) : query.OrderBy(r => r.Description),
                "status" => isDescending ? query.OrderByDescending(r => r.Status) : query.OrderBy(r => r.Status),
                "submittedby" => isDescending ? query.OrderByDescending(r => r.SubmittedBy) : query.OrderBy(r => r.SubmittedBy),
                "submitteddate" => isDescending ? query.OrderByDescending(r => r.SubmittedDate) : query.OrderBy(r => r.SubmittedDate),
                "submittingdepartment" => isDescending ? query.OrderByDescending(r => r.SubmittingDepartment) : query.OrderBy(r => r.SubmittingDepartment),
                "createddate" => isDescending ? query.OrderByDescending(r => r.CreatedDate) : query.OrderBy(r => r.CreatedDate),
                _ => query.OrderByDescending(r => r.CreatedDate) // Default sort
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error applying sorting for OrderBy: {OrderBy}", orderBy);
            return query.OrderByDescending(r => r.CreatedDate); // Fallback to default sort
        }
    }
    #endregion

    #region CRUD Action Methods

    /// <summary>
    /// Handle view report details - Show comprehensive read-only modal
    /// /// </summary>
    /// <param name="report">Report to view</param>
    public async Task OnViewReportAsync(Report report)
    {
        Logger.LogInformation("View report details requested: {ReportCode}", report.Code);

        try
        {
            isLoading = true;
            StateHasChanged();

            // Get detailed report information
            var reportQuery = new GetReportByCodeQuery(new ReportID(report.Code));
            var reportResult = await Mediator.SendAsync(reportQuery, CancellationToken.None);

            if (reportResult.IsSuccess && reportResult.Value != null)
            {
                SelectedReport = reportResult.Value;
            }
            else
            {
                SelectedReport = report; // Fallback to grid data
            }

            // Load associated hazards for this report
            await LoadAssociatedHazardsAsync(report.Code);

            // Show the details modal
            ShowDetailsModal = true;

            Logger.LogInformation("Displaying details for report: {ReportCode} with {HazardCount} hazards",
                report.Code, AssociatedHazards.Count);
            await NotificationHelper.ShowInfoAsync($"Displaying comprehensive details for {report.Code}", 4000);
            
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading report details for {ReportCode}", report.Code);

            await NotificationHelper.ShowErrorAsync("Failed to load report details");
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Handle edit report request - Navigate to HazardReporting page in edit mode
    /// </summary>
    /// <param name="report">Report to edit</param>
    public async Task OnEditReportAsync(Report report)
    {
        Logger.LogInformation("Edit report requested: {ReportCode}", report.Code);

        try
        {
            var confirmed = await DialogService.Confirm(
                $"Edit report '{report.Code} - {report.Name}'?\n\nThis will navigate to the hazard reporting form in edit mode.",
                "Edit Report",
                new ConfirmOptions()
                {
                    OkButtonText = "Yes, Edit Report",
                    CancelButtonText = "Cancel"
                });

            if (confirmed == true)
            {
                Navigation.NavigateToSecure($"/SMSRiskManagement/HazardReporting?mode=edit&reportCode={report.Code}");

                Logger.LogInformation("Navigating to edit report: {ReportCode}", report.Code);
                await NotificationHelper.ShowInfoAsync($"Opening {report.Code} for editing...", 4000);
                
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error navigating to edit report {ReportCode}", report.Code);
            await NotificationHelper.ShowErrorAsync("Failed to navigate to edit form");
        }
    }

    /// <summary>
    /// Handle delete report request - Show confirmation and delete via CQRS
    /// </summary>
    /// <param name="report">Report to delete</param>
    public async Task OnDeleteReportAsync(Report report)
    {
        Logger.LogInformation("Delete report requested: {ReportCode}", report.Code);

        try
        {
            // Load associated hazards to show in confirmation
            await LoadAssociatedHazardsAsync(report.Code);

            var hazardCount = AssociatedHazards.Count;

            var confirmationMessage = $"Are you sure you want to delete report '{report.Code}'?\n\n" +
                                    $"Report Details:\n" +
                                    $"• Name: {report.Name ?? "Unnamed Report"}\n" +
                                    $"• Status: {report.Status ?? "Unknown"}\n" +
                                    $"• Associated Hazards: {hazardCount}\n\n" +
                                    (hazardCount > 0 ? "??  WARNING: This report has associated hazards that may also be affected.\n\n" : "") +
                                    "?? This action cannot be undone!";

            var confirmed = await DialogService.Confirm(
                confirmationMessage,
                "Confirm Delete Report",
                new ConfirmOptions()
                {
                    OkButtonText = "Yes, Delete Report",
                    CancelButtonText = "Cancel",
                    AutoFocusFirstElement = false
                });

            if (confirmed == true)
            {
                var deleteCommand = new DeleteReportCommand(new ReportID(report.Code));
                var result = await Mediator.SendAsync(deleteCommand, CancellationToken.None);

                if (result.IsSuccess && result.Value)
                {
                    Logger.LogInformation("Successfully deleted report: {ReportCode}", report.Code);

                    // Reload the grid data
                    await LoadInitialData();
                    await NotificationHelper.ShowSuccessAsync($"Report {report.Code} has been successfully deleted.");
                    
                }
                else
                {
                    throw new InvalidOperationException(result.Error?.Message ?? "Failed to delete report");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting report: {ReportCode}", report.Code);
            await NotificationHelper.ShowErrorAsync("Failed to delete the report");
        }
    }
    #endregion

    #region Modal Management Methods

    /// <summary>
    /// Close the details modal
    /// </summary>
    public void CloseDetailsModal()
    {
        ShowDetailsModal = false;
        SelectedReport = null;
        AssociatedHazards.Clear();
        StateHasChanged();
    }

    /// <summary>
    /// Edit report from details modal
    /// </summary>
    /// <param name="report">Report to edit</param>
    public async Task EditFromDetailsModal(Report report)
    {
        CloseDetailsModal();
        await OnEditReportAsync(report);
    }
    #endregion

    #region Helper Methods

    /// <summary>
    /// Load hazards associated with a specific report
    /// </summary>
    /// <param name="reportCode">Report code to load hazards for</param>
    private async Task LoadAssociatedHazardsAsync(string reportCode)
    {
        try
        {
            Logger.LogInformation("Loading hazards for report: {ReportCode}", reportCode);

            var hazardsQuery = new GetHazardsByReportCodeQuery(new ReportID(reportCode));
            var hazardsResult = await Mediator.SendAsync(hazardsQuery, CancellationToken.None);

            if (hazardsResult.IsSuccess && hazardsResult.Value != null)
            {
                AssociatedHazards = hazardsResult.Value.ToList();
                Logger.LogInformation("Loaded {Count} hazards for report {ReportCode}",
                    AssociatedHazards.Count, reportCode);
            }
            else
            {
                AssociatedHazards = new List<Hazard>();
                Logger.LogWarning("No hazards found for report {ReportCode}: {Error}",
                    reportCode, hazardsResult.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading hazards for report {ReportCode}", reportCode);
            AssociatedHazards = new List<Hazard>();
        }
    }

    /// <summary>
    /// Get Radzen badge style for status
    /// </summary>
    public BadgeStyle GetStatusBadgeStyle(string? status)
    {
        return status?.ToLower() switch
        {
            "active" => BadgeStyle.Success,
            "pending" => BadgeStyle.Warning,
            "closed" => BadgeStyle.Secondary,
            "cancelled" => BadgeStyle.Danger,
            _ => BadgeStyle.Info
        };
    }

    /// <summary>
    /// Get Radzen badge style for stage
    /// </summary>
    public BadgeStyle GetStageBadgeStyle(string? stage)
    {
        return stage?.ToLower() switch
        {
            "validation" => BadgeStyle.Primary,
            "assessment" => BadgeStyle.Info,
            "mitigation" => BadgeStyle.Warning,
            "closure" => BadgeStyle.Success,
            _ => BadgeStyle.Secondary
        };
    }

    private static Expression<Func<Report, object>> GetPropertyExpression(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(Report), "x");
        var property = Expression.Property(parameter, propertyName);
        var conversion = Expression.Convert(property, typeof(object));
        return Expression.Lambda<Func<Report, object>>(conversion, parameter);
    }

    
    /// <summary>
    /// Placeholder for future action implementation (kept for backward compatibility)
    /// </summary>
    private void ShowActions(Report report)
    {
        Logger.LogInformation("Actions requested for report: {Code}", report.Code);
    }

    private async Task<Result<bool>> ResetReportValidation(string reportCode)
    {
        try
        {
            Logger.LogInformation("Resetting ReportValidation for ReportCode: {ReportCode}", reportCode);
            var reportId = new ReportID(reportCode);

            var queryHazard = new GetHazardsByReportCodeQuery(new ReportID(reportCode));
            var hazardResult = await Mediator.SendAsync(queryHazard, CancellationToken.None);

            if (hazardResult != null)
            {
                var hazards = hazardResult.Value;
                foreach (Hazard hazard in hazards)
                {
                    hazard.HazardRiskLevel = RiskLevel.Unkonwn;
                    hazard.InitialAverageScore = 0;
                    hazard.ResidualAverageScore = 0;
                    hazard.ResidualRiskMatrixCode = "TBD";
                    hazard.InitialRiskMatrixCode = "TBD";
                    var cmdHazardReset = new ResetHazardScoresCommand(hazard);
                    var hazardResetResult = await Mediator.SendAsync(cmdHazardReset, CancellationToken.None);


                }
            }


            var validationQuery = new GetReportValidationByReportIdQuery(reportId);
            var validationResult = await Mediator.SendAsync(validationQuery, CancellationToken.None);
            if (validationResult.IsSuccess && validationResult.Value != null)
            {
                var validation = validationResult.Value;
                var cmd = new ResetReportValidationCommand(new ReportValidationID(validation.Code));
                var cmdReset = await Mediator.SendAsync(cmd, CancellationToken.None);

                if (cmdReset.IsSuccess)
                {
                    var flowControl = await UpdateReportStatus(reportCode, ReportStatus.NeedsValidation);
                    if (!flowControl)
                    {
                        throw new Exception($"Failed to Update Report Status during Create new Risk Assessment: {DomainErrors.ReportValidationError.CreateFailed.Message}");
                        
                    }

                    Logger.LogInformation("Successfully reset ReportValidation {ValidationCode} for ReportCode: {ReportCode}", validation.Code, reportCode);
                    return true;    
                }
                else
                {
                    throw new InvalidOperationException($"Failed to reset ReportValidation: {cmdReset.Error?.Message}");
                    
                }
            }
            else
            {
                Logger.LogWarning("No ReportValidation found for ReportCode: {ReportCode}. Creating new validation...", reportCode);
                // If no existing validation found, create a new one
                var createresult =await CreateNewReportValidation(reportCode);
                return createresult;
            }

        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error resetting ReportValidation for ReportCode: {ReportCode}", reportCode);
            throw; // Re-throw to be handled by the calling method
        }


        
    }

    private async Task <Result<bool>>CreateNewReportValidation(string reportCode)
    {
        try
        {
            Logger.LogInformation("Creating new ReportValidation for ReportCode: {ReportCode}", reportCode);

            // Get the report details first
            var reportQuery = new GetReportByCodeQuery(new ReportID(reportCode));
            var reportResult = await Mediator.SendAsync(reportQuery, CancellationToken.None);

            if (reportResult.IsSuccess && reportResult.Value != null)
            {
                var report = reportResult.Value;

                // Create new ReportValidation using the static factory method
                var validation = SMS_Domain.Entities.ReportValidation.Create(reportCode, CurrentUserService?.UserDisplayName);
                validation.ValidationComments = $"Created from Investigation return to validation workflow on {DateTime.UtcNow:yyyy-MM-dd HH:mm}";

                var createCommand = new CreateReportValidationCommand(validation);
                var createResult = await Mediator.SendAsync(createCommand, CancellationToken.None);

                if (createResult.IsSuccess)
                {

                    bool flowControl = await UpdateReportStatus(reportCode, ReportStatus.NeedsValidation);
                    if (!flowControl)
                    {
                        throw new Exception($"Failed to Update Report Status during Create new Risk Assessment: {DomainErrors.ReportValidationError.CreateFailed.Message}");
                    }




                    Logger.LogInformation("Successfully created new ReportValidation {ValidationCode} for ReportCode: {ReportCode}",
                        createResult.Value.Code, reportCode);
                }
                else
                {
                    Logger.LogError("Failed to create new ReportValidation for ReportCode: {ReportCode}, Error: {Error}",
                        reportCode, createResult.Error?.Message);
                    throw new InvalidOperationException($"Failed to create new ReportValidation: {createResult.Error?.Message}");
                }
                return createResult.IsSuccess;
            }
            else
            {
                throw new InvalidOperationException($"Report {reportCode} not found, cannot create validation");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating new ReportValidation for ReportCode: {ReportCode}", reportCode);
            throw;
        }
    }
    
    private async Task<bool> UpdateReportStatus(string reportcode, ReportStatus status)
    {
        var updatestatuscmd = new UpdateReportStatusCommand(reportcode, status, CurrentUserService?.UserDisplayName);
        var getupdateResult = await Mediator.SendAsync(updatestatuscmd, CancellationToken.None);
        if (!getupdateResult.IsSuccess)
        {
            await NotificationHelper.ShowErrorAsync($"Report{reportcode} Status Was not Updated");
            return false;
        }
        return true;
    }




#endregion

    public async Task OnResetReportAsync(Report report)
    {
        if (report == null)
        {
            Logger.LogWarning("OnResetReportAsync called with null report");
            await NotificationHelper.ShowErrorAsync("Invalid report selected");
            return;
        }

        Logger.LogInformation("Reset Report requested: {ReportCode}", report.Code);

        try
        {
            isLoading = true;
            StateHasChanged();

            // Load associated hazards to show impact
            await LoadAssociatedHazardsAsync(report.Code);
            var hazardCount = AssociatedHazards?.Count ?? 0;

            // Build detailed confirmation message and show custom modal
            resetConfirmationMessage = BuildResetConfirmationMessage(report, hazardCount);
            reportToReset = report;
            showResetConfirmModal = true;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error preparing reset confirmation for report {ReportCode}", report.Code);
            await NotificationHelper.ShowErrorAsync($"Error preparing reset confirmation: {ex.Message}");
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Handle the actual reset confirmation from custom modal
    /// </summary>
    private async Task HandleResetConfirmation()
    {
        if (reportToReset == null) return;

        try
        {
            showResetConfirmModal = false;
            isLoading = true;
            StateHasChanged();

            Logger.LogInformation("User confirmed reset for report {ReportCode}", reportToReset.Code);

            // Perform the reset operation
            var result = await ResetReportValidation(reportToReset.Code);

            if (result.IsSuccess && result.Value)
            {
                Logger.LogInformation("Successfully reset report validation for {ReportCode}", reportToReset.Code);

                // Show success notification
                await NotificationHelper.ShowSuccessAsync($"Report '{reportToReset.Code}' validation has been successfully reset");

                // Refresh the data grid to reflect changes
                await LoadInitialData();

                // Update UI state
                StateHasChanged();
            }
            else
            {
                var errorMessage = result.Error?.Message ?? "Unknown error occurred during reset";
                Logger.LogError("Failed to reset report validation for {ReportCode}: {Error}", reportToReset.Code, errorMessage);

                await NotificationHelper.ShowErrorAsync($"Failed to reset report validation: {errorMessage}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error during reset operation for report {ReportCode}", reportToReset?.Code);
            await NotificationHelper.ShowErrorAsync($"An unexpected error occurred while resetting the report: {ex.Message}");
        }
        finally
        {
            // Clean up
            reportToReset = null;
            resetConfirmationMessage = string.Empty;
            isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Cancel the reset operation
    /// </summary>
    private void CancelResetConfirmation()
    {
        showResetConfirmModal = false;
        reportToReset = null;
        resetConfirmationMessage = string.Empty;
        StateHasChanged();
        
        Logger.LogInformation("User cancelled reset operation for report {ReportCode}", reportToReset?.Code);
    }

    /// <summary>
    /// Builds a detailed confirmation message for report reset
    /// </summary>
    private string BuildResetConfirmationMessage(Report report, int hazardCount)
    {
        var message = $"Are you sure you want to reset the validation for report '{report.Code}'?\n\n" +
                       $"Report Details:\n" +
                       $"Code: {report.Code}\n" +
                       $"Name: {report.Name ?? "Unnamed Report"}\n" +
                       $"Status: {report.Status ?? "Unknown"}\n" +
                       $"Created: {report.CreatedDate:yyyy-MM-dd}\n" +
                       $"Associated Hazards: {hazardCount}\n\n";

        if (hazardCount > 0)
        {
            message += "WARNING: This report has associated hazards that may also be affected by this reset.\n\n";
        }

        message += "This action will:\n" +
                   "✓ Reset the report validation status\n" +
                   "✓ Clear any validation history\n" +
                   "✓ Potentially affect associated hazards\n" +
                   "✓ Require re-validation of the report";

        return message;
    }

    /// <summary>
    /// Gets the final warning message for display
    /// </summary>
    private string GetResetFinalWarning()
    {
        return "THIS ACTION CANNOT BE UNDONE!";
    }
}

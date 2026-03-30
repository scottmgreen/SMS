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
/// Mitigation Listing Component - Enhanced with full CRUD operations and advanced filtering
/// Provides comprehensive data grid listing with view details, edit navigation, and delete functionality
/// </summary>
public partial class MitigationListing : ComponentBase
{
    private string BasicTextStyle = "font-size:smaller;font-weight: 600";

    #region Parameters
    [Parameter] public string? ReportId { get; set; }
    [Parameter] public string? HazardCode { get; set; }
    [Parameter] public bool ShowBulkApprove { get; set; } = true;
    [Parameter] public string Title { get; set; } = "Mitigations";
    #endregion

    #region Dependencies
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<MitigationListing> Logger { get; set; } = default!;
    
    [Inject] private INotificationHelper  NotificationHelper { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ICurrentUserService CurrentUserService { get; set; } = default!;
    #endregion

    #region Properties
    private RadzenDataGrid<MitigationModel>? mitigationsGrid;
    private IEnumerable<Mitigation> mitigations = new List<Mitigation>();
    private List<Mitigation> allMitigations = new List<Mitigation>(); // Store all mitigations for client-side filtering
    private IEnumerable<MitigationModel> mitigationModels = new List<MitigationModel>();
    private List<MitigationModel> allMitigationModels = new List<MitigationModel>(); // Store all models for client-side filtering
    private IEnumerable<Mitigation> selectedMitigations = new List<Mitigation>();
    private int totalCount;
    private bool isLoading = false;
    private bool ShowViewDialog = false;
    private bool ShowBulkApprovalDialog = false;
    private bool IsProcessingBulkApproval = false;
    private Mitigation? SelectedMitigation = null;

    // For context display
    private Hazard? ContextHazard = null;
    private Report? ContextReport = null;
    #endregion
    
    #region Lifecycle Methods
    protected override async Task OnInitializedAsync()
    {
        await LoadContextData();
        await LoadInitialData();
    }

    protected override async Task OnParametersSetAsync()
    {
        // Reload data when parameters change
        await LoadContextData();
        await LoadInitialData();
    }
    #endregion

    #region Context Loading Methods
    private async Task LoadContextData()
    {
        try
        {
            // Load hazard context if provided
            if (!string.IsNullOrEmpty(HazardCode))
            {
                var hazardQuery = new GetHazardByCodeQuery(new HazardID(HazardCode));
                var hazardResult = await Mediator.SendAsync(hazardQuery, CancellationToken.None);

                if (hazardResult.IsSuccess && hazardResult.Value != null)
                {
                    ContextHazard = hazardResult.Value;
                    Logger.LogInformation("Loaded context hazard: {HazardCode}", HazardCode);
                }
            }

            // Load report context if provided
            if (!string.IsNullOrEmpty(ReportId))
            {
                var reportQuery = new GetReportByCodeQuery(new ReportID(ReportId));
                var reportResult = await Mediator.SendAsync(reportQuery, CancellationToken.None);

                if (reportResult.IsSuccess && reportResult.Value != null)
                {
                    ContextReport = reportResult.Value;
                    Logger.LogInformation("Loaded context report: {ReportId}", ReportId);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading context data for Report: {ReportId}, Hazard: {HazardCode}", ReportId, HazardCode);
        }
    }
    #endregion

    #region Data Loading Methods
    private async Task LoadInitialData()
    {
        try
        {
            isLoading = true;
            StateHasChanged();

            Logger.LogInformation("Loading mitigations for listing view");

            if (!string.IsNullOrEmpty(HazardCode))
            {
                // Load mitigations for specific hazard
                var query = new GetMitigationsByHazardCodeQuery(HazardCode);
                var result = await Mediator.SendAsync(query, CancellationToken.None);

                if (result.IsSuccess && result.Value != null)
                {
                    var hazardMitigations = result.Value.ToList();

                    // Further filter by report if provided
                    if (!string.IsNullOrEmpty(ReportId) && ContextHazard?.ReportCode != null)
                    {
                        hazardMitigations = hazardMitigations
                            .Where(m => ContextHazard.ReportCode.Equals(ReportId, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                    }

                    allMitigations = hazardMitigations;
                    Logger.LogInformation("Loaded {Count} mitigations for hazard {HazardCode}", allMitigations.Count, HazardCode);
                }
                else
                {
                    allMitigations = new List<Mitigation>();
                    Logger.LogInformation("No mitigations found for hazard {HazardCode}", HazardCode);
                }
            }
            else
            {
                // Load all mitigations
                var query = new GetAllMitigationsQuery();
                var result = await Mediator.SendAsync(query, CancellationToken.None);

                if (result.IsSuccess && result.Value != null)
                {
                    allMitigations = result.Value.ToList();
                    Logger.LogInformation("Loaded {Count} total mitigations", allMitigations.Count);
                }
                else
                {
                    allMitigations = new List<Mitigation>();
                    await NotificationHelper.ShowErrorAsync("Failed to load mitigations");
                    Logger.LogError("Failed to load mitigations");
                }
            }

            // Create view models with Report ID and Hazard ID information
            await CreateMitigationViewModels();

            // Initially show all data
            mitigations = allMitigations;
            mitigationModels = allMitigationModels;
            totalCount = allMitigationModels.Count();

            // Show success notification if we have data
            if (totalCount > 0)
            {
                await NotificationHelper.ShowSuccessAsync($"Successfully loaded {totalCount} mitigations");

                if (totalCount == 0)
                {
                    await NotificationHelper.ShowInfoAsync("No mitigations found");
                }
            }
            else
            {
                await NotificationHelper.ShowErrorAsync("Failed to load mitigations");
                Logger.LogError("Error loading mitigations");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading mitigations");
            await NotificationHelper.ShowErrorAsync($"Error loading mitigations: {ex.Message}");
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

            // If we don't have all mitigation models yet, load them first
            if (allMitigationModels == null || !allMitigationModels.Any())
            {
                Logger.LogInformation("No mitigation models cached, loading initial data");
                await LoadInitialData();
                return;
            }

            // Start with all mitigation models
            var query = allMitigationModels.AsQueryable();
            Logger.LogInformation("Starting with {Count} total mitigation models", query.Count());

            // Apply filtering
            if (!string.IsNullOrEmpty(args.Filter))
            {
                Logger.LogInformation("Applying filter: {Filter}", args.Filter);
                query = ApplyFiltering(query, args);
                Logger.LogInformation("After filtering: {Count} mitigations", query.Count());
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
                query = query.OrderByDescending(m => m.Mitigation.CreatedDate ?? DateTime.MinValue);
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

            mitigationModels = query.ToList();
            mitigations = mitigationModels.Select(m => m.Mitigation).ToList();

            Logger.LogInformation("Applied filtering/sorting/paging. Showing {Count} of {Total} mitigations", 
                mitigationModels.Count(), totalCount);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error in LoadData with args: Skip={Skip}, Top={Top}, OrderBy={OrderBy}, Filter={Filter}", 
                args.Skip, args.Top, args.OrderBy, args.Filter);
            await NotificationHelper.ShowErrorAsync($"Error loading data: {ex.Message}");
            
            // Fallback to show all data without filtering/sorting
            try
            {
                mitigationModels = allMitigationModels ?? new List<MitigationModel>();
                mitigations = allMitigations ?? new List<Mitigation>();
                totalCount = mitigationModels.Count();
            }
            catch (Exception fallbackEx)
            {
                Logger.LogError(fallbackEx, "Error in LoadData fallback");
                mitigationModels = new List<MitigationModel>();
                mitigations = new List<Mitigation>();
                totalCount = 0;
            }
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }
    #endregion

    #region Filtering and Sorting Methods
    private IQueryable<MitigationModel> ApplyFiltering(IQueryable<MitigationModel> query, LoadDataArgs args)
    {
        try
        {
            // Handle simple string filter (when user types in the general filter)
            if (!string.IsNullOrEmpty(args.Filter) && !args.Filter.Contains("("))
            {
                var filterValue = args.Filter.ToLower();
                query = query.Where(m => 
                    (!string.IsNullOrEmpty(m.Code) && m.Code.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(m.Description) && m.Description.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(m.HazardCode) && m.HazardCode.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(m.ReportCode) && m.ReportCode.ToLower().Contains(filterValue))
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
                            query = ApplyStringFilter(query, m => m.ReportCode, filterValue, filterOperator);
                            break;
                        case "hazardcode":
                            query = ApplyStringFilter(query, m => m.HazardCode, filterValue, filterOperator);
                            break;
                        case "code":
                            query = ApplyStringFilter(query, m => m.Code, filterValue, filterOperator);
                            break;
                        case "description":
                            query = ApplyStringFilter(query, m => m.Description, filterValue, filterOperator);
                            break;
                        case "status":
                            query = ApplyEnumFilter(query, m => m.Status.ToString(), filterValue, filterOperator);
                            break;
                        case "progress":
                            if (int.TryParse(filter.FilterValue?.ToString(), out var progressValue))
                            {
                                query = ApplyNumericFilter(query, m => m.Progress, progressValue, filterOperator);
                            }
                            break;
                        case "targetdate":
                            if (DateTime.TryParse(filter.FilterValue?.ToString(), out var dateValue))
                            {
                                query = ApplyDateFilter(query, m => m.TargetDate, dateValue, filterOperator);
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

    private IQueryable<MitigationModel> ApplyStringFilter(IQueryable<MitigationModel> query, Expression<Func<MitigationModel, string?>> propertySelector, string filterValue, FilterOperator filterOperator)
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

    private IQueryable<MitigationModel> ApplyEnumFilter(IQueryable<MitigationModel> query, Expression<Func<MitigationModel, string?>> propertySelector, string filterValue, FilterOperator filterOperator)
    {
        return filterOperator switch
        {
            FilterOperator.Equals => query.Where(CombineExpressions(propertySelector, value => !string.IsNullOrEmpty(value) && value.ToLower() == filterValue)),
            FilterOperator.NotEquals => query.Where(CombineExpressions(propertySelector, value => string.IsNullOrEmpty(value) || value.ToLower() != filterValue)),
            FilterOperator.Contains => query.Where(CombineExpressions(propertySelector, value => !string.IsNullOrEmpty(value) && value.ToLower().Contains(filterValue))),
            _ => query.Where(CombineExpressions(propertySelector, value => !string.IsNullOrEmpty(value) && value.ToLower() == filterValue))
        };
    }

    private IQueryable<MitigationModel> ApplyNumericFilter(IQueryable<MitigationModel> query, Expression<Func<MitigationModel, int>> propertySelector, int filterValue, FilterOperator filterOperator)
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

    private IQueryable<MitigationModel> ApplyDateFilter(IQueryable<MitigationModel> query, Expression<Func<MitigationModel, DateTime?>> propertySelector, DateTime filterValue, FilterOperator filterOperator)
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

    private Expression<Func<MitigationModel, bool>> CombineExpressions<T>(Expression<Func<MitigationModel, T>> propertySelector, Expression<Func<T, bool>> condition)
    {
        var parameter = propertySelector.Parameters[0];
        var property = propertySelector.Body;
        var conditionBody = condition.Body;
        var conditionParameter = condition.Parameters[0];

        // Replace the condition parameter with the property expression
        var visitor = new ParameterReplacementVisitor(conditionParameter, property);
        var newConditionBody = visitor.Visit(conditionBody);

        return Expression.Lambda<Func<MitigationModel, bool>>(newConditionBody, parameter);
    }

    private IQueryable<MitigationModel> ApplySorting(IQueryable<MitigationModel> query, string orderBy)
    {
        try
        {
            if (string.IsNullOrEmpty(orderBy)) return query;

            var parts = orderBy.Split(' ');
            var propertyName = parts[0].ToLower();
            var isDescending = parts.Length > 1 && parts[1].ToLower() == "desc";

            return propertyName switch
            {
                "reportcode" => isDescending ? query.OrderByDescending(m => m.ReportCode ?? "") : query.OrderBy(m => m.ReportCode ?? ""),
                "hazardcode" => isDescending ? query.OrderByDescending(m => m.HazardCode ?? "") : query.OrderBy(m => m.HazardCode ?? ""),
                "code" => isDescending ? query.OrderByDescending(m => m.Code ?? "") : query.OrderBy(m => m.Code ?? ""),
                "description" => isDescending ? query.OrderByDescending(m => m.Description ?? "") : query.OrderBy(m => m.Description ?? ""),
                "status" => isDescending ? query.OrderByDescending(m => m.Status.ToString()) : query.OrderBy(m => m.Status.ToString()),
                "progress" => isDescending ? query.OrderByDescending(m => m.Progress) : query.OrderBy(m => m.Progress),
                "targetdate" => isDescending ? query.OrderByDescending(m => m.TargetDate) : query.OrderBy(m => m.TargetDate),
                _ => query.OrderByDescending(m => m.Mitigation.CreatedDate ?? DateTime.MinValue) // Default sort
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error applying sorting for OrderBy: {OrderBy}", orderBy);
            return query.OrderByDescending(m => m.Mitigation.CreatedDate ?? DateTime.MinValue); // Fallback to default sort
        }
    }
    #endregion

    #region View Model Creation
    private async Task CreateMitigationViewModels()
    {
        try
        {
            var viewModels = new List<MitigationModel>();

            // Group mitigations by hazard code for efficient loading
            var hazardCodes = allMitigations.Select(m => m.HazardCode).Distinct().ToList();
            var hazardLookup = new Dictionary<string, Hazard>();

            // Load all required hazards in parallel
            var hazardTasks = hazardCodes.Where(hc => !string.IsNullOrEmpty(hc))
                .Select(async hazardCode =>
                {
                    try
                    {
                        var hazardQuery = new GetHazardByCodeQuery(new HazardID(hazardCode));
                        var hazardResult = await Mediator.SendAsync(hazardQuery, CancellationToken.None);

                        if (hazardResult.IsSuccess && hazardResult.Value != null)
                        {
                            return new { HazardCode = hazardCode, Hazard = hazardResult.Value };
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(ex, "Failed to load hazard {HazardCode}", hazardCode);
                    }
                    return null;
                }).ToArray();

            var hazardResults = await Task.WhenAll(hazardTasks);

            // Build lookup dictionary
            foreach (var hazardResult in hazardResults.Where(hr => hr != null))
            {
                hazardLookup[hazardResult.HazardCode] = hazardResult.Hazard;
            }

            // Create view models
            foreach (var mitigation in allMitigations)
            {
                var hazardCode = mitigation.HazardCode ?? "Unknown";
                var reportId = "Unknown";

                // Try to get report ID from hazard
                if (!string.IsNullOrEmpty(mitigation.HazardCode) && 
                    hazardLookup.TryGetValue(mitigation.HazardCode, out var hazard))
                {
                    reportId = hazard.ReportCode ?? "Unknown";
                }
                else if (ContextHazard != null)
                {
                    reportId = ContextHazard.ReportCode ?? "Unknown";
                }
                else if (!string.IsNullOrEmpty(ReportId))
                {
                    reportId = ReportId;
                }

                viewModels.Add(new MitigationModel
                {
                    Mitigation = mitigation,
                    Code = mitigation.Code,
                    HazardCode = hazardCode,
                    ReportCode = reportId,
                    Description = mitigation.Description,
                    Status = mitigation.Status,
                    Progress = mitigation.Progress,
                    TargetDate = mitigation.TargetDate
                });
            }

            allMitigationModels = viewModels;
            Logger.LogInformation("Created {Count} mitigation view models", viewModels.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating mitigation view models");
            allMitigationModels = new List<MitigationModel>();
        }
    }
    #endregion

    #region Helper Methods
    private static Expression<Func<MitigationModel, object>> GetViewModelPropertyExpression(string propertyName)
    {
        var parameter = Expression.Parameter(typeof(MitigationModel), "x");
        var property = Expression.Property(parameter, propertyName);
        var conversion = Expression.Convert(property, typeof(object));
        return Expression.Lambda<Func<MitigationModel, object>>(conversion, parameter);
    }
    #endregion

    #region CRUD Action Methods
    private async Task ViewMitigation(Mitigation mitigation)
    {
        try
        {
            Logger.LogInformation("Viewing mitigation: {Code}", mitigation.Code);
            SelectedMitigation = mitigation;
            ShowViewDialog = true;
            StateHasChanged();
            await NotificationHelper.ShowInfoAsync($"Viewing details for mitigation {mitigation.Code}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error viewing mitigation {Code}", mitigation.Code);
            await NotificationHelper.ShowErrorAsync("Error viewing mitigation");
        }
    }

    private async Task EditMitigation(Mitigation mitigation)
    {
        try
        {
            Logger.LogInformation("Editing mitigation: {Code}", mitigation.Code);
            Navigation.NavigateToSecure($"/SMSRiskManagement/HazardMitigation/Edit/{mitigation.Code}");
            await NotificationHelper.ShowInfoAsync($"Opening mitigation editor for {mitigation.Code}");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error editing mitigation {Code}", mitigation.Code);
            await NotificationHelper.ShowErrorAsync("Error opening mitigation editor");
        }
    }

    private async Task QuickApproveMitigation(Mitigation mitigation)
    {
        try
        {
            mitigation.Status = MitigationStatus.Approved;
            mitigation.UpdatedDate = DateTime.UtcNow;
            mitigation.UpdatedBy = CurrentUserService.UserCode ?? "System";
            mitigation.ApprovedBy = CurrentUserService?.UserCode ?? "System";  // ? FIXED: Set ApprovedBy property

            var updateCommand = new UpdateMitigationCommand(mitigation);
            var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                await NotificationHelper.ShowSuccessAsync($"Mitigation {mitigation.Code} approved successfully");
                await LoadInitialData();
                StateHasChanged();
            }
            else
            {
                await NotificationHelper.ShowErrorAsync($"Failed to approve mitigation: {result.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error quick approving mitigation {Code}", mitigation.Code);
            await NotificationHelper.ShowErrorAsync("Error approving mitigation");
        }
    }

    private async Task OpenBulkApprovalDialog()
    {
        try
        {
            var approvableMitigations = allMitigations.Where(m => m.Status != MitigationStatus.Approved).ToList();

            if (!approvableMitigations.Any())
            {
                await NotificationHelper.ShowInfoAsync("All mitigations are already approved");
                return;
            }

            selectedMitigations = approvableMitigations;
            ShowBulkApprovalDialog = true;
            StateHasChanged();

            Logger.LogInformation("Opening bulk approval dialog for {Count} mitigations", approvableMitigations.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening bulk approval dialog");
            await NotificationHelper.ShowErrorAsync("Error opening bulk approval dialog");
        }
    }

    private async Task CloseBulkApprovalDialog()
    {
        ShowBulkApprovalDialog = false;
        selectedMitigations = new List<Mitigation>();
        StateHasChanged();
    }

    private async Task ProcessBulkApproval()
    {
        try
        {
            IsProcessingBulkApproval = true;
            StateHasChanged();

            var mitigationsToApprove = selectedMitigations.ToList();
            var successCount = 0;
            var errorCount = 0;

            Logger.LogInformation("Starting bulk approval for {Count} mitigations", mitigationsToApprove.Count);

            foreach (var mitigation in mitigationsToApprove)
            {
                try
                {
                    mitigation.Status = MitigationStatus.Approved;
                    mitigation.UpdatedDate = DateTime.UtcNow;
                    mitigation.UpdatedBy = CurrentUserService?.UserCode ?? "System";
                    mitigation.ApprovedBy = CurrentUserService?.UserCode ?? "System";  // ? FIXED: Set ApprovedBy property

                    var updateCommand = new UpdateMitigationCommand(mitigation);
                    var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

                    if (result.IsSuccess)
                    {
                        successCount++;
                        Logger.LogInformation("Approved mitigation: {Code}", mitigation.Code);
                    }
                    else
                    {
                        errorCount++;
                        Logger.LogError("Failed to approve mitigation {Code}: {Error}", mitigation.Code, result.Error?.Message);
                    }
                }
                catch (Exception ex)
                {
                    errorCount++;
                    Logger.LogError(ex, "Error approving mitigation {Code}", mitigation.Code);
                }
            }

            if (successCount > 0)
            {
                await NotificationHelper.ShowSuccessAsync($"Successfully approved {successCount} mitigation(s)");
            }

            if (errorCount > 0)
            {
                await NotificationHelper.ShowErrorAsync($"Failed to approve {errorCount} mitigation(s)");
            }

            await LoadInitialData();
            await CloseBulkApprovalDialog();

            Logger.LogInformation("Bulk approval completed: {SuccessCount} approved, {ErrorCount} failed",
                successCount, errorCount);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing bulk approval");
            await NotificationHelper.ShowErrorAsync("Error processing bulk approval");
        }
        finally
        {
            IsProcessingBulkApproval = false;
            StateHasChanged();
        }
    }

    private async Task ViewHistory(Mitigation mitigation)
    {
        try
        {
            Logger.LogInformation("Viewing mitigation history: {Code}", mitigation.Code);
            await NotificationHelper.ShowInfoAsync($"History functionality for mitigation {mitigation.Code} needs to be implemented");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error viewing mitigation history {Code}", mitigation.Code);
            await NotificationHelper.ShowErrorAsync("Error viewing mitigation history");
        }
    }

    private async Task DeleteMitigation(Mitigation mitigation)
    {
        try
        {
            var confirmResult = await DialogService.Confirm(
                message: $"Are you sure you want to delete mitigation '{mitigation.Name}' ({mitigation.Code})?\n\nThis action cannot be undone.",
                title: "Confirm Deletion",
                options: new ConfirmOptions
                {
                    OkButtonText = "Yes, Delete",
                    CancelButtonText = "Cancel",
                    Width = "400px"
                });

            if (confirmResult == true)
            {
                Logger.LogInformation("Deleting mitigation: {Code}", mitigation.Code);

                var deleteCommand = new DeleteMitigationCommand(new MitigationID(mitigation.Id.Value));
                var result = await Mediator.SendAsync(deleteCommand, CancellationToken.None);

                if (result.IsSuccess)
                {
                    await NotificationHelper.ShowSuccessAsync($"Mitigation '{mitigation.Name}' deleted successfully");
                    await LoadInitialData();
                    StateHasChanged();
                }
                else
                {
                    await NotificationHelper.ShowErrorAsync($"Failed to delete mitigation: {result.Error?.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting mitigation {Code}", mitigation.Code);
            await NotificationHelper.ShowErrorAsync("Error deleting mitigation");
        }
    }
    #endregion
}

/// <summary>
/// View model for mitigation listing that includes Report ID and Hazard ID
/// </summary>
public class MitigationModel
{
    public Mitigation Mitigation { get; set; } = default!;
    public string Code { get; set; } = string.Empty;
    public string HazardCode { get; set; } = string.Empty;
    public string ReportCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MitigationStatus Status { get; set; } 
    public int Progress { get; set; }
    public DateTime? TargetDate { get; set; }
}
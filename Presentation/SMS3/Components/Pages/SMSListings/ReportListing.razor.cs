using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.IO.Compression;

using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

using Radzen;
// NEW: EventBus Integration
using SMS_Application.Interfaces;
using SMS_Application.Queries;

using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Domain.Errors;
using SMS_Domain.Events;
using SMS_Domain.Interfaces;

using SMS_Shared.Configuration;

using SMS3.Components.Pages.SMSRiskManagement;
using SMS3.Components.Shared;
using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration.Extensions;

namespace SMS3.Components.Pages.SMSListings;

/// <summary>
/// Report Listing Component - Enhanced with full CRUD operations
/// Provides comprehensive data grid listing with view details, edit navigation, and delete functionality
/// </summary>
public partial class ReportListing : ComponentBase
{
    private string BasicTextStyle = "font-size:smaller;font-weight: 600";

    #region Dependencies
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<ReportListing> _logger { get; set; } = default!;

    [Inject] private DialogService _dialogService { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;

    [Inject] private ICurrentUserService _currentUserService { get; set; } = default!;
    [Inject] private IJSRuntime _jsRuntime { get; set; } = default!;

    // NEW: EventBus Integration
    [Inject] private IBaseEventBus _eventBus { get; set; } = default!;
    #endregion

    #region Properties
    private RadzenDataGrid<Report>? reportsGrid;
    private IEnumerable<Report> reports = new List<Report>();
    private IList<Report> selectedReports = new List<Report>();
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

    // NEW: EventBus Testing Properties
    private bool IsEventBusTestRunning { get; set; } = false;
    private string EventBusTestMessage { get; set; } = string.Empty;
    private string EventBusTestError { get; set; } = string.Empty;

    // Modal properties for hazard description
    private bool ShowDescriptionModal = false;
    private string SelectedDescription = string.Empty;
    private string SelectedReportId = string.Empty;

    private static readonly PropertyInfo[] ReportExportProperties = typeof(Report)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.GetMethod is not null)
        .ToArray();

    private static readonly PropertyInfo[] HazardExportProperties = typeof(Hazard)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.GetMethod is not null)
        .ToArray();

    private static readonly PropertyInfo[] HazardLocationExportProperties = typeof(HazardLocation)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.GetMethod is not null)
        .ToArray();

    private static readonly PropertyInfo[] RiskAssessmentExportProperties = typeof(RiskAssessment)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.GetMethod is not null)
        .ToArray();
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

            _logger.LogInformation("Loading reports for listing view");

            var query = new GetAllReportsQuery();
            var result = await _mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value is not null)
            {
                allReports = result.Value; // Store all reports for filtering/sorting
                reports = allReports; // Initially show all reports
                totalCount = allReports.Count();
                _logger.LogInformation("Loaded {Count} reports for listing", totalCount);

                await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", $"Successfully loaded {totalCount} reports"));
               
            }
            else
            {
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Failed to load reports"));
                _logger.LogError("Failed to load reports: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading reports");
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Error loading reports"));
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

            // If we don't have all reports yet, load them first
            if (allReports is null || !allReports.Any())
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

            if (selectedReports.Count > 0)
            {
                var selectedCodes = selectedReports.Select(r => r.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
                selectedReports = allReports
                    .Where(r => selectedCodes.Contains(r.Code))
                    .ToList();
            }

            _logger.LogInformation("Applied filtering/sorting/paging. Showing {Count} of {Total} reports", 
                reports.Count(), totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in LoadData");
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Error loading data"));
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
            _logger.LogError(ex, "Error applying filters");
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
            _logger.LogError(ex, "Error applying sorting for OrderBy: {OrderBy}", orderBy);
            return query.OrderByDescending(r => r.CreatedDate); // Fallback to default sort
        }
    }
    #endregion

    #region Selection + CSV Export

    private bool IsReportSelected(Report report)
    {
        return selectedReports.Any(r => r.Code == report.Code);
    }

    private void OnReportSelectionChanged(Report report, bool isSelected)
    {
        if (isSelected)
        {
            if (!selectedReports.Any(r => r.Code == report.Code))
            {
                selectedReports.Add(report);
            }
        }
        else
        {
            var existing = selectedReports.FirstOrDefault(r => r.Code == report.Code);
            if (existing is not null)
            {
                selectedReports.Remove(existing);
            }
        }
    }

    private bool IsAllVisibleReportsSelected()
    {
        var visibleReports = reports.ToList();
        if (!visibleReports.Any())
        {
            return false;
        }

        var selectedCodes = selectedReports.Select(r => r.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return visibleReports.All(r => selectedCodes.Contains(r.Code));
    }

    private void OnSelectAllVisibleReportsChanged(bool isSelected)
    {
        var visibleReports = reports.ToList();
        if (!visibleReports.Any())
        {
            return;
        }

        if (isSelected)
        {
            var selectedCodes = selectedReports.Select(r => r.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var report in visibleReports)
            {
                if (!selectedCodes.Contains(report.Code))
                {
                    selectedReports.Add(report);
                }
            }
        }
        else
        {
            var visibleCodes = visibleReports.Select(r => r.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
            selectedReports = selectedReports
                .Where(r => !visibleCodes.Contains(r.Code))
                .ToList();
        }
    }

    private async Task OnExportSelectedReportsAsync()
    {
        if (!selectedReports.Any())
        {
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Warning("Warning", "Please select at least one report to export"));
            return;
        }

        try
        {
            var zipBytes = await BuildExportPackageAsync(selectedReports);
            var base64 = Convert.ToBase64String(zipBytes);
            var fileName = $"reports-export-package-{DateTime.UtcNow:yyyyMMdd-HHmmss}.zip";

            await _jsRuntime.InvokeVoidAsync("downloadFile", fileName, "application/zip", base64);
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", $"Exported {selectedReports.Count} report package(s)"));
            _logger.LogInformation("Exported package for {Count} selected reports", selectedReports.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export selected reports package");
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Failed to export selected reports package"));
        }
    }

    private async Task<byte[]> BuildExportPackageAsync(IEnumerable<Report> reportsToExport)
    {
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var report in reportsToExport)
            {
                if (string.IsNullOrWhiteSpace(report.Code))
                {
                    continue;
                }

                await AddReportFolderToArchiveAsync(archive, report);
            }
        }

        return memoryStream.ToArray();
    }

    private async Task AddReportFolderToArchiveAsync(ZipArchive archive, Report report)
    {
        var folderName = $"Report_{SanitizeFileNamePart(report.Code)}";

        AddCsvEntry(
            archive,
            $"{folderName}/Report_{SanitizeFileNamePart(report.Code)}.csv",
            BuildCsv(new[] { report }, ReportExportProperties));

        var hazardsQuery = new GetHazardsByReportCodeQuery(new ReportID(report.Code));
        var hazardsResult = await _mediator.SendAsync(hazardsQuery, CancellationToken.None);
        var hazards = hazardsResult.IsSuccess && hazardsResult.Value is not null
            ? hazardsResult.Value.ToList()
            : new List<Hazard>();

        AddCsvEntry(
            archive,
            $"{folderName}/Hazards_{SanitizeFileNamePart(report.Code)}.csv",
            BuildCsv(hazards, HazardExportProperties));

        var hazardLocations = new List<HazardLocation>();
        var riskAssessments = new List<RiskAssessment>();

        foreach (var hazard in hazards)
        {
            if (!string.IsNullOrWhiteSpace(hazard.Code))
            {
                var locationQuery = new GetHazardLocationsByHazardCodeQuery(hazard.Code);
                var locationResult = await _mediator.SendAsync(locationQuery, CancellationToken.None);

                if (locationResult.IsSuccess && locationResult.Value is not null)
                {
                    hazardLocations.AddRange(locationResult.Value);
                }
            }

            if (!string.IsNullOrWhiteSpace(hazard.Code))
            {
                var riskAssessmentQuery = new GetRiskAssessmentsByHazardCodeQuery(new HazardID(hazard.Code));
                var riskAssessmentResult = await _mediator.SendAsync(riskAssessmentQuery, CancellationToken.None);

                if (riskAssessmentResult.IsSuccess && riskAssessmentResult.Value is not null)
                {
                    riskAssessments.AddRange(riskAssessmentResult.Value);
                }
            }
        }

        var distinctLocations = hazardLocations
            .GroupBy(location => location.Code)
            .Select(group => group.First())
            .ToList();

        var distinctRiskAssessments = riskAssessments
            .GroupBy(assessment => assessment.Code)
            .Select(group => group.First())
            .ToList();

        AddCsvEntry(
            archive,
            $"{folderName}/HazardLocations_{SanitizeFileNamePart(report.Code)}.csv",
            BuildCsv(distinctLocations, HazardLocationExportProperties));

        AddCsvEntry(
            archive,
            $"{folderName}/RiskAssessments_{SanitizeFileNamePart(report.Code)}.csv",
            BuildCsv(distinctRiskAssessments, RiskAssessmentExportProperties));
    }

    private static void AddCsvEntry(ZipArchive archive, string entryPath, string csvContent)
    {
        var entry = archive.CreateEntry(entryPath, CompressionLevel.Fastest);
        using var entryStream = entry.Open();
        using var writer = new StreamWriter(entryStream, Encoding.UTF8);
        writer.Write(csvContent);
    }

    private static string BuildCsv<T>(IEnumerable<T> records, PropertyInfo[] properties)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", properties.Select(p => EscapeCsv(p.Name))));

        foreach (var record in records)
        {
            var row = properties
                .Select(property => property.GetValue(record))
                .Select(FormatCsvValue);

            sb.AppendLine(string.Join(",", row));
        }

        return sb.ToString();
    }

    private static string FormatCsvValue(object? value)
    {
        return value switch
        {
            null => string.Empty,
            DateTime dateTime => EscapeCsv(dateTime.ToString("O")),
            DateTimeOffset dateTimeOffset => EscapeCsv(dateTimeOffset.ToString("O")),
            _ => EscapeCsv(value.ToString() ?? string.Empty)
        };
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains('"'))
        {
            value = value.Replace("\"", "\"\"");
        }

        if (value.Contains(',') || value.Contains('\n') || value.Contains('\r') || value.Contains('"'))
        {
            return $"\"{value}\"";
        }

        return value;
    }

    private static string SanitizeFileNamePart(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return new string(value.Select(c => invalidChars.Contains(c) ? '_' : c).ToArray());
    }

    #endregion

    #region CRUD Action Methods

    /// <summary>
    /// Handle view report details - Show comprehensive read-only modal
    /// /// </summary>
    /// <param name="report">Report to view</param>
    public async Task OnViewReportAsync(Report report)
    {
        _logger.LogInformation("View report details requested: {ReportCode}", report.Code);

        try
        {
            isLoading = true;
            StateHasChanged();

            // Get detailed report information
            var reportQuery = new GetReportByCodeQuery(new ReportID(report.Code));
            var reportResult = await _mediator.SendAsync(reportQuery, CancellationToken.None);

            if (reportResult.IsSuccess && reportResult.Value is not null)
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

            _logger.LogInformation("Displaying details for report: {ReportCode} with {HazardCount} hazards",
                report.Code, AssociatedHazards.Count);
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Info("Information", $"Displaying comprehensive details for {report.Code}"));
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading report details for {ReportCode}", report.Code);

            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Failed to load report details"));
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
        _logger.LogInformation("Edit report requested: {ReportCode}", report.Code);

        try
        {
            var confirmed = await _dialogService.Confirm(
                $"Edit report '{report.Code} - {report.Name}'?\n\nThis will navigate to the Initial hazard reporting form in edit mode.",
                "Edit Report",
                new ConfirmOptions()
                {
                    OkButtonText = "Yes, Edit Report",
                    CancelButtonText = "Cancel"
                });

            if (confirmed == true)
            {
                var queryHazard = new GetHazardsByReportCodeQuery(new ReportID(report.Code));
                var hazardResult = await _mediator.SendAsync(queryHazard, CancellationToken.None);

                var initialHazard = hazardResult?.Value?.FirstOrDefault(h => h.IsInitialHazard);

                if (initialHazard is not null && !string.IsNullOrEmpty(initialHazard.Code))
                {
                    _navigation.NavigateToSecure($"/SMSRiskManagement/HazardReporting/{initialHazard.Code}");
                    _logger.LogInformation("Navigating to edit report: {ReportCode}", report.Code);
                    await _eventBus.PublishUIEventAsync(UINotificationEvent.Info("Navigation", $"Opening {report.Code} for editing..."));
                }
                else
                {
                    _logger.LogWarning("No initial hazard found for report: {ReportCode}", report.Code);
                    await _eventBus.PublishUIEventAsync(UINotificationEvent.Warning("Navigation", $"No initial hazard found for {report.Code}."));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to edit report {ReportCode}", report.Code);
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Navigation Error", "Failed to navigate to edit form"));
        }
    }

    /// <summary>
    /// Handle delete report request - Show confirmation and delete via CQRS
    /// </summary>
    /// <param name="report">Report to delete</param>
    public async Task OnDeleteReportAsync(Report report)
    {
        _logger.LogInformation("Delete report requested: {ReportCode}", report.Code);

        try
        {
            // Load associated hazards to show in confirmation
            await LoadAssociatedHazardsAsync(report.Code);

            var hazardCount = AssociatedHazards.Count;

            //var confirmationMessage = $"Are you sure you want to delete report '{report.Code}'?\n\n" +
            //                        $"Report Details:\n\n" +
            //                        $"• Name: {report.Name ?? "Unnamed Report"}\n" +
            //                        $"• Status: {report.Status ?? "Unknown"}\n" +
            //                        $"• Associated Hazards: {hazardCount}\n\n" +
            //                        (hazardCount > 0 ? "WARNING: This report has associated hazards that may also be affected.\n\n" : "") +
            //                        "This action cannot be undone!";

            string htmlMessage = $"<div style=\"white-space: pre-line;\">" +
                     $"<p>Are you sure you want to delete report '<b>{report.Code}</b>'?</p>" +
                     $"<p><b>Report Details:</b></p>" +
                     $"<ul style=\"list-style-type: none; padding-left: 10px;\">" +
                     $"<li>• Name: {report.Name ?? "Unnamed Report"}</li>" +
                     $"<li>• Status: {report.Status ?? "Unknown"}</li>" +
                     $"<li>• Associated Hazards: {hazardCount}</li>" +
                     $"</ul>" +
                     (hazardCount > 0 ? $"<p style=\"color: #dc3545; font-weight: bold; margin-top: 15px;\">? WARNING: This report has associated hazards that may also be affected.</p>" : "") +
                     $"<p style=\"font-weight: bold; margin-top: 15px;\">? This action cannot be undone!</p>" +
                     $"</div>";
            RenderFragment messageFragment = builder => builder.AddContent(0, (MarkupString)htmlMessage);
            var confirmed = await _dialogService.Confirm(
                messageFragment,
                "Confirm Delete Report",
                new ConfirmOptions()
                {
                    Width = "600px",
                    OkButtonText = "Yes, Delete Report",
                    CancelButtonText = "Cancel",
                    AutoFocusFirstElement = false
                });

            if (confirmed == true)
            {
                var deleteCommand = new DeleteReportCommand(new ReportID(report.Code));
                var result = await _mediator.SendAsync(deleteCommand, CancellationToken.None);

                if (result.IsSuccess && result.Value)
                {
                    _logger.LogInformation("Successfully deleted report: {ReportCode}", report.Code);

                    // Reload the grid data
                    await LoadInitialData();
                    await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", $"Report {report.Code} has been successfully deleted."));
                    
                }
                else
                {
                    throw new InvalidOperationException(result.Error?.Message ?? "Failed to delete report");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting report: {ReportCode}", report.Code);
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Failed to delete the report"));
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
            _logger.LogInformation("Loading hazards for report: {ReportCode}", reportCode);

            var hazardsQuery = new GetHazardsByReportCodeQuery(new ReportID(reportCode));
            var hazardsResult = await _mediator.SendAsync(hazardsQuery, CancellationToken.None);

            if (hazardsResult.IsSuccess && hazardsResult.Value is not null)
            {
                AssociatedHazards = hazardsResult.Value.ToList();
                _logger.LogInformation("Loaded {Count} hazards for report {ReportCode}",
                    AssociatedHazards.Count, reportCode);

                // ?? ENHANCED: Load HazardLocation for each hazard to ensure map data is available
                foreach (var hazard in AssociatedHazards)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(hazard.Code))
                        {
                            var locationQuery = new GetHazardLocationsByHazardCodeQuery(hazard.Code);
                            var locationResult = await _mediator.SendAsync(locationQuery, CancellationToken.None);

                            if (locationResult.IsSuccess && locationResult.Value?.Any() == true)
                            {
                                hazard.HazardLocation = locationResult.Value.FirstOrDefault();
                                _logger.LogInformation("Loaded location for hazard {HazardCode}: Lat={Lat}, Lng={Lng}, Desc={Desc}",
                                    hazard.Code,
                                    hazard.HazardLocation?.Latitude,
                                    hazard.HazardLocation?.Longitude,
                                    hazard.HazardLocation?.Description);
                            }
                            else
                            {
                                _logger.LogInformation("No location found for hazard {HazardCode}", hazard.Code);
                            }
                        }
                    }
                    catch (Exception locationEx)
                    {
                        _logger.LogWarning(locationEx, "Failed to load location for hazard {HazardCode}", hazard.Code);
                    }
                }

                // Log summary of locations loaded
                var hazardsWithLocation = AssociatedHazards.Count(h => h.HazardLocation?.Latitude.HasValue == true && h.HazardLocation?.Longitude.HasValue == true);
                _logger.LogInformation("Location summary: {WithLocation}/{Total} hazards have valid coordinates", hazardsWithLocation, AssociatedHazards.Count);
            }
            else
            {
                AssociatedHazards = new List<Hazard>();
                _logger.LogWarning("No hazards found for report {ReportCode}: {Error}",
                    reportCode, hazardsResult.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading hazards for report {ReportCode}", reportCode);
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
        _logger.LogInformation("Actions requested for report: {Code}", report.Code);
    }

    private async Task<Result<bool>> ResetReportValidation(string reportCode)
    {
        try
        {
            _logger.LogInformation("Resetting ReportValidation for ReportCode: {ReportCode}", reportCode);
            var reportId = new ReportID(reportCode);

            var queryHazard = new GetHazardsByReportCodeQuery(new ReportID(reportCode));
            var hazardResult = await _mediator.SendAsync(queryHazard, CancellationToken.None);

            if (hazardResult is not null)
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
                    var hazardResetResult = await _mediator.SendAsync(cmdHazardReset, CancellationToken.None);


                }
            }


            var validationQuery = new GetReportValidationByReportIdQuery(reportId);
            var validationResult = await _mediator.SendAsync(validationQuery, CancellationToken.None);
            if (validationResult.IsSuccess && validationResult.Value is not null)
            {
                var validation = validationResult.Value;
                var cmd = new ResetReportValidationCommand(new ReportValidationID(validation.Code));
                var cmdReset = await _mediator.SendAsync(cmd, CancellationToken.None);

                if (cmdReset.IsSuccess)
                {
                    var flowControl = await UpdateReportStatus(reportCode, ReportStatus.NeedsValidation);
                    if (!flowControl)
                    {
                        throw new Exception($"Failed to Update Report Status during Create new Risk Assessment: {DomainErrors.ReportValidationError.CreateFailed.Message}");
                        
                    }

                    _logger.LogInformation("Successfully reset ReportValidation {ValidationCode} for ReportCode: {ReportCode}", validation.Code, reportCode);
                    return true;    
                }
                else
                {
                    throw new InvalidOperationException($"Failed to reset ReportValidation: {cmdReset.Error?.Message}");
                    
                }
            }
            else
            {
                _logger.LogWarning("No ReportValidation found for ReportCode: {ReportCode}. Creating new validation...", reportCode);
                // If no existing validation found, create a new one
                var createresult =await CreateNewReportValidation(reportCode);
                return createresult;
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting ReportValidation for ReportCode: {ReportCode}", reportCode);
            throw; // Re-throw to be handled by the calling method
        }


        
    }

    private async Task <Result<bool>>CreateNewReportValidation(string reportCode)
    {
        try
        {
            _logger.LogInformation("Creating new ReportValidation for ReportCode: {ReportCode}", reportCode);

            // Get the report details first
            var reportQuery = new GetReportByCodeQuery(new ReportID(reportCode));
            var reportResult = await _mediator.SendAsync(reportQuery, CancellationToken.None);

            if (reportResult.IsSuccess && reportResult.Value is not null)
            {
                var report = reportResult.Value;

                // Create new ReportValidation using the static factory method
                var validation = SMS_Domain.Entities.ReportValidation.Create(reportCode, _currentUserService?.UserDisplayName ?? "Unknown User");
                validation.ValidationComments = $"Created from Investigation return to validation workflow on {DateTime.UtcNow:yyyy-MM-dd HH:mm}";

                var createCommand = new CreateReportValidationCommand(validation);
                var createResult = await _mediator.SendAsync(createCommand, CancellationToken.None);

                if (createResult.IsSuccess)
                {

                    bool flowControl = await UpdateReportStatus(reportCode, ReportStatus.NeedsValidation);
                    if (!flowControl)
                    {
                        throw new Exception($"Failed to Update Report Status during Create new Risk Assessment: {DomainErrors.ReportValidationError.CreateFailed.Message}");
                    }




                    _logger.LogInformation("Successfully created new ReportValidation {ValidationCode} for ReportCode: {ReportCode}",
                        createResult.Value.Code, reportCode);
                }
                else
                {
                    _logger.LogError("Failed to create new ReportValidation for ReportCode: {ReportCode}, Error: {Error}",
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
            _logger.LogError(ex, "Error creating new ReportValidation for ReportCode: {ReportCode}", reportCode);
            throw;
        }
    }
    
    private async Task<bool> UpdateReportStatus(string reportcode, ReportStatus status)
    {
        var updatestatuscmd = new UpdateReportStatusCommand(reportcode, status, _currentUserService?.UserDisplayName ?? "Unknown User");
        var getupdateResult = await _mediator.SendAsync(updatestatuscmd, CancellationToken.None);
        if (!getupdateResult.IsSuccess)
        {
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", $"Report{reportcode} Status Was not Updated"));
            return false;
        }
        return true;
    }


public async Task OnResetReportAsync(Report report)
    {
        if (report is null)
        {
            _logger.LogWarning("OnResetReportAsync called with null report");
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Invalid report selected"));
            return;
        }

        _logger.LogInformation("Reset Report requested: {ReportCode}", report.Code);

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
            _logger.LogError(ex, "Error preparing reset confirmation for report {ReportCode}", report.Code);
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", $"Error preparing reset confirmation: {ex.Message}"));
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
        if (reportToReset is null) return;

        try
        {
            showResetConfirmModal = false;
            isLoading = true;
            StateHasChanged();

            _logger.LogInformation("User confirmed reset for report {ReportCode}", reportToReset.Code);

            // Perform the reset operation
            var result = await ResetReportValidation(reportToReset.Code);

            if (result.IsSuccess && result.Value)
            {
                _logger.LogInformation("Successfully reset report validation for {ReportCode}", reportToReset.Code);

                // Show success notification
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", $"Report '{reportToReset.Code}' validation has been successfully reset"));

                // Refresh the data grid to reflect changes
                await LoadInitialData();

                // Update UI state
                StateHasChanged();
            }
            else
            {
                var errorMessage = result.Error?.Message ?? "Unknown error occurred during reset";
                _logger.LogError("Failed to reset report validation for {ReportCode}: {Error}", reportToReset.Code, errorMessage);

                await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", $"Failed to reset report validation: {errorMessage}"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during reset operation for report {ReportCode}", reportToReset?.Code);
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", $"An unexpected error occurred while resetting the report: {ex.Message}"));
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
        
        _logger.LogInformation("User cancelled reset operation for report {ReportCode}", reportToReset?.Code);
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
                   "Reset the report validation status\n" +
                   "Clear any validation history\n" +
                   "Potentially affect associated hazards\n" +
                   "Require re-validation of the report";

        return message;
    }

    /// <summary>
    /// Gets the final warning message for display
    /// </summary>
    private string GetResetFinalWarning()
    {
        return "THIS ACTION CANNOT BE UNDONE!";
    }

    #endregion

    #region Map Functionality (Added to match HazardLocationListing)

    /// <summary>
    /// View location map in a modal (similar to HazardLocationListing.ViewLocationMap)
    /// </summary>
    /// <param name="hazard">Hazard with location to view</param>
    public async Task ViewHazardLocationMap(Hazard hazard)
    {
        if (hazard?.HazardLocation is null || !HasValidCoordinates(hazard.HazardLocation))
        {
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Warning("Warning", "No valid location coordinates available for this hazard"));
            return;
        }

        try
        {
            _logger.LogInformation("Opening location map for hazard: {HazardCode}", hazard.Code);

            var title = $"Location Map - {hazard.Code}";
            var subtitle = $"Hazard: {hazard.Name ?? "Unnamed Hazard"}";

            await _dialogService.OpenAsync(title, ds =>
            {
                var content = new RenderFragment(builder =>
                {
                    builder.OpenElement(0, "div");
                    builder.AddAttribute(1, "style", "width: 100%; height: 100%;");

                    // Add subtitle if available
                    if (!string.IsNullOrEmpty(subtitle))
                    {
                        builder.OpenElement(2, "div");
                        builder.AddAttribute(3, "class", "mb-3 text-muted");
                        builder.AddAttribute(4, "style", "font-size: 0.9rem;");
                        builder.AddContent(5, subtitle);
                        builder.CloseElement();
                    }

                    // Add HazardLocationDisplay component (same as HazardLocationListing)
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

                    builder.AddMarkupContent(30, $"<strong>Hazard Code:</strong> {hazard.Code}<br/>");
                    builder.AddMarkupContent(31, $"<strong>Report Code:</strong> {hazard.ReportCode}<br/>");
                    if (hazard.HazardLocation.Latitude.HasValue && hazard.HazardLocation.Longitude.HasValue)
                    {
                        builder.AddMarkupContent(32, $"<strong>Coordinates:</strong> {hazard.HazardLocation.Latitude:F6}, {hazard.HazardLocation.Longitude:F6}<br/>");
                    }

                    builder.CloseElement(); // col-md-6

                    // Right column
                    builder.OpenElement(35, "div");
                    builder.AddAttribute(36, "class", "col-md-6");

                    builder.AddMarkupContent(37, $"<strong>Created:</strong> {hazard.HazardLocation.CreatedDate:yyyy-MM-dd HH:mm}<br/>");
                    builder.AddMarkupContent(38, $"<strong>Created By:</strong> {hazard.HazardLocation.CreatedBy ?? "Unknown"}<br/>");
                    if (!string.IsNullOrEmpty(hazard.HazardLocation.Description))
                    {
                        builder.AddMarkupContent(39, $"<strong>Description:</strong> {hazard.HazardLocation.Description}");
                    }

                    builder.CloseElement(); // col-md-6
                    builder.CloseElement(); // row
                    builder.CloseElement(); // details container
                    builder.CloseElement(); // main div
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

            await _eventBus.PublishUIEventAsync(UINotificationEvent.Info("Information", $"Opened location map for {hazard.Code}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening location map for hazard {HazardCode}", hazard.Code);
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Error opening location map"));
        }
    }

    /// <summary>
    /// Check if hazard location has valid coordinates
    /// </summary>
    /// <param name="location">HazardLocation to check</param>
    /// <returns>True if has valid coordinates</returns>
    private bool HasValidCoordinates(HazardLocation? location)
    {
        return location?.Latitude.HasValue == true && 
               location?.Longitude.HasValue == true && 
               location.Latitude.Value != 0 && 
               location.Longitude.Value != 0;
    }

    #endregion

    #region EventBus Testing Methods

    /// <summary>
    /// Test EventBus Phase 1 implementation with SPI threshold event
    /// Demonstrates integration with existing SMS infrastructure
    /// </summary>
    

    /// <summary>
    /// Clear EventBus test results
    /// </summary>
    private void ClearEventBusTestResults()
    {
        EventBusTestMessage = string.Empty;
        EventBusTestError = string.Empty;
        StateHasChanged();
    }

   

    #endregion

    #region Description Modal Methods

    /// <summary>
    /// Render the hazard description column in the data grid
    /// </summary>
    private void RenderHazardDescriptionColumn(RenderTreeBuilder builder, bool includeActions = true)
    {
        builder.OpenComponent<RadzenDataGridColumn<Report>>(10);
        builder.AddAttribute(11, "Title", "Description");
        builder.AddAttribute(12, "Width", "100px");
        builder.AddAttribute(13, "Sortable", false);
        builder.AddAttribute(14, "Template", (RenderFragment<Report>)(report =>
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
                    (args) => ShowDescriptionDialog(report)));
                templateBuilder.CloseComponent();
            }
             )));
        builder.CloseComponent();
    }

    /// <summary>
    /// Show the description dialog for a report
    /// </summary>
    private async Task ShowDescriptionDialog(Report report)
    {
        try
        {
            SelectedDescription = report.Description ?? "No description available";
            SelectedReportId = report.Code ?? "Unknown";
            ShowDescriptionModal = true;
            StateHasChanged();

            _logger.LogInformation("Showing description modal for report {ReportId}", report.Code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing description modal for report {ReportId}", report.Code);
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Error showing description details"));
        }
    }

    /// <summary>
    /// Close the description modal
    /// </summary>
    private void CloseDescriptionModal()
    {
        ShowDescriptionModal = false;
        SelectedDescription = string.Empty;
        SelectedReportId = string.Empty;
        StateHasChanged();
    }

    #endregion
}


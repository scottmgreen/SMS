using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.IO.Compression;
using System.Globalization;
using System.Net.Http;
using System.Collections;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

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
using SMS3.Configuration;
using SMS3.Configuration.Extensions;

namespace SMS3.Components.Pages.SMSListings;

/// <summary>
/// Report Listing Component - Enhanced with full CRUD operations
/// Provides comprehensive data grid listing with view details, edit navigation, and delete functionality
/// </summary>
public partial class ReportListing : ComponentBase
{
    private string _basicTextStyle = "font-size:smaller;font-weight: 600";

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
    private RadzenDataGrid<Report>? _reportsGrid;
    private IEnumerable<Report> _reports = new List<Report>();
    private IList<Report> _selectedReports = new List<Report>();
    private List<Report> _allReports = new List<Report>(); // Store all reports for client-side filtering
    private readonly Dictionary<string, string> _userCodeToFullName = new(StringComparer.OrdinalIgnoreCase);
    private int _totalCount;
    private bool _isLoading = false;

    // Custom confirmation modal properties
    private bool _showResetConfirmModal = false;
    private string _resetConfirmationMessage = string.Empty;
    private Report? _reportToReset = null;

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
    private bool _isEventBusTestRunning { get; set; } = false;
    private string _eventBusTestMessage { get; set; } = string.Empty;
    private string _eventBusTestError { get; set; } = string.Empty;

    // Modal properties for hazard description
    private bool _showDescriptionModal = false;
    private string _selectedDescription = string.Empty;
    private string _selectedReportId = string.Empty;

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

    private static readonly PropertyInfo[] RiskAnalysisExportProperties = typeof(RiskAnalysis)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.GetMethod is not null)
        .ToArray();

    private static readonly PropertyInfo[] ScoringPanelExportProperties = typeof(ScoringPanel)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.GetMethod is not null)
        .ToArray();

    private static readonly PropertyInfo[] MitigationExportProperties = typeof(Mitigation)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.GetMethod is not null)
        .ToArray();

    private static readonly PropertyInfo[] HazardFileExportProperties = typeof(HazardFile)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.GetMethod is not null)
        .ToArray();

    private static readonly PropertyInfo[] InvestigationExportProperties = typeof(Investigation)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.GetMethod is not null)
        .ToArray();

    private static readonly PropertyInfo[] InterviewExportProperties = typeof(Interview)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.GetMethod is not null)
        .ToArray();

    private static readonly PropertyInfo[] ReportValidationExportProperties = typeof(SMS_Domain.Entities.ReportValidation)
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
            _isLoading = true;
            StateHasChanged();

            _logger.LogInformation("Loading reports for listing view");

            await LoadUserDisplayMapAsync();

            var query = new GetAllReportsQuery();
            var result = await _mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value is not null)
            {
                _allReports = result.Value; // Store all reports for filtering/sorting
                _reports = _allReports.Take(15).ToList(); // Initially show first page
                _totalCount = _allReports.Count();
                _logger.LogInformation("Loaded {Count} reports for listing", _totalCount);

                await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", $"Successfully loaded {_totalCount} reports"));
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
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task LoadUserDisplayMapAsync()
    {
        _userCodeToFullName.Clear();

        var usersResult = await _mediator.SendAsync(new GetAllSMSApplicationUsersQuery(), CancellationToken.None);
        if (!usersResult.IsSuccess || usersResult.Value is null)
        {
            return;
        }

        foreach (var user in usersResult.Value)
        {
            if (string.IsNullOrWhiteSpace(user.Code))
            {
                continue;
            }

            var firstName = user.FirstName?.Value?.Trim() ?? string.Empty;
            var lastName = user.LastName?.Value?.Trim() ?? string.Empty;
            var fullName = string.Join(" ", new[] { firstName, lastName }.Where(x => !string.IsNullOrWhiteSpace(x)));

            if (!string.IsNullOrWhiteSpace(fullName))
            {
                _userCodeToFullName[user.Code] = fullName;
            }
        }
    }

    private string GetActorDisplayName(string? actorCode)
    {
        if (string.IsNullOrWhiteSpace(actorCode))
        {
            return SystemConstants.FlyPdxApiSource;
        }

        return _userCodeToFullName.TryGetValue(actorCode, out var fullName)
            ? fullName
            : actorCode;
    }

    private async Task OnExportSelectedReportsPdfAsync()
    {
        var exportableReports = _selectedReports
            .Where(IsReportExportEligible)
            .GroupBy(r => r.Code, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        if (!exportableReports.Any())
        {
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Warning("Warning", "Select at least one report that is not REPORT_NEEDS_VALIDATION to export."));
            return;
        }

        try
        {
            var excludedCount = _selectedReports.Count - exportableReports.Count;
            if (excludedCount > 0)
            {
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Warning("Warning", $"{excludedCount} selected report(s) were skipped because status is REPORT_NEEDS_VALIDATION."));
            }

            var zipBytes = await BuildExportPackagePdfAsync(exportableReports);
            var base64 = Convert.ToBase64String(zipBytes);
            var fileName = $"reports-export-package-pdf-{DateTime.UtcNow:yyyyMMdd-HHmmss}.zip";

            await _jsRuntime.InvokeVoidAsync("downloadFile", fileName, "application/zip", base64);
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", $"Exported {exportableReports.Count} report package(s) in PDF format"));
            _logger.LogInformation("Exported PDF package for {Count} selected reports", exportableReports.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export selected reports PDF package");
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Failed to export selected reports PDF package"));
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

            // If we don't have all reports yet, load them first
            if (_allReports is null || !_allReports.Any())
            {
                await LoadInitialData();
                return;
            }

            // Start with all reports
            var query = _allReports.AsQueryable();

            // Apply filtering
            if (!string.IsNullOrEmpty(args.Filter))
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
                query = query.OrderByDescending(r => r.CreatedDate);
            }

            // Apply paging
            if (args.Skip.HasValue && args.Skip > 0)
            {
                query = query.Skip(args.Skip.Value);
            }

            const int pageSize = 15;
            query = query.Take(pageSize);

            _reports = query.ToList();

            if (_selectedReports.Count > 0)
            {
                var selectedCodes = _selectedReports.Select(r => r.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
                _selectedReports = _allReports
                    .Where(r => selectedCodes.Contains(r.Code))
                    .ToList();
            }

            _logger.LogInformation("Applied filtering/sorting/paging. Showing {Count} of {Total} reports", 
                _reports.Count(), _totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in LoadData");
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Error loading data"));
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
                    (!string.IsNullOrEmpty(r.CreatedBy) && r.CreatedBy.ToLower().Contains(filterValue)) ||
                    (!string.IsNullOrEmpty(r.UpdatedBy) && r.UpdatedBy.ToLower().Contains(filterValue))
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
                        case "createdby":
                            query = ApplyStringFilter(query, r => r.CreatedBy, filterValue, filterOperator);
                            break;
                        case "updatedby":
                            query = ApplyStringFilter(query, r => r.UpdatedBy, filterValue, filterOperator);
                            break;
                        case "createddate":
                            if (DateTime.TryParse(filter.FilterValue?.ToString(), out var dateValue))
                            {
                                query = ApplyDateFilter(query, r => r.CreatedDate, dateValue, filterOperator);
                            }
                            break;
                        case "updateddate":
                            if (DateTime.TryParse(filter.FilterValue?.ToString(), out var updatedDateValue))
                            {
                                query = ApplyDateFilter(query, r => r.UpdatedDate, updatedDateValue, filterOperator);
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
                "createdby" => isDescending ? query.OrderByDescending(r => r.CreatedBy) : query.OrderBy(r => r.CreatedBy),
                "createddate" => isDescending ? query.OrderByDescending(r => r.CreatedDate) : query.OrderBy(r => r.CreatedDate),
                "updatedby" => isDescending ? query.OrderByDescending(r => r.UpdatedBy) : query.OrderBy(r => r.UpdatedBy),
                "updateddate" => isDescending ? query.OrderByDescending(r => r.UpdatedDate) : query.OrderBy(r => r.UpdatedDate),
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
        if (!IsReportExportEligible(report))
        {
            return false;
        }

        return _selectedReports.Any(r => r.Code == report.Code);
    }

    private static bool IsReportExportEligible(Report report)
    {
        return !string.Equals(report.Status?.Trim(), ReportStatus.NeedsValidation.Value, StringComparison.OrdinalIgnoreCase);
    }

    private bool HasExportableSelection()
    {
        return _selectedReports.Any(IsReportExportEligible);
    }

    private void OnReportSelectionChanged(Report report, bool isSelected)
    {
        if (!IsReportExportEligible(report))
        {
            return;
        }

        if (isSelected)
        {
            if (!_selectedReports.Any(r => r.Code == report.Code))
            {
                _selectedReports.Add(report);
            }
        }
        else
        {
            var existing = _selectedReports.FirstOrDefault(r => r.Code == report.Code);
            if (existing is not null)
            {
                _selectedReports.Remove(existing);
            }
        }
    }

    private bool IsAllVisibleReportsSelected()
    {
        var visibleReports = _reports.Where(IsReportExportEligible).ToList();
        if (!visibleReports.Any())
        {
            return false;
        }

        var selectedCodes = _selectedReports.Select(r => r.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return visibleReports.All(r => selectedCodes.Contains(r.Code));
    }

    private void OnSelectAllVisibleReportsChanged(bool isSelected)
    {
        var visibleReports = _reports.Where(IsReportExportEligible).ToList();
        if (!visibleReports.Any())
        {
            return;
        }

        if (isSelected)
        {
            var selectedCodes = _selectedReports.Select(r => r.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var report in visibleReports)
            {
                if (!selectedCodes.Contains(report.Code))
                {
                    _selectedReports.Add(report);
                }
            }
        }
        else
        {
            var visibleCodes = visibleReports.Select(r => r.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
            _selectedReports = _selectedReports
                .Where(r => !visibleCodes.Contains(r.Code))
                .ToList();
        }
    }

    private async Task OnExportSelectedReportsAsync()
    {
        var exportableReports = _selectedReports
            .Where(IsReportExportEligible)
            .GroupBy(r => r.Code, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        if (!exportableReports.Any())
        {
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Warning("Warning", "Select at least one report that is not REPORT_NEEDS_VALIDATION to export."));
            return;
        }

        try
        {
            var excludedCount = _selectedReports.Count - exportableReports.Count;
            if (excludedCount > 0)
            {
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Warning("Warning", $"{excludedCount} selected report(s) were skipped because status is REPORT_NEEDS_VALIDATION."));
            }

            var zipBytes = await BuildExportPackageAsync(exportableReports);
            var base64 = Convert.ToBase64String(zipBytes);
            var fileName = $"reports-export-package-{DateTime.UtcNow:yyyyMMdd-HHmmss}.zip";

            await _jsRuntime.InvokeVoidAsync("downloadFile", fileName, "application/zip", base64);
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", $"Exported {exportableReports.Count} report package(s)"));
            _logger.LogInformation("Exported package for {Count} selected reports", exportableReports.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export selected reports package");
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Failed to export selected reports package"));
        }
    }

    private async Task<byte[]> BuildExportPackageAsync(IEnumerable<Report> reportsToExport)
    {
        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(20)
        };

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var report in reportsToExport)
            {
                if (string.IsNullOrWhiteSpace(report.Code))
                {
                    continue;
                }

                await AddReportFolderToArchiveAsync(archive, report, httpClient);
            }
        }

        return memoryStream.ToArray();
    }

    private async Task<byte[]> BuildExportPackagePdfAsync(IEnumerable<Report> reportsToExport)
    {
        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(20)
        };

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var report in reportsToExport)
            {
                if (string.IsNullOrWhiteSpace(report.Code))
                {
                    continue;
                }

                await AddReportFolderToArchivePdfAsync(archive, report, httpClient);
            }
        }

        return memoryStream.ToArray();
    }

    private async Task AddReportFolderToArchivePdfAsync(ZipArchive archive, Report report, HttpClient httpClient)
    {
        var folderName = $"Report_{SanitizeFileNamePart(report.Code)}";

        var hazardsQuery = new GetHazardsByReportCodeQuery(new ReportID(report.Code));
        var hazardsResult = await _mediator.SendAsync(hazardsQuery, CancellationToken.None);
        var hazards = hazardsResult.IsSuccess && hazardsResult.Value is not null
            ? hazardsResult.Value.ToList()
            : new List<Hazard>();

        var hazardCodes = hazards
            .Where(h => !string.IsNullOrWhiteSpace(h.Code))
            .Select(h => h.Code)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var hazardLocations = new List<HazardLocation>();
        var riskAssessments = new List<RiskAssessment>();
        var scoringPanels = new List<ScoringPanel>();
        var mitigations = new List<Mitigation>();
        var hazardFiles = new List<HazardFile>();

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

            if (!string.IsNullOrWhiteSpace(hazard.Code))
            {
                var scoringPanelQuery = new GetScoringPanelsByHazardCodeQuery(hazard.Code);
                var scoringPanelResult = await _mediator.SendAsync(scoringPanelQuery, CancellationToken.None);

                if (scoringPanelResult.IsSuccess && scoringPanelResult.Value is not null)
                {
                    scoringPanels.AddRange(scoringPanelResult.Value);
                }
            }

            if (!string.IsNullOrWhiteSpace(hazard.Code))
            {
                var mitigationQuery = new GetMitigationsByHazardCodeQuery(hazard.Code);
                var mitigationResult = await _mediator.SendAsync(mitigationQuery, CancellationToken.None);

                if (mitigationResult.IsSuccess && mitigationResult.Value is not null)
                {
                    mitigations.AddRange(mitigationResult.Value);
                }
            }

            if (!string.IsNullOrWhiteSpace(hazard.Code))
            {
                var hazardFilesQuery = new GetHazardFilesByHazardCodeQuery(hazard.Code, includeFileData: true);
                var hazardFilesResult = await _mediator.SendAsync(hazardFilesQuery, CancellationToken.None);

                if (hazardFilesResult.IsSuccess && hazardFilesResult.Value is not null)
                {
                    hazardFiles.AddRange(hazardFilesResult.Value);
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

        var distinctScoringPanels = scoringPanels
            .GroupBy(panel => panel.Code)
            .Select(group => group.First())
            .ToList();

        var distinctMitigations = mitigations
            .GroupBy(mitigation => mitigation.Code)
            .Select(group => group.First())
            .ToList();

        var distinctHazardFiles = hazardFiles
            .GroupBy(file => file.Code)
            .Select(group => group.First())
            .ToList();

        AddPdfEntryIfAny(
            archive,
            $"{folderName}/Report_{SanitizeFileNamePart(report.Code)}.pdf",
            BuildTablePdf($"Report - {report.Code}", new[] { report }, ReportExportProperties));

        AddPdfEntryIfAny(
            archive,
            $"{folderName}/Hazards_{SanitizeFileNamePart(report.Code)}.pdf",
            hazards,
            records => BuildTablePdf($"Hazards - {report.Code}", records, HazardExportProperties));

        var riskAssessmentCodes = distinctRiskAssessments
            .Where(a => !string.IsNullOrWhiteSpace(a.Code))
            .Select(a => a.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var allRiskAnalysisQuery = new GetAllRiskAnalysisQuery();
        var allRiskAnalysisResult = await _mediator.SendAsync(allRiskAnalysisQuery, CancellationToken.None);
        var riskAnalyses = allRiskAnalysisResult.IsSuccess && allRiskAnalysisResult.Value is not null
            ? allRiskAnalysisResult.Value
                .Where(analysis =>
                    (!string.IsNullOrWhiteSpace(analysis.HazardCode) && hazardCodes.Contains(analysis.HazardCode)) ||
                    (!string.IsNullOrWhiteSpace(analysis.RiskAssessmentCode) && riskAssessmentCodes.Contains(analysis.RiskAssessmentCode)))
                .GroupBy(analysis => analysis.Code)
                .Select(group => group.First())
                .ToList()
            : new List<RiskAnalysis>();

        var allInvestigationsQuery = new GetAllInvestigationsQuery();
        var allInvestigationsResult = await _mediator.SendAsync(allInvestigationsQuery, CancellationToken.None);
        var investigations = allInvestigationsResult.IsSuccess && allInvestigationsResult.Value is not null
            ? allInvestigationsResult.Value
                .Where(investigation =>
                    string.Equals(investigation.ReportCode, report.Code, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrWhiteSpace(investigation.HazardCode) && hazardCodes.Contains(investigation.HazardCode)))
                .GroupBy(investigation => investigation.Code)
                .Select(group => group.First())
                .ToList()
            : new List<Investigation>();

        var investigationCodes = investigations
            .Where(i => !string.IsNullOrWhiteSpace(i.Code))
            .Select(i => i.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var allInterviewsQuery = new GetAllInterviewsQuery();
        var allInterviewsResult = await _mediator.SendAsync(allInterviewsQuery, CancellationToken.None);
        var interviews = allInterviewsResult.IsSuccess && allInterviewsResult.Value is not null
            ? allInterviewsResult.Value
                .Where(interview =>
                    !string.IsNullOrWhiteSpace(interview.InvestigationCode)
                    && investigationCodes.Contains(interview.InvestigationCode))
                .GroupBy(interview => interview.Code)
                .Select(group => group.First())
                .ToList()
            : new List<Interview>();

        var reportValidationQuery = new GetReportValidationByReportIdQuery(new ReportID(report.Code));
        var reportValidationResult = await _mediator.SendAsync(reportValidationQuery, CancellationToken.None);
        var reportValidations = reportValidationResult.IsSuccess && reportValidationResult.Value is not null
            ? new List<SMS_Domain.Entities.ReportValidation> { reportValidationResult.Value }
            : new List<SMS_Domain.Entities.ReportValidation>();

        AddPdfEntryIfAny(
            archive,
            $"{folderName}/HazardLocations_{SanitizeFileNamePart(report.Code)}.pdf",
            distinctLocations,
            records => BuildTablePdf($"Hazard Locations - {report.Code}", records, HazardLocationExportProperties));

        AddPdfEntryIfAny(
            archive,
            $"{folderName}/RiskAssessments_{SanitizeFileNamePart(report.Code)}.pdf",
            distinctRiskAssessments,
            records => BuildTablePdf($"Risk Assessments - {report.Code}", records, RiskAssessmentExportProperties));

        AddPdfEntryIfAny(
            archive,
            $"{folderName}/RiskAnalyses_{SanitizeFileNamePart(report.Code)}.pdf",
            riskAnalyses,
            records => BuildTablePdf($"Risk Analyses - {report.Code}", records, RiskAnalysisExportProperties));

        AddPdfEntryIfAny(
            archive,
            $"{folderName}/ScoringPanels_{SanitizeFileNamePart(report.Code)}.pdf",
            distinctScoringPanels,
            records => BuildTablePdf($"Scoring Panels - {report.Code}", records, ScoringPanelExportProperties));

        AddPdfEntryIfAny(
            archive,
            $"{folderName}/Mitigations_{SanitizeFileNamePart(report.Code)}.pdf",
            distinctMitigations,
            records => BuildTablePdf($"Mitigations - {report.Code}", records, MitigationExportProperties));

        AddPdfEntryIfAny(
            archive,
            $"{folderName}/HazardFiles_{SanitizeFileNamePart(report.Code)}.pdf",
            distinctHazardFiles,
            records => BuildTablePdf($"Hazard Files - {report.Code}", records, HazardFileExportProperties));

        AddPdfEntryIfAny(
            archive,
            $"{folderName}/Investigations_{SanitizeFileNamePart(report.Code)}.pdf",
            investigations,
            records => BuildTablePdf($"Investigations - {report.Code}", records, InvestigationExportProperties));

        AddPdfEntryIfAny(
            archive,
            $"{folderName}/Interviews_{SanitizeFileNamePart(report.Code)}.pdf",
            interviews,
            records => BuildTablePdf($"Interviews - {report.Code}", records, InterviewExportProperties));

        AddPdfEntryIfAny(
            archive,
            $"{folderName}/ReportValidation_{SanitizeFileNamePart(report.Code)}.pdf",
            reportValidations,
            records => BuildTablePdf($"Report Validation - {report.Code}", records, ReportValidationExportProperties));

        await AddHazardFileContentEntriesAsync(archive, folderName, distinctHazardFiles);
        await AddLocationThumbnailEntriesAsync(archive, folderName, distinctLocations, httpClient);

        var reportSummary = BuildExportManifest(
            report,
            hazards.Count,
            distinctLocations.Count,
            distinctRiskAssessments.Count,
            riskAnalyses.Count,
            distinctScoringPanels.Count,
            distinctMitigations.Count,
            distinctHazardFiles.Count,
            investigations.Count,
            interviews.Count,
            reportValidations.Count);

        AddTextEntry(
            archive,
            $"{folderName}/ExportManifest_{SanitizeFileNamePart(report.Code)}.txt",
            reportSummary);

        AddTextEntry(
            archive,
            $"ReportSummary_{SanitizeFileNamePart(report.Code)}.txt",
            reportSummary);
    }

    private async Task AddReportFolderToArchiveAsync(ZipArchive archive, Report report, HttpClient httpClient)
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

        var hazardCodes = hazards
            .Where(h => !string.IsNullOrWhiteSpace(h.Code))
            .Select(h => h.Code)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        AddCsvEntry(
            archive,
            $"{folderName}/Hazards_{SanitizeFileNamePart(report.Code)}.csv",
            BuildCsv(hazards, HazardExportProperties));

        var hazardLocations = new List<HazardLocation>();
        var riskAssessments = new List<RiskAssessment>();
        var scoringPanels = new List<ScoringPanel>();
        var mitigations = new List<Mitigation>();
        var hazardFiles = new List<HazardFile>();

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

            if (!string.IsNullOrWhiteSpace(hazard.Code))
            {
                var scoringPanelQuery = new GetScoringPanelsByHazardCodeQuery(hazard.Code);
                var scoringPanelResult = await _mediator.SendAsync(scoringPanelQuery, CancellationToken.None);

                if (scoringPanelResult.IsSuccess && scoringPanelResult.Value is not null)
                {
                    scoringPanels.AddRange(scoringPanelResult.Value);
                }
            }

            if (!string.IsNullOrWhiteSpace(hazard.Code))
            {
                var mitigationQuery = new GetMitigationsByHazardCodeQuery(hazard.Code);
                var mitigationResult = await _mediator.SendAsync(mitigationQuery, CancellationToken.None);

                if (mitigationResult.IsSuccess && mitigationResult.Value is not null)
                {
                    mitigations.AddRange(mitigationResult.Value);
                }
            }

            if (!string.IsNullOrWhiteSpace(hazard.Code))
            {
                var hazardFilesQuery = new GetHazardFilesByHazardCodeQuery(hazard.Code, includeFileData: true);
                var hazardFilesResult = await _mediator.SendAsync(hazardFilesQuery, CancellationToken.None);

                if (hazardFilesResult.IsSuccess && hazardFilesResult.Value is not null)
                {
                    hazardFiles.AddRange(hazardFilesResult.Value);
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

        var distinctScoringPanels = scoringPanels
            .GroupBy(panel => panel.Code)
            .Select(group => group.First())
            .ToList();

        var distinctMitigations = mitigations
            .GroupBy(mitigation => mitigation.Code)
            .Select(group => group.First())
            .ToList();

        var distinctHazardFiles = hazardFiles
            .GroupBy(file => file.Code)
            .Select(group => group.First())
            .ToList();

        var riskAssessmentCodes = distinctRiskAssessments
            .Where(a => !string.IsNullOrWhiteSpace(a.Code))
            .Select(a => a.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var allRiskAnalysisQuery = new GetAllRiskAnalysisQuery();
        var allRiskAnalysisResult = await _mediator.SendAsync(allRiskAnalysisQuery, CancellationToken.None);
        var riskAnalyses = allRiskAnalysisResult.IsSuccess && allRiskAnalysisResult.Value is not null
            ? allRiskAnalysisResult.Value
                .Where(analysis =>
                    (!string.IsNullOrWhiteSpace(analysis.HazardCode) && hazardCodes.Contains(analysis.HazardCode)) ||
                    (!string.IsNullOrWhiteSpace(analysis.RiskAssessmentCode) && riskAssessmentCodes.Contains(analysis.RiskAssessmentCode)))
                .GroupBy(analysis => analysis.Code)
                .Select(group => group.First())
                .ToList()
            : new List<RiskAnalysis>();

        var allInvestigationsQuery = new GetAllInvestigationsQuery();
        var allInvestigationsResult = await _mediator.SendAsync(allInvestigationsQuery, CancellationToken.None);
        var investigations = allInvestigationsResult.IsSuccess && allInvestigationsResult.Value is not null
            ? allInvestigationsResult.Value
                .Where(investigation =>
                    string.Equals(investigation.ReportCode, report.Code, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrWhiteSpace(investigation.HazardCode) && hazardCodes.Contains(investigation.HazardCode)))
                .GroupBy(investigation => investigation.Code)
                .Select(group => group.First())
                .ToList()
            : new List<Investigation>();

        var investigationCodes = investigations
            .Where(i => !string.IsNullOrWhiteSpace(i.Code))
            .Select(i => i.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var allInterviewsQuery = new GetAllInterviewsQuery();
        var allInterviewsResult = await _mediator.SendAsync(allInterviewsQuery, CancellationToken.None);
        var interviews = allInterviewsResult.IsSuccess && allInterviewsResult.Value is not null
            ? allInterviewsResult.Value
                .Where(interview =>
                    !string.IsNullOrWhiteSpace(interview.InvestigationCode)
                    && investigationCodes.Contains(interview.InvestigationCode))
                .GroupBy(interview => interview.Code)
                .Select(group => group.First())
                .ToList()
            : new List<Interview>();

        var reportValidationQuery = new GetReportValidationByReportIdQuery(new ReportID(report.Code));
        var reportValidationResult = await _mediator.SendAsync(reportValidationQuery, CancellationToken.None);
        var reportValidations = reportValidationResult.IsSuccess && reportValidationResult.Value is not null
            ? new List<SMS_Domain.Entities.ReportValidation> { reportValidationResult.Value }
            : new List<SMS_Domain.Entities.ReportValidation>();

        AddCsvEntry(
            archive,
            $"{folderName}/HazardLocations_{SanitizeFileNamePart(report.Code)}.csv",
            BuildCsv(distinctLocations, HazardLocationExportProperties));

        AddCsvEntry(
            archive,
            $"{folderName}/RiskAssessments_{SanitizeFileNamePart(report.Code)}.csv",
            BuildCsv(distinctRiskAssessments, RiskAssessmentExportProperties));

        AddCsvEntry(
            archive,
            $"{folderName}/RiskAnalyses_{SanitizeFileNamePart(report.Code)}.csv",
            BuildCsv(riskAnalyses, RiskAnalysisExportProperties));

        AddCsvEntry(
            archive,
            $"{folderName}/ScoringPanels_{SanitizeFileNamePart(report.Code)}.csv",
            BuildCsv(distinctScoringPanels, ScoringPanelExportProperties));

        AddCsvEntry(
            archive,
            $"{folderName}/Mitigations_{SanitizeFileNamePart(report.Code)}.csv",
            BuildCsv(distinctMitigations, MitigationExportProperties));

        AddCsvEntry(
            archive,
            $"{folderName}/HazardFiles_{SanitizeFileNamePart(report.Code)}.csv",
            BuildCsv(distinctHazardFiles, HazardFileExportProperties));

        AddCsvEntry(
            archive,
            $"{folderName}/Investigations_{SanitizeFileNamePart(report.Code)}.csv",
            BuildCsv(investigations, InvestigationExportProperties));

        AddCsvEntry(
            archive,
            $"{folderName}/Interviews_{SanitizeFileNamePart(report.Code)}.csv",
            BuildCsv(interviews, InterviewExportProperties));

        AddCsvEntry(
            archive,
            $"{folderName}/ReportValidation_{SanitizeFileNamePart(report.Code)}.csv",
            BuildCsv(reportValidations, ReportValidationExportProperties));

        await AddHazardFileContentEntriesAsync(archive, folderName, distinctHazardFiles);
        await AddLocationThumbnailEntriesAsync(archive, folderName, distinctLocations, httpClient);

        var reportSummary = BuildExportManifest(
            report,
            hazards.Count,
            distinctLocations.Count,
            distinctRiskAssessments.Count,
            riskAnalyses.Count,
            distinctScoringPanels.Count,
            distinctMitigations.Count,
            distinctHazardFiles.Count,
            investigations.Count,
            interviews.Count,
            reportValidations.Count);

        AddTextEntry(
            archive,
            $"{folderName}/ExportManifest_{SanitizeFileNamePart(report.Code)}.txt",
            reportSummary);

        AddTextEntry(
            archive,
            $"ReportSummary_{SanitizeFileNamePart(report.Code)}.txt",
            reportSummary);
    }

    private static void AddCsvEntry(ZipArchive archive, string entryPath, string csvContent)
    {
        var entry = archive.CreateEntry(entryPath, CompressionLevel.Fastest);
        using var entryStream = entry.Open();
        using var writer = new StreamWriter(entryStream, Encoding.UTF8);
        writer.Write(csvContent);
    }

    private static void AddPdfEntry(ZipArchive archive, string entryPath, byte[] pdfBytes)
    {
        var entry = archive.CreateEntry(entryPath, CompressionLevel.Fastest);
        using var entryStream = entry.Open();
        entryStream.Write(pdfBytes, 0, pdfBytes.Length);
    }

    private static void AddPdfEntryIfAny<T>(
        ZipArchive archive,
        string entryPath,
        IEnumerable<T>? records,
        Func<IReadOnlyCollection<T>, byte[]> pdfFactory)
    {
        var items = records?.ToList() ?? new List<T>();
        if (items.Count == 0)
        {
            return;
        }

        AddPdfEntry(archive, entryPath, pdfFactory(items));
    }

    private static void AddPdfEntryIfAny(ZipArchive archive, string entryPath, byte[]? pdfBytes)
    {
        if (pdfBytes is null || pdfBytes.Length == 0)
        {
            return;
        }

        AddPdfEntry(archive, entryPath, pdfBytes);
    }

    private static void AddTextEntry(ZipArchive archive, string entryPath, string content)
    {
        var entry = archive.CreateEntry(entryPath, CompressionLevel.Fastest);
        using var entryStream = entry.Open();
        using var writer = new StreamWriter(entryStream, Encoding.UTF8);
        writer.Write(content);
    }

    private static void AddBinaryEntry(ZipArchive archive, string entryPath, byte[] content)
    {
        var entry = archive.CreateEntry(entryPath, CompressionLevel.Optimal);
        using var entryStream = entry.Open();
        entryStream.Write(content, 0, content.Length);
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

    private static byte[] BuildTablePdf<T>(string title, IEnumerable<T> records, PropertyInfo[] properties)
    {
        var items = records?.ToList() ?? new List<T>();

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(QuestPDF.Helpers.PageSizes.A4);
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(column =>
                {
                    column.Item().Background("#111111").Padding(12).Text("PDX SMS Export").FontSize(16).Bold().FontColor("#FFFFFF");
                    column.Item().Background("#003F40").PaddingHorizontal(12).PaddingVertical(8).Text(title).FontSize(11).Bold().FontColor("#FFFFFF");
                    column.Item().PaddingTop(6).Text($"Generated UTC: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}").FontSize(8).FontColor(QuestPDF.Helpers.Colors.Grey.Darken1);
                });

                page.Content().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(200);
                        columns.RelativeColumn();
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background("#00AF9B").Border(1).BorderColor("#E6E6E6").Padding(6)
                            .Text("Field Name").Bold().FontSize(9).FontColor("#111111");
                        header.Cell().Background("#00AF9B").Border(1).BorderColor("#E6E6E6").Padding(6)
                            .Text("Field Value").Bold().FontSize(9).FontColor("#111111");
                    });

                    if (items.Count == 0)
                    {
                        table.Cell().ColumnSpan(2).Border(1).BorderColor("#E6E6E6").Padding(8)
                            .Text("No records found.");
                    }
                    else
                    {
                        foreach (var item in items)
                        {
                            foreach (var property in properties)
                            {
                                var value = property.GetValue(item);
                                var text = FormatPdfValue(value);

                                table.Cell().Border(1).BorderColor("#E6E6E6").Background("#FAFAFA").Padding(6)
                                    .Text(property.Name).FontSize(8).SemiBold();

                                table.Cell().Border(1).BorderColor("#E6E6E6").Padding(6)
                                    .Text(string.IsNullOrWhiteSpace(text) ? "-" : text).FontSize(8);
                            }
                        }
                    }
                });

                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private static string FormatPdfValue(object? value)
    {
        if (value is null)
        {
            return string.Empty;
        }

        if (value is IEnumerable enumerable && value is not string)
        {
            return TruncateForPdf(FormatEnumerableValue(enumerable));
        }

        return TruncateForPdf(FormatObjectValue(value));
    }

    private static string TruncateForPdf(string value)
    {
        const int maxLength = 320;
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength] + "…";
    }

    private static string FormatCsvValue(object? value)
    {
        return value switch
        {
            null => string.Empty,
            bool flag => EscapeCsv(flag ? "TRUE" : "FALSE"),
            DateTime dateTime => EscapeCsv(dateTime.ToString("O")),
            DateTimeOffset dateTimeOffset => EscapeCsv(dateTimeOffset.ToString("O")),
            byte[] bytes => EscapeCsv($"[{bytes.Length} bytes]"),
            IEnumerable enumerable when value is not string => EscapeCsv(FormatEnumerableValue(enumerable)),
            _ => EscapeCsv(value.ToString() ?? string.Empty)
        };
    }

    private static string FormatEnumerableValue(IEnumerable values)
    {
        var items = new List<string>();

        foreach (var item in values)
        {
            var formatted = FormatObjectValue(item);
            if (!string.IsNullOrWhiteSpace(formatted))
            {
                items.Add(formatted);
            }
        }

        return string.Join(" | ", items);
    }

    private static string FormatObjectValue(object? value)
    {
        if (value is null)
        {
            return string.Empty;
        }

        if (value is DateTime dateTime)
        {
            return dateTime.ToString("O");
        }

        if (value is DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.ToString("O");
        }

        if (value is bool flag)
        {
            return flag ? "TRUE" : "FALSE";
        }

        if (value is string text)
        {
            return text;
        }

        if (value is byte[] bytes)
        {
            return $"[{bytes.Length} bytes]";
        }

        var valueType = value.GetType();
        if (valueType.IsPrimitive || value is decimal)
        {
            return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
        }

        var codeProperty = valueType.GetProperty("Code", BindingFlags.Public | BindingFlags.Instance);
        if (codeProperty is not null)
        {
            var codeValue = codeProperty.GetValue(value)?.ToString();
            if (!string.IsNullOrWhiteSpace(codeValue))
            {
                return codeValue;
            }
        }

        var idValueProperty = valueType.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
        if (idValueProperty is not null)
        {
            var idValue = idValueProperty.GetValue(value)?.ToString();
            if (!string.IsNullOrWhiteSpace(idValue))
            {
                return idValue;
            }
        }

        return value.ToString() ?? string.Empty;
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

    private async Task AddHazardFileContentEntriesAsync(ZipArchive archive, string folderName, IReadOnlyCollection<HazardFile> hazardFiles)
    {
        if (hazardFiles.Count == 0)
        {
            return;
        }

        var usedFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var hazardFile in hazardFiles)
        {
            var fileBytes = await TryResolveHazardFileBytesAsync(hazardFile);
            if (fileBytes is null || fileBytes.Length == 0)
            {
                continue;
            }

            var hazardPart = string.IsNullOrWhiteSpace(hazardFile.HazardCode)
                ? "UnknownHazard"
                : SanitizeFileNamePart(hazardFile.HazardCode);

            var baseName = !string.IsNullOrWhiteSpace(hazardFile.FileName)
                ? SanitizeFileNamePart(hazardFile.FileName)
                : $"{SanitizeFileNamePart(hazardFile.Code)}.bin";

            var candidateName = baseName;
            var suffix = 1;
            while (!usedFileNames.Add($"{hazardPart}/{candidateName}"))
            {
                var fileNameNoExt = Path.GetFileNameWithoutExtension(baseName);
                var extension = Path.GetExtension(baseName);
                candidateName = $"{fileNameNoExt}_{suffix}{extension}";
                suffix++;
            }

            AddBinaryEntry(
                archive,
                $"{folderName}/HazardFiles/{hazardPart}/{candidateName}",
                fileBytes);
        }
    }

    private static async Task<byte[]?> TryResolveHazardFileBytesAsync(HazardFile hazardFile)
    {
        if (hazardFile.FileData is { Length: > 0 })
        {
            return hazardFile.FileData;
        }

        if (!string.IsNullOrWhiteSpace(hazardFile.FilePath) && File.Exists(hazardFile.FilePath))
        {
            try
            {
                return await File.ReadAllBytesAsync(hazardFile.FilePath);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    private async Task AddLocationThumbnailEntriesAsync(
        ZipArchive archive,
        string folderName,
        IReadOnlyCollection<HazardLocation> locations,
        HttpClient httpClient)
    {
        if (locations.Count == 0)
        {
            return;
        }

        foreach (var location in locations)
        {
            if (!location.Latitude.HasValue || !location.Longitude.HasValue)
            {
                continue;
            }

            var locationCode = string.IsNullOrWhiteSpace(location.Code)
                ? $"Location_{Guid.NewGuid():N}"
                : SanitizeFileNamePart(location.Code);

            try
            {
                var thumbnailBytes = await TryDownloadStaticMapThumbnailAsync(httpClient, location.Latitude.Value, location.Longitude.Value);
                if (thumbnailBytes is not null && thumbnailBytes.Length > 0)
                {
                    AddBinaryEntry(
                        archive,
                        $"{folderName}/LocationThumbnails/{locationCode}.png",
                        thumbnailBytes);
                    continue;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed downloading static thumbnail for hazard location {LocationCode}", location.Code);
            }

            var fallbackSvg = BuildFallbackLocationSvgThumbnail(location);
            AddBinaryEntry(
                archive,
                $"{folderName}/LocationThumbnails/{locationCode}.svg",
                Encoding.UTF8.GetBytes(fallbackSvg));
        }
    }

    private static async Task<byte[]?> TryDownloadStaticMapThumbnailAsync(HttpClient httpClient, decimal latitude, decimal longitude)
    {
        // External static map host may be blocked in secured/offline environments.
        // Keep export deterministic by using SVG fallback thumbnails unless this is explicitly re-enabled.
        const bool enableExternalStaticMapDownload = false;
        if (!enableExternalStaticMapDownload)
        {
            return null;
        }

        var lat = latitude.ToString("0.######", CultureInfo.InvariantCulture);
        var lng = longitude.ToString("0.######", CultureInfo.InvariantCulture);
        var url = $"https://staticmap.openstreetmap.de/staticmap.php?center={lat},{lng}&zoom=15&size=640x360&maptype=mapnik&markers={lat},{lng},red-pushpin";

        try
        {
            using var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var contentType = response.Content.Headers.ContentType?.MediaType;
            if (string.IsNullOrWhiteSpace(contentType) || !contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException)
        {
            return null;
        }
    }

    private static string BuildFallbackLocationSvgThumbnail(HazardLocation location)
    {
        var latText = location.Latitude?.ToString("0.######", CultureInfo.InvariantCulture) ?? "N/A";
        var lngText = location.Longitude?.ToString("0.######", CultureInfo.InvariantCulture) ?? "N/A";
        var description = string.IsNullOrWhiteSpace(location.Description)
            ? "Location"
            : EscapeXml(location.Description);

        return $"""
<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"640\" height=\"360\" viewBox=\"0 0 640 360\">
  <defs>
    <linearGradient id=\"bg\" x1=\"0\" y1=\"0\" x2=\"0\" y2=\"1\">
      <stop offset=\"0%\" stop-color=\"#eef5ff\" />
      <stop offset=\"100%\" stop-color=\"#dde8f8\" />
    </linearGradient>
  </defs>
  <rect x=\"0\" y=\"0\" width=\"640\" height=\"360\" fill=\"url(#bg)\" />
  <g stroke=\"#c7d6ea\" stroke-width=\"1\" opacity=\"0.65\">
    <line x1=\"0\" y1=\"60\" x2=\"640\" y2=\"60\" />
    <line x1=\"0\" y1=\"120\" x2=\"640\" y2=\"120\" />
    <line x1=\"0\" y1=\"180\" x2=\"640\" y2=\"180\" />
    <line x1=\"0\" y1=\"240\" x2=\"640\" y2=\"240\" />
    <line x1=\"0\" y1=\"300\" x2=\"640\" y2=\"300\" />
    <line x1=\"80\" y1=\"0\" x2=\"80\" y2=\"360\" />
    <line x1=\"160\" y1=\"0\" x2=\"160\" y2=\"360\" />
    <line x1=\"240\" y1=\"0\" x2=\"240\" y2=\"360\" />
    <line x1=\"320\" y1=\"0\" x2=\"320\" y2=\"360\" />
    <line x1=\"400\" y1=\"0\" x2=\"400\" y2=\"360\" />
    <line x1=\"480\" y1=\"0\" x2=\"480\" y2=\"360\" />
    <line x1=\"560\" y1=\"0\" x2=\"560\" y2=\"360\" />
  </g>
  <g transform=\"translate(320,170)\">
    <circle cx=\"0\" cy=\"0\" r=\"18\" fill=\"#dc3545\" stroke=\"#ffffff\" stroke-width=\"3\" />
    <path d=\"M0 18 L-8 34 L8 34 Z\" fill=\"#dc3545\" />
  </g>
  <rect x=\"16\" y=\"270\" width=\"608\" height=\"74\" rx=\"8\" fill=\"#ffffff\" fill-opacity=\"0.9\" />
  <text x=\"28\" y=\"296\" font-family=\"Segoe UI, Arial, sans-serif\" font-size=\"16\" fill=\"#1f2d3d\" font-weight=\"600\">{description}</text>
  <text x=\"28\" y=\"321\" font-family=\"Consolas, monospace\" font-size=\"14\" fill=\"#334e68\">Lat: {latText}   Lng: {lngText}</text>
  <text x=\"28\" y=\"341\" font-family=\"Segoe UI, Arial, sans-serif\" font-size=\"12\" fill=\"#627d98\">Fallback map thumbnail generated during export.</text>
</svg>
""";
    }

    private static string EscapeXml(string value)
    {
        return value
            .Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal)
            .Replace("\"", "&quot;", StringComparison.Ordinal)
            .Replace("'", "&apos;", StringComparison.Ordinal);
    }

    private static string BuildExportManifest(
        Report report,
        int hazardCount,
        int hazardLocationCount,
        int riskAssessmentCount,
        int riskAnalysisCount,
        int scoringPanelCount,
        int mitigationCount,
        int hazardFileCount,
        int investigationCount,
        int interviewCount,
        int reportValidationCount)
    {
        var sb = new StringBuilder();
        sb.AppendLine("PDXSMS Comprehensive Report Export Package");
        sb.AppendLine($"Generated UTC: {DateTime.UtcNow:O}");
        sb.AppendLine($"Report Code: {report.Code}");
        sb.AppendLine($"Report Name: {report.Name}");
        sb.AppendLine($"Report Status: {report.Status}");
        sb.AppendLine();
        sb.AppendLine("Included Data Counts:");
        sb.AppendLine($"- Hazards: {hazardCount}");
        sb.AppendLine($"- Hazard Locations: {hazardLocationCount}");
        sb.AppendLine($"- Risk Assessments: {riskAssessmentCount}");
        sb.AppendLine($"- Risk Analyses: {riskAnalysisCount}");
        sb.AppendLine($"- Scoring Panels: {scoringPanelCount}");
        sb.AppendLine($"- Mitigations: {mitigationCount}");
        sb.AppendLine($"- Hazard Files (metadata): {hazardFileCount}");
        sb.AppendLine($"- Investigations: {investigationCount}");
        sb.AppendLine($"- Interviews: {interviewCount}");
        sb.AppendLine($"- Report Validations: {reportValidationCount}");
        sb.AppendLine();
        sb.AppendLine("Included Folders:");
        sb.AppendLine("- HazardFiles/* (exported evidence files when data/path is available)");
        sb.AppendLine("- LocationThumbnails/* (map thumbnails per location, with SVG fallback)");
        return sb.ToString();
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
            _isLoading = true;
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
            _isLoading = false;
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
                $"Edit report '{report.Code} - {report.Name}'?\n\nThis will navigate to the Technical hazard reporting form in edit mode.",
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
                    _navigation.NavigateToSecure($"/SMSRiskManagement/HazardReporting/{initialHazard.Code}?returnTo=report-listing");
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
        if (!ReportStatus.TryFromValue(status, out var reportStatus) || reportStatus is null)
        {
            return BadgeStyle.Info;
        }

        if (reportStatus == ReportStatus.NeedsValidation)
            return BadgeStyle.Warning;
        if (reportStatus == ReportStatus.ValidationCompleted ||
            reportStatus == ReportStatus.ReadyForProcessing ||
            reportStatus == ReportStatus.MitigationComplete ||
            reportStatus == ReportStatus.Closed)
            return BadgeStyle.Success;
        if (reportStatus == ReportStatus.ValidationRevised ||
            reportStatus == ReportStatus.RiskAssessmentInProgress ||
            reportStatus == ReportStatus.RiskAssessmentSubmitted ||
            reportStatus == ReportStatus.InMitigation ||
            reportStatus == ReportStatus.UnderInvestigation)
            return BadgeStyle.Info;
        if (reportStatus == ReportStatus.RiskRegistryOnly)
            return BadgeStyle.Danger;

        return BadgeStyle.Secondary;
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

            if (hazardResult.IsSuccess && hazardResult.Value is not null)
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

                    if (!hazardResetResult.IsSuccess)
                    {
                        throw new InvalidOperationException($"Failed to reset hazard scores for {hazard.Code}: {hazardResetResult.Error?.Message}");
                    }


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
                var validation = SMS_Domain.Entities.ReportValidation.Create(
                    reportCode,
                    string.IsNullOrWhiteSpace(_currentUserService?.UserCode)
                        ? SystemConstants.FlyPdxApiSource
                        : _currentUserService.UserCode);
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
        var updatestatuscmd = new UpdateReportStatusCommand(
            reportcode,
            status,
            string.IsNullOrWhiteSpace(_currentUserService?.UserCode)
                ? SystemConstants.FlyPdxApiSource
                : _currentUserService.UserCode);
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
            _isLoading = true;
            StateHasChanged();

            // Load associated hazards to show impact
            await LoadAssociatedHazardsAsync(report.Code);
            var hazardCount = AssociatedHazards?.Count ?? 0;

            // Build detailed confirmation message and show custom modal
            _resetConfirmationMessage = BuildResetConfirmationMessage(report, hazardCount);
            _reportToReset = report;
            _showResetConfirmModal = true;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error preparing reset confirmation for report {ReportCode}", report.Code);
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", $"Error preparing reset confirmation: {ex.Message}"));
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Handle the actual reset confirmation from custom modal
    /// </summary>
    private async Task HandleResetConfirmation()
    {
        if (_reportToReset is null) return;

        try
        {
            _showResetConfirmModal = false;
            _isLoading = true;
            StateHasChanged();

            _logger.LogInformation("User confirmed reset for report {ReportCode}", _reportToReset.Code);

            // Perform the reset operation
            var result = await ResetReportValidation(_reportToReset.Code);

            if (result.IsSuccess && result.Value)
            {
                _logger.LogInformation("Successfully reset report validation for {ReportCode}", _reportToReset.Code);

                // Show success notification
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", $"Report '{_reportToReset.Code}' validation has been successfully reset"));

                // Refresh the data grid to reflect changes
                await LoadInitialData();

                // Update UI state
                StateHasChanged();
            }
            else
            {
                var errorMessage = result.Error?.Message ?? "Unknown error occurred during reset";
                _logger.LogError("Failed to reset report validation for {ReportCode}: {Error}", _reportToReset.Code, errorMessage);

                await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", $"Failed to reset report validation: {errorMessage}"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during reset operation for report {ReportCode}", _reportToReset?.Code);
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", $"An unexpected error occurred while resetting the report: {ex.Message}"));
        }
        finally
        {
            // Clean up
            _reportToReset = null;
            _resetConfirmationMessage = string.Empty;
            _isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Cancel the reset operation
    /// </summary>
    private void CancelResetConfirmation()
    {
        _showResetConfirmModal = false;
        _reportToReset = null;
        _resetConfirmationMessage = string.Empty;
        StateHasChanged();
        
        _logger.LogInformation("User cancelled reset operation for report {ReportCode}", _reportToReset?.Code);
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
        _eventBusTestMessage = string.Empty;
        _eventBusTestError = string.Empty;
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
            _selectedDescription = report.Description ?? "No description available";
            _selectedReportId = report.Code ?? "Unknown";
            _showDescriptionModal = true;
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
        _showDescriptionModal = false;
        _selectedDescription = string.Empty;
        _selectedReportId = string.Empty;
        StateHasChanged();
    }

    #endregion
}


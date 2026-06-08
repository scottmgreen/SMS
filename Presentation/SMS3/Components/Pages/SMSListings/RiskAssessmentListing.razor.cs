using System.Linq.Expressions;

using SMS_Application.Interfaces;
using SMS_Application.Commands;
using SMS_Application.Queries;

using SMS_Domain.Entities;
using SMS_Domain.Events;

using SMS_Domain.Enums;
using SMS_Domain.Errors;
using SMS_Domain.ValueObjects;

using SMS_Shared.Configuration;

using SMS3.Components.Shared;
using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration.Extensions;

namespace SMS3.Components.Pages.SMSListings;

/// <summary>
/// Risk Assessment Listing Component - Enhanced with full CRUD operations and advanced filtering
/// Provides comprehensive data grid listing with view details, edit navigation, and delete functionality
/// </summary>
public partial class RiskAssessmentListing : ComponentBase
{
    private sealed class AssessmentValidationSnapshot
    {
        public int RequiredSteps { get; init; }
        public int ValidatedSteps { get; init; }
        public int ProgressPercent { get; init; }
        public bool IsFullyValidated => ValidatedSteps >= RequiredSteps;
        public string Summary { get; init; } = string.Empty;
    }

    private string BasicTextStyle = "font-size:smaller;font-weight: 600";

    #region Dependencies
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<RiskAssessmentListing> _logger { get; set; } = default!;
    [Inject] private IBaseEventBus _eventBus { get; set; } = default!;
 
    [Inject] private DialogService _dialogService { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    #endregion

    #region Properties
    private RadzenDataGrid<RiskAssessment>? assessmentsGrid;
    private IEnumerable<RiskAssessment> assessments = new List<RiskAssessment>();
    private List<RiskAssessment> allAssessments = new List<RiskAssessment>(); // Store all assessments for client-side filtering
    private HashSet<string> riskRegistryOnlyReportCodes = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, AssessmentValidationSnapshot> assessmentValidationStatus = new(StringComparer.OrdinalIgnoreCase);
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

            _logger.LogInformation("Loading risk assessments for listing view");

            var query = new GetAllRiskAssessmentsQuery();
            var result = await _mediator.SendAsync(query, CancellationToken.None);

            var reportsQuery = new GetAllReportsQuery();
            var reportsResult = await _mediator.SendAsync(reportsQuery, CancellationToken.None);
            if (reportsResult.IsSuccess && reportsResult.Value is not null)
            {
                riskRegistryOnlyReportCodes = reportsResult.Value
                    .Where(r => IsRiskRegistryOnlyStatus(r.Status))
                    .Select(r => (r.Code ?? string.Empty).Trim())
                    .Where(code => !string.IsNullOrWhiteSpace(code))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
            }
            else
            {
                riskRegistryOnlyReportCodes.Clear();
            }

            if (result.IsSuccess && result.Value is not null)
            {
                allAssessments = result.Value.ToList(); // Ensure it's a concrete list
                await LoadAssessmentValidationStatusAsync(allAssessments);
                assessments = allAssessments; // Initially show all assessments
                totalCount = allAssessments.Count();
                _logger.LogInformation("Loaded {Count} risk assessments for listing", totalCount);

                // Only show success notification if we have data
                if (totalCount > 0)
                {
                    await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", $"Successfully loaded {totalCount} risk assessments"));
                }

                else
                {
                    await _eventBus.PublishUIEventAsync(UINotificationEvent.Info("Information", "No risk assessments found"));
                }
            }
            else
            {
                // Initialize with empty lists to prevent null reference issues
                allAssessments = new List<RiskAssessment>();
                assessments = allAssessments;
                assessmentValidationStatus.Clear();
                totalCount = 0;
                
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Failed to load risk assessments"));
                _logger.LogError("Failed to load risk assessments: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            // Ensure we always have valid collections even if an error occurs
            allAssessments = new List<RiskAssessment>();
            assessments = allAssessments;
            assessmentValidationStatus.Clear();
            totalCount = 0;
            
            _logger.LogError(ex, "Error loading risk assessments");
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", $"Error loading risk assessments: {ex.Message}"));
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private bool IsRiskRegistryOnlyAssessment(RiskAssessment assessment)
    {
        // Primary source of truth: persisted assessment typing
        if (assessment.AssessmentType == RiskAssessmentType.RiskRegistryOnly)
        {
            // Keep category check explicit so the listing logic stays aligned with model intent.
            return assessment.RiskAssessmentCategory == RiskAssessmentCategory.Technical;
        }

        // Backward-compatibility fallback: older records may only be inferred from report status.
        var reportCode = assessment.ReportCode?.Trim();
        return !string.IsNullOrWhiteSpace(reportCode) && riskRegistryOnlyReportCodes.Contains(reportCode);
    }

    private async Task LoadAssessmentValidationStatusAsync(List<RiskAssessment> riskAssessments)
    {
        try
        {
            var hazardsResult = await _mediator.SendAsync(new GetAllHazardsQuery(), CancellationToken.None);
            var analysisResult = await _mediator.SendAsync(new GetAllRiskAnalysisQuery(), CancellationToken.None);
            var panelsResult = await _mediator.SendAsync(new GetAllScoringPanelsQuery(), CancellationToken.None);
            var mitigationsResult = await _mediator.SendAsync(new GetAllMitigationsQuery(), CancellationToken.None);

            var allHazards = hazardsResult.IsSuccess && hazardsResult.Value is not null
                ? hazardsResult.Value
                : new List<Hazard>();

            var allAnalyses = analysisResult.IsSuccess && analysisResult.Value is not null
                ? analysisResult.Value
                : new List<RiskAnalysis>();

            var allPanels = panelsResult.IsSuccess && panelsResult.Value is not null
                ? panelsResult.Value
                : new List<ScoringPanel>();

            var allMitigations = mitigationsResult.IsSuccess && mitigationsResult.Value is not null
                ? mitigationsResult.Value
                : new List<Mitigation>();

            var map = new Dictionary<string, AssessmentValidationSnapshot>(StringComparer.OrdinalIgnoreCase);

            foreach (var assessment in riskAssessments)
            {
                var code = assessment.Code?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(code))
                {
                    continue;
                }

                if (IsRiskRegistryOnlyAssessment(assessment))
                {
                    var rrOnlyScored = IsRiskRegistryOnlyScored(assessment, allHazards, allPanels);
                    map[code] = new AssessmentValidationSnapshot
                    {
                        RequiredSteps = 1,
                        ValidatedSteps = rrOnlyScored ? 1 : 0,
                        ProgressPercent = rrOnlyScored ? 100 : 0,
                        Summary = rrOnlyScored
                            ? "Risk Registry Only: hazard scored (initial + residual)"
                            : "Risk Registry Only: hazard scoring incomplete"
                    };

                    continue;
                }

                var requiredSteps = 5;

                var step1Valid = IsStep1Valid(assessment);
                var step2Valid = IsStep2Valid(assessment);
                var step3Valid = IsStep3Valid(assessment, allAnalyses);
                var step4Valid = IsStep4Valid(assessment, allPanels);
                var step5Valid = requiredSteps == 5 && IsStep5Valid(assessment, allAnalyses, allMitigations);

                var validatedSteps = 0;
                if (step1Valid) validatedSteps++;
                if (step2Valid) validatedSteps++;
                if (step3Valid) validatedSteps++;
                if (step4Valid) validatedSteps++;
                if (requiredSteps == 5 && step5Valid) validatedSteps++;

                var progressPercent = requiredSteps > 0
                    ? (int)Math.Round((double)validatedSteps / requiredSteps * 100)
                    : 0;

                map[code] = new AssessmentValidationSnapshot
                {
                    RequiredSteps = requiredSteps,
                    ValidatedSteps = validatedSteps,
                    ProgressPercent = progressPercent,
                    Summary = $"Validated {validatedSteps}/{requiredSteps} steps"
                };
            }

            assessmentValidationStatus = map;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to compute detailed validation status; using fallback progress indicators");
            assessmentValidationStatus.Clear();
        }
    }

    private static bool IsRiskRegistryOnlyScored(RiskAssessment assessment, IEnumerable<Hazard> allHazards, IEnumerable<ScoringPanel> allPanels)
    {
        var assessmentCode = assessment.Code?.Trim();
        var hazardCode = assessment.HazardCode?.Trim();

        if (string.IsNullOrWhiteSpace(hazardCode))
        {
            hazardCode = assessment.PrimaryHazardId?.Trim();
        }

        if (string.IsNullOrWhiteSpace(hazardCode))
        {
            return false;
        }

        var primaryHazard = allHazards.FirstOrDefault(h =>
            string.Equals(h.Code?.Trim(), hazardCode, StringComparison.OrdinalIgnoreCase));

        if (primaryHazard is null)
        {
            return false;
        }

        var hazardScored = primaryHazard.InitialAverageScore.HasValue
            && primaryHazard.InitialAverageScore.Value > 0;

        if (hazardScored)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(assessmentCode))
        {
            return false;
        }

        return allPanels.Any(p =>
            string.Equals(p.RiskAssessmentCode?.Trim(), assessmentCode, StringComparison.OrdinalIgnoreCase)
            && p.InitialLikelihood.HasValue && p.InitialLikelihood.Value > 0
            && p.InitialSeverity.HasValue && p.InitialSeverity.Value > 0
            && p.InitialScore.HasValue && p.InitialScore.Value > 0);
    }

    private AssessmentValidationSnapshot GetValidationSnapshot(RiskAssessment assessment)
    {
        var code = assessment.Code?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(code) && assessmentValidationStatus.TryGetValue(code, out var snapshot))
        {
            return snapshot;
        }

        var isRiskRegistryOnly = IsRiskRegistryOnlyAssessment(assessment);
        var required = isRiskRegistryOnly ? 1 : 5;
        var fallbackValidated = isRiskRegistryOnly
            ? ((assessment.Status == RiskAssessmentStatus.AssessmentComplete || assessment.Stage == RiskAssessmentStage.Completed) ? 1 : 0)
            : Math.Clamp(assessment.CurrentStep, 0, required);
        var fallbackPercent = required > 0 ? (int)Math.Round((double)fallbackValidated / required * 100) : 0;

        return new AssessmentValidationSnapshot
        {
            RequiredSteps = required,
            ValidatedSteps = fallbackValidated,
            ProgressPercent = fallbackPercent,
            Summary = $"Estimated {fallbackValidated}/{required} (fallback)"
        };
    }

    private static bool IsStep1Valid(RiskAssessment assessment)
    {
        var step1Fields = new[]
        {
            assessment.LeadAssessorId,
            assessment.SystemDescription,
            assessment.SystemBoundaries,
            assessment.SystemPurpose,
            assessment.FiveMPersonnel,
            assessment.FiveMEquipment,
            assessment.FiveMProcedures,
            assessment.FiveMResources,
            assessment.FiveMPhysicalEnvironment,
            assessment.FiveMOperationalEnvironment
        };

        var completeCount = step1Fields.Count(v => !string.IsNullOrWhiteSpace(v) && v.Trim().Length >= 10);
        return completeCount >= 4;
    }

    private static bool IsStep2Valid(RiskAssessment assessment)
    {
        return assessment.IdentifiedHazardIds.Any(id => !string.IsNullOrWhiteSpace(id))
            || !string.IsNullOrWhiteSpace(assessment.HazardCode);
    }

    private static bool IsStep3Valid(RiskAssessment assessment, IEnumerable<RiskAnalysis> allAnalyses)
    {
        var code = assessment.Code?.Trim();
        if (string.IsNullOrWhiteSpace(code)) return false;

        return allAnalyses.Any(a =>
            string.Equals(a.RiskAssessmentCode?.Trim(), code, StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(a.InitialWorstCredibleOutcome)
            && a.InitialWorstCredibleOutcome.Trim().Length >= 10
            && !string.IsNullOrWhiteSpace(a.InitialRootCause)
            && a.InitialRootCause.Trim().Length >= 10
            && !string.IsNullOrWhiteSpace(a.InitialAdditionalComments)
            && a.InitialAdditionalComments.Trim().Length >= 10);
    }

    private static bool IsStep4Valid(RiskAssessment assessment, IEnumerable<ScoringPanel> allPanels)
    {
        var code = assessment.Code?.Trim();
        if (string.IsNullOrWhiteSpace(code)) return false;

        return allPanels.Any(p =>
            string.Equals(p.RiskAssessmentCode?.Trim(), code, StringComparison.OrdinalIgnoreCase)
            && p.InitialLikelihood.HasValue && p.InitialLikelihood.Value > 0
            && p.InitialSeverity.HasValue && p.InitialSeverity.Value > 0
            && p.InitialScore.HasValue && p.InitialScore.Value > 0);
    }

    private static bool IsStep5Valid(RiskAssessment assessment, IEnumerable<RiskAnalysis> allAnalyses, IEnumerable<Mitigation> allMitigations)
    {
        var code = assessment.Code?.Trim();
        if (string.IsNullOrWhiteSpace(code)) return false;

        var hasResidualAnalysis = allAnalyses.Any(a =>
            string.Equals(a.RiskAssessmentCode?.Trim(), code, StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(a.ResidualWorstCredibleOutcome)
            && a.ResidualWorstCredibleOutcome.Trim().Length >= 10
            && !string.IsNullOrWhiteSpace(a.ResidualRootCause)
            && a.ResidualRootCause.Trim().Length >= 10
            && !string.IsNullOrWhiteSpace(a.ResidualAdditionalComments)
            && a.ResidualAdditionalComments.Trim().Length >= 10);

        var hasMitigations = allMitigations.Any(m =>
            string.Equals(m.RiskAssessmentCode?.Trim(), code, StringComparison.OrdinalIgnoreCase));

        return hasResidualAnalysis || hasMitigations;
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

    private async Task LoadData(LoadDataArgs args)
    {
        try
        {
            isLoading = true;
            StateHasChanged();

            _logger.LogInformation("LoadData called with Skip: {Skip}, Top: {Top}, OrderBy: {OrderBy}, Filter: {Filter}", 
                args.Skip, args.Top, args.OrderBy, args.Filter);

            // If we don't have all assessments yet, load them first
            if (allAssessments is null || !allAssessments.Any())
            {
                _logger.LogInformation("No assessments cached, loading initial data");
                await LoadInitialData();
                return;
            }

            // Start with all assessments
            var query = allAssessments.AsQueryable();
            _logger.LogInformation("Starting with {Count} total assessments", query.Count());

            // Apply filtering
            if (!string.IsNullOrEmpty(args.Filter))
            {
                _logger.LogInformation("Applying filter: {Filter}", args.Filter);
                query = ApplyFiltering(query, args);
                _logger.LogInformation("After filtering: {Count} assessments", query.Count());
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
                query = query.OrderByDescending(a => a.CreatedDate ?? DateTime.MinValue);
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

            assessments = query.ToList();

            _logger.LogInformation("Applied filtering/sorting/paging. Showing {Count} of {Total} assessments", 
                assessments.Count(), totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in LoadData with args: Skip={Skip}, Top={Top}, OrderBy={OrderBy}, Filter={Filter}", 
                args.Skip, args.Top, args.OrderBy, args.Filter);
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", $"Error loading data: {ex.Message}"));
            
            // Fallback to show all data without filtering/sorting
            try
            {
                assessments = allAssessments ?? new List<RiskAssessment>();
                totalCount = assessments.Count();
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "Error in LoadData fallback");
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
            _logger.LogInformation("ApplyFiltering called with Filter: {Filter}, Filters count: {FilterCount}", 
                args.Filter, args.Filters?.Count() ?? 0);

            // Handle simple string filter (when user types in the general filter)
            if (!string.IsNullOrEmpty(args.Filter) && !args.Filter.Contains("("))
            {
                var filterValue = args.Filter.ToLower();
                _logger.LogInformation("Applying simple string filter: {FilterValue}", filterValue);
                
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

            _logger.LogInformation("Applying sorting: Property={PropertyName}, Descending={IsDescending}", propertyName, isDescending);

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
            _logger.LogError(ex, "Error applying sorting for OrderBy: {OrderBy}", orderBy);
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
            _logger.LogInformation("Viewing risk assessment: {Code}", assessment.Code);
            SelectedAssessment = assessment;
            ShowViewDialog = true;
            StateHasChanged();
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Info("Information", $"Viewing details for assessment {assessment.Code}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error viewing risk assessment {Code}", assessment.Code);
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Error viewing risk assessment"));
        }
    }

    private async Task EditAssessment(RiskAssessment assessment)
    {
        try
        {
            _logger.LogInformation("Editing risk assessment: {Code}", assessment.Code);
            await NavigateToTechnicalAssessment(assessment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error editing risk assessment {Code}", assessment.Code);
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Error opening risk assessment editor"));
        }
    }

    private async Task OnEditAssessmentFromDialog()
    {
        if (SelectedAssessment is null)
        {
            return;
        }

        ShowViewDialog = false;
        await EditAssessment(SelectedAssessment);
    }
    
    private async Task NavigateToTechnicalAssessment(RiskAssessment assessment)
    {
        try
        {
            // ? PROPER WAY: Get ReportCode via HazardCode using CQRS
            var reportCode = await GetReportCodeFromAssessmentAsync(assessment);

            if (string.IsNullOrEmpty(reportCode))
            {
                _logger.LogWarning("Could not determine ReportCode for assessment {AssessmentCode}", assessment.Code);
                await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Could not determine report code for this assessment"));
                return;
            }

            var isRiskRegistryOnly = IsRiskRegistryOnlyAssessment(assessment);
            var targetStep = ResolveAssessmentEditStep(assessment, isRiskRegistryOnly);

            var navigationUrl = $"/SMSRiskManagement/TechnicalAssessment/{reportCode}/{assessment.HazardCode}/{targetStep}?returnTo=risk-assessment-listing";

            _logger.LogInformation("Navigating to Technical Assessment: {Url}", navigationUrl);
            _navigation.NavigateToSecure(navigationUrl);
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Info("Information", $"Opening technical assessment for {assessment.Code}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to Technical Assessment for {AssessmentCode}", assessment.Code);
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Failed to navigate to Technical Assessment"));
        }
    }

    private static int ResolveAssessmentEditStep(RiskAssessment assessment, bool isRiskRegistryOnly)
    {
        if (isRiskRegistryOnly)
        {
            return 4;
        }

        if (assessment.Status == RiskAssessmentStatus.AssessmentComplete)
        {
            return 1;
        }

        return Math.Clamp(assessment.CurrentStep, 1, 5);
    }

    private async Task<string?> GetReportCodeFromAssessmentAsync(RiskAssessment assessment)
    {
        try
        {
            if (string.IsNullOrEmpty(assessment.HazardCode))
            {
                _logger.LogWarning("Assessment {AssessmentCode} has no HazardCode", assessment.Code);
                return null;
            }

            _logger.LogInformation("Getting ReportCode via HazardCode {HazardCode} from assessment {AssessmentCode}",
                assessment.HazardCode, assessment.Code);

            var hazardQuery = new GetHazardByCodeQuery(new HazardID(assessment.HazardCode));
            var hazardResult = await _mediator.SendAsync(hazardQuery, CancellationToken.None);

            if (hazardResult.IsSuccess && hazardResult.Value is not null)
            {
                var reportCode = hazardResult.Value.ReportCode;
                _logger.LogInformation("Found ReportCode {ReportCode} for HazardCode {HazardCode}",
                    reportCode, assessment.HazardCode);
                return reportCode;
            }

            _logger.LogWarning("Failed to load Hazard {HazardCode}: {Error}",
                assessment.HazardCode, hazardResult.Error?.Message ?? "Unknown error");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ReportCode from assessment {AssessmentCode} via HazardCode {HazardCode}",
                assessment.Code, assessment.HazardCode);
            return null;
        }
    }

    private async Task DeleteAssessment(RiskAssessment assessment)
    {
        try
        {
            var confirmResult = await _dialogService.Confirm(
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
                _logger.LogInformation("Deleting risk assessment: {Code}", assessment.Code);

                var deleteCommand = new DeleteRiskAssessmentCommand(new RiskAssessmentID(assessment.Id.Value));
                var result = await _mediator.SendAsync(deleteCommand, CancellationToken.None);

                if (result.IsSuccess)
                {
                    await _eventBus.PublishUIEventAsync(UINotificationEvent.Success("Success", $"Risk assessment '{assessment.Name}' deleted successfully"));
                    _logger.LogInformation("Successfully deleted risk assessment: {Code}", assessment.Code);

                    // Refresh the data grid by reloading initial data
                    await LoadInitialData();
                    StateHasChanged();
                }
                else
                {
                    await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", $"Failed to delete risk assessment: {result.Error?.Message}"));
                    _logger.LogError("Failed to delete risk assessment {Code}: {Error}", assessment.Code, result.Error?.Message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting risk assessment {Code}", assessment.Code);
            await _eventBus.PublishUIEventAsync(UINotificationEvent.Error("Error", "Error deleting risk assessment"));
        }
    }
    #endregion
}

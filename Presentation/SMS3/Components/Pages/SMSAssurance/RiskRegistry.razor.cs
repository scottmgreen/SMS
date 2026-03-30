using System.Data.Common;


using Radzen;

using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;

using SMS_Domain.Enums;

using SMS3.Components.Shared;
using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.SMSAssurance;

/// <summary>
/// Risk Registry - Comprehensive overview of all identified risks, their assessments, and mitigation status
/// Part of SMS Assurance module for tracking and monitoring organizational risk exposure
/// </summary>
public partial class RiskRegistry : ComponentBase
{
    #region Services and Parameters

    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<RiskRegistry> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    [Inject] private INotificationHelper NotificationHelper { get; set; } = default!;

    #endregion

    #region State Properties

    private bool IsLoading { get; set; } = true;
    private RadzenDataGrid<RiskRegistryEntry>? RiskRegistryGrid;

    #endregion

    #region Data Properties

    public List<RiskRegistryEntry> RiskRegistryEntries { get; set; } = new();
    public List<RiskRegistryEntry> FilteredRiskRegistryEntries { get; set; } = new();

    #endregion

    #region Filter Properties

    private string? SelectedStatusFilter { get; set; }
    private string? SelectedRiskLevelFilter { get; set; }
    private string SearchText { get; set; } = string.Empty;

    private List<FilterOption> StatusFilterOptions { get; set; } = new();
    private List<FilterOption> RiskLevelFilterOptions { get; set; } = new();

    private string BasicTextStyle = "font-size:smaller;font-weight: 600";
    private void GetStatusFilters()
    {
        StatusFilterOptions = MitigationStatus.GetAllValues()
            .Select(hc => new FilterOption(hc.Value, hc.Name))
            .ToList();
    }
    private void GetRiskLevelFilters()
    {
        RiskLevelFilterOptions = RiskLevel.GetAllValues()
            .Select(hc => new FilterOption(hc.Value, hc.Name))
            .ToList();
    }

    
    

    #endregion

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        Logger.LogInformation("🚀 Risk Registry: Initializing page...");
        try
        {
            GetRiskLevelFilters();
            GetStatusFilters();
            await LoadRiskRegistryData();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Risk Registry: Exception during initialization");
        }
        Logger.LogInformation("🏁 Risk Registry: Initialization complete");
    }

    #endregion

    #region Data Loading Methods

    /// <summary>
    /// Load all risk registry data by combining hazards, risk assessments, and mitigations
    /// </summary>
    private async Task LoadRiskRegistryData()
    {
        try
        {
            IsLoading = true;
            StateHasChanged();

            Logger.LogInformation("🔍 Risk Registry: Starting data load...");

            // Load all required data in parallel
            var reportsTask = LoadAllReportsAsync();
            var hazardsTask = LoadAllHazardsAsync();
            var assessmentsTask = LoadAllRiskAssessmentsAsync();
            var mitigationsTask = LoadAllMitigationsAsync();

            await Task.WhenAll(hazardsTask, assessmentsTask, mitigationsTask);

            var reports = await reportsTask;
            var hazards = await hazardsTask;
            var assessments = await assessmentsTask;
            var mitigations = await mitigationsTask;

            Logger.LogInformation("📊 Risk Registry: Data loaded - Hazards: {HazardCount}, Assessments: {AssessmentCount}, Mitigations: {MitigationCount}",
                hazards.Count, assessments.Count, mitigations.Count);

            // Log some sample data for debugging
            if (hazards.Any())
            {
                var sampleHazard = hazards.First();
                Logger.LogInformation("📝 Sample Hazard: Code={Code}, Description={Description}, ReportCode={ReportCode}, RiskLevel={RiskLevel}, InitialRiskMatrixCode={MatrixCode}",
                    sampleHazard.Code, sampleHazard.Description, sampleHazard.ReportCode, sampleHazard.HazardRiskLevel, sampleHazard.InitialRiskMatrixCode);
            }

            // Build risk registry entries
            RiskRegistryEntries = BuildRiskRegistryEntries(reports,hazards, assessments, mitigations);

            Logger.LogInformation("🎯 Risk Registry: Built {EntryCount} registry entries", RiskRegistryEntries.Count);

            // Apply current filters
            ApplyFilters();

            Logger.LogInformation("✅ Risk Registry: After filtering - {FilteredCount} entries visible", FilteredRiskRegistryEntries.Count);

            // Force UI refresh
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Error loading Risk Registry data");
            await NotificationHelper.ShowErrorAsync("Error loading Risk Registry data");

        }
        finally
        {
            IsLoading = false;
            // Force UI refresh twice to ensure Blazor picks up the changes
            StateHasChanged();
            await Task.Delay(50); // Small delay to ensure state propagation
            StateHasChanged();
        }
    }

    /// <summary>
    /// Load all reports from the system
    /// </summary>
    private async Task<List<Report>> LoadAllReportsAsync()
    {
        try
        {
            Logger.LogInformation("🔍 Loading all hazards...");
            var query = new GetAllReportsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                var reports = result.Value.ToList();
                Logger.LogInformation("✅ Loaded {Count} reports successfully", reports.Count);
                return reports;
            }
            else
            {
                Logger.LogWarning("⚠️ GetAllReportsQuery failed or returned null. IsSuccess: {IsSuccess}, Error: {Error}",
                    result.IsSuccess, result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Exception loading hazards");
        }

        return new List<Report>();
    }

    /// <summary>
    /// Load all hazards from the system
    /// </summary>
    private async Task<List<Hazard>> LoadAllHazardsAsync()
    {
        try
        {
            Logger.LogInformation("🔍 Loading all hazards...");
            var query = new GetAllHazardsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                var hazards = result.Value.ToList();
                Logger.LogInformation("✅ Loaded {Count} hazards successfully", hazards.Count);
                return hazards;
            }
            else
            {
                Logger.LogWarning("⚠️ GetAllHazardsQuery failed or returned null. IsSuccess: {IsSuccess}, Error: {Error}",
                    result.IsSuccess, result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Exception loading hazards");
        }

        return new List<Hazard>();
    }

    /// <summary>
    /// Load all risk assessments from the system
    /// </summary>
    private async Task<List<RiskAssessment>> LoadAllRiskAssessmentsAsync()
    {
        try
        {
            Logger.LogInformation("🔍 Loading all risk assessments...");
            var query = new GetAllRiskAssessmentsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                var assessments = result.Value.ToList();
                Logger.LogInformation("✅ Loaded {Count} risk assessments successfully", assessments.Count);
                return assessments;
            }
            else
            {
                Logger.LogWarning("⚠️ GetAllRiskAssessmentsQuery failed or returned null. IsSuccess: {IsSuccess}, Error: {Error}",
                    result.IsSuccess, result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Exception loading risk assessments");
        }

        return new List<RiskAssessment>();
    }

    /// <summary>
    /// Load all mitigations from the system
    /// </summary>
    private async Task<List<Mitigation>> LoadAllMitigationsAsync()
    {
        try
        {
            Logger.LogInformation("🔍 Loading all mitigations...");
            var query = new GetAllMitigationsQuery();
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {
                var mitigations = result.Value.ToList();
                Logger.LogInformation("✅ Loaded {Count} mitigations successfully", mitigations.Count);
                return mitigations;
            }
            else
            {
                Logger.LogWarning("⚠️ GetAllMitigationsQuery failed or returned null. IsSuccess: {IsSuccess}, Error: {Error}",
                    result.IsSuccess, result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "❌ Exception loading mitigations");
        }

        return new List<Mitigation>();
    }

    #endregion

    #region Data Processing Methods

    /// <summary>
    /// Build risk registry entries by correlating hazards, assessments, and mitigations
    /// </summary>
    private List<RiskRegistryEntry> BuildRiskRegistryEntries(List<Report> reports,List<Hazard> hazards,List<RiskAssessment> assessments,List<Mitigation> mitigations)
    {
        
        var entries = new List<RiskRegistryEntry>();

        foreach (var hazard in hazards)
        {
            try
            {
                Logger.LogDebug("🔍 Processing hazard: {HazardCode} - {Description}", hazard.Code, hazard.Description);

                // Find associated risk assessment

                var report = reports.Where(r => r.Code.Trim() == hazard.ReportCode.Trim()).FirstOrDefault();
                
                
                
                var assessment = assessments.FirstOrDefault(a =>
                    a.HazardCode == hazard.Code ||
                    a.IdentifiedHazardIds?.Contains(hazard.Code) == true);

                if (assessment != null)
                {
                    Logger.LogDebug("✅ Found assessment {AssessmentCode} for hazard {HazardCode}", assessment.Code, hazard.Code);
                }
                else
                {
                    Logger.LogDebug("⚠️ No assessment found for hazard {HazardCode}", hazard.Code);
                }

                // Find associated mitigations for this hazard
                var hazardMitigations = mitigations.Where(m => m.HazardCode.Trim() == hazard.Code.Trim()).ToList();

                Logger.LogDebug("🛡️ Found {MitigationCount} mitigations for hazard {HazardCode}", hazardMitigations.Count, hazard.Code);

                if (hazardMitigations.Any())
                {
                    // Create one entry per mitigation
                    foreach (var mitigation in hazardMitigations)
                    {
                        var entry = CreateRiskRegistryEntry(report,hazard, assessment, mitigation);
                        entries.Add(entry);
                        Logger.LogDebug("➕ Added entry for hazard {HazardCode} with mitigation {MitigationCode}", hazard.Code, mitigation.Code);
                    }
                }
                else
                {
                    // Create entry without mitigation
                    var entry = CreateRiskRegistryEntry(report,hazard, assessment, null);
                    entries.Add(entry);
                    Logger.LogDebug("➕ Added entry for hazard {HazardCode} without mitigation", hazard.Code);
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "❌ Error processing hazard {HazardCode} for risk registry", hazard.Code);
            }
        }

        Logger.LogInformation("🏁 Built {TotalEntries} total registry entries", entries.Count);
        return entries.OrderByDescending(e => e.LastUpdated).ToList();
    }

    /// <summary>
    /// Create a single risk registry entry from hazard, assessment, and mitigation data
    /// </summary>
    private RiskRegistryEntry CreateRiskRegistryEntry(Report report, Hazard hazard, RiskAssessment? assessment, Mitigation? mitigation)
    {

        RiskLevel initialRiskLevel = null; // Default fallback
        string initialMatrixCode = string.Empty; // Default fallback
        RiskLevel residualRiskLevel = null;  // Default fallback
        string residualMatrixCode = string.Empty; // Default fallback

        // Parse matrix codes with null checks
        if (string.IsNullOrEmpty(hazard.InitialRiskMatrixCode) || hazard.HazardRiskLevel == RiskLevel.Unkonwn)
        {
            initialRiskLevel = RiskLevel.Unkonwn;
            initialMatrixCode = "TBD";
        }
        else
        {
            var (initialSeverity, initialLikelihood) = AviationRiskMatrixCalculator.ParseMatrixCode(hazard.InitialRiskMatrixCode.Trim());
            
            if (initialSeverity.HasValue && initialLikelihood.HasValue)
            {
                initialRiskLevel = AviationRiskMatrixCalculator.GetAviationRiskLevel(initialSeverity.Value, initialLikelihood.Value);
                initialMatrixCode = AviationRiskMatrixCalculator.GetMatrixCode(initialSeverity.Value, initialLikelihood.Value);
            }
            else if (!string.IsNullOrEmpty(hazard.InitialRiskMatrixCode))
            {
                initialMatrixCode = hazard.InitialRiskMatrixCode;
            }
        }
        if (string.IsNullOrEmpty(hazard.ResidualRiskMatrixCode))
        {
            residualRiskLevel = RiskLevel.Unkonwn;
            residualMatrixCode = "TBD";
        }
        else 
        {
            var (residualSeverity, residualLikelihood) = AviationRiskMatrixCalculator.ParseMatrixCode(hazard.ResidualRiskMatrixCode.Trim());
            if (residualSeverity.HasValue && residualLikelihood.HasValue)
            {
                residualRiskLevel = AviationRiskMatrixCalculator.GetAviationRiskLevel(residualSeverity.Value, residualLikelihood.Value);
                residualMatrixCode = AviationRiskMatrixCalculator.GetMatrixCode(residualSeverity.Value, residualLikelihood.Value);
            }
            else if (!string.IsNullOrEmpty(hazard.ResidualRiskMatrixCode))
            {
                residualMatrixCode = hazard.ResidualRiskMatrixCode;
            }

        }
        var entry = new RiskRegistryEntry
        {
            ReportStatus = report.Status ?? ReportStatus.Unknown,
            ReportCode = hazard.ReportCode ?? "N/A",
            HazardCode = hazard.Code,
            HazardDescription = hazard.Description ?? "No description available",
            InitialHazardRiskLevel = initialRiskLevel ?? RiskLevel.Unkonwn,
            InitialRiskMatrixCode  = initialMatrixCode,
            ResidualHazardRiskLevel = residualRiskLevel ?? RiskLevel.Unkonwn,
            ResidualRiskMatrixCode = residualMatrixCode,
            MitigationDescription = mitigation?.Name ?? mitigation?.Description ?? "No mitigation assigned",
            MitigationStatus = mitigation?.Status, // ✅ Can be null now
            TargetDate = mitigation?.TargetDate,
            AssignedTo = mitigation?.AssignedTo ?? assessment?.LeadAssessorId ?? "Unassigned",
            LastUpdated = assessment?.UpdatedDate ?? hazard.UpdatedDate ?? hazard.CreatedDate ?? DateTime.UtcNow,

            // Additional context for navigation and details
            HazardCategory = hazard.HazardCategory,
            HazardType = hazard.HazardType,
            RiskAssessmentCode = assessment?.Code,
            MitigationCode = mitigation?.Code
        };

        Logger.LogDebug("✅ Created entry: {HazardId} -> MitigationStatus: '{Status}'", hazard.Code, entry.MitigationStatus);
        return entry;
    }

    /// <summary>
    /// Extract risk matrix code from hazard data
    /// </summary>
    private string GetHazardRiskMatrixCode(Hazard hazard)
    {
        // Try to get from initial risk assessment first
        //if (!string.IsNullOrEmpty(hazard.InitialRiskMatrixCode) && hazard.InitialRiskMatrixCode != "-")
        //{
        //    return hazard.InitialRiskMatrixCode;
        //}

        // Fall back to residual risk assessment
        if (!string.IsNullOrEmpty(hazard.ResidualRiskMatrixCode) && hazard.ResidualRiskMatrixCode != "-")
        {
            return hazard.ResidualRiskMatrixCode;
        }

        // Return default if no assessment available
        return "-";
    }

    /// <summary>
    /// Extract risk level from hazard data
    /// </summary>
    private string GetHazardRiskLevel(Hazard hazard)
    {
        if (!string.IsNullOrEmpty(hazard.HazardRiskLevel) && hazard.HazardRiskLevel != RiskLevel.Unkonwn)
        {
            return hazard.HazardRiskLevel;
        }

        return "TBD";
    }

    #endregion

    #region Filter and Search Methods

    /// <summary>
    /// Handle filter dropdown changes
    /// </summary>
    private async Task OnFilterChanged()
    {
        ApplyFilters();
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Handle search text input changes
    /// </summary>
    private async Task OnSearchChanged(ChangeEventArgs args)
    {
        SearchText = args.Value?.ToString() ?? string.Empty;
        ApplyFilters();
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Apply all current filters to the data
    /// </summary>
    /// <summary>
    /// Apply all current filters to the data
    /// </summary>
    private void ApplyFilters()
    {
        Logger.LogInformation("🔍 Applying filters - SelectedStatusFilter: {StatusFilter}, SelectedRiskLevelFilter: {RiskFilter}, SearchText: {SearchText}",
            SelectedStatusFilter, SelectedRiskLevelFilter, SearchText);

        var filtered = RiskRegistryEntries.AsEnumerable();

        Logger.LogInformation("📊 Starting with {Count} total entries", RiskRegistryEntries.Count);

        // Apply status filter - ✅ FIXED: Add null checking
        if (!string.IsNullOrEmpty(SelectedStatusFilter))
        {
            filtered = filtered.Where(e => e.MitigationStatus?.Value == SelectedStatusFilter);
            Logger.LogInformation("🔽 After status filter: {Count} entries", filtered.Count());
        }

        // Apply risk level filter - ✅ FIXED: Add null checking
        if (!string.IsNullOrEmpty(SelectedRiskLevelFilter))
        {
            filtered = filtered.Where(e => e.ResidualHazardRiskLevel?.Value == SelectedRiskLevelFilter);
            Logger.LogInformation("🔽 After risk level filter: {Count} entries", filtered.Count());
        }

        // Apply search filter
        if (!string.IsNullOrEmpty(SearchText))
        {
            var searchLower = SearchText.ToLowerInvariant();
            filtered = filtered.Where(e =>
                e.ReportCode.ToLowerInvariant().Contains(searchLower) ||
                e.HazardCode.ToLowerInvariant().Contains(searchLower) ||
                (e.HazardDescription?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                (e.MitigationDescription?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                (e.AssignedTo?.ToLowerInvariant().Contains(searchLower) ?? false));
            Logger.LogInformation("🔽 After search filter: {Count} entries", filtered.Count());
        }

        FilteredRiskRegistryEntries = filtered.ToList();
        Logger.LogInformation("✅ Final filtered entries: {Count}", FilteredRiskRegistryEntries.Count);

        
    }

    #endregion

    #region UI Helper Methods

    
    /// <summary>
    /// Get aviation matrix cell style with background color (SAME AS STEP 4)
    /// </summary>
    private string GetAviationMatrixCellStyle(string matrixCode)
    {
        if (string.IsNullOrEmpty(matrixCode) || matrixCode == "-")
        {
            return "background: #f8f9fa; color: #6c757d;";
        }

        var (severity, likelihood) = AviationRiskMatrixCalculator.ParseMatrixCode(matrixCode.Trim());

        if (severity.HasValue && likelihood.HasValue)
        {
            return AviationRiskMatrixCalculator.GetMatrixCellStyle(severity.Value, likelihood.Value);
        }

        return "background: #f8f9fa; color: #6c757d;";
    }

    #endregion

    #region Statistics Methods

    
    

    #endregion

  
    #region Data Models

    /// <summary>
    /// Risk Registry Entry model for grid display
    /// </summary>
    public class RiskRegistryEntry
    {
        public string ReportCode { get; set; } = string.Empty;

        public string HazardCode { get; set; } = string.Empty;
        public string HazardDescription { get; set; } = string.Empty;
        public string InitialRiskMatrixCode { get; set; } = string.Empty;

        public string? ResidualRiskMatrixCode { get; set; } = string.Empty;
        public RiskLevel? InitialHazardRiskLevel { get; set; }

        public RiskLevel? ResidualHazardRiskLevel { get; set; }
        public string? MitigationDescription { get; set; }

        public string ReportStatus { get; set; } 
        public MitigationStatus? MitigationStatus { get; set; } 
        public DateTime? TargetDate { get; set; }
        public string? AssignedTo { get; set; }
        public DateTime? LastUpdated { get; set; }

        // Additional properties for context
        public string? HazardCategory { get; set; }
        public string? HazardType { get; set; }
        public string? RiskAssessmentCode { get; set; }
        public string? MitigationCode { get; set; }
    }

    /// <summary>
    /// Filter option for dropdowns
    /// </summary>
    

    #endregion

    #region Bootstrap Helper Methods

    /// <summary>
    /// Get Bootstrap color class for mitigation status
    /// </summary>
    //private string GetBootstrapMitigationStatusColor(MitigationStatus? status)
    //{
    //    if (status == null)
    //        return "secondary";

    //    return status switch
    //    {
    //        _ when status == MitigationStatus.PendingApproval => "warning",
    //        _ when status == MitigationStatus.Approved => "success",
    //        _ when status == MitigationStatus.InProgressDueDate => "primary",
    //        _ when status == MitigationStatus.Complete => "success",
    //        _ when status == MitigationStatus.MonitoringHazard => "success",
    //        _ when status == MitigationStatus.Rejected => "danger",
    //        _ => "secondary"
    //    };
    //}
    /// <summary>
    /// Get Bootstrap color class for risk level using RiskLevel enum
    /// </summary>
    private string GetBootstrapRiskLevelColor(RiskLevel? riskLevel)
    {
        return riskLevel?.BootstrapClass ?? "secondary";
    }

    /// <summary>
    /// Get CSS style for risk level using RiskLevel enum
    /// </summary>
    private string GetRiskLevelStyle(RiskLevel? riskLevel)
    {
        return riskLevel?.GetCssStyle() ?? "background: #6c757d; color: #ffffff;";
    }
    #endregion
}
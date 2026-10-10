using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using SMS3.Components.Pages.SMSRiskManagement;
using SMS3.Components.Pages.SMSRiskManagement.Components;
using SMS3.Components.Pages.SMSRiskManagement.Models;
using SMS3.Components.Shared;
using SMS_Application.Interfaces;
using SMS_Application.Queries;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
namespace SMS3.Components.Pages.SMSRiskManagement.Components;

public partial class TechnicalAssessmentStep4
{

[Parameter] public Step4Model Step4 { get; set; } = default!;
    [Parameter] public EventCallback<Step4Model> Step4Changed { get; set; }
    [Parameter] public List<Hazard> Step4Hazards { get; set; } = new();
    [Parameter] public List<SMSStakeholderUser> AvailableStakeholders { get; set; } = new();
    [Parameter] public List<SMSApplicationUser> AvailableAssessors { get; set; } = new();
    [Parameter] public RiskAssessment? CurrentRiskAssessment { get; set; }
    [Parameter] public RiskAssessmentID? RiskAssessmentId { get; set; }
    [Parameter] public int CurrentStep { get; set; } = 4;

    [Inject] private IBaseMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<TechnicalAssessmentStep4> Logger { get; set; } = default!;

    private bool _isInitialized = false;
    private bool _isRefreshing = false;
    private string _lastHazardListHash = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        if (!_isInitialized)
        {
            await InitializeStep4Data();
            _isInitialized = true;
        }
    }

    private bool IsAdditionalCommentsInvalid()
    {
        return string.IsNullOrWhiteSpace(Step4?.AdditionalComments)
            || Step4.AdditionalComments.Trim().Length < 10;
    }

    protected override async Task OnParametersSetAsync()
    {
        var currentHazardListHash = GetHazardListHash();
        if (currentHazardListHash != _lastHazardListHash && !_isRefreshing)
        {
            _lastHazardListHash = currentHazardListHash;
            await InitializeStep4Data();
        }
    }

    private string GetHazardListHash()
    {
        if (Step4Hazards?.Any() != true) return "empty";
        var codes = string.Join(",", Step4Hazards.Select(h => h.Code).OrderBy(c => c));
        return $"{Step4Hazards.Count}:{codes.GetHashCode()}";
    }

    private async Task InitializeStep4Data()
    {
        if (_isRefreshing) return;
        
        try
        {
            _isRefreshing = true;
            await LoadStep4DataFromHazardEntities();
            Logger.LogInformation("Initialized Step 4 data for {Count} hazards", Step4Hazards?.Count ?? 0);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error initializing Step 4 data");
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    /// <summary>
    /// ✅ REFACTORED: Simplified method that reads from updated hazard entities instead of duplicating scoring panel logic
    /// HazardScoringPanel component handles all complex scoring calculations and updates hazard entities
    /// This method just reads the calculated values from the hazard entities
    /// </summary>
    private async Task HandleHazardScored(string hazardCode)
    {
        try
        {
            Logger.LogInformation("Handling hazard scored: {HazardCode}", hazardCode);
            
            // ✅ Let HazardScoringPanel handle all the complex logic
            // We just need to update our Step4 model from the updated hazard entity
            await UpdateStep4FromHazardEntity(hazardCode);
            
            await SaveCurrentRiskAssessmentAsync();
            
            if (Step4Changed.HasDelegate)
            {
                await Step4Changed.InvokeAsync(Step4);
            }
            
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling hazard scored for {HazardCode}", hazardCode);
        }
    }

    private async Task OnAdditionalCommentsChanged(ChangeEventArgs args)
    {
        Step4.AdditionalComments = args.Value?.ToString() ?? string.Empty;

        if (Step4Changed.HasDelegate)
        {
            await Step4Changed.InvokeAsync(Step4);
        }
    }

    /// <summary>
    /// ✅ NEW: Update Step4 model from hazard entity that was already updated by HazardScoringPanel
    /// This eliminates the need to duplicate the complex scoring calculation logic
    /// </summary>
    private async Task UpdateStep4FromHazardEntity(string hazardCode)
    {
        try
        {
            // Load the updated hazard entity to get the calculated values
            var hazardQuery = new GetHazardByCodeQuery(new HazardID(hazardCode));
            var hazardResult = await Mediator.SendAsync(hazardQuery, CancellationToken.None);

            if (hazardResult.IsSuccess && hazardResult.Value is not null)
            {
                var hazard = hazardResult.Value;

                // Update Step4 dictionaries from the hazard entity (which HazardScoringPanel already updated)
                if (hazard.InitialAverageScore.HasValue)
                {
                    Step4.HazardAverageScores[hazardCode] = (double)hazard.InitialAverageScore.Value;
                    Step4.HazardRiskLevels[hazardCode] = hazard.HazardRiskLevel ?? RiskLevel.Unkonwn;
                    Step4.HazardMatrixCodes[hazardCode] = hazard.InitialRiskMatrixCode ?? "Unknown";
                   
                }
                else
                {
                    // Remove if no score available
                    Step4.HazardAverageScores.Remove(hazardCode);
                    Step4.HazardRiskLevels.Remove(hazardCode);
                    Step4.HazardMatrixCodes.Remove(hazardCode);
                    
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating Step4 from hazard entity: {HazardCode}", hazardCode);
        }
    }

    /// <summary>
    /// ✅ REFACTORED: Load Step4 data from hazard entities instead of duplicating scoring panel queries
    /// This method initializes Step4 model from existing hazard entity data
    /// </summary>
    private async Task LoadStep4DataFromHazardEntities()
    {
        if (Step4Hazards?.Any() != true) return;

        try
        {
            Step4.HazardAverageScores.Clear();
            Step4.HazardRiskLevels.Clear();
            Step4.HazardMatrixCodes.Clear();

            Logger.LogInformation("Loading Step4 data from hazard entities for {Count} hazards", Step4Hazards.Count);

            foreach (var hazard in Step4Hazards)
            {
                try
                {
                    // Read calculated values directly from hazard entity
                    if (hazard.InitialAverageScore.HasValue)
                    {
                        Step4.HazardAverageScores[hazard.Code] = (double)hazard.InitialAverageScore.Value;
                        Step4.HazardRiskLevels[hazard.Code] = hazard.HazardRiskLevel ?? RiskLevel.Unkonwn;
                        Step4.HazardMatrixCodes[hazard.Code] = hazard.InitialRiskMatrixCode ?? "Unknown";

                        Logger.LogInformation("✅ Loaded Step4 data for hazard {HazardCode} from entity: Score={Score:F2}, Matrix={Matrix}, Risk={Risk}",
                            hazard.Code, hazard.InitialAverageScore.Value, hazard.InitialRiskMatrixCode, hazard.HazardRiskLevel?.Value ?? "Unknown");
                    }
                    else
                    {
                        Logger.LogInformation("⚠️ No initial scoring data available for hazard {HazardCode}", hazard.Code);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error loading data for hazard {HazardCode}", hazard.Code);
                }
            }

            Logger.LogInformation("✅ LoadStep4DataFromHazardEntities completed: {Count} hazards with scores",
                Step4.HazardAverageScores.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading Step4 data from hazard entities");
        }
    }

    /// <summary>
    /// Save the current risk assessment data when hazard scores are updated
    /// </summary>
    private async Task SaveCurrentRiskAssessmentAsync()
    {
        try
        {
            if (Step4Hazards?.Any() != true || !Step4.HazardAverageScores.Any())
            {
                Logger.LogInformation("Skipping risk assessment save - no completed hazard assessments");
                return;
            }

            // Calculate overall final scores from all hazard averages
            var (finalSeverity, finalLikelihood, finalRiskLevel) = CalculateOverallRiskAssessment();

            // Get the risk assessment ID from the current context
            var riskAssessmentId = GetCurrentRiskAssessmentId();
            if (string.IsNullOrEmpty(riskAssessmentId))
            {
                Logger.LogWarning("Cannot save risk assessment - no assessment ID available");
                return;
            }

            // Save Step 4 risk assessment data
            var saveStep4Command = new SaveStep4Command(
                new RiskAssessmentID(riskAssessmentId),
                finalSeverity,
                finalLikelihood,
                finalRiskLevel,
                Step4.AdditionalComments);

            var result = await Mediator.SendAsync(saveStep4Command, CancellationToken.None);

            if (result.IsSuccess)
            {
                Logger.LogInformation("Successfully saved Step 4 risk assessment data after hazard score update");
            }
            else
            {
                Logger.LogError("Failed to save Step 4 risk assessment: {Error}", result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving current risk assessment");
        }
    }

    /// <summary>
    /// Calculate overall risk assessment from all completed hazard assessments
    /// </summary>
    private (int? finalSeverity, int? finalLikelihood, RiskLevel finalRiskLevel) CalculateOverallRiskAssessment()
    {
        var completedHazards = Step4.HazardAverageScores.Keys.ToList();
        
        if (!completedHazards.Any())
        {
            return (null, null, RiskLevel.Unkonwn);
        }

        // Use highest risk approach for final severity/likelihood
        var highestRiskHazard = Step4.HazardAverageScores.OrderByDescending(kvp => kvp.Value).First();
        var highestRiskHazardCode = highestRiskHazard.Key;

        // Get the matrix code for the highest risk hazard
        var highestRiskMatrixCode = Step4.HazardMatrixCodes.GetValueOrDefault(highestRiskHazardCode, "Unknown");

        // Parse matrix code back to severity/likelihood via centralized calculator
        var (severity, likelihood) = AviationRiskMatrixCalculator.ParseMatrixCode(highestRiskMatrixCode);
        
        // Get risk level from Step4 model
        var finalRiskLevel = Step4.HazardRiskLevels.GetValueOrDefault(highestRiskHazardCode, RiskLevel.Unkonwn);

        return (severity, likelihood, finalRiskLevel);
    }

    /// <summary>
    /// Get the current risk assessment ID from the parent context
    /// </summary>
    private string GetCurrentRiskAssessmentId()
    {
        if (RiskAssessmentId is not null && !string.IsNullOrEmpty(RiskAssessmentId.Value))
        {
            return RiskAssessmentId.Value;
        }
        
        // Fallback - this should not happen in normal operation
        Logger.LogWarning("No RiskAssessmentId parameter provided to TechnicalAssessmentStep4");
        return string.Empty;
    }

    private string GetSeverityLabel(int severity)
    {
        return HazardSeverity.GetDisplayName(severity);
    }

    private List<string> GetHazardsAtMatrixCode(int severity, int likelihood)
    {
        var targetMatrixCode = AviationRiskMatrixCalculator.GetMatrixCode(severity, likelihood);
        return Step4Hazards
            .Where(h => Step4.HazardAverageScores.ContainsKey(h.Code) || h.InitialAverageScore.HasValue)
            .Where(h =>
            {
                var matrixCode = Step4.HazardMatrixCodes.GetValueOrDefault(h.Code, h.InitialRiskMatrixCode ?? string.Empty);
                return string.Equals(matrixCode?.Trim(), targetMatrixCode.Trim(), StringComparison.OrdinalIgnoreCase);
            })
            .Select(h => h.Code)
            .OrderBy(h => h)
            .ToList();
    }

        
    private string GetFieldValidation(string? text, int minLength)
    {
        var length = text?.Trim().Length ?? 0;

        if (length == 0)
            return $"0/{minLength} characters";
        else if (length < minLength)
            return $"{length}/{minLength} characters (minimum {minLength} required)";
        else
            return $"{length} characters ✓";
    }
}



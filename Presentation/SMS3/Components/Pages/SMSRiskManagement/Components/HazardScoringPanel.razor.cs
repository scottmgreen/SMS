using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Domain.ValueObjects;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;
using SMS_Application.Interfaces;
using SMS3.Components.Shared;
using SMS3.Components.Shared.UIHelpers;
using Radzen;
using Radzen.Blazor;
using Microsoft.AspNetCore.Components;
using SMS3.Components.Pages.SMSRiskManagement.Models;

namespace SMS3.Components.Pages.SMSRiskManagement.Components;

public partial class HazardScoringPanel : ComponentBase
{
    [Parameter] public Hazard Hazard { get; set; } = new(new HazardID("HZ-0000"));
    [Parameter] public Step4Model? Step4 { get; set; }  // Made nullable to handle null cases
    [Parameter] public Step5Model? Step5 { get; set; } // Made nullable to handle null cases
    [Parameter] public List<SMSStakeholderUser> AvailableStakeholders { get; set; } = new();
    [Parameter] public List<SMSApplicationUser> AvailableAssessors { get; set; } = new();
    [Parameter] public RiskAssessment? CurrentRiskAssessment { get; set; }   // ✅ SINGLE risk assessment parameter
    [Parameter] public int CurrentStep { get; set; }   // Current step (4 or 5)
    [Parameter] public EventCallback OnHazardScored { get; set; }
    
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<HazardScoringPanel> Logger { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;

    [Inject] private ICurrentUserService CurrentUserService { get; set; } = default!;

    private RadzenDataGrid<ScoringPanel>? ScoringGrid;
    private List<ScoringPanel> HazardScoringPanels = new();
    private bool IsSubmitting = false;
    private bool _isDataLoaded = false;
    private string _lastHazardCode = string.Empty;

    // Local properties to track calculated hazard scoring data (for future database update)
    private double? CalculatedAverageScore = null;
    private string CalculatedMatrixCode = string.Empty;
    private RiskLevel CalculatedRiskLevel = null;

    // Dropdown options - Use centralized helpers
    private List<SeverityOption> SeverityOptions => DropdownHelper.GetSeverityOptions();
    private List<LikelihoodOption> LikelihoodOptions => DropdownHelper.GetLikelihoodOptions();

    protected override async Task OnInitializedAsync()
    {
        if (!string.IsNullOrEmpty(Hazard?.Code) && Hazard.Code != "HZ-0000")
        {
            await LoadHazardScoringPanelsInternal();
            _lastHazardCode = Hazard.Code;
            _isDataLoaded = true;
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        // Only reload if hazard actually changed (prevent infinite loops)
        if (!string.IsNullOrEmpty(Hazard?.Code) &&
            Hazard.Code != "HZ-0000" &&
            Hazard.Code != _lastHazardCode &&
            !IsSubmitting)
        {
            await LoadHazardScoringPanelsInternal();
            _lastHazardCode = Hazard.Code;
            _isDataLoaded = true;
        }
        
        // Ensure property mapping is current for existing panels when CurrentStep changes
        foreach (var panel in HazardScoringPanels)
        {
            MapPropertiesBasedOnStep(panel);
        }
    }

    private async Task LoadHazardScoringPanelsInternal()
    {
        if (string.IsNullOrEmpty(Hazard?.Code) || Hazard.Code == "HZ-0000")
        {
            HazardScoringPanels = new List<ScoringPanel>();
            return;
        }

        try
        {
            Logger.LogInformation("Loading scoring panels for hazard {HazardCode}", Hazard.Code);

            var query = new GetScoringPanelsByHazardCodeQuery(Hazard.Code);
            var result = await Mediator.SendAsync(query, CancellationToken.None);

            if (result.IsSuccess && result.Value != null)
            {

                await CopyStep4ScoresToStep5IfNeeded(result.Value);

                // 🎯 CRITICAL FIX: Filter panels by CurrentRiskAssessment context
                var targetRiskAssessmentCode = GetTargetRiskAssessmentCode();

                Logger.LogInformation("Filtering panels by target assessment: {TargetCode}", targetRiskAssessmentCode);

                var filteredPanels = result.Value
                    .Where(p => p.RiskAssessmentCode.Trim() == targetRiskAssessmentCode)
                    .ToList();

                // Map properties based on CurrentStep for all loaded panels
                foreach (var panel in filteredPanels)
                {
                    MapPropertiesBasedOnStep(panel);
                }

                HazardScoringPanels = filteredPanels;

                Logger.LogInformation("Filtered to {Count} scoring panels for hazard {HazardCode} and assessment {AssessmentCode}", filteredPanels.Count, Hazard.Code, targetRiskAssessmentCode);

                // Recalculate scoring data after loading panels
                await RecalculateHazardScoringData();
            }
            else
            {
                HazardScoringPanels = new List<ScoringPanel>();
                Logger.LogInformation("No scoring panels found for hazard {HazardCode}", Hazard.Code);

                // Clear scoring data if no panels
                await RecalculateHazardScoringData();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading scoring panels for hazard {HazardCode}", Hazard.Code);
            HazardScoringPanels = new List<ScoringPanel>();

            // Clear scoring data on error
            await RecalculateHazardScoringData();
        }
    }

    private string GetMemberName(string? userCode)
    {
        var stakeholder = AvailableStakeholders.FirstOrDefault(s => s.Code.Trim() == userCode.Trim());
        return stakeholder?.DisplayName ?? userCode ?? "Unknown";
    }

    /// <summary>
    /// Map the compatibility properties based on CurrentStep
    /// Step 4 = Initial properties, Step 5 = Residual properties
    /// </summary>
    private void MapPropertiesBasedOnStep(ScoringPanel panel)
    {
        if (CurrentStep == 5)
        {
            // Step 5: Map from Residual properties
            panel.Likelihood = panel.ResidualLikelihood;
            panel.Severity = panel.ResidualSeverity;
            panel.Score = panel.ResidualScore;
            panel.Rationale = panel.ResidualRationale;
        }
        else
        {
            // Step 4 (default): Map from Initial properties
            panel.Likelihood = panel.InitialLikelihood;
            panel.Severity = panel.InitialSeverity;
            panel.Score = panel.InitialScore;
            panel.Rationale = panel.InitialRationale;
        }
    }

    /// <summary>
    /// Update the actual entity properties from compatibility properties before saving
    /// </summary>
    private void UpdateEntityPropertiesFromMapped(ScoringPanel panel)
    {
        if (CurrentStep == 5)
        {
            // Step 5: Update Residual properties from compatibility properties
            panel.ResidualLikelihood = panel.Likelihood;
            panel.ResidualSeverity = panel.Severity;
            panel.ResidualScore = panel.Score;
            panel.ResidualRationale = panel.Rationale;
        }
        else
        {
            // Step 4: Update Initial properties from compatibility properties
            panel.InitialLikelihood = panel.Likelihood;
            panel.InitialSeverity = panel.Severity;
            panel.InitialScore = panel.Score;
            panel.InitialRationale = panel.Rationale;
        }
    }

    private bool HasScore(ScoringPanel panel)
    {
        return panel.Severity.HasValue && panel.Likelihood.HasValue && panel.Score.HasValue;
    }

    private bool CanSubmitScore(ScoringPanel panel)
    {
        return !HasScore(panel) && panel.Severity.HasValue && panel.Likelihood.HasValue &&
               panel.Severity.Value > 0 && panel.Likelihood.Value > 0;
    }

    private async Task SubmitScore(ScoringPanel panel)
    {
        if (!CanSubmitScore(panel) || IsSubmitting) return;

        try
        {
            // Local variable to track rationale input
            string rationaleInput = panel.Rationale ?? string.Empty;
            bool dialogResult = false;

            // Show rationale dialog using proper component
            var rationaleResult = await DialogService.OpenAsync("Score Rationale",
                ds => 
                {
                    return builder =>
                    {
                        builder.OpenComponent<RationaleInputDialog>(0);
                        builder.AddAttribute(1, "MemberName", GetMemberName(panel.SMSUserCode));
                        builder.AddAttribute(2, "HazardCode", Hazard.Code);
                        builder.AddAttribute(3, "MatrixCode", GetPreviewMatrixCode(panel));
                        builder.AddAttribute(4, "RationaleInput", rationaleInput);
                        builder.AddAttribute(5, "RationaleInputChanged", EventCallback.Factory.Create<string>(this, value => 
                        {
                            rationaleInput = value;
                            var logText = value?.Length > 50 ? value.Substring(0, 50) + "..." : value ?? "";
                            Logger.LogInformation("Rationale updated: {Rationale}", logText);
                        }));
                        builder.AddAttribute(6, "OnResult", EventCallback.Factory.Create<bool>(this, result => 
                        {
                            dialogResult = result;
                            var rationaleLength = rationaleInput?.Length ?? 0;
                            Logger.LogInformation("Dialog result: {Result}, Rationale length: {Length}", result, rationaleLength);
                            ds.Close(result);
                        }));
                        builder.CloseComponent();
                    };
                },
                new DialogOptions { Width = "750px", Height = "400px", Resizable = true });

            var hasRationale = !string.IsNullOrWhiteSpace(rationaleInput);
            

            // Check both the dialog result and the rationale content
            if (rationaleResult != true && !dialogResult)
            {
                Logger.LogInformation("Dialog was cancelled by user");
                return;
            }

            if (string.IsNullOrWhiteSpace(rationaleInput))
            {
                Logger.LogWarning("Dialog returned success but rationale is empty");
                return;
            }

            IsSubmitting = true;
            Logger.LogInformation("Submitting score for panel {PanelCode} on hazard {HazardCode}: {Severity} x {Likelihood} with rationale",
                panel.Code, Hazard.Code, panel.Severity, panel.Likelihood);

            // Calculate score and set rationale
            panel.Score = panel.Severity * panel.Likelihood;
            panel.Rationale = rationaleInput;
            panel.RiskAssessmentCode = CurrentRiskAssessment?.Code ?? string.Empty;
            
            // Update the actual entity properties based on CurrentStep before saving
            UpdateEntityPropertiesFromMapped(panel);
            
            // Update via CQRS
            var updateCommand = new UpdateScoringPanelCommand(panel);
            var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                var logRationale = string.IsNullOrEmpty(panel.Rationale) ? "" : panel.Rationale.Substring(0, Math.Min(50, panel.Rationale.Length));
                Logger.LogInformation("Score submitted for panel {PanelCode}: {Severity} x {Likelihood} = {Score} with rationale: {Rationale}",
                    panel.Code, panel.Severity, panel.Likelihood, panel.Score, logRationale);

                // Reload ONLY this component's data
                await LoadHazardScoringPanelsInternal();

                // CRITICAL: Recalculate the hazard average scoring data
                await RecalculateHazardScoringData();

                // Notify parent component to refresh matrix and step data
                if (OnHazardScored.HasDelegate)
                {
                    await OnHazardScored.InvokeAsync();
                }
            }
            else
            {
                Logger.LogError("Failed to submit score for panel {PanelCode}: {Error}", panel.Code, result.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error submitting score for panel {PanelCode}", panel.Code);
        }
        finally
        {
            IsSubmitting = false;
            StateHasChanged(); // Only trigger UI update after the operation is complete
        }
    }

    private void EditScore(ScoringPanel panel)
    {
        panel.Severity = null;
        panel.Likelihood = null;
        panel.Score = null;
        panel.Rationale = null; // Also clear rationale when editing
        panel.RiskAssessmentCode = CurrentRiskAssessment?.Code ?? string.Empty;
        // Don't call StateHasChanged() here to prevent render loops
    }

    private async Task ShowRationale(ScoringPanel panel)
    {
        if (string.IsNullOrEmpty(panel.Rationale)) return;

        await DialogService.OpenAsync($"Score by {GetMemberName(panel.SMSUserCode)} Rationale",
        ds =>
        {
            return builder =>
            {
                builder.OpenComponent<RationaleViewDialog>(0);
                builder.AddAttribute(1, "HazardCode", Hazard.Code);
                builder.AddAttribute(2, "MatrixCode", GetPanelMatrixCode(panel));
                builder.AddAttribute(3, "Rationale", panel.Rationale);
                builder.AddAttribute(4, "OnClose", EventCallback.Factory.Create(this, () => ds.Close()));
                builder.CloseComponent();
            };
        },
        new DialogOptions { Width = "700px", Height = "450px", Resizable = true });
    }

    private double? GetAverageScore()
    {
        var completedPanels = HazardScoringPanels.Where(p => HasScore(p)).ToList();
        if (!completedPanels.Any()) return null;

        return completedPanels.Average(p => (double)p.Score!.Value);
    }

    private int GetCompletedScoreCount()
    {
        return HazardScoringPanels.Count(HasScore);
    }

    private async Task OpenPanelDialog()
    {
        // Get currently selected stakeholder codes from existing panels
        var selectedCodes = HazardScoringPanels.Select(p => p.SMSUserCode!).ToList();

        // Debug logging to see what we're passing
        Logger.LogInformation("Opening panel dialog for hazard {HazardCode} with {Count} existing panel members: {Members}",
        Hazard.Code, selectedCodes.Count, string.Join(", ", selectedCodes));

        var result = await DialogService.OpenAsync<PanelManagementDialog>($" <br/> Manage  Panel for {Hazard.Code}",
        new Dictionary<string, object>
        {
                { "HazardCode", Hazard.Code },
                { "HazardDescription", Hazard.Description },
                { "AvailableStakeholders", AvailableStakeholders },
                { "SelectedStakeholderCodes", selectedCodes }
        },
        new DialogOptions { Width = "600px", Height = "620px" });

        if (result is List<string> newSelectedCodes)
        {
            Logger.LogInformation("Panel dialog returned {Count} selected codes: {Members}",newSelectedCodes.Count, string.Join(", ", newSelectedCodes));
            await SavePanelChanges(newSelectedCodes);
        }
        else
        {
            Logger.LogInformation("Panel dialog was cancelled or returned null");
        }
    }

    private async Task SavePanelChanges(List<string> selectedStakeholderCodes)
    {
        try
        {
            Logger.LogInformation("Updating panel for hazard {HazardCode} with {Count} members", Hazard.Code, selectedStakeholderCodes.Count);

            // Validate we have a valid risk assessment
            if (CurrentRiskAssessment?.Code == null)
            {
                Logger.LogError("Cannot save panel changes - CurrentRiskAssessment is null for hazard {HazardCode}", Hazard.Code);
                return;
            }

            var targetAssessmentCode = CurrentRiskAssessment.Code.Trim();
            Logger.LogInformation("Using risk assessment code: {AssessmentCode}", targetAssessmentCode);

            // Get ALL panels for this hazard for deletion
            var query = new GetScoringPanelsByHazardCodeQuery(Hazard.Code);
            var allPanelsResult = await Mediator.SendAsync(query, CancellationToken.None);

            if (allPanelsResult.IsSuccess && allPanelsResult.Value != null)
            {
                // Find panels for this assessment that should be removed
                var panelsToRemove = allPanelsResult.Value
                    .Where(p => p.RiskAssessmentCode.Trim() == targetAssessmentCode && 
                               !selectedStakeholderCodes.Contains(p.SMSUserCode!))
                    .ToList();

                Logger.LogInformation("Removing {Count} panels for unselected stakeholders in assessment {AssessmentCode}", 
                    panelsToRemove.Count, targetAssessmentCode);

                // Delete panels for unselected stakeholders
                foreach (var panel in panelsToRemove)
                {
                    Logger.LogInformation("Deleting panel {PanelCode} for stakeholder {StakeholderCode}", panel.Code, panel.SMSUserCode);

                    var deleteCommand = new DeleteScoringPanelCommand(new ScoringPanelID(panel.Id.Value));
                    await Mediator.SendAsync(deleteCommand, CancellationToken.None);
                }
            }

            // Add panels for newly selected stakeholders
            var existingCodes = HazardScoringPanels.Select(p => p.SMSUserCode).ToList();
            var newStakeholderCodes = selectedStakeholderCodes.Except(existingCodes).ToList();

            foreach (var stakeholderCode in newStakeholderCodes)
            {
                // Create new panel for the current assessment
                var newPanel = new ScoringPanel(new ScoringPanelID("SP-0000"))
                {
                    Code = "SP-0000", // Will be generated by database
                    HazardCode = Hazard.Code,
                    SMSUserCode = stakeholderCode,
                    InitialSeverity = null,
                    InitialLikelihood = null,
                    InitialScore = null,
                    InitialRationale = null,
                    ResidualSeverity = null,
                    ResidualLikelihood = null,
                    ResidualScore = null,
                    ResidualRationale = null,
                    RiskAssessmentCode = targetAssessmentCode
                };

                Logger.LogInformation("Creating panel for stakeholder {StakeholderCode} in assessment {AssessmentCode}",
                    stakeholderCode, targetAssessmentCode);

                var createCommand = new CreateScoringPanelCommand(newPanel);
                await Mediator.SendAsync(createCommand, CancellationToken.None);
            }

            // Reload the panels
            await LoadHazardScoringPanelsInternal();

            Logger.LogInformation("Panel updated successfully for hazard {HazardCode}", Hazard.Code);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating panel for hazard {HazardCode}", Hazard.Code);
        }
    }

    private string GetSeverityLabel(int severity)
    {
        return severity switch
        {
            1 => "Minor",
            2 => "Moderate",
            3 => "Serious",
            4 => "Major",
            5 => "Catastrophic",
            _ => ""
        };
    }

    private string GetLikelihoodLabel(int likelihood)
    {
        return likelihood switch
        {
            1 => "Rare",
            2 => "Unlikely",
            3 => "Possible",
            4 => "Likely",
            5 => "Frequent",
            _ => ""
        };
    }

    private string GetHazardMatrixCode(double averageScore)
    {
        // DEPRECATED: This method calculates from score averages (incorrect for aviation standards)
        // Use GetHazardMatrixCodeFromPanels() instead
        var bestMatch = GetBestSeverityLikelihoodMatch(averageScore);
        return AviationRiskMatrixCalculator.GetMatrixCode(bestMatch.severity, bestMatch.likelihood);
    }

    /// <summary>
    /// Calculate hazard matrix code using CORRECT aviation methodology
    /// This averages severity and likelihood separately, then determines matrix code
    /// </summary>
    private string GetHazardMatrixCodeFromPanels()
    {
        var completedPanels = HazardScoringPanels.Where(p => HasScore(p)).ToList();
        if (!completedPanels.Any()) return "-";

        // Aviation standard: Average severity and likelihood separately
        var averageSeverity = completedPanels.Average(p => (double)p.Severity!.Value);
        var averageLikelihood = completedPanels.Average(p => (double)p.Likelihood!.Value);


        // Use the authoritative calculator method
        return AviationRiskMatrixCalculator.GetAverageMatrixCode(averageSeverity, averageLikelihood);
    }
    //private string GetHazardRiskLevelFromPanels()
    //{
    //    var completedPanels = HazardScoringPanels.Where(p => HasScore(p)).ToList();
    //    if (!completedPanels.Any()) return "-";

    //    // Aviation standard: Average severity and likelihood separately
    //    var averageSeverity = completedPanels.Average(p => (double)p.Severity!.Value);
    //    var averageLikelihood = completedPanels.Average(p => (double)p.Likelihood!.Value);
    //    var averageSeverityInt = Convert.ToInt32(completedPanels.Average(p => p.Severity!.Value));
    //    var averageLikelihoodInt = Convert.ToInt32(completedPanels.Average(p => p.Likelihood!.Value));

    //    // Use the authoritative calculator method
    //    return AviationRiskMatrixCalculator.GetAviationRiskLevel(averageSeverityInt, averageLikelihoodInt);
    //}
    private string GetHazardRiskLevelFromPanels()
    {
        var completedPanels = HazardScoringPanels.Where(p => HasScore(p)).ToList();
        if (!completedPanels.Any()) return "-";

        // Aviation standard: Average severity and likelihood separately
        var averageSeverity = completedPanels.Average(p => (double)p.Severity!.Value);
        var averageLikelihood = completedPanels.Average(p => (double)p.Likelihood!.Value);

        // ✅ FIX: Use Math.Round() instead of Convert.ToInt32() which truncates
        var roundedSeverity = (int)Math.Round(averageSeverity);
        var roundedLikelihood = (int)Math.Round(averageLikelihood);

        // Use the authoritative calculator method and return the Name property
        return AviationRiskMatrixCalculator.GetAviationRiskLevel(roundedSeverity, roundedLikelihood).Name;
    }
    private string GetPanelMatrixCode(ScoringPanel panel)
    {
        return AviationRiskMatrixCalculator.GetPanelMatrixCode(panel);
    }

    private string GetPreviewMatrixCode(ScoringPanel panel)
    {
        return AviationRiskMatrixCalculator.GetPanelMatrixCode(panel);
    }

    private string GetPanelScoreStyle(ScoringPanel panel)
    {
        if (!panel.Severity.HasValue || !panel.Likelihood.HasValue)
            return "background: #6c757d; color: white; padding: 4px 8px; border-radius: 4px; font-weight: bold; font-size: 0.9rem; display: inline-block; text-align: center; min-width: 30px;";

        return AviationRiskMatrixCalculator.GetScoreDisplayStyle(panel.Severity.Value, panel.Likelihood.Value, false);
    }

    private string GetPreviewScoreStyle(ScoringPanel panel)
    {
        if (!panel.Severity.HasValue || !panel.Likelihood.HasValue)
            return "background: #6c757d; color: white; padding: 4px 8px; border-radius: 4px; font-weight: bold; font-size: 0.9rem; display: inline-block; text-align: center; min-width: 30px;";

        return AviationRiskMatrixCalculator.GetScoreDisplayStyle(panel.Severity.Value, panel.Likelihood.Value, true);
    }

    private string GetAverageScoreStyle(double averageScore)
    {
        // Get the correct aviation matrix code from panels, not from score average
        var completedPanels = HazardScoringPanels.Where(p => HasScore(p)).ToList();
        if (!completedPanels.Any())
            return "background: #6c757d; color: white; padding: 6px 10px; border-radius: 4px; font-weight: bold; font-size: 1rem; display: inline-block; text-align: center; min-width: 35px; border: 1px solid rgba(0,0,0,0.2);";

        // Use correct aviation calculation
        var averageSeverity = completedPanels.Average(p => (double)p.Severity!.Value);
        var averageLikelihood = completedPanels.Average(p => (double)p.Likelihood!.Value);
        var roundedSeverity = (int)Math.Round(averageSeverity);
        var roundedLikelihood = (int)Math.Round(averageLikelihood);

        var backgroundColor = AviationRiskMatrixCalculator.GetAviationMatrixColor(roundedSeverity, roundedLikelihood);
        var textColor = AviationRiskMatrixCalculator.IsLightColor(backgroundColor) ? "#000" : "#fff";

        return $"background: {backgroundColor}; color: {textColor}; padding: 6px 10px; border-radius: 4px; font-weight: bold; font-size: 1rem; display: inline-block; text-align: center; min-width: 35px; border: 1px solid rgba(0,0,0,0.2);";
    }

    private string GetAviationMatrixColor(int severity, int likelihood)
    {
        return AviationRiskMatrixCalculator.GetAviationMatrixColor(severity, likelihood);
    }

    private bool IsLightColor(string hexColor)
    {
        return AviationRiskMatrixCalculator.IsLightColor(hexColor);
    }

    private (int severity, int likelihood) GetBestSeverityLikelihoodMatch(double score)
    {
        var bestMatch = (severity: 1, likelihood: 1);
        var bestDistance = double.MaxValue;

        for (int severity = 1; severity <= 5; severity++)
        {
            for (int likelihood = 1; likelihood <= 5; likelihood++)
            {
                var calculatedScore = severity * likelihood;
                var distance = Math.Abs(calculatedScore - score);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestMatch = (severity, likelihood);
                }
            }
        }

        return bestMatch;
    }

    private bool AllScoresComplete()
    {
        return HazardScoringPanels.Any() && HazardScoringPanels.All(HasScore);
    }

    /// <summary>
    /// Get the current calculated hazard scoring data (for debugging/inspection)
    /// </summary>
    public (double? AverageScore, string MatrixCode, string RiskLevel) GetCalculatedScoringData()
    {
        return (CalculatedAverageScore, CalculatedMatrixCode, CalculatedRiskLevel);
    }

    /// <summary>
    /// Debug method to show detailed calculation breakdown for hazard average
    /// </summary>
    public string GetCalculationDebugInfo()
    {
        var completedPanels = HazardScoringPanels.Where(p => HasScore(p)).ToList();
        if (!completedPanels.Any()) return "No completed panels";

        var info = $"=== HAZARD {Hazard.Code} CALCULATION DEBUG ===\n";
        info += $"Panel Count: {completedPanels.Count}\n";

        foreach (var panel in completedPanels)
        {
            var panelCode = GetPanelMatrixCode(panel);
            info += $"  {GetMemberName(panel.SMSUserCode)}: Sev={panel.Severity}, Like={panel.Likelihood}, Score={panel.Score}, Code={panelCode}\n";
        }

        var avgSeverity = completedPanels.Average(p => (double)p.Severity!.Value);
        var avgLikelihood = completedPanels.Average(p => (double)p.Likelihood!.Value);
        var avgScore = completedPanels.Average(p => (double)p.Score!.Value);
        var roundedSev = (int)Math.Round(avgSeverity);
        var roundedLike = (int)Math.Round(avgLikelihood);
        var matrixCode = AviationRiskMatrixCalculator.GetMatrixCode(roundedSev, roundedLike);

        info += $"Averages: Sev={avgSeverity:F2}→{roundedSev}, Like={avgLikelihood:F2}→{roundedLike}, Score={avgScore:F2}\n";
        info += $"CORRECT MATRIX CODE: {matrixCode}\n";
        info += "=== END DEBUG ===";

        return info;
    }

    /// <summary>
    /// Recalculate the hazard's average scoring data from all completed panels
    /// ✅ FIXED: Now uses centralized AviationRiskMatrixCalculator for consistency
    /// </summary>
    private async Task RecalculateHazardScoringData()
    {
        try
        {
            // ✅ USE THE NEW CENTRALIZED CALCULATION METHOD
            var useResidual = CurrentStep == 5;
            var calculation = AviationRiskMatrixCalculator.CalculateHazardRisk(HazardScoringPanels, useResidual);

            if (calculation.IsValid)
            {
                // Store calculated values locally (for compatibility with existing code)
                CalculatedAverageScore = (double)calculation.AverageScore;
                CalculatedMatrixCode = calculation.MatrixCode;
                CalculatedRiskLevel = calculation.RiskLevel;

                Logger.LogInformation("✅ Recalculated hazard {HazardCode} scoring data: AvgSev={Severity:F2}→{RoundedSev}, AvgLike={Likelihood:F2}→{RoundedLike}, Matrix={MatrixCode}, Risk={RiskLevel}",
                    Hazard.Code, calculation.AverageSeverity, calculation.RoundedSeverity, 
                    calculation.AverageLikelihood, calculation.RoundedLikelihood, 
                    calculation.MatrixCode, calculation.RiskLevel?.Value ?? "Unknown");
            }
            else
            {
                // Clear calculated values if no scores available
                CalculatedAverageScore = null;
                CalculatedMatrixCode = string.Empty;
                CalculatedRiskLevel = RiskLevel.Unkonwn;

                Logger.LogInformation("Cleared hazard {HazardCode} scoring data - no completed panel scores available", Hazard.Code);
            }

            // Update the Hazard entity and save to database
            await UpdateHazardWithScoringData();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Error recalculating hazard {HazardCode} scoring data", Hazard.Code);
        }
    }

    /// <summary>
    /// Update the Hazard entity with calculated scoring data and save to database
    /// ✅ FIXED: Preserves original HazardRiskLevel in Step 5, only updates it in Step 4
    /// </summary>
    private async Task UpdateHazardWithScoringData()
    {
        try
        {
            // Validate we have a valid hazard to update
            if (Hazard == null || string.IsNullOrEmpty(Hazard.Code) || Hazard.Code == "HZ-0000")
            {
                Logger.LogWarning("Skipping hazard update - invalid hazard data");
                return;
            }

            Logger.LogInformation("Updating hazard {HazardCode} with calculated scoring data in database", Hazard.Code);

            // ✅ USE THE NEW CENTRALIZED CALCULATION METHOD
            var useResidual = CurrentStep == 5;
            var calculation = AviationRiskMatrixCalculator.CalculateHazardRisk(HazardScoringPanels, useResidual);

            if (CurrentStep == 4)
            {
                // Step 4: Update Initial assessment data AND base HazardRiskLevel
                Hazard.Status = HazardStatus.InitialHazardScoring;
                Hazard.InitialAverageScore = calculation.IsValid ? calculation.AverageScore : null;
                Hazard.InitialRiskMatrixCode = calculation.IsValid ? calculation.MatrixCode : null;

                // ✅ STEP 4: Update the base HazardRiskLevel (this is the hazard's inherent risk)
                Hazard.HazardRiskLevel = calculation.IsValid ? calculation.RiskLevel : RiskLevel.Unkonwn;

                // ✅ ALWAYS update Step4 dictionary with calculated values (with null check)
                if (calculation.IsValid && Step4 != null)
                {
                    Step4.HazardAverageScores[Hazard.Code] = (double)calculation.AverageScore;
                    Step4.HazardRiskLevels[Hazard.Code] = calculation.RiskLevel;
                    Step4.HazardMatrixCodes[Hazard.Code] = calculation.MatrixCode;
                    
                    Logger.LogInformation("✅ Updated Step4 model dictionaries for hazard {HazardCode}", Hazard.Code);
                }
                else if (Step4 != null)
                {
                    Step4.HazardAverageScores.Remove(Hazard.Code);
                    Step4.HazardRiskLevels.Remove(Hazard.Code);
                    Step4.HazardMatrixCodes.Remove(Hazard.Code);
                    
                    Logger.LogInformation("✅ Cleared Step4 model dictionaries for hazard {HazardCode}", Hazard.Code);
                }
                else
                {
                    Logger.LogWarning("⚠️ Step4 model is null - cannot update dictionaries for hazard {HazardCode} (CurrentStep: {CurrentStep})", 
                        Hazard.Code, CurrentStep);
                }

                Logger.LogInformation("Step 4: Updated base HazardRiskLevel to {RiskLevel} for hazard {HazardCode}",
                    Hazard.HazardRiskLevel?.Value ?? "Unknown", Hazard.Code);
            }
            else if (CurrentStep == 5)
            {
                // Step 5: Update Residual assessment data AND update HazardRiskLevel to final residual risk
                Hazard.Status = HazardStatus.ResidualHazardScoring;
                Hazard.ResidualAverageScore = calculation.IsValid ? calculation.AverageScore : null;
                Hazard.ResidualRiskMatrixCode = calculation.IsValid ? calculation.MatrixCode : null;

                // ✅ CORRECT: In Step 5, when residual scoring is completed, update HazardRiskLevel to reflect final risk
                // This represents the final risk level after mitigation implementation
                if (calculation.IsValid)
                {
                    Hazard.HazardRiskLevel = calculation.RiskLevel;
                    Logger.LogInformation("Step 5: Updated HazardRiskLevel to final residual risk level {RiskLevel} for hazard {HazardCode}",
                        calculation.RiskLevel.Value, Hazard.Code);
                }
                else
                {
                    // ✅ IMPORTANT: If no valid residual scoring, preserve the original Step 4 risk level
                    Logger.LogInformation("Step 5: No valid residual scoring - preserved original HazardRiskLevel ({RiskLevel}) for hazard {HazardCode}",
                        Hazard.HazardRiskLevel?.Value ?? "Unknown", Hazard.Code);
                }

                Logger.LogInformation("Step 5: Residual assessment completed - Matrix: {ResidualMatrix}, Score: {ResidualScore}",
                    calculation.IsValid ? calculation.MatrixCode : "N/A",
                    calculation.IsValid ? calculation.AverageScore.ToString("F2") : "N/A");

                // ✅ Update Step5 dictionary if needed (uncomment when Step5 model is ready)
                // if (calculation.IsValid && Step5 != null)
                // {
                //     Step5.HazardAverageScores[Hazard.Code] = (double)calculation.AverageScore;
                //     Step5.HazardRiskLevels[Hazard.Code] = calculation.RiskLevel;
                //     Step5.HazardMatrixCodes[Hazard.Code] = calculation.MatrixCode;
                // }
            }

            Hazard.UpdatedDate = DateTime.UtcNow;
            Hazard.UpdatedBy = CurrentUserService.UserCode;

            Logger.LogInformation("Calculated scoring data for hazard {HazardCode}: Step={Step}, IsValid={IsValid}, CalculatedRiskLevel={CalculatedRiskLevel}, MatrixCode={MatrixCode}, AverageScore={AverageScore}",
                Hazard.Code, CurrentStep, calculation.IsValid, calculation.RiskLevel?.Value ?? "Unknown", calculation.MatrixCode, calculation.AverageScore);

            // Save via CQRS
            var updateHazardCommand = new UpdateHazardCommand(Hazard);
            var result = await Mediator.SendAsync(updateHazardCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                Logger.LogInformation("✅ Successfully updated hazard {HazardCode} in database: AverageScore={AverageScore}, RiskMatrixCode={RiskMatrixCode}, BaseHazardRiskLevel={BaseRiskLevel}",
                    Hazard.Code, 
                    CurrentStep == 4 ? Hazard.InitialAverageScore?.ToString("F2") : Hazard.ResidualAverageScore?.ToString("F2") ?? "null", 
                    CurrentStep == 4 ? Hazard.InitialRiskMatrixCode : Hazard.ResidualRiskMatrixCode ?? "null", 
                    Hazard.HazardRiskLevel?.Value ?? "null");
            }
            else
            {
                Logger.LogError("❌ Failed to update hazard {HazardCode} in database: {Error}", Hazard.Code, result.Error?.Message ?? "Unknown error");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "💥 Error updating hazard {HazardCode} with scoring data in database", Hazard.Code);
        }
    }

    /// <summary>
    /// 🎯 NEW: Get the target risk assessment code based on current context
    /// </summary>
    private string GetTargetRiskAssessmentCode()
    {
        // Use the single CurrentRiskAssessment parameter
        if (!string.IsNullOrEmpty(CurrentRiskAssessment?.Code))
        {
            return CurrentRiskAssessment.Code.Trim();
        }

        
        Logger.LogWarning("No valid risk assessment code found for hazard {HazardCode} scoring panel", Hazard.Code);
        return string.Empty;
    }

    /// <summary>
    /// 🎯 Copy Step 4 scores to existing Step 5 panels if needed
    /// </summary>
    //private async Task CopyStep4ScoresToStep5IfNeeded(IEnumerable<ScoringPanel> allPanels)
    //{
    //    try
    //    {
    //        // Only proceed if we have the required assessment reference
    //        if (string.IsNullOrEmpty(CurrentRiskAssessment?.Code))
    //        {
    //            Logger.LogInformation("No CurrentRiskAssessment available for copying Step 4 scores");
    //            return;
    //        }

    //        var targetCode = CurrentRiskAssessment.Code.Trim();

    //        // Get existing panels for the current assessment
    //        var existingPanels = allPanels
    //            .Where(p => p.RiskAssessmentCode.Trim() == targetCode)
    //            .ToList();

    //        if (!existingPanels.Any())
    //        {
    //            Logger.LogInformation("No panels found for assessment {AssessmentCode}", targetCode);
    //            return;
    //        }

    //        // Find panels with scores that can be copied (typically Step 4 panels)
    //        var panelsWithScores = existingPanels
    //            .Where(p => p.Severity.HasValue && p.Likelihood.HasValue)
    //            .ToList();

    //        if (!panelsWithScores.Any())
    //        {
    //            Logger.LogInformation("No panels with scores found to copy from for assessment {AssessmentCode}", targetCode);
    //            return;
    //        }

    //        Logger.LogInformation("Found {Count} panels with scores for hazard {HazardCode} in assessment {AssessmentCode}",
    //            panelsWithScores.Count, Hazard.Code, targetCode);

    //        // If this is Step 5 and we have Step 4 data, we could copy it, but for now just log
    //        if (CurrentStep == 5)
    //        {
    //            Logger.LogInformation("Step 5 context detected - panels will use existing assessment data");
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Logger.LogError(ex, "Error in CopyStep4ScoresToStep5IfNeeded for hazard {HazardCode}", Hazard.Code);
    //    }
    //}

    /// <summary>
    /// 🎯 Copy Step 4 scores to existing Step 5 panels ONLY if Residual scores are empty
    /// </summary>
    private async Task CopyStep4ScoresToStep5IfNeeded(IEnumerable<ScoringPanel> allPanels)
    {
        try
        {
            // Only proceed if we're in Step 5 and have the required assessment reference
            if (CurrentStep != 5 || string.IsNullOrEmpty(CurrentRiskAssessment?.Code))
            {
                return;
            }

            var targetCode = CurrentRiskAssessment.Code.Trim();

            // Get existing panels for the current assessment
            var existingPanels = allPanels
                .Where(p => p.RiskAssessmentCode.Trim() == targetCode)
                .ToList();

            if (!existingPanels.Any())
            {
                Logger.LogInformation("No panels found for assessment {AssessmentCode} to copy scores", targetCode);
                return;
            }

            // ✅ KEY CONDITION: Find panels that have Initial scores but EMPTY Residual scores
            var panelsNeedingCopy = existingPanels
                .Where(p =>
                    // Has Initial scores from Step 4
                    p.InitialSeverity.HasValue && p.InitialLikelihood.HasValue && p.InitialScore.HasValue &&
                    // AND Residual scores are empty (haven't been set in Step 5 yet)
                    !p.ResidualSeverity.HasValue && !p.ResidualLikelihood.HasValue && !p.ResidualScore.HasValue)
                .ToList();

            if (!panelsNeedingCopy.Any())
            {
                Logger.LogInformation("No panels need score copying for hazard {HazardCode} - either no Initial scores or Residual scores already exist",
                    Hazard.Code);
                return;
            }

            Logger.LogInformation("Copying Initial scores to empty Residual scores for {Count} panels on hazard {HazardCode}",
                panelsNeedingCopy.Count, Hazard.Code);

            bool anyUpdated = false;

            foreach (var panel in panelsNeedingCopy)
            {
                // Copy Initial scores to Residual as starting point (only if Residual is empty)
                panel.ResidualSeverity = panel.InitialSeverity;
                panel.ResidualLikelihood = panel.InitialLikelihood;
                panel.ResidualScore = panel.InitialScore;
                panel.ResidualRationale = $"Initial assessment: {panel.InitialRationale ?? "No rationale provided"}"; // Prefix to indicate copied

                Logger.LogInformation("Copying Initial scores to empty Residual for panel {PanelCode}: {Sev}x{Like}={Score}",
                    panel.Code, panel.InitialSeverity, panel.InitialLikelihood, panel.InitialScore);

                // Save the updated panel
                var updateCommand = new UpdateScoringPanelCommand(panel);
                var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

                if (result.IsSuccess)
                {
                    Logger.LogInformation("✅ Successfully copied Initial scores to empty Residual for panel {PanelCode}", panel.Code);
                    anyUpdated = true;
                }
                else
                {
                    Logger.LogError("❌ Failed to copy scores for panel {PanelCode}: {Error}",
                        panel.Code, result.Error?.Message ?? "Unknown error");
                }
            }

            if (anyUpdated)
            {
                Logger.LogInformation("✅ Completed copying Initial scores to empty Residual scores for hazard {HazardCode}", Hazard.Code);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error copying Step 4 scores to Step 5 for hazard {HazardCode}", Hazard.Code);
        }
    }
    private string GetHazardCategoryDisplay(string? categoryValue)
    {
        if (string.IsNullOrEmpty(categoryValue)) return "";

        var category = HazardCategory.FromValue(categoryValue);
        return category?.Name ?? categoryValue.Replace("_", " ");
    }

    private string GetHazardTypeDisplay(string? typeValue)
    {
        if (string.IsNullOrEmpty(typeValue)) return "";

        var hazardType = HazardType.FromValue(typeValue);
        return hazardType?.Name ?? typeValue.Replace("_", " ");
    }

    private string GetHazardDisplayText(Hazard hazard)
    {
        var category = GetHazardCategoryDisplay(hazard.HazardCategory);
        var type = GetHazardTypeDisplay(hazard.HazardType);
        return $"{category.ToUpper()} - {type.ToUpper()}";
    }
}

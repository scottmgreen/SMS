using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Domain.ValueObjects;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;
using SMS_Application.Interfaces;
using SMS3.Components.Shared;
using Radzen;
using Radzen.Blazor;
using Microsoft.AspNetCore.Components;

namespace SMS3.Components.Pages.SMSRiskManagement.Components;

public partial class HazardScoringPanel : ComponentBase
{
    [Parameter] public Hazard Hazard { get; set; } = new(new HazardID("HZ-0000"));
    [Parameter] public Step4Model Step4 { get; set; } = new();
    [Parameter] public Step5Model Step5 { get; set; } = new();
    [Parameter] public List<SMSStakeholderUser> AvailableStakeholders { get; set; } = new();
    [Parameter] public List<SMSApplicationUser> AvailableAssessors { get; set; } = new();
    [Parameter] public RiskAssessment? CurrentRiskAssessment { get; set; }   // 🎯 Current risk assessment context
    [Parameter] public int CurrentStep { get; set; }   // 🎯 Current risk assessment context
    [Parameter] public RiskAssessment? TechnicalRiskAssessment { get; set; }   // 🎯 For Step 4 data lookup  
   
    [Parameter] public EventCallback OnHazardScored { get; set; }
    [Parameter] public RiskAssessmentID? RiskAssessmentId { get; set; } // NEW: Pass assessment ID to link scoring panels

    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<HazardScoringPanel> Logger { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;

    private RadzenDataGrid<ScoringPanel>? ScoringGrid;
    private List<ScoringPanel> HazardScoringPanels = new();
    private bool IsSubmitting = false;
    private bool _isDataLoaded = false;
    private string _lastHazardCode = string.Empty;

    // Local properties to track calculated hazard scoring data (for future database update)
    private double? CalculatedAverageScore = null;
    private string CalculatedMatrixCode = string.Empty;
    private string CalculatedRiskLevel = string.Empty;

    // Dropdown options - Updated to use A-E letter system
    private List<SeverityOption> SeverityOptions = new()
    {
        new(1, "1 - Minor"),
        new(2, "2 - Moderate"),
        new(3, "3 - Serious"),
        new(4, "4 - Major"),
        new(5, "5 - Catastrophic")
    };

    private List<LikelihoodOption> LikelihoodOptions = new()
    {
        new(1, "A - Rare"),
        new(2, "B - Unlikely"),
        new(3, "C - Possible"),
        new(4, "D - Likely"),
        new(5, "E - Frequent")
    };

    public record SeverityOption(int Value, string Text);
    public record LikelihoodOption(int Value, string Text);

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
            // Local variable to track rationale input with reactive updates
            string rationaleInput = panel.Rationale ?? string.Empty;

            // Show rationale dialog before submitting
            var rationaleResult = await DialogService.OpenAsync("Score Rationale", 
                ds => BuildRationaleDialog(ds, rationaleInput, panel, (value) => { rationaleInput = value; ds.Refresh(); }),
                new DialogOptions { Width = "700px", Height = "450px", Resizable = true });

            // If user cancelled or didn't provide rationale, don't submit
            if (rationaleResult != true || string.IsNullOrWhiteSpace(rationaleInput))
            {
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
                Logger.LogInformation("Score submitted for panel {PanelCode}: {Severity} x {Likelihood} = {Score} with rationale: {Rationale}",
                    panel.Code, panel.Severity, panel.Likelihood, panel.Score,
                    string.IsNullOrEmpty(panel.Rationale) ? "" : panel.Rationale.Substring(0, Math.Min(50, panel.Rationale.Length)));

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

    private RenderFragment BuildRationaleDialog(DialogService ds, string rationaleInput, ScoringPanel panel, Action<string> onValueChanged)
    {
        return builder =>
        {
            builder.OpenComponent<RadzenStack>(0);
            builder.AddAttribute(1, "Gap", "1rem");
            
            // Header div
            builder.OpenElement(2, "div");
            builder.AddAttribute(3, "style", "background-color: #212e61 !important; color: white; padding: 1rem 1.25rem; margin: -1rem -1rem 0 -1rem; border-radius: 8px 8px 0 0;");
            
            builder.OpenElement(4, "div");
            builder.AddAttribute(5, "style", "display: flex; align-items: center; gap: 0.75rem;");
            
            builder.OpenElement(6, "i");
            builder.AddAttribute(7, "class", "fas fa-comment-dots");
            builder.AddAttribute(8, "style", "font-size: 1.25rem; opacity: 0.9;");
            builder.CloseElement();
            
            builder.OpenElement(9, "div");
            builder.OpenElement(10, "h5");
            builder.AddAttribute(11, "style", "margin: 0; font-weight: 600; font-size: 1.1rem;");
            builder.AddContent(12, $"Please provide {GetMemberName(panel.SMSUserCode)} rationale for scoring {Hazard.Code} as {GetPreviewMatrixCode(panel)}");
            builder.CloseElement();
            builder.CloseElement();
            
            builder.CloseElement();
            builder.CloseElement();
            
            // Form field
            builder.OpenComponent<RadzenFormField>(13);
            builder.AddAttribute(14, "Text", "Rationale");
            builder.AddAttribute(15, "Variant", Variant.Outlined);
            
            builder.OpenComponent<RadzenTextArea>(16);
            builder.AddAttribute(17, "Value", rationaleInput);
            builder.AddAttribute(18, "ValueChanged", EventCallback.Factory.Create<string>(this, onValueChanged));
            builder.AddAttribute(19, "Rows", 6);
            builder.AddAttribute(20, "Style", "width: 100%;");
            builder.AddAttribute(21, "Placeholder", "Enter your rationale for this risk assessment score...\n\nConsider:\n• Why this severity level is appropriate\n• Why this likelihood level is justified\n• Any supporting evidence or experience");
            builder.CloseComponent();
            
            builder.CloseComponent();
            
            // Button stack
            builder.OpenComponent<RadzenStack>(22);
            builder.AddAttribute(23, "Orientation", Orientation.Horizontal);
            builder.AddAttribute(24, "JustifyContent", JustifyContent.End);
            builder.AddAttribute(25, "Gap", "0.5rem");
            
            builder.OpenComponent<RadzenButton>(26);
            builder.AddAttribute(27, "Text", "Cancel");
            builder.AddAttribute(28, "ButtonStyle", ButtonStyle.Light);
            builder.AddAttribute(29, "Click", EventCallback.Factory.Create(this, () => ds.Close(false)));
            builder.CloseComponent();
            
            builder.OpenComponent<RadzenButton>(30);
            builder.AddAttribute(31, "Text", "Submit Score");
            builder.AddAttribute(32, "Icon", "save");
            builder.AddAttribute(33, "ButtonStyle", ButtonStyle.Success);
            builder.AddAttribute(34, "Click", EventCallback.Factory.Create(this, () => { panel.Rationale = rationaleInput; ds.Close(!string.IsNullOrWhiteSpace(rationaleInput)); }));
            builder.AddAttribute(35, "Disabled", string.IsNullOrWhiteSpace(rationaleInput));
            builder.CloseComponent();
            
            builder.CloseComponent();
            
            builder.CloseComponent();
        };
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
        ds => BuildRationaleViewDialog(ds, panel),
        new DialogOptions { Width = "700px", Height = "450px", Resizable = true });
}

private RenderFragment BuildRationaleViewDialog(DialogService ds, ScoringPanel panel)
{
    return builder =>
    {
        builder.OpenComponent<RadzenStack>(0);
        builder.AddAttribute(1, "Gap", "1rem");
        
        // Header div
        builder.OpenElement(2, "div");
        builder.AddAttribute(3, "style", "background-color: #212e61 !important; color: white; padding: 1rem 1.25rem; margin: -1rem -1rem 0 -1rem; border-radius: 8px 8px 0 0;");
        
        builder.OpenElement(4, "div");
        builder.AddAttribute(5, "style", "display: flex; align-items: center; gap: 0.75rem;");
        
        builder.OpenElement(6, "i");
        builder.AddAttribute(7, "class", "fas fa-user-check");
        builder.AddAttribute(8, "style", "font-size: 1.25rem; opacity: 0.9;");
        builder.CloseElement();
        
        builder.OpenElement(9, "div");
        builder.OpenElement(10, "h5");
        builder.AddAttribute(11, "style", "margin: 0; font-weight: 600; font-size: 1.1rem;");
        builder.AddContent(12, $"Rationale for {Hazard.Code} risk assessment {GetPanelMatrixCode(panel)} score");
        builder.CloseElement();
        builder.CloseElement();
        
        builder.CloseElement();
        builder.CloseElement();
        
        // Form field
        builder.OpenComponent<RadzenFormField>(13);
        builder.AddAttribute(14, "Text", "Rationale");
        builder.AddAttribute(15, "Variant", Variant.Outlined);
        
        builder.OpenComponent<RadzenTextArea>(16);
        builder.AddAttribute(17, "Value", panel.Rationale);
        builder.AddAttribute(18, "ReadOnly", true);
        builder.AddAttribute(19, "Rows", 8);
        builder.AddAttribute(20, "Style", "width: 100%; background-color: #f8f9fa; border: 2px solid #e9ecef;");
        builder.CloseComponent();
        
        builder.CloseComponent();
        
        // Button stack
        builder.OpenComponent<RadzenStack>(21);
        builder.AddAttribute(22, "Orientation", Orientation.Horizontal);
        builder.AddAttribute(23, "JustifyContent", JustifyContent.End);
        
        builder.OpenComponent<RadzenButton>(24);
        builder.AddAttribute(25, "Text", "Close");
        builder.AddAttribute(26, "Icon", "close");
        builder.AddAttribute(27, "ButtonStyle", ButtonStyle.Primary);
        builder.AddAttribute(28, "Click", EventCallback.Factory.Create(this, () => ds.Close()));
        builder.CloseComponent();
        
        builder.CloseComponent();
        
        builder.CloseComponent();
    };
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

    var result = await DialogService.OpenAsync<PanelManagementDialog>($"Manage  Panel for {Hazard.Code}",
        new Dictionary<string, object>
        {
                { "HazardCode", Hazard.Code },
                { "HazardDescription", Hazard.Description },
                { "AvailableStakeholders", AvailableStakeholders },
                { "SelectedStakeholderCodes", selectedCodes }
        },
        new DialogOptions { Width = "700px", Height = "600px" });

    if (result is List<string> newSelectedCodes)
    {
        Logger.LogInformation("Panel dialog returned {Count} selected codes: {Members}",
            newSelectedCodes.Count, string.Join(", ", newSelectedCodes));
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

        // 🎯 SMART: Get the correct risk assessment code based on current context
        // var targetRiskAssessmentCode = GetTargetRiskAssessmentCode();
        // Logger.LogInformation("Using target risk assessment code: {TargetCode}", targetRiskAssessmentCode);

        // 🔥 FIX: Get ALL panels for this hazard (both Initial and Residual) for deletion
        var query = new GetScoringPanelsByHazardCodeQuery(Hazard.Code);
        var allPanelsResult = await Mediator.SendAsync(query, CancellationToken.None);

        if (allPanelsResult.IsSuccess && allPanelsResult.Value != null)
        {
            // Find all panels for unselected stakeholders (both Initial and Residual)
            var allPanelsToRemove = allPanelsResult.Value
                .Where(p => !selectedStakeholderCodes.Contains(p.SMSUserCode!))
                .ToList();

            Logger.LogInformation("🗑️ Removing {Count} panels (both Initial and Residual) for unselected stakeholders", allPanelsToRemove.Count);

            // Delete ALL panels for unselected stakeholders
            foreach (var panel in allPanelsToRemove)
            {
                Logger.LogInformation("Deleting panel {PanelCode} for stakeholder {StakeholderCode} in assessment {AssessmentCode}",
                    panel.Code, panel.SMSUserCode, panel.RiskAssessmentCode);

                var deleteCommand = new DeleteScoringPanelCommand(new ScoringPanelID(panel.Id.Value));
                await Mediator.SendAsync(deleteCommand, CancellationToken.None);
            }
        }

        // Add panels for newly selected stakeholders (create BOTH Initial and Residual)
        var existingCodes = HazardScoringPanels.Select(p => p.SMSUserCode).ToList();
        var newStakeholderCodes = selectedStakeholderCodes.Except(existingCodes).ToList();

        foreach (var stakeholderCode in newStakeholderCodes)
        {
            // Create Initial Risk Assessment panel (Step 4)
            var step4Panel = new ScoringPanel(new ScoringPanelID("SP-0000"))
            {
                Code = "SP-0000", // Will be generated by database
                HazardCode = Hazard.Code,
                SMSUserCode = stakeholderCode,
                Severity = null,
                Likelihood = null,
                Score = null,
                RiskAssessmentCode = TechnicalRiskAssessment.Code // ✅ FIXED: Use correct assessment code
            };

            Logger.LogInformation("Creating Initial panel for {StakeholderCode} with assessment code: {AssessmentCode}",stakeholderCode, TechnicalRiskAssessment.Code);

            var step4Command = new CreateScoringPanelCommand(step4Panel);
            await Mediator.SendAsync(step4Command, CancellationToken.None);

           
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

    Logger.LogInformation("Hazard {HazardCode} calculation: AvgSev={AvgSev:F2}, AvgLike={AvgLike:F2}",
        Hazard.Code, averageSeverity, averageLikelihood);

    // Use the authoritative calculator method
    return AviationRiskMatrixCalculator.GetAverageMatrixCode(averageSeverity, averageLikelihood);
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
/// This prepares the data for future database update (not saving yet)
/// </summary>
private async Task RecalculateHazardScoringData()
{
    var completedPanels = HazardScoringPanels.Where(p => HasScore(p)).ToList();

    if (completedPanels.Any())
    {
        // Calculate averages separately for severity and likelihood (aviation standard)
        var averageSeverity = completedPanels.Average(p => (double)p.Severity!.Value);
        var averageLikelihood = completedPanels.Average(p => (double)p.Likelihood!.Value);
        var averageScore = completedPanels.Average(p => (double)p.Score!.Value);

        // Use aviation standard calculation for matrix code
        var matrixCode = AviationRiskMatrixCalculator.GetAverageMatrixCode(averageSeverity, averageLikelihood);
        var roundedSeverity = (int)Math.Round(averageSeverity);
        var roundedLikelihood = (int)Math.Round(averageLikelihood);
        var riskLevel = AviationRiskMatrixCalculator.GetAviationRiskLevel(roundedSeverity, roundedLikelihood);

        // Store calculated values locally
        CalculatedAverageScore = averageScore;  // Keep score average for reporting
        CalculatedMatrixCode = matrixCode;      // Use correct aviation matrix code
        CalculatedRiskLevel = riskLevel;

        Logger.LogInformation("Recalculated hazard {HazardCode} scoring data: AvgSev={Severity:F2}→{RoundedSev}, AvgLike={Likelihood:F2}→{RoundedLike}, Matrix={MatrixCode}, Risk={RiskLevel}",
            Hazard.Code, averageSeverity, roundedSeverity, averageLikelihood, roundedLikelihood, matrixCode, riskLevel);

        // NEW: Update the Hazard entity and save to database
        await UpdateHazardWithScoringData();
    }
    else
    {
        // Clear calculated values if no scores available
        CalculatedAverageScore = null;
        CalculatedMatrixCode = string.Empty;
        CalculatedRiskLevel = string.Empty;

        Logger.LogInformation("Cleared hazard {HazardCode} scoring data - no completed panel scores available", Hazard.Code);

        // NEW: Clear hazard scoring data in database too
        await UpdateHazardWithScoringData();
    }
}

/// <summary>
/// Update the Hazard entity with calculated scoring data and save to database
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

        if (CurrentRiskAssessment.AssessmentType == RiskAssessmentType.Initial)
        {
            Hazard.InitialAverageScore = (decimal?)CalculatedAverageScore;
            Hazard.InitialRiskMatrixCode = CalculatedMatrixCode;  // Aviation matrix code (like "2B", "3D")
        }
        else
        {
            Hazard.ResidualAverageScore = (decimal?)CalculatedAverageScore;
            Hazard.ResidualRiskMatrixCode = CalculatedMatrixCode;  // Aviation matrix code (like "2B", "3D")
        }




        Hazard.RiskLevel = CalculatedRiskLevel;
        Hazard.UpdatedDate = DateTime.UtcNow;
        Hazard.UpdatedBy = "SYSTEM"; // Set updated by system for scoring updates

        // Save via CQRS
        var updateHazardCommand = new UpdateHazardCommand(Hazard);
        var result = await Mediator.SendAsync(updateHazardCommand, CancellationToken.None);

        if (result.IsSuccess)
        {
            Logger.LogInformation("Successfully updated hazard {HazardCode} in database: AverageScore={AverageScore}, RiskMatrixCode={RiskMatrixCode}, RiskLevel={RiskLevel}",
                Hazard.Code, Hazard.InitialAverageScore?.ToString("F2") ?? "null", Hazard.InitialRiskMatrixCode ?? "null", Hazard.RiskLevel ?? "null");
        }
        else
        {
            Logger.LogError("Failed to update hazard {HazardCode} in database: {Error}", Hazard.Code, result.Error?.Message ?? "Unknown error");
        }
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Error updating hazard {HazardCode} with scoring data in database", Hazard.Code);
    }
}

/// <summary>
/// 🎯 NEW: Get the target risk assessment code based on current context
/// </summary>
private string GetTargetRiskAssessmentCode()
{
    // Prefer CurrentRiskAssessment, fall back to RiskAssessmentId
    if (CurrentRiskAssessment?.Code != null)
    {
        return CurrentRiskAssessment.Code;
    }

    if (RiskAssessmentId?.Value != null)
    {
        return RiskAssessmentId.Value;
    }

    // Final fallback - determine from available assessments
    

    return string.Empty;
}

/// <summary>
/// 🎯 Copy Step 4 scores to existing Step 5 panels if needed
/// </summary>
private async Task CopyStep4ScoresToStep5IfNeeded(IEnumerable<ScoringPanel> allPanels)
{
    try
    {
        // Only proceed if we have the required assessment references
        if (TechnicalRiskAssessment?.Code.Trim() == null )
        {
            return;
        }

        // Get existing Step 5 panels
        var existingStep5Panels = allPanels
            .Where(p => p.RiskAssessmentCode.Trim() == TechnicalRiskAssessment.Code)
            .ToList();

        if (!existingStep5Panels.Any())
        {
            Logger.LogInformation("No panels found to update");
            return;
        }

        // Find Step 4 panels to copy from
        var step4Panels = allPanels
            .Where(p => p.RiskAssessmentCode.Trim() == TechnicalRiskAssessment.Code && p.Severity.HasValue && p.Likelihood.HasValue)
            .ToList();

        if (!step4Panels.Any())
        {
            Logger.LogInformation("No Step 4 panels with scores found to copy from");
            return;
        }

        Logger.LogInformation("🎯 Updating {Count} Step 5 panels with Step 4 scoring values for hazard {HazardCode}",
            existingStep5Panels.Count, Hazard.Code);

        // Update existing Step 5 panels with Step 4 data
        var updatedCount = 0;
        foreach (var step5Panel in existingStep5Panels)
        {
            // Find matching Step 4 panel by stakeholder code
            var matchingStep4Panel = step4Panels
                .FirstOrDefault(p => p.SMSUserCode == step5Panel.SMSUserCode);

            if (matchingStep4Panel != null)
            {
                // Only update if Step 5 panel doesn't already have scoring data
                if (!step5Panel.Severity.HasValue || !step5Panel.Likelihood.HasValue)
                {
                    step5Panel.Severity = matchingStep4Panel.Severity;     // ✅ COPY from Step 4
                    step5Panel.Likelihood = matchingStep4Panel.Likelihood; // ✅ COPY from Step 4
                                                                           // Score and Rationale remain null - requires explicit Step 5 submission

                    // var updateCommand = new UpdateScoringPanelCommand(step5Panel);
                    // var updateResult = await Mediator.SendAsync(updateCommand, CancellationToken.None);

                    if (true) //updateResult.IsSuccess)
                    {
                        updatedCount++;
                        Logger.LogInformation("✅ Updated Step 5 panel for {StakeholderCode}: Severity={Severity}, Likelihood={Likelihood}",
                            step5Panel.SMSUserCode, matchingStep4Panel.Severity, matchingStep4Panel.Likelihood);
                    }
                    else
                    {
                        //Logger.LogError("❌ Failed to update Step 5 panel for {StakeholderCode}: {Error}", 
                        //    step5Panel.SMSUserCode, updateResult.Error?.Message);
                    }
                }
                else
                {
                    Logger.LogInformation("Step 5 panel for {StakeholderCode} already has scoring data - skipping",
                        step5Panel.SMSUserCode);
                }
            }
        }

        Logger.LogInformation("🎉 Updated {UpdatedCount} Step 5 panels with Step 4 scoring values", updatedCount);
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Error copying Step 4 scores to existing Step 5 panels");
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

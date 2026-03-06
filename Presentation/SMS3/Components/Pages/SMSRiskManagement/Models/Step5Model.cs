namespace SMS3.Components.Pages.SMSRiskManagement.Models;

/// <summary>
/// Step 5: Risk Mitigation & Implementation
/// </summary>
public class Step5Model
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ICurrentUserService CurrentUserService { get; set; } = default!;

    public Step5Model(IMediator mediator, ICurrentUserService currentUserService)
    {
        Mediator = mediator;
        CurrentUserService = currentUserService;
    }

    public Dictionary<string, List<string>> SavedMitigationStrategies { get; set; } = new();
    public Dictionary<string, List<Mitigation>> HazardMitigations { get; set; } = new();
    public Dictionary<string, List<string>> ResidualRiskPanels { get; set; } = new();
        
    // Dictionary mapping hazard codes to their corresponding Residual RiskAnalysis entities
    public Dictionary<string, RiskAnalysis> HazardResidualRiskAnalyses { get; set; } = new();

    /// <summary>
    /// Gets the RiskAnalysis entity for a specific hazard based on CurrentStep, creating a new one if it doesn't exist
    /// </summary>
    public RiskAnalysis GetHazardResidualAnalysis(string hazardCode)
    {
        return GetHazardResidualAnalysis(hazardCode, 5);
    }
    
    /// <summary>
    /// Gets the RiskAnalysis entity for a specific hazard based on CurrentStep, creating a new one if it doesn't exist
    /// </summary>
    public RiskAnalysis GetHazardResidualAnalysis(string hazardCode, int currentStep)
    {
        if (string.IsNullOrEmpty(hazardCode))
        {
            return CreateNewRiskAnalysis(hazardCode ?? string.Empty, string.Empty);
        }

        if (!HazardResidualRiskAnalyses.ContainsKey(hazardCode))
        {
            HazardResidualRiskAnalyses[hazardCode] = CreateNewRiskAnalysis(hazardCode, string.Empty);
        }

        // Update the AssessmentType based on CurrentStep
        var analysis = HazardResidualRiskAnalyses[hazardCode];
        
        return analysis;
    }

    private RiskAnalysis CreateNewRiskAnalysis(string hazardCode, string riskAssessmentCode)
    {
        return new RiskAnalysis(new RiskAnalysisID("RA-0000"))
        {
            HazardCode = hazardCode,
            RiskAssessmentCode = riskAssessmentCode,
            AssessmentType = RiskAnalysisType.Initial, // ✅ FIXED: Start as Initial, not Residual
            InitialWorstCredibleOutcome = string.Empty,
            InitialRootCause = string.Empty,
            InitialAdditionalComments = string.Empty,
            ResidualWorstCredibleOutcome = string.Empty,
            ResidualRootCause = string.Empty,
            ResidualAdditionalComments = string.Empty,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "SYSTEM",
            UpdatedDate = DateTime.UtcNow,
            UpdatedBy = "SYSTEM"
        };
    }
    
    public async Task LoadRiskAnalysisAsync(List<Hazard> availableHazards, string riskAssessmentCode)
    {
        if (Mediator == null || availableHazards == null) return;

        try
        {
            var hazardCodes = availableHazards.Select(h => h.Code).ToList();

            Console.WriteLine($"Step5: Loading RiskAnalysis for {hazardCodes.Count} hazards in assessment {riskAssessmentCode}");

            // Load all existing RiskAnalysis entities
            var allAnalysisQuery = new GetAllRiskAnalysisQuery();
            var allAnalysisResult = await Mediator.SendAsync(allAnalysisQuery, CancellationToken.None);

            if (!allAnalysisResult.IsSuccess || allAnalysisResult.Value == null)
            {
                Console.WriteLine("Step5: Failed to load RiskAnalysis entities or none found");
                return;
            }

            // Find existing RiskAnalysis entities for our hazards and risk assessment
            var existingAnalyses = allAnalysisResult.Value
                .Where(ra => hazardCodes.Contains(ra.HazardCode) &&
                            ra.RiskAssessmentCode.Trim() == riskAssessmentCode.Trim())
                .ToList();

            Console.WriteLine($"Step5: Found {existingAnalyses.Count} existing RiskAnalysis entities");

            // Load the existing analyses into Step 5's dictionary
            foreach (var analysis in existingAnalyses)
            {
                // ✅ CRITICAL: Use the SAME RiskAnalysis entity from Step 3
                // This preserves both Initial properties (from Step 3) and allows Residual properties (for Step 5)
                HazardResidualRiskAnalyses[analysis.HazardCode] = analysis;

                Console.WriteLine($"Step5: Loaded RiskAnalysis for {analysis.HazardCode} - Code: {analysis.Code}");
                Console.WriteLine($"  - Initial properties: WorstOutcome='{analysis.InitialWorstCredibleOutcome?.Substring(0, Math.Min(50, analysis.InitialWorstCredibleOutcome?.Length ?? 0))}...'");
                Console.WriteLine($"  - Residual properties: WorstOutcome='{analysis.ResidualWorstCredibleOutcome?.Substring(0, Math.Min(50, analysis.ResidualWorstCredibleOutcome?.Length ?? 0))}...'");
            }

            // For hazards without existing RiskAnalysis, create placeholder entities
            var hazardsWithoutAnalysis = availableHazards
                .Where(h => !HazardResidualRiskAnalyses.ContainsKey(h.Code))
                .ToList();

            if (hazardsWithoutAnalysis.Any())
            {
                Console.WriteLine($"Step5: Creating placeholder RiskAnalysis for {hazardsWithoutAnalysis.Count} hazards without existing analysis");

                foreach (var hazard in hazardsWithoutAnalysis)
                {
                    var newAnalysis = CreateNewRiskAnalysis(hazard.Code, riskAssessmentCode);
                    HazardResidualRiskAnalyses[hazard.Code] = newAnalysis;

                    Console.WriteLine($"Step5: Created placeholder RiskAnalysis for {hazard.Code}");
                }
            }

            Console.WriteLine($"Step5: Total RiskAnalysis entities loaded: {HazardResidualRiskAnalyses.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in LoadRiskAnalysisAsync: {ex.Message}");
        }
    }

    public async Task LoadFromAssessmentAsync(RiskAssessment assessment, List<Hazard> availableHazards)
    {
        if (assessment == null) return;

        if (Mediator != null && availableHazards?.Any() == true)
        {
            // ✅ CRITICAL: Load RiskAnalysis entities from Step 3 FIRST
            await LoadRiskAnalysisAsync(availableHazards, assessment.Code);

            // Then load mitigations
            await LoadMitigationEntitiesAsync(availableHazards);
        }
    }

    
    public (bool isValid, string message) Validate()
    {
        return (true, "Step 5 validation passed");
    }

    
    public async Task ApplyToAssessmentAsync(RiskAssessment assessment, List<Hazard> availableHazards)
    {
        if (Mediator == null || assessment == null || availableHazards == null) return;

        try
        {
            assessment.CompleteStep(5);
            await SaveResidualRiskAnalysesAsync(assessment);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying Step 5 to assessment: {ex.Message}");
        }
    }
    private async Task SaveResidualRiskAnalysesAsync(RiskAssessment assessment)
    {
        foreach (var analysisKvp in HazardResidualRiskAnalyses)
        {
            try
            {
                var analysis = analysisKvp.Value;
                var hazardCode = analysisKvp.Key;

                var existingQuery = new GetRiskAnalysisByHazardAndAssessmentQuery(hazardCode, assessment.Code);
                var existingResult = await Mediator.SendAsync(existingQuery, CancellationToken.None);

                if (existingResult.IsSuccess && existingResult.Value != null)
                {
                    var existingAnalysis = existingResult.Value;

                    existingAnalysis.ResidualWorstCredibleOutcome = analysis.ResidualWorstCredibleOutcome;
                    existingAnalysis.ResidualRootCause = analysis.ResidualRootCause;
                    existingAnalysis.ResidualAdditionalComments = analysis.ResidualAdditionalComments;
                    existingAnalysis.UpdatedDate = DateTime.UtcNow;
                    existingAnalysis.UpdatedBy = "SYSTEM";

                    // Ensure RiskAssessmentCode is properly set
                    if (string.IsNullOrEmpty(existingAnalysis.RiskAssessmentCode))
                    {
                        existingAnalysis.RiskAssessmentCode = assessment.Code;
                    }

                    // Save the merged entity
                    var updateCommand = new UpdateRiskAnalysisCommand(existingAnalysis);
                    var updateResult = await Mediator.SendAsync(updateCommand, CancellationToken.None);

                    if (updateResult.IsSuccess)
                    {
                        HazardResidualRiskAnalyses[hazardCode] = updateResult.Value;
                        Console.WriteLine($"✅ Updated RiskAnalysis {existingAnalysis.Code} for {hazardCode} - PRESERVED Initial properties");
                    }
                    else
                    {
                        Console.WriteLine($"❌ Failed to update RiskAnalysis for {hazardCode}: {updateResult.Error?.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"❌ Could not load existing RiskAnalysis for {hazardCode} to preserve Initial properties");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating RiskAnalysis for hazard {analysisKvp.Key}: {ex.Message}");
            }
        }
    }
    

    private async Task LoadMitigationEntitiesAsync(List<Hazard> availableHazards)
    {
        try
        {
            if (HazardMitigations == null)
                HazardMitigations = new Dictionary<string, List<Mitigation>>();
            if (SavedMitigationStrategies == null)
                SavedMitigationStrategies = new Dictionary<string, List<string>>();

            foreach (var hazard in availableHazards)
            {
                try
                {
                    var query = new GetMitigationsByHazardCodeQuery(hazard.Code);
                    var result = await Mediator.SendAsync(query, CancellationToken.None);

                    if (result.IsSuccess && result.Value?.Any() == true)
                    {
                        var mitigations = result.Value.ToList();
                        HazardMitigations[hazard.Code] = mitigations;
                        SavedMitigationStrategies[hazard.Code] = mitigations.Select(m => m.Name ?? "Unnamed Mitigation").ToList();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading mitigations for hazard {hazard.Code}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in LoadMitigationEntitiesAsync: {ex.Message}");
        }
    }

    
    
}
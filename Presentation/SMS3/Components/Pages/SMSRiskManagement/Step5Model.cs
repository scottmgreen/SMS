namespace SMS3.Components.Pages.SMSRiskManagement;

/// <summary>
/// Step 5: Risk Mitigation & Implementation
/// </summary>
public class Step5Model
{
    private const string NEW_RISK_ANALYSIS_SEED_CODE = "RA-0000";

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
        return new RiskAnalysis(new RiskAnalysisID(NEW_RISK_ANALYSIS_SEED_CODE))
        {
            Code = NEW_RISK_ANALYSIS_SEED_CODE,
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
    // Add this method to Step5Model.cs right after the CreateNewRiskAnalysis method:

    /// <summary>
    /// ✅ CRITICAL: Load existing RiskAnalysis entities from Step 3 for Step 5 to work with
    /// This ensures we have the complete RiskAnalysis with both Initial and Residual properties
    /// </summary>
    public async Task LoadRiskAnalysisAsync(IMediator mediator, List<Hazard> availableHazards, string riskAssessmentCode)
    {
        if (mediator == null || availableHazards == null) return;

        try
        {
            var hazardCodes = availableHazards.Select(h => h.Code).ToList();

            Console.WriteLine($"Step5: Loading RiskAnalysis for {hazardCodes.Count} hazards in assessment {riskAssessmentCode}");

            // Load all existing RiskAnalysis entities
            var allAnalysisQuery = new GetAllRiskAnalysisQuery();
            var allAnalysisResult = await mediator.SendAsync(allAnalysisQuery, CancellationToken.None);

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

    // And fix your LoadFromAssessmentAsync method:
    public async Task LoadFromAssessmentAsync(RiskAssessment assessment, IMediator mediator, List<Hazard> availableHazards)
    {
        if (assessment == null) return;

        

        if (mediator != null && availableHazards?.Any() == true)
        {
            // ✅ CRITICAL: Load RiskAnalysis entities from Step 3 FIRST
            await LoadRiskAnalysisAsync(mediator, availableHazards, assessment.Code);

            // Then load mitigations
            await LoadMitigationEntitiesAsync(mediator, availableHazards);
        }
    }

    // And add the missing LoadFromAssessment method:
    
    /// <summary>
    /// Load existing Residual RiskAnalysis entities from the database for each hazard
    /// </summary>
    //public async Task LoadExistingResidualRiskAnalysesAsync(IMediator mediator, List<Hazard> availableHazards)
    //{
    //    if (mediator == null || availableHazards == null) return;

    //    var hazardCodes = availableHazards.Select(h => h.Code).ToList();

    //    var allAnalysisQuery = new GetAllRiskAnalysisQuery();
    //    var allAnalysisResult = await mediator.SendAsync(allAnalysisQuery, CancellationToken.None);

    //    if (!allAnalysisResult.IsSuccess || allAnalysisResult.Value == null)
    //    {
    //        return;
    //    }

    //    var allAssessmentsQuery = new GetAllRiskAssessmentsQuery();
    //    var assessmentsResult = await mediator.SendAsync(allAssessmentsQuery, CancellationToken.None);

    //    if (!assessmentsResult.IsSuccess || assessmentsResult.Value == null)
    //    {
    //        return;
    //    }

    //    // ? ENHANCED: Filter by AssessmentType.Residual instead of RiskAssessmentType.Residual
    //    var residualAssessmentCodes = assessmentsResult.Value
    //        .Where(a => a.AssessmentType == RiskAssessmentType.Residual)
    //        .Select(a => a.Code)
    //        .ToHashSet();

    //    // ? ENHANCED: Filter by both hazard codes AND AssessmentType = Residual
    //    var residualAnalyses = allAnalysisResult.Value
    //        .Where(ra => hazardCodes.Contains(ra.HazardCode) && 
    //                    residualAssessmentCodes.Contains(ra.RiskAssessmentCode) &&
    //                    ra.AssessmentType == RiskAnalysisType.Residual) // ? CRITICAL: Filter by Residual AssessmentType
    //        .ToList();

    //    foreach (var analysis in residualAnalyses)
    //    {
    //        // ? ENHANCED: Ensure AssessmentType is properly set to Residual
    //        if (analysis.AssessmentType != RiskAnalysisType.Residual)
    //        {
    //            analysis.AssessmentType = RiskAnalysisType.Residual;
    //        }

    //        // Ensure the RiskAssessmentCode is set correctly for Residual assessments
    //        if (string.IsNullOrEmpty(analysis.RiskAssessmentCode) || analysis.RiskAssessmentCode == "RA-0000")
    //        {
    //            // Find the correct Residual assessment code for this analysis
    //            var residualAssessment = assessmentsResult.Value
    //                .FirstOrDefault(a => a.AssessmentType == RiskAssessmentType.Residual && 
    //                                    hazardCodes.Contains(a.HazardCode ?? string.Empty));

    //            if (residualAssessment != null)
    //            {
    //                analysis.RiskAssessmentCode = residualAssessment.Code;
    //            }
    //        }

    //        this.HazardResidualRiskAnalyses[analysis.HazardCode] = analysis;
    //    }
    //}

    public (bool isValid, string message) Validate()
    {
        return (true, "Step 5 validation passed");
    }

    public void ApplyToAssessment(RiskAssessment assessment)
    {
        assessment.CompleteStep(5);
    }

    public async Task ApplyToAssessmentAsync(RiskAssessment assessment, IMediator mediator, List<Hazard> availableHazards)
    {
        if (mediator == null || assessment == null || availableHazards == null) return;

        try
        {
            assessment.CompleteStep(5);
            await SaveResidualRiskAnalysesAsync(mediator, assessment);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying Step 5 to assessment: {ex.Message}");
        }
    }
    private async Task SaveResidualRiskAnalysesAsync(IMediator mediator, RiskAssessment assessment)
    {
        foreach (var analysisKvp in HazardResidualRiskAnalyses)
        {
            try
            {
                var analysis = analysisKvp.Value;
                var hazardCode = analysisKvp.Key;

                // ✅ CRITICAL FIX: Load the existing entity from database to preserve Initial properties
                var existingQuery = new GetRiskAnalysisByHazardAndAssessmentQuery(hazardCode, assessment.Code);
                var existingResult = await mediator.SendAsync(existingQuery, CancellationToken.None);

                if (existingResult.IsSuccess && existingResult.Value != null)
                {
                    var existingAnalysis = existingResult.Value;

                    // ✅ PRESERVE Initial properties from database
                    // ✅ UPDATE only Residual properties from Step 5
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
                    var updateResult = await mediator.SendAsync(updateCommand, CancellationToken.None);

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
    //private async Task SaveResidualRiskAnalysesAsync(IMediator mediator, RiskAssessment assessment)
    //{
    //    foreach (var analysisKvp in HazardResidualRiskAnalyses)
    //    {
    //        try
    //        {
    //            var analysis = analysisKvp.Value;
    //            var hazardCode = analysisKvp.Key;

    //            //// ? CRITICAL: Ensure AssessmentType is set to Residual before saving
    //            //if (analysis.AssessmentType != RiskAnalysisType.Residual)
    //            //{
    //            //    analysis.AssessmentType = RiskAnalysisType.Residual;
    //            //}

    //            // ? ENHANCED: Ensure RiskAssessmentCode is properly set
    //            if (string.IsNullOrEmpty(analysis.RiskAssessmentCode) && assessment != null)
    //            {
    //                analysis.RiskAssessmentCode = assessment.Code;
    //            }

    //            // ? Choose correct CQRS command based on entity state
    //            //if (string.IsNullOrEmpty(analysis.Code) || analysis.Code == NEW_RISK_ANALYSIS_SEED_CODE)
    //            //{
    //            //    // Create new RiskAnalysis
    //            //    var createCommand = new CreateRiskAnalysisCommand(analysis);
    //            //    var createResult = await mediator.SendAsync(createCommand, CancellationToken.None);

    //            //    if (createResult.IsSuccess)
    //            //    {
    //            //        HazardResidualRiskAnalyses[hazardCode] = createResult.Value;
    //            //    }
    //            //}
    //            //else
    //            //{
    //                // Update existing RiskAnalysis
    //                var updateCommand = new UpdateRiskAnalysisCommand(analysis);
    //                var updateResult = await mediator.SendAsync(updateCommand, CancellationToken.None);

    //                if (updateResult.IsSuccess)
    //                {
    //                    HazardResidualRiskAnalyses[hazardCode] = updateResult.Value;
    //                }
    //            //}
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine($"Error updating Residual RiskAnalysis for hazard {analysisKvp.Key}: {ex.Message}");
    //        }
    //    }
    //}



    private async Task LoadMitigationEntitiesAsync(IMediator mediator, List<Hazard> availableHazards)
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
                    var result = await mediator.SendAsync(query, CancellationToken.None);

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

    public string ApplyToAssessmentString(List<string> appliedMitigations)
    {
        return string.Join("; ", appliedMitigations ?? new List<string>());
    }

    
}
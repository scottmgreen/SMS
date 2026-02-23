namespace SMS3.Components.Pages.SMSRiskManagement.Models;

/// <summary>
/// Step 3: Risk Analysis
/// </summary>
public class Step3Model
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private AuthenticationService AuthService { get; set; } = default!;

    public Step3Model(IMediator mediator, AuthenticationService authService)
    {
        Mediator = mediator;
        AuthService = authService;
        
    }

    public Dictionary<string, RiskAnalysis> Step3RiskAnalyses { get; set; } = new();

    public (bool isValid, string message) Validate(List<Hazard> availableHazards = null)
    {
        return Validate(availableHazards, 3);
    }
    
    public (bool isValid, string message) Validate(List<Hazard> availableHazards, int currentStep)
    {
        if (availableHazards == null || !availableHazards.Any())
        {
            return (false, "No hazards available for risk analysis");
        }

        var incompleteHazards = new List<string>();
        var analysisCount = 0;

        foreach (var hazard in availableHazards)
        {
            if (Step3RiskAnalyses.TryGetValue(hazard.Code, out var analysis))
            {
                // Determine which properties to validate based on CurrentStep
                bool worstOutcomeValid, rootCauseValid, additionalCommentsValid;
                
                if (currentStep == 5)
                {
                    // Step 5: Validate Residual properties
                    worstOutcomeValid = !string.IsNullOrWhiteSpace(analysis.ResidualWorstCredibleOutcome) && analysis.ResidualWorstCredibleOutcome.Length >= 10;
                    rootCauseValid = !string.IsNullOrWhiteSpace(analysis.ResidualRootCause) && analysis.ResidualRootCause.Length >= 10;
                    additionalCommentsValid = !string.IsNullOrWhiteSpace(analysis.ResidualAdditionalComments) && analysis.ResidualAdditionalComments.Length >= 10;
                }
                else
                {
                    // Steps 1-4: Validate Initial properties
                    worstOutcomeValid = !string.IsNullOrWhiteSpace(analysis.InitialWorstCredibleOutcome) && analysis.InitialWorstCredibleOutcome.Length >= 10;
                    rootCauseValid = !string.IsNullOrWhiteSpace(analysis.InitialRootCause) && analysis.InitialRootCause.Length >= 10;
                    additionalCommentsValid = !string.IsNullOrWhiteSpace(analysis.InitialAdditionalComments) && analysis.InitialAdditionalComments.Length >= 10;
                }

                if (!worstOutcomeValid && !rootCauseValid && !additionalCommentsValid)
                {
                    incompleteHazards.Add($"{hazard.Code} (missing analysis data)");
                }
                else
                {
                    analysisCount++;
                }
            }
            else
            {
                incompleteHazards.Add($"{hazard.Code} (no analysis found)");
            }
        }

        if (incompleteHazards.Any())
        {
            return (false, $"Risk analysis incomplete for {incompleteHazards.Count}/{availableHazards.Count} hazards: {string.Join("; ", incompleteHazards)}");
        }

        var stageName = currentStep == 5 ? "Residual" : "Initial";
        return (true, $"Step 3 validation passed - {analysisCount}/{availableHazards.Count} hazards have complete {stageName} risk analysis");
    }

    public RiskAnalysis GetHazardAnalysis(string hazardCode)
    {
        return Step3RiskAnalyses[hazardCode];
    }


    
    public async Task LoadExistingRiskAnalysesAsync(List<Hazard> availableHazards)
    {
        if (Mediator == null || availableHazards == null) return;

        // ✅ CLEAR existing data first to ensure fresh load
        Step3RiskAnalyses.Clear();

        var hazardCodes = availableHazards.Select(h => h.Code).ToList();
        Console.WriteLine($"🔍 LoadExistingRiskAnalysesAsync: Looking for RiskAnalysis for {hazardCodes.Count} hazards: {string.Join(", ", hazardCodes)}");

        // ✅ Get ALL RiskAnalysis entities from database
        var allAnalysisQuery = new GetAllRiskAnalysisQuery();
        var allAnalysisResult = await Mediator.SendAsync(allAnalysisQuery, CancellationToken.None);

        if (!allAnalysisResult.IsSuccess || allAnalysisResult.Value == null)
        {
            Console.WriteLine("❌ Failed to load RiskAnalysis entities from database");
            return;
        }

        var allAnalyses = allAnalysisResult.Value.ToList();
        Console.WriteLine($"🔍 Found {allAnalyses.Count} total RiskAnalysis entities in database");

        // ✅ Debug: Show all analyses that match our hazard codes
        var matchingAnalyses = allAnalyses.Where(ra => hazardCodes.Contains(ra.HazardCode)).ToList();
        Console.WriteLine($"🔍 Found {matchingAnalyses.Count} RiskAnalysis entities matching our hazard codes:");
        foreach (var analysis in matchingAnalyses)
        {
            Console.WriteLine($"   - HazardCode: {analysis.HazardCode}, RiskAssessmentCode: {analysis.RiskAssessmentCode}, Code: {analysis.Code}");
        }

        // ✅ Get ALL RiskAssessments
        var allAssessmentsQuery = new GetAllRiskAssessmentsQuery();
        var assessmentsResult = await Mediator.SendAsync(allAssessmentsQuery, CancellationToken.None);

        if (!assessmentsResult.IsSuccess || assessmentsResult.Value == null)
        {
            Console.WriteLine("❌ Failed to load RiskAssessments from database");
            return;
        }

        var allAssessments = assessmentsResult.Value.ToList();
        Console.WriteLine($"🔍 Found {allAssessments.Count} total RiskAssessments in database");

        // ✅ ENHANCED FILTERING: Be more flexible with RiskAssessment matching
        var validAssessmentCodes = new HashSet<string>();

        // Add all technical assessment codes
        foreach (var assessment in allAssessments.Where(a => a.RiskAssessmentCategory == RiskAssessmentCategory.Technical))
        {
            validAssessmentCodes.Add(assessment.Code);
            Console.WriteLine($"🔍 Added Technical RiskAssessment: {assessment.Code}");
        }

        // Also check for assessments that reference our hazards
        foreach (var assessment in allAssessments)
        {
            if (!string.IsNullOrEmpty(assessment.HazardCode) && hazardCodes.Contains(assessment.HazardCode))
            {
                validAssessmentCodes.Add(assessment.Code);
                Console.WriteLine($"🔍 Added RiskAssessment by HazardCode: {assessment.Code} (HazardCode: {assessment.HazardCode})");
            }

            if (assessment.IdentifiedHazardIds?.Any(hazardCodes.Contains) == true)
            {
                validAssessmentCodes.Add(assessment.Code);
                Console.WriteLine($"🔍 Added RiskAssessment by IdentifiedHazardIds: {assessment.Code}");
            }
        }

        Console.WriteLine($"🔍 Valid assessment codes for filtering: {string.Join(", ", validAssessmentCodes)}");

        // ✅ IMPROVED FILTERING: Load analyses for our hazards that belong to valid assessments
        var existingAnalyses = allAnalyses
            .Where(ra => hazardCodes.Contains(ra.HazardCode) &&
                        (validAssessmentCodes.Contains(ra.RiskAssessmentCode) ||
                         string.IsNullOrEmpty(ra.RiskAssessmentCode))) // Also include orphaned analyses
            .ToList();

        Console.WriteLine($"🔍 After filtering: Found {existingAnalyses.Count} relevant RiskAnalysis entities");

        // ✅ Load existing analyses into Step3 dictionary with debugging
        foreach (var analysis in existingAnalyses)
        {
            // Fix RiskAssessmentCode if needed
            if (string.IsNullOrEmpty(analysis.RiskAssessmentCode) || analysis.RiskAssessmentCode == "RA-0000")
            {
                var assessment = allAssessments
                    .FirstOrDefault(a => a.RiskAssessmentCategory == RiskAssessmentCategory.Technical &&
                                       (a.HazardCode == analysis.HazardCode ||
                                        a.IdentifiedHazardIds?.Contains(analysis.HazardCode) == true));

                if (assessment != null)
                {
                    analysis.RiskAssessmentCode = assessment.Code;
                    Console.WriteLine($"🔧 Fixed RiskAssessmentCode for {analysis.HazardCode}: {assessment.Code}");
                }
            }

            Step3RiskAnalyses[analysis.HazardCode] = analysis;
            Console.WriteLine($"✅ Loaded RiskAnalysis for {analysis.HazardCode}: {analysis.Code} (Assessment: {analysis.RiskAssessmentCode})");
        }

        Console.WriteLine($"✅ Step3RiskAnalyses now contains {Step3RiskAnalyses.Count} entries: {string.Join(", ", Step3RiskAnalyses.Keys)}");

        // ✅ Create new analyses for missing hazards (but only if they don't exist in database)
        var hazardsWithoutAnalysis = availableHazards
            .Where(h => !Step3RiskAnalyses.ContainsKey(h.Code))
            .ToList();

        if (hazardsWithoutAnalysis.Any())
        {
            Console.WriteLine($"⚠️  {hazardsWithoutAnalysis.Count} hazards don't have RiskAnalysis: {string.Join(", ", hazardsWithoutAnalysis.Select(h => h.Code))}");
            await CreateNewRiskAnalysesForNewHazards(hazardsWithoutAnalysis, allAssessments);
        }

        Console.WriteLine($"🏁 LoadExistingRiskAnalysesAsync completed. Final Step3RiskAnalyses count: {Step3RiskAnalyses.Count}");
    }

    /// <summary>
    /// Create new RiskAnalysis entities ONLY for hazards that were added in Step 2 and don't have analysis yet
    /// </summary>
    private async Task CreateNewRiskAnalysesForNewHazards(List<Hazard> newHazards, IEnumerable<RiskAssessment> availableAssessments)
    {
        if (Mediator == null || !newHazards.Any()) return;

        try
        {
            foreach (var hazard in newHazards)
            {
                // Find the appropriate assessment for this hazard
                var assessment = availableAssessments
                    .FirstOrDefault(a => a.HazardCode == hazard.Code ||
                                        a.IdentifiedHazardIds?.Contains(hazard.Code) == true);

                if (assessment == null)
                {
                    // If no specific assessment found, use the first technical assessment
                    assessment = availableAssessments
                        .FirstOrDefault(a => a.RiskAssessmentCategory == RiskAssessmentCategory.Technical);
                }

                if (assessment != null)
                {
                    // Create new RiskAnalysis entity for the newly added hazard
                    var newAnalysis = new RiskAnalysis(new RiskAnalysisID("RA-0000"))
                    {
                        Code = "RA-0000", // Will be generated by database
                        HazardCode = hazard.Code,
                        RiskAssessmentCode = assessment.Code,
                        AssessmentType = RiskAnalysisType.Initial,
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

                    // Save via CQRS
                    var createCommand = new CreateRiskAnalysisCommand(newAnalysis);
                    var result = await Mediator.SendAsync(createCommand, CancellationToken.None);

                    if (result.IsSuccess && result.Value != null)
                    {
                        // Add the newly created analysis to our dictionary
                        Step3RiskAnalyses[hazard.Code] = result.Value;
                        Console.WriteLine($"✅ Created new RiskAnalysis for newly added hazard {hazard.Code} with code {result.Value.Code}");
                    }
                    else
                    {
                        Console.WriteLine($"❌ Failed to create RiskAnalysis for newly added hazard {hazard.Code}: {result.Error?.Message ?? "Unknown error"}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating new RiskAnalysis entities for newly added hazards: {ex.Message}");
        }
    }



    public async Task ApplyToAssessmentAsync(RiskAssessment assessment, List<Hazard> availableHazards, int currentStep)
    {
        if (Mediator == null || assessment == null || availableHazards == null) return;

        try
        {
            assessment.CompleteStep(currentStep == 5 ? 5 : 3);

            await SaveRiskAnalysesAsync(assessment);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying Step 3 to assessment: {ex.Message}");
        }
    }

    private async Task SaveRiskAnalysesAsync(RiskAssessment assessment)
    {
        await SaveRiskAnalysesAsync(assessment, 3);
    }
    
    private async Task SaveRiskAnalysesAsync(RiskAssessment assessment, int currentStep)
    {
        foreach (var analysisKvp in Step3RiskAnalyses)
        {
            try
            {
                var analysis = analysisKvp.Value;
                var hazardCode = analysisKvp.Key;
                
               
                

                // Ensure RiskAssessmentCode is properly set
                if (string.IsNullOrEmpty(analysis.RiskAssessmentCode) && assessment != null)
                {
                    analysis.RiskAssessmentCode = assessment.Code;
                    analysis.UpdatedBy = AuthService.CurrentUserDisplayName;
                    analysis.UpdatedDate = DateTime.UtcNow;
                }

                               
                // Update existing RiskAnalysis
                var updateCommand = new UpdateRiskAnalysisCommand(analysis);
                var updateResult = await Mediator.SendAsync(updateCommand, CancellationToken.None);
                    
                if (updateResult.IsSuccess)
                {
                    Step3RiskAnalyses[hazardCode] = updateResult.Value;
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating RiskAnalysis for hazard {analysisKvp.Key}: {ex.Message}");
            }
        }
    }

    public async Task LoadFromAssessmentAsync(RiskAssessment assessment, List<Hazard> reportHazards)
    {
        if (assessment == null) return;
        await LoadExistingRiskAnalysesAsync(reportHazards);
    }

    
}

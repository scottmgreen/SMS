namespace SMS3.Components.Pages.SMSRiskManagement;

/// <summary>
/// Step 3: Risk Analysis
/// </summary>
public class Step3Model
{
    
  
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


    //public async Task LoadExistingRiskAnalysesAsync(IMediator mediator,List<Hazard> availableHazards)
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


    //    var initialAssessmentCodes = assessmentsResult.Value
    //        .Select(a => a.Code)
    //        .ToHashSet();

    //    var initialAnalyses = allAnalysisResult.Value
    //        .Where(ra => hazardCodes.Contains(ra.HazardCode) && initialAssessmentCodes.Contains(ra.RiskAssessmentCode)  ) 
    //        .ToList();

    //    foreach (var analysis in initialAnalyses)
    //    {

    //        // Ensure the RiskAssessmentCode is set correctly for Initial assessments
    //        if (string.IsNullOrEmpty(analysis.RiskAssessmentCode) || analysis.RiskAssessmentCode == "RA-0000")
    //        {
    //            // Find the correct Initial assessment code for this analysis
    //            var initialAssessment = assessmentsResult.Value
    //                .FirstOrDefault(a => hazardCodes.Contains(a.HazardCode ?? string.Empty));

    //            if (initialAssessment != null)
    //            {
    //                analysis.RiskAssessmentCode = initialAssessment.Code;
    //            }
    //        }

    //        Step3RiskAnalyses[analysis.HazardCode] = analysis;
    //    }
    //}

    public async Task LoadExistingRiskAnalysesAsync(IMediator mediator, List<Hazard> availableHazards)
    {
        if (mediator == null || availableHazards == null) return;

        var hazardCodes = availableHazards.Select(h => h.Code).ToList();

        var allAnalysisQuery = new GetAllRiskAnalysisQuery();
        var allAnalysisResult = await mediator.SendAsync(allAnalysisQuery, CancellationToken.None);

        if (!allAnalysisResult.IsSuccess || allAnalysisResult.Value == null)
        {
            return;
        }

        var allAssessmentsQuery = new GetAllRiskAssessmentsQuery();
        var assessmentsResult = await mediator.SendAsync(allAssessmentsQuery, CancellationToken.None);

        if (!assessmentsResult.IsSuccess || assessmentsResult.Value == null)
        {
            return;
        }

        var assessmentCodes = assessmentsResult.Value
            .Select(a => a.Code)
            .ToHashSet();

        var existingAnalyses = allAnalysisResult.Value
            .Where(ra => hazardCodes.Contains(ra.HazardCode) && assessmentCodes.Contains(ra.RiskAssessmentCode))
            .ToList();

        // Load existing analyses into Step3 dictionary
        foreach (var analysis in existingAnalyses)
        {
            // Ensure the RiskAssessmentCode is set correctly
            if (string.IsNullOrEmpty(analysis.RiskAssessmentCode) || analysis.RiskAssessmentCode == "RA-0000")
            {
                var assessment = assessmentsResult.Value
                    .FirstOrDefault(a => hazardCodes.Contains(a.HazardCode ?? string.Empty));

                if (assessment != null)
                {
                    analysis.RiskAssessmentCode = assessment.Code;
                }
            }

            Step3RiskAnalyses[analysis.HazardCode] = analysis;
        }

        // ✅ ONLY create RiskAnalysis for hazards added in Step 2 (hazards without existing analysis)
        var hazardsWithoutAnalysis = availableHazards
            .Where(h => !Step3RiskAnalyses.ContainsKey(h.Code))
            .ToList();

        if (hazardsWithoutAnalysis.Any())
        {
            Console.WriteLine($"Creating new RiskAnalysis entities for {hazardsWithoutAnalysis.Count} newly added hazards");
            await CreateNewRiskAnalysesForNewHazards(mediator, hazardsWithoutAnalysis, assessmentsResult.Value);
        }
    }

    /// <summary>
    /// Create new RiskAnalysis entities ONLY for hazards that were added in Step 2 and don't have analysis yet
    /// </summary>
    private async Task CreateNewRiskAnalysesForNewHazards(IMediator mediator, List<Hazard> newHazards, IEnumerable<RiskAssessment> availableAssessments)
    {
        if (mediator == null || !newHazards.Any()) return;

        try
        {
            foreach (var hazard in newHazards)
            {
                // Find the appropriate assessment for this hazard
                var assessment = availableAssessments
                    .FirstOrDefault(a => a.HazardCode == hazard.Code ||
                                        (a.IdentifiedHazardIds?.Contains(hazard.Code) == true));

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
                    var result = await mediator.SendAsync(createCommand, CancellationToken.None);

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



    public async Task ApplyToAssessmentAsync(IMediator mediator, RiskAssessment assessment, List<Hazard> availableHazards, int currentStep)
    {
        if (mediator == null || assessment == null || availableHazards == null) return;

        try
        {
            assessment.CompleteStep(currentStep == 5 ? 5 : 3);

            await SaveRiskAnalysesAsync(mediator, assessment);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying Step 3 to assessment: {ex.Message}");
        }
    }

    private async Task SaveRiskAnalysesAsync(IMediator mediator, RiskAssessment assessment)
    {
        await SaveRiskAnalysesAsync(mediator, assessment, 3);
    }
    
    private async Task SaveRiskAnalysesAsync(IMediator mediator, RiskAssessment assessment, int currentStep)
    {
        foreach (var analysisKvp in Step3RiskAnalyses)
        {
            try
            {
                var analysis = analysisKvp.Value;
                var hazardCode = analysisKvp.Key;
                
                // Determine AssessmentType based on CurrentStep
                

                // Ensure RiskAssessmentCode is properly set
                if (string.IsNullOrEmpty(analysis.RiskAssessmentCode) && assessment != null)
                {
                    analysis.RiskAssessmentCode = assessment.Code;
                }

                               
                // Update existing RiskAnalysis
                var updateCommand = new UpdateRiskAnalysisCommand(analysis);
                var updateResult = await mediator.SendAsync(updateCommand, CancellationToken.None);
                    
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

    public async Task LoadFromAssessmentAsync(IMediator mediator, RiskAssessment assessment, List<Hazard> reportHazards)
    {
        if (assessment == null) return;

        
        await LoadExistingRiskAnalysesAsync(mediator, reportHazards);
    }

    
}

using System.ComponentModel.DataAnnotations;

namespace SMS3.Components.Pages.SMSRiskManagement;

/// <summary>
/// Step 1: System Description and 5M Framework Analysis
/// </summary>
public class Step1Model
{
    #region System Overview Properties

    [Required(ErrorMessage = "Lead Assessor is required.")]
    [Display(Name = "Lead Assessor")]
    public string LeadAssessor { get; set; } = string.Empty;

    [Required(ErrorMessage = "System Description is required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "System Description must be between 10 and 1000 characters.")]
    [Display(Name = "System Description")]
    public string SystemDescription { get; set; } = string.Empty;

    [Required(ErrorMessage = "System Boundaries are required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "System Boundaries must be between 10 and 1000 characters.")]
    [Display(Name = "System Boundaries")]
    public string SystemBoundaries { get; set; } = string.Empty;

    [Required(ErrorMessage = "System Purpose is required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "System Purpose must be between 10 and 1000 characters.")]
    [Display(Name = "System Purpose")]
    public string SystemPurpose { get; set; } = string.Empty;

    #endregion

    #region 5M Framework Properties

    [Required(ErrorMessage = "Personnel Factors (5M People) are required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Personnel Factors must be between 10 and 1000 characters.")]
    [Display(Name = "Personnel Factors")]
    public string FiveMPersonnel { get; set; } = string.Empty;

    [Required(ErrorMessage = "Equipment Factors (5M Equipment) are required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Equipment Factors must be between 10 and 1000 characters.")]
    [Display(Name = "Equipment Factors")]
    public string FiveMEquipment { get; set; } = string.Empty;

    [Required(ErrorMessage = "Procedure Factors (5M Procedures) are required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Procedure Factors must be between 10 and 1000 characters.")]
    [Display(Name = "Procedure Factors")]
    public string FiveMProcedures { get; set; } = string.Empty;

    [Required(ErrorMessage = "Resource Factors (5M Resources) are required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Resource Factors must be between 10 and 1000 characters.")]
    [Display(Name = "Resource Factors")]
    public string FiveMResources { get; set; } = string.Empty;

    [Required(ErrorMessage = "Physical Environment Factors (5M Environment) are required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Physical Environment must be between 10 and 1000 characters.")]
    [Display(Name = "Physical Environment")]
    public string FiveMPhysicalEnvironment { get; set; } = string.Empty;

    [Required(ErrorMessage = "Operational Environment Factors are required.")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Operational Environment must be between 10 and 1000 characters.")]
    [Display(Name = "Operational Environment")]
    public string FiveMOperationalEnvironment { get; set; } = string.Empty;

    #endregion

    #region Stakeholder Properties

    [Display(Name = "Stakeholder Groups")]
    public string StakeholderGroups { get; set; } = string.Empty;

    [Display(Name = "Individual Stakeholders")]
    public string SelectedIndividualStakeholders { get; set; } = string.Empty;

    public List<string> SelectedStakeholderGroupIds { get; set; } = new();
    public List<string> SelectedIndividualStakeholderIds { get; set; } = new();

    #endregion

    #region Validation and Application Methods

    public (bool isValid, string message) Validate()
    {
        var step1Fields = new Dictionary<string, string>
        {
            { nameof(LeadAssessor), LeadAssessor },
            { nameof(SystemDescription), SystemDescription },
            { nameof(SystemBoundaries), SystemBoundaries },
            { nameof(SystemPurpose), SystemPurpose },
            { nameof(FiveMPersonnel), FiveMPersonnel },
            { nameof(FiveMEquipment), FiveMEquipment },
            { nameof(FiveMProcedures), FiveMProcedures },
            { nameof(FiveMResources), FiveMResources },
            { nameof(FiveMPhysicalEnvironment), FiveMPhysicalEnvironment },
            { nameof(FiveMOperationalEnvironment), FiveMOperationalEnvironment }
        };

        int fieldsWithData = 0;
        var missingFields = new List<string>();

        foreach (var field in step1Fields)
        {
            var value = field.Value?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(value))
            {
                missingFields.Add(field.Key);
            }
            else if (value.Length >= 10)
            {
                fieldsWithData++;
            }
        }

        if (fieldsWithData < 4)
        {
            return (false, $"Need at least 4 complete fields (found {fieldsWithData}). Missing: {string.Join(", ", missingFields)}");
        }

        return (true, $"Step 1 validation passed with {fieldsWithData} complete fields");
    }

    public void ApplyToAssessment(RiskAssessment assessment)
    {
        assessment.SystemDescription = SystemDescription.Trim();
        assessment.SystemBoundaries = SystemBoundaries.Trim();
        assessment.SystemPurpose = SystemPurpose.Trim();
        assessment.FiveMPersonnel = FiveMPersonnel.Trim();
        assessment.FiveMEquipment = FiveMEquipment.Trim();
        assessment.FiveMProcedures = FiveMProcedures.Trim();
        assessment.FiveMResources = FiveMResources.Trim();
        assessment.FiveMOperationalEnvironment = FiveMOperationalEnvironment.Trim();
        assessment.FiveMPhysicalEnvironment = FiveMPhysicalEnvironment.Trim();

        assessment.LeadAssessorId = LeadAssessor.Trim();

        // Handle stakeholder groups
        if (!string.IsNullOrEmpty(StakeholderGroups.Trim()))
        {
            var groups = StakeholderGroups.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(g => g.Trim())
                .Where(g => !string.IsNullOrEmpty(g));

            foreach (var group in groups)
            {
                assessment.AddStakeholder(group);
            }
        }

        assessment.CompleteStep(1);
    }

    public void LoadFromAssessment(RiskAssessment assessment)
    {
        if (assessment == null) return;

        if (string.IsNullOrEmpty(LeadAssessor)) LeadAssessor = assessment.LeadAssessorId ?? string.Empty;
        if (string.IsNullOrEmpty(SystemDescription)) SystemDescription = assessment.SystemDescription ?? string.Empty;
        if (string.IsNullOrEmpty(SystemBoundaries)) SystemBoundaries = assessment.SystemBoundaries ?? string.Empty;
        if (string.IsNullOrEmpty(SystemPurpose)) SystemPurpose = assessment.SystemPurpose ?? string.Empty;

        if (string.IsNullOrEmpty(FiveMPersonnel)) FiveMPersonnel = assessment.FiveMPersonnel ?? string.Empty;
        if (string.IsNullOrEmpty(FiveMEquipment)) FiveMEquipment = assessment.FiveMEquipment ?? string.Empty;
        if (string.IsNullOrEmpty(FiveMProcedures)) FiveMProcedures = assessment.FiveMProcedures ?? string.Empty;
        if (string.IsNullOrEmpty(FiveMResources)) FiveMResources = assessment.FiveMResources ?? string.Empty;
        if (string.IsNullOrEmpty(FiveMPhysicalEnvironment)) FiveMPhysicalEnvironment = assessment.FiveMPhysicalEnvironment ?? string.Empty;
        if (string.IsNullOrEmpty(FiveMOperationalEnvironment)) FiveMOperationalEnvironment = assessment.FiveMOperationalEnvironment ?? string.Empty;

        // Initialize stakeholder collections if empty - IMPORTANT FOR UI BINDING
        if (SelectedStakeholderGroupIds == null)
        {
            SelectedStakeholderGroupIds = new List<string>();
        }

        if (SelectedIndividualStakeholderIds == null)
        {
            SelectedIndividualStakeholderIds = new List<string>();
        }

        // Load stakeholder data from assessment if available
        if (assessment.StakeholderIds?.Any() == true)
        {
            foreach (var stakeholderId in assessment.StakeholderIds)
            {
                // For now, treat all stakeholders as groups since we don't have a clear distinction
                // This can be enhanced later to properly categorize groups vs individuals
                if (!SelectedStakeholderGroupIds.Contains(stakeholderId))
                {
                    SelectedStakeholderGroupIds.Add(stakeholderId);
                }
            }

            // Update the StakeholderGroups string for display
            StakeholderGroups = string.Join(", ", SelectedStakeholderGroupIds);
        }
    }

    #endregion

    #region Helper Classes

    public class StakeholderSelection
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Organization { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    #endregion
}

/// <summary>
/// Step 2: Hazard Identification
/// </summary>
public class Step2Model
{
    public List<string> HazardIds { get; set; } = new();
    public List<string> HazardDescriptions { get; set; } = new();
    public List<string> HazardCategories { get; set; } = new();

    public (bool isValid, string message) Validate()
    {
        // Validation is handled by parent component checking ReportHazards
        return (true, "Step 2 validation handled by main validator");
    }

    public void LoadFromAssessment(RiskAssessment assessment)
    {
        if (assessment == null) return;

        // ? FIXED: Properly load hazard ID from assessment
        if (assessment.IdentifiedHazardIds?.Any() == true)
        {
            HazardIds = assessment.IdentifiedHazardIds.ToList();

            // Initialize descriptions and categories lists to match hazard ID count
            HazardDescriptions = new List<string>(new string[HazardIds.Count]);
            HazardCategories = new List<string>(new string[HazardIds.Count]);

            // Fill with placeholder data - actual hazard data will be loaded separately
            for (int i = 0; i < HazardIds.Count; i++)
            {
                HazardDescriptions[i] = $"Hazard {HazardIds[i]}";
                HazardCategories[i] = "General";
            }
        }
        else
        {
            // Initialize empty lists if no hazards in assessment
            HazardIds = new List<string>();
            HazardDescriptions = new List<string>();
            HazardCategories = new List<string>();
        }
    }

    public void ApplyToAssessment(RiskAssessment assessment)
    {
        // ? FIXED: Update assessment with all identified hazards
        assessment.ClearIdentifiedHazards(); // Clear existing hazards first

        foreach (var hazardId in HazardIds)
        {
            if (!string.IsNullOrEmpty(hazardId))
            {
                var hazardDescription = HazardDescriptions.Count > HazardIds.IndexOf(hazardId)
                    ? HazardDescriptions[HazardIds.IndexOf(hazardId)]
                    : $"Hazard {hazardId}";

                assessment.AddIdentifiedHazard(hazardId, hazardDescription);
            }
        }

        assessment.CompleteStep(2);
    }
}

/// <summary>
/// Step 3: Risk Analysis
/// </summary>
public class Step3Model
{
    #region Risk Analysis Method Properties

    public string RiskAnalysisMethod { get; set; } = "SMS Risk Matrix";
    public string RiskCriteria { get; set; } = string.Empty;

    #endregion

    #region Multiple Hazard Risk Analysis Properties

    // ? REFACTORED: Use actual RiskAnalysis entities instead of helper class
    public Dictionary<string, RiskAnalysis> HazardRiskAnalyses { get; set; } = new();

    #endregion

    #region Validation and Application Methods

    public (bool isValid, string message) Validate(List<Hazard> availableHazards = null)
    {
        if (availableHazards == null || !availableHazards.Any())
        {
            return (false, "No hazards available for risk analysis");
        }

        var incompleteHazards = new List<string>();
        var analysisCount = 0;

        foreach (var hazard in availableHazards)
        {
            if (HazardRiskAnalyses.TryGetValue(hazard.Code, out var analysis))
            {
                var worstOutcomeValid = !string.IsNullOrWhiteSpace(analysis.WorstCredibleOutcome) && analysis.WorstCredibleOutcome.Length >= 10;
                var rootCauseValid = !string.IsNullOrWhiteSpace(analysis.RootCause) && analysis.RootCause.Length >= 10;
                var additionalCommentsValid = !string.IsNullOrWhiteSpace(analysis.AdditionalComments) && analysis.AdditionalComments.Length >= 10;

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

        return (true, $"Step 3 validation passed - {analysisCount}/{availableHazards.Count} hazards have complete risk analysis");
    }

    public RiskAnalysis GetHazardAnalysis(string hazardCode)
    {
        if (string.IsNullOrEmpty(hazardCode))
        {
            return CreateNewRiskAnalysis(hazardCode ?? string.Empty, string.Empty);
        }

        if (!HazardRiskAnalyses.ContainsKey(hazardCode))
        {
            HazardRiskAnalyses[hazardCode] = CreateNewRiskAnalysis(hazardCode, string.Empty);
        }

        return HazardRiskAnalyses[hazardCode];
    }

    private RiskAnalysis CreateNewRiskAnalysis(string hazardCode, string riskAssessmentCode)
    {
        // ? FIX: Generate a predictable ID based on HazardCode and RiskAssessmentCode 
        // This ensures we don't create multiple records for the same hazard+assessment combo
        var analysisId = $"RA-{hazardCode}-{riskAssessmentCode}".Replace("HZ-", "").Replace("RS-", "").Replace("RR-", "");
        
        return new RiskAnalysis(new RiskAnalysisID(analysisId))
        {
            Code = analysisId,
            HazardCode = hazardCode,
            RiskAssessmentCode = riskAssessmentCode,
            WorstCredibleOutcome = string.Empty,
            RootCause = string.Empty,
            AdditionalComments = string.Empty
        };
    }

    public async Task LoadExistingRiskAnalysesAsync(IMediator mediator, RiskAssessment assessment, List<Hazard> availableHazards)
    {
        if (mediator == null || assessment == null || availableHazards == null) return;

        Console.WriteLine($"?? Step3: Loading INITIAL RiskAnalyses for {availableHazards.Count} hazards");
        Console.WriteLine($"    ?? Current Assessment: {assessment.Code} (Type: {assessment.AssessmentType})");

        foreach (var hazard in availableHazards)
        {
            try
            {
                // ? CRITICAL FIX: Each hazard may have its own Initial RiskAssessment
                // We need to find the Initial RiskAssessment for THIS specific hazard, then find its RiskAnalysis
                
                // First, get all RiskAssessments for this hazard
                var hazardAssessmentsQuery = new GetRiskAssessmentsByHazardCodeQuery(new HazardID(hazard.Code));
                var assessmentsResult = await mediator.SendAsync(hazardAssessmentsQuery, CancellationToken.None);
                
                if (assessmentsResult.IsSuccess && assessmentsResult.Value?.Any() == true)
                {
                    // Find the Initial RiskAssessment for this hazard
                    var hazardInitialAssessment = assessmentsResult.Value
                        .FirstOrDefault(x => x.AssessmentType == RiskAssessmentType.Initial);
                    
                    if (hazardInitialAssessment != null)
                    {
                        Console.WriteLine($"?? Step3: Found Initial RiskAssessment {hazardInitialAssessment.Code} for Hazard {hazard.Code}");
                        
                        // Now find the RiskAnalysis for this hazard + its specific Initial RiskAssessment
                        var query = new GetRiskAnalysisByHazardAndAssessmentQuery(hazard.Code, hazardInitialAssessment.Code);
                        var result = await mediator.SendAsync(query, CancellationToken.None);

                        if (result.IsSuccess && result.Value != null)
                        {
                            // ? Found existing Initial RiskAnalysis - load it for Step 3 editing
                            HazardRiskAnalyses[hazard.Code] = result.Value;
                            Console.WriteLine($"? Step3: Loaded INITIAL RiskAnalysis {result.Value.Code} for Hazard {hazard.Code}");
                            Console.WriteLine($"    ?? Assessment: {hazardInitialAssessment.Code}");
                            Console.WriteLine($"    ?? Data: WCO={!string.IsNullOrEmpty(result.Value.WorstCredibleOutcome)}, RC={!string.IsNullOrEmpty(result.Value.RootCause)}, AC={!string.IsNullOrEmpty(result.Value.AdditionalComments)}");
                        }
                        else
                        {
                            Console.WriteLine($"?? Step3: No INITIAL RiskAnalysis found for Hazard {hazard.Code} + Assessment {hazardInitialAssessment.Code}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"?? Step3: No Initial RiskAssessment found for Hazard {hazard.Code}");
                    }
                }
                else
                {
                    Console.WriteLine($"?? Step3: No RiskAssessments found for Hazard {hazard.Code}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Step3: Error loading INITIAL RiskAnalysis for hazard {hazard.Code}: {ex.Message}");
            }
        }
        
        Console.WriteLine($"?? Step3: Loaded {HazardRiskAnalyses.Count} INITIAL RiskAnalyses out of {availableHazards.Count} hazards");
    }

    public void InitializeHazardAnalyses(List<Hazard> availableHazards)
    {
        if (availableHazards == null) return;

        foreach (var hazard in availableHazards)
        {
            if (!HazardRiskAnalyses.ContainsKey(hazard.Code))
            {
                HazardRiskAnalyses[hazard.Code] = CreateNewRiskAnalysis(hazard.Code, string.Empty);
            }
        }
    }

    public void ApplyToAssessment(RiskAssessment assessment)
    {
        assessment.RiskAnalysisMethod = RiskAnalysisMethod;
        assessment.RiskCriteria = RiskCriteria;
        assessment.CompleteStep(3);
    }

    public async Task ApplyToAssessmentAsync(RiskAssessment assessment, IMediator mediator, List<Hazard> availableHazards)
    {
        if (mediator == null || assessment == null || availableHazards == null) return;

        try
        {
            // Apply to assessment
            assessment.RiskAnalysisMethod = RiskAnalysisMethod;
            assessment.RiskCriteria = RiskCriteria;
            assessment.CompleteStep(3);

            // Save all RiskAnalysis entities
            await SaveRiskAnalysesAsync(mediator, assessment);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying Step 3 to assessment: {ex.Message}");
        }
    }

    private async Task SaveRiskAnalysesAsync(IMediator mediator, RiskAssessment assessment)
    {
        Console.WriteLine($"?? Step3: Saving INITIAL RiskAnalyses for {HazardRiskAnalyses.Count} hazards");
        Console.WriteLine($"    ?? Current Assessment: {assessment.Code} (Type: {assessment.AssessmentType})");

        foreach (var analysisKvp in HazardRiskAnalyses)
        {
            try
            {
                var analysis = analysisKvp.Value;
                var hazardCode = analysisKvp.Key;
                
                Console.WriteLine($"?? Step3: Processing INITIAL RiskAnalysis for Hazard {hazardCode}");
                Console.WriteLine($"    ?? RiskAnalysis Code: {analysis.Code}");
                Console.WriteLine($"    ?? Current Assessment Code: {analysis.RiskAssessmentCode}");
                Console.WriteLine($"    ?? Has WCO: {!string.IsNullOrEmpty(analysis.WorstCredibleOutcome)} (Length: {analysis.WorstCredibleOutcome?.Length ?? 0})");
                Console.WriteLine($"    ?? Has RC: {!string.IsNullOrEmpty(analysis.RootCause)} (Length: {analysis.RootCause?.Length ?? 0})");
                Console.WriteLine($"    ?? Has AC: {!string.IsNullOrEmpty(analysis.AdditionalComments)} (Length: {analysis.AdditionalComments?.Length ?? 0})");

                // ? CRITICAL FIX: Keep the original RiskAssessmentCode from the loaded analysis
                // Don't overwrite it with the passed assessment.Code since each hazard may have its own assessment
                // The analysis.RiskAssessmentCode was correctly set during loading
                
                // ? ALWAYS UPDATE - we loaded existing Initial RiskAnalysis records, so we always update them
                var updateCommand = new UpdateRiskAnalysisCommand(analysis);
                var updateResult = await mediator.SendAsync(updateCommand, CancellationToken.None);
                
                if (updateResult.IsSuccess)
                {
                    // Update our local dictionary with the updated record
                    HazardRiskAnalyses[hazardCode] = updateResult.Value;
                    Console.WriteLine($"? Step3: Successfully updated INITIAL RiskAnalysis {analysis.Code} for Hazard {hazardCode}");
                    Console.WriteLine($"    ?? Final Assessment Code: {updateResult.Value.RiskAssessmentCode}");
                }
                else
                {
                    Console.WriteLine($"? Step3: Failed to update INITIAL RiskAnalysis {analysis.Code}: {updateResult.Error?.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Step3: Error updating INITIAL RiskAnalysis for hazard {analysisKvp.Key}: {ex.Message}");
            }
        }

        Console.WriteLine($"?? Step3: Completed saving INITIAL RiskAnalyses for {HazardRiskAnalyses.Count} hazards");
    }

    public async Task LoadFromAssessmentAsync(RiskAssessment assessment, IMediator mediator, List<Hazard> reportHazards)
    {
        if (assessment == null) return;

        // Load basic properties
        if (string.IsNullOrEmpty(RiskAnalysisMethod) && !string.IsNullOrEmpty(assessment.RiskAnalysisMethod))
        {
            RiskAnalysisMethod = assessment.RiskAnalysisMethod;
        }

        if (string.IsNullOrEmpty(RiskCriteria) && !string.IsNullOrEmpty(assessment.RiskCriteria))
        {
            RiskCriteria = assessment.RiskCriteria;
        }

        // Load existing RiskAnalysis entities
        await LoadExistingRiskAnalysesAsync(mediator, assessment, reportHazards);
    }

    // Maintain backward compatibility
    public void LoadFromAssessment(RiskAssessment assessment, List<Hazard> reportHazards)
    {
        if (assessment == null) return;

        if (string.IsNullOrEmpty(RiskAnalysisMethod) && !string.IsNullOrEmpty(assessment.RiskAnalysisMethod))
        {
            RiskAnalysisMethod = assessment.RiskAnalysisMethod;
        }

        if (string.IsNullOrEmpty(RiskCriteria) && !string.IsNullOrEmpty(assessment.RiskCriteria))
        {
            RiskCriteria = assessment.RiskCriteria;
        }

        // Initialize empty analyses for backward compatibility
        InitializeHazardAnalyses(reportHazards);
    }

    #endregion
}

/// <summary>
/// Step 4: Risk Assessment & Scoring Panel
/// </summary>
public class Step4Model
{
    #region Risk Assessment Properties

    //public string TolerabilityFramework { get; set; } = "PDX-SMS Default";
    //public string RiskAcceptanceCriteria { get; set; } = string.Empty;

    #endregion

    #region Scoring Panel Properties

    public List<string> SelectedPanelMembers { get; set; } = new();
    public Dictionary<string, List<string>> HazardPanelMembers { get; set; } = new();
    public Dictionary<string, List<PanelMemberScoreData>> PanelScores { get; set; } = new();
    public Dictionary<string, double> HazardAverageScores { get; set; } = new();
    public Dictionary<string, string> HazardRiskLevels { get; set; } = new();
    public Dictionary<string, string> HazardMatrixCodes { get; set; } = new(); // NEW: Store matrix codes like "3B", "5A"

    #endregion

    #region Helper Properties for UI

    public List<PanelMemberScoreData> CompletedScores
    {
        get
        {
            return PanelScores.Values
                .SelectMany(scores => scores)
                .Where(score => score.IsComplete)
                .ToList();
        }
    }

    public List<PanelMemberScoreData> PendingScores
    {
        get
        {
            var allExpectedScores = new List<PanelMemberScoreData>();

            foreach (var hazardPanelKvp in HazardPanelMembers)
            {
                var hazardId = hazardPanelKvp.Key;
                var panelMemberIds = hazardPanelKvp.Value;

                foreach (var memberId in panelMemberIds)
                {
                    var existingScore = PanelScores.ContainsKey(hazardId)
                        ? PanelScores[hazardId].FirstOrDefault(s => s.MemberId == memberId)
                        : null;

                    if (existingScore == null || !existingScore.IsComplete)
                    {
                        allExpectedScores.Add(new PanelMemberScoreData
                        {
                            HazardId = hazardId,
                            MemberId = memberId,
                            MemberName = memberId
                        });
                    }
                }
            }

            return allExpectedScores;
        }
    }

    #endregion

    #region Validation and Application Methods

    public (bool isValid, string message) Validate()
    {
        //if (string.IsNullOrWhiteSpace(TolerabilityFramework))
        //{
        //    return (false, "Tolerability framework is required");
        //}

        return (true, "Step 4 validation passed");
    }

    public async Task ApplyToAssessmentAsync(RiskAssessment assessment, IMediator mediator, List<Hazard> availableHazards)
    {
        // CRITICAL: Save Step 4 Risk Assessment data with calculated scores
        await SaveStep4RiskAssessmentAsync(assessment, mediator, availableHazards);

        // Apply to assessment
        //assessment.TolerabilityFramework = TolerabilityFramework;
        //assessment.RiskAcceptanceCriteria = RiskAcceptanceCriteria;
        assessment.CompleteStep(4);
    }

    public void ApplyToAssessment(RiskAssessment assessment)
    {
        // Legacy method - still needed for synchronous calls
        //assessment.TolerabilityFramework = TolerabilityFramework;
        //assessment.RiskAcceptanceCriteria = RiskAcceptanceCriteria;
        assessment.CompleteStep(4);
    }

    /// <summary>
    /// Save the overall risk assessment with calculated final scores from panel consensus
    /// </summary>
    private async Task SaveStep4RiskAssessmentAsync(RiskAssessment assessment, IMediator mediator, List<Hazard> availableHazards)
    {
        if (mediator == null || assessment == null || availableHazards == null) return;

        try
        {
            // Calculate overall final scores from all hazard averages
            var (finalSeverity, finalLikelihood, finalRiskLevel, assessmentRationale) = CalculateOverallRiskAssessment(availableHazards);
            var riskAssessmentId = new RiskAssessmentID(assessment.Code);

            // Use the existing SaveStep4Command to save risk assessment data
            var saveStep4Command = new SaveStep4Command(
                riskAssessmentId,
                finalSeverity,
                finalLikelihood,
                finalRiskLevel,
                "Acceptable", // Default tolerability - can be enhanced
                assessmentRationale);

            var result = await mediator.SendAsync(saveStep4Command, CancellationToken.None);

            if (result.IsSuccess)
            {
                Console.WriteLine($"Successfully saved Step 4 risk assessment data for {assessment.Code}");
            }
            else
            {
                Console.WriteLine($"Failed to save Step 4 risk assessment: {result.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving Step 4 risk assessment: {ex.Message}");
        }
    }

    /// <summary>
    /// Calculate overall risk assessment from all completed hazard assessments
    /// </summary>
    private (int? finalSeverity, int? finalLikelihood, string finalRiskLevel, string assessmentRationale) CalculateOverallRiskAssessment(List<Hazard> availableHazards)
    {
        var completedHazards = HazardAverageScores.Keys.ToList();

        if (!completedHazards.Any())
        {
            return (null, null, "Unknown", "No hazard assessments completed yet.");
        }

        // Calculate weighted average or take highest risk approach
        var avgScores = HazardAverageScores.Values.ToList();
        var maxScore = avgScores.Max();
        var avgScore = avgScores.Average();

        // Use highest risk approach for final severity/likelihood
        var highestRiskHazard = HazardAverageScores.OrderByDescending(kvp => kvp.Value).First();
        var highestRiskHazardCode = highestRiskHazard.Key;

        // Get the matrix code for the highest risk hazard
        var highestRiskMatrixCode = HazardMatrixCodes.ContainsKey(highestRiskHazardCode)
            ? HazardMatrixCodes[highestRiskHazardCode]
            : "Unknown";

        // Parse matrix code back to severity/likelihood
        var (severity, likelihood) = ParseMatrixCode(highestRiskMatrixCode);

        // Determine final risk level
        var finalRiskLevel = severity.HasValue && likelihood.HasValue
            ? GetAviationRiskLevel(severity.Value, likelihood.Value)
            : "Unknown";

        // Build assessment rationale
        var rationale = $"Risk assessment based on {completedHazards.Count} hazard(s). " +
                       $"Highest risk: {highestRiskHazardCode} ({highestRiskMatrixCode}, Risk Level: {HazardRiskLevels.GetValueOrDefault(highestRiskHazardCode, "Unknown")}). " +
                       $"Average risk score: {avgScore:F2}. " +
                       $"Matrix codes assessed: {string.Join(", ", HazardMatrixCodes.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}. " +
                       $"Assessment completed on {DateTime.UtcNow:yyyy-MM-dd HH:mm}.";

        return (severity, likelihood, finalRiskLevel, rationale);
    }

    /// <summary>
    /// Parse matrix code back to severity and likelihood values
    /// </summary>
    private (int? severity, int? likelihood) ParseMatrixCode(string matrixCode)
    {
        if (string.IsNullOrEmpty(matrixCode) || matrixCode.Length < 2)
            return (null, null);

        // Extract severity (first part) and likelihood letter (last part)
        var severityPart = matrixCode.Substring(0, matrixCode.Length - 1);
        var likelihoodLetter = matrixCode.Substring(matrixCode.Length - 1);

        if (!int.TryParse(severityPart, out int severity))
            return (null, null);

        var likelihood = likelihoodLetter.ToUpper() switch
        {
            "A" => 1,
            "B" => 2,
            "C" => 3,
            "D" => 4,
            "E" => 5,
            _ => (int?)null
        };

        return (severity, likelihood);
    }

    /// <summary>
    /// Get aviation risk level from severity and likelihood
    /// </summary>
    private string GetAviationRiskLevel(int severity, int likelihood)
    {
        return (severity, likelihood) switch
        {
            (5, 3) or (5, 4) or (5, 5) or (4, 4) or (4, 5) or (3, 5) => "High",
            (5, 2) or (4, 3) or (3, 4) or (2, 5) => "Medium",
            (5, 1) or (4, 2) or (3, 2) or (3, 3) or (2, 3) or (2, 4) or (1, 5) => "Low",
            (4, 1) or (3, 1) or (2, 1) or (2, 2) or (1, 1) or (1, 2) or (1, 3) or (1, 4) => "Acceptable",
            _ => "Unknown"
        };
    }
    public async Task SaveToAssessment(RiskAssessment assessment, IMediator mediator, List<Hazard> availableHazards)
    {
                
        // Complete the step
        assessment.CompleteStep(4);
    }
    public void LoadFromAssessment(RiskAssessment assessment)
    {
        if (assessment == null) return;

        //if (string.IsNullOrEmpty(TolerabilityFramework))
        //{
        //    TolerabilityFramework = "PDX-SMS Default";
        //}
    }

    /// <summary>
    /// Load existing scoring panels for all hazards using CQRS query
    /// This should be called when Step 4 loads to populate scoring panel data
    /// </summary>
    public async Task LoadExistingScoringPanelsAsync(IMediator mediator, List<Hazard> availableHazards)
    {
        if (mediator == null || availableHazards == null) return;

        foreach (var hazard in availableHazards)
        {
            try
            {
                var query = new GetScoringPanelsByHazardCodeQuery(hazard.Code);
                var result = await mediator.SendAsync(query, CancellationToken.None);

                if (result.IsSuccess && result.Value?.Any() == true)
                {
                    // Initialize collections if needed
                    if (!PanelScores.ContainsKey(hazard.Code))
                    {
                        PanelScores[hazard.Code] = new List<PanelMemberScoreData>();
                    }

                    if (!HazardPanelMembers.ContainsKey(hazard.Code))
                    {
                        HazardPanelMembers[hazard.Code] = new List<string>();
                    }

                    // Process existing scoring panels
                    foreach (var panel in result.Value)
                    {
                        // Add to panel members if not already there
                        if (!HazardPanelMembers[hazard.Code].Contains(panel.SMSUserCode))
                        {
                            HazardPanelMembers[hazard.Code].Add(panel.SMSUserCode);
                        }

                        // Add/update score data if scores exist
                        if (panel.Severity.HasValue && panel.Likelihood.HasValue && panel.Score.HasValue)
                        {
                            var existingScore = PanelScores[hazard.Code]
                                .FirstOrDefault(s => s.MemberId == panel.SMSUserCode);

                            if (existingScore != null)
                            {
                                // Update existing score
                                existingScore.SeverityScore = panel.Severity.Value;
                                existingScore.LikelihoodScore = panel.Likelihood.Value;
                                existingScore.SubmittedDate = (panel.UpdatedDate ?? panel.CreatedDate) ?? DateTime.UtcNow;
                            }
                            else
                            {
                                // Add new score
                                PanelScores[hazard.Code].Add(new PanelMemberScoreData
                                {
                                    HazardId = hazard.Code,
                                    MemberId = panel.SMSUserCode,
                                    MemberName = panel.SMSUserCode, // Will be resolved by UI
                                    SeverityScore = panel.Severity.Value,
                                    LikelihoodScore = panel.Likelihood.Value,
                                    SubmittedDate = (panel.UpdatedDate ?? panel.CreatedDate) ?? DateTime.UtcNow
                                });
                            }
                        }
                    }

                    // Recalculate averages for this hazard
                    RecalculateHazardAverage(hazard.Code);
                }
            }
            catch (Exception ex)
            {
                // Log error but continue with other hazards
                Console.WriteLine($"Error loading scoring panels for hazard {hazard.Code}: {ex.Message}");
            }
        }
    }

    //public void AddPanelMemberScore(string hazardId, PanelMemberScoreData score)
    //{
    //    if (!PanelScores.ContainsKey(hazardId))
    //    {
    //        PanelScores[hazardId] = new List<PanelMemberScoreData>();
    //    }

    //    PanelScores[hazardId].RemoveAll(s => s.MemberId == score.MemberId);
    //    PanelScores[hazardId].Add(score);
    //    RecalculateHazardAverage(hazardId);
    //}

    public void RecalculateHazardAverage(string hazardId)
    {
        if (!PanelScores.ContainsKey(hazardId))
        {
            return;
        }

        var completedScores = PanelScores[hazardId].Where(s => s.IsComplete).ToList();
        if (completedScores.Any())
        {
            var average = completedScores.Average(s => s.CalculatedScore);
            HazardAverageScores[hazardId] = average;
            HazardRiskLevels[hazardId] = DetermineRiskLevel(average);
        }
        else
        {
            HazardAverageScores.Remove(hazardId);
            HazardRiskLevels.Remove(hazardId);
        }
    }

    private string DetermineRiskLevel(double score)
    {
        return score switch
        {
            >= 15 => "High",
            >= 8 => "Medium",
            _ => "Low"
        };
    }

    //public void AssignPanelMembersToHazard(string hazardId, List<string> memberIds)
    //{
    //    HazardPanelMembers[hazardId] = memberIds.ToList();
    //}
    #endregion

    #region Helper Classes

    public class PanelMemberScoreData
    {
        public string PanelMemberId { get; set; } = string.Empty;
        public string MemberId { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public string HazardId { get; set; } = string.Empty;
        public int SeverityScore { get; set; }
        public int LikelihoodScore { get; set; }
        public double CalculatedScore => SeverityScore * LikelihoodScore;
        public string RiskLevel { get; set; } = string.Empty;
        public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;
        public bool IsComplete => SeverityScore > 0 && LikelihoodScore > 0;

        public PanelMemberScoreData()
        {
            MemberId = PanelMemberId;
        }
    }

    #endregion
}

/// <summary>
/// Step 5: Risk Mitigation & Implementation
/// </summary>
public class Step5Model
{
    #region Risk Mitigation Properties

    
    public Dictionary<string, List<string>> SavedMitigationStrategies { get; set; } = new();

    #endregion

    #region Hazard Mitigation Strategies

    // ? UPDATED: Dictionary of HazardCode to List of Mitigation entities (using proper domain entities)
    public Dictionary<string, List<Mitigation>> HazardMitigations { get; set; } = new();

    // Dictionary of HazardCode to List of Panel Member IDs for residual risk assessment
    public Dictionary<string, List<string>> ResidualRiskPanels { get; set; } = new();

    // Dictionary of HazardCode to List of Residual Risk Scores
    public Dictionary<string, List<ResidualRiskScoreData>> ResidualRiskScores { get; set; } = new();

    #endregion

    #region Apply To Assessment String Helper

    public string ApplyToAssessmentString(List<string> appliedMitigations)
    {
        return string.Join("; ", appliedMitigations ?? new List<string>());
    }

    #endregion

    #region Validation and Application Methods

    public (bool isValid, string message) Validate()
    {
        //bool hasImplementation = !string.IsNullOrWhiteSpace(ImplementationStrategy);

        //if (!true)
        //{
        //    return (false, "Implementation strategy is required");
        //}

        return (true, "Step 5 validation passed");
    }

    public void ApplyToAssessment(RiskAssessment assessment)
    {
        assessment.CompleteStep(5);
    }

    /// <summary>
    /// Load Step 5 data from RiskAssessment and associated mitigation entities
    /// ? ENHANCED: Now loads both assessment data AND mitigation entities
    /// </summary>
    public async Task LoadFromAssessmentAsync(RiskAssessment assessment, IMediator mediator, List<Hazard> availableHazards)
    {
        if (assessment == null) return;

        // Load implementation data from assessment (existing functionality)
        LoadFromAssessment(assessment);

        // ? NEW: Load mitigation entities for all hazards
        if (mediator != null && availableHazards?.Any() == true)
        {
            await LoadMitigationEntitiesAsync(mediator, availableHazards);
        }
    }

    /// <summary>
    /// Load basic assessment implementation data (original functionality)
    /// </summary>
    public void LoadFromAssessment(RiskAssessment assessment)
    {
        if (assessment == null) return;

        //// Load implementation data if available
        //if (string.IsNullOrEmpty(ImplementationStrategy))
        //{
        //    ImplementationStrategy = assessment.ImplementationStrategy ?? string.Empty;
        //}

        //if (OverallTargetDate == null && assessment.OverallTargetDate.HasValue)
        //{
        //    OverallTargetDate = assessment.OverallTargetDate;
        //}

        //if (string.IsNullOrEmpty(ImplementationNotes))
        //{
        //    ImplementationNotes = assessment.ImplementationNotes ?? string.Empty;
        //}
    }

    /// <summary>
    /// ? NEW: Load mitigation entities from database for all hazards
    /// </summary>
    private async Task LoadMitigationEntitiesAsync(IMediator mediator, List<Hazard> availableHazards)
    {
        try
        {
            // Initialize dictionaries if needed
            if (HazardMitigations == null)
                HazardMitigations = new Dictionary<string, List<Mitigation>>();
            if (SavedMitigationStrategies == null)
                SavedMitigationStrategies = new Dictionary<string, List<string>>();

            // Load mitigations for each hazard
            foreach (var hazard in availableHazards)
            {
                try
                {
                    var query = new GetMitigationsByHazardCodeQuery(hazard.Code);
                    var result = await mediator.SendAsync(query, CancellationToken.None);

                    if (result.IsSuccess && result.Value?.Any() == true)
                    {
                        var mitigations = result.Value.ToList();

                        // Populate both dictionaries
                        HazardMitigations[hazard.Code] = mitigations;
                        SavedMitigationStrategies[hazard.Code] = mitigations.Select(m => m.Name ?? "Unnamed Mitigation").ToList();

                        // Log successful loading
                        global::System.Console.WriteLine($"? Step5Model: Loaded {mitigations.Count} mitigations for hazard {hazard.Code}");
                    }
                    else
                    {
                        global::System.Console.WriteLine($"??  Step5Model: No mitigations found for hazard {hazard.Code}");
                    }
                }
                catch (Exception ex)
                {
                    global::System.Console.WriteLine($"? Step5Model: Error loading mitigations for hazard {hazard.Code}: {ex.Message}");
                }
            }

            global::System.Console.WriteLine($"? Step5Model: Completed loading mitigations for {availableHazards.Count} hazards");
        }
        catch (Exception ex)
        {
            global::System.Console.WriteLine($"? Step5Model: Error in LoadMitigationEntitiesAsync: {ex.Message}");
        }
    }

    #endregion

    #region Helper Classes

    public class ResidualRiskScoreData
    {
        public string HazardId { get; set; } = string.Empty;
        public string MemberId { get; set; } = string.Empty;
        public int SeverityScore { get; set; }
        public int LikelihoodScore { get; set; }
        public double CalculatedScore { get; set; }
        public bool IsComplete { get; set; }
        public DateTime? SubmittedDate { get; set; }
    }

    #endregion
}
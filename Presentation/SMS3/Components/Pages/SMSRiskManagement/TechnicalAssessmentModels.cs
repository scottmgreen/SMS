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

        if (SelectedStakeholderGroupIds == null)
        {
            SelectedStakeholderGroupIds = new List<string>();
        }

        if (SelectedIndividualStakeholderIds == null)
        {
            SelectedIndividualStakeholderIds = new List<string>();
        }

        if (assessment.StakeholderIds?.Any() == true)
        {
            foreach (var stakeholderId in assessment.StakeholderIds)
            {
                if (!SelectedStakeholderGroupIds.Contains(stakeholderId))
                {
                    SelectedStakeholderGroupIds.Add(stakeholderId);
                }
            }

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
        return (true, "Step 2 validation handled by main validator");
    }

    public void LoadFromAssessment(RiskAssessment assessment)
    {
        if (assessment == null) return;

        if (assessment.IdentifiedHazardIds?.Any() == true)
        {
            HazardIds = assessment.IdentifiedHazardIds.ToList();
            HazardDescriptions = new List<string>(new string[HazardIds.Count]);
            HazardCategories = new List<string>(new string[HazardIds.Count]);

            for (int i = 0; i < HazardIds.Count; i++)
            {
                HazardDescriptions[i] = $"Hazard {HazardIds[i]}";
                HazardCategories[i] = "General";
            }
        }
        else
        {
            HazardIds = new List<string>();
            HazardDescriptions = new List<string>();
            HazardCategories = new List<string>();
        }
    }

    public void ApplyToAssessment(RiskAssessment assessment)
    {
        assessment.ClearIdentifiedHazards();

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
    private const string NEW_RISK_ANALYSIS_SEED_CODE = "RA-0000";

    public string RiskAnalysisMethod { get; set; } = "SMS Risk Matrix";
    public string RiskCriteria { get; set; } = string.Empty;
    public Dictionary<string, RiskAnalysis> HazardRiskAnalyses { get; set; } = new();

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
        return new RiskAnalysis(new RiskAnalysisID(NEW_RISK_ANALYSIS_SEED_CODE))
        {
            Code = NEW_RISK_ANALYSIS_SEED_CODE,
            HazardCode = hazardCode,
            RiskAssessmentCode = riskAssessmentCode,
            WorstCredibleOutcome = string.Empty,
            RootCause = string.Empty,
            AdditionalComments = string.Empty
        };
    }

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

        var initialAssessmentCodes = assessmentsResult.Value
            .Where(a => a.AssessmentType == RiskAssessmentType.Initial)
            .Select(a => a.Code)
            .ToHashSet();

        var initialAnalyses = allAnalysisResult.Value
            .Where(ra => hazardCodes.Contains(ra.HazardCode) && 
                        initialAssessmentCodes.Contains(ra.RiskAssessmentCode))
            .ToList();

        foreach (var analysis in initialAnalyses)
        {
            HazardRiskAnalyses[analysis.HazardCode] = analysis;
        }
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
            assessment.RiskAnalysisMethod = RiskAnalysisMethod;
            assessment.RiskCriteria = RiskCriteria;
            assessment.CompleteStep(3);

            await SaveRiskAnalysesAsync(mediator, assessment);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying Step 3 to assessment: {ex.Message}");
        }
    }

    private async Task SaveRiskAnalysesAsync(IMediator mediator, RiskAssessment assessment)
    {
        foreach (var analysisKvp in HazardRiskAnalyses)
        {
            try
            {
                var analysis = analysisKvp.Value;
                var hazardCode = analysisKvp.Key;
                
                var updateCommand = new UpdateRiskAnalysisCommand(analysis);
                var updateResult = await mediator.SendAsync(updateCommand, CancellationToken.None);
                
                if (updateResult.IsSuccess)
                {
                    HazardRiskAnalyses[hazardCode] = updateResult.Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating RiskAnalysis for hazard {analysisKvp.Key}: {ex.Message}");
            }
        }
    }

    public async Task LoadFromAssessmentAsync(RiskAssessment assessment, IMediator mediator, List<Hazard> reportHazards)
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

        await LoadExistingRiskAnalysesAsync(mediator, reportHazards);
    }

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

        InitializeHazardAnalyses(reportHazards);
    }
}

/// <summary>
/// Step 4: Risk Assessment & Scoring Panel
/// </summary>
public class Step4Model
{
    public List<string> SelectedPanelMembers { get; set; } = new();
    public Dictionary<string, List<string>> HazardPanelMembers { get; set; } = new();
    public Dictionary<string, List<PanelMemberScoreData>> PanelScores { get; set; } = new();
    public Dictionary<string, double> HazardAverageScores { get; set; } = new();
    public Dictionary<string, string> HazardRiskLevels { get; set; } = new();
    public Dictionary<string, string> HazardMatrixCodes { get; set; } = new();

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

    public (bool isValid, string message) Validate()
    {
        return (true, "Step 4 validation passed");
    }

    public async Task ApplyToAssessmentAsync(RiskAssessment assessment, IMediator mediator, List<Hazard> availableHazards)
    {
        await SaveStep4RiskAssessmentAsync(assessment, mediator, availableHazards);
        assessment.CompleteStep(4);
    }

    public void ApplyToAssessment(RiskAssessment assessment)
    {
        assessment.CompleteStep(4);
    }

    private async Task SaveStep4RiskAssessmentAsync(RiskAssessment assessment, IMediator mediator, List<Hazard> availableHazards)
    {
        if (mediator == null || assessment == null || availableHazards == null) return;

        try
        {
            var (finalSeverity, finalLikelihood, finalRiskLevel, assessmentRationale) = CalculateOverallRiskAssessment(availableHazards);
            var riskAssessmentId = new RiskAssessmentID(assessment.Code);

            var saveStep4Command = new SaveStep4Command(
                riskAssessmentId,
                finalSeverity,
                finalLikelihood,
                finalRiskLevel,
                "Acceptable",
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

    private (int? finalSeverity, int? finalLikelihood, string finalRiskLevel, string assessmentRationale) CalculateOverallRiskAssessment(List<Hazard> availableHazards)
    {
        var completedHazards = HazardAverageScores.Keys.ToList();

        if (!completedHazards.Any())
        {
            return (null, null, "Unknown", "No hazard assessments completed yet.");
        }

        var avgScores = HazardAverageScores.Values.ToList();
        var maxScore = avgScores.Max();
        var avgScore = avgScores.Average();

        var highestRiskHazard = HazardAverageScores.OrderByDescending(kvp => kvp.Value).First();
        var highestRiskHazardCode = highestRiskHazard.Key;

        var highestRiskMatrixCode = HazardMatrixCodes.ContainsKey(highestRiskHazardCode)
            ? HazardMatrixCodes[highestRiskHazardCode]
            : "Unknown";

        var (severity, likelihood) = ParseMatrixCode(highestRiskMatrixCode);

        var finalRiskLevel = severity.HasValue && likelihood.HasValue
            ? GetAviationRiskLevel(severity.Value, likelihood.Value)
            : "Unknown";

        var rationale = $"Risk assessment based on {completedHazards.Count} hazard(s). " +
                       $"Highest risk: {highestRiskHazardCode} ({highestRiskMatrixCode}, Risk Level: {HazardRiskLevels.GetValueOrDefault(highestRiskHazardCode, "Unknown")}). " +
                       $"Average risk score: {avgScore:F2}. " +
                       $"Matrix codes assessed: {string.Join(", ", HazardMatrixCodes.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}. " +
                       $"Assessment completed on {DateTime.UtcNow:yyyy-MM-dd HH:mm}.";

        return (severity, likelihood, finalRiskLevel, rationale);
    }

    private (int? severity, int? likelihood) ParseMatrixCode(string matrixCode)
    {
        if (string.IsNullOrEmpty(matrixCode) || matrixCode.Length < 2)
            return (null, null);

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
        assessment.CompleteStep(4);
    }

    public void LoadFromAssessment(RiskAssessment assessment)
    {
        if (assessment == null) return;
    }

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
                    if (!PanelScores.ContainsKey(hazard.Code))
                    {
                        PanelScores[hazard.Code] = new List<PanelMemberScoreData>();
                    }

                    if (!HazardPanelMembers.ContainsKey(hazard.Code))
                    {
                        HazardPanelMembers[hazard.Code] = new List<string>();
                    }

                    foreach (var panel in result.Value)
                    {
                        if (!HazardPanelMembers[hazard.Code].Contains(panel.SMSUserCode))
                        {
                            HazardPanelMembers[hazard.Code].Add(panel.SMSUserCode);
                        }

                        if (panel.Severity.HasValue && panel.Likelihood.HasValue && panel.Score.HasValue)
                        {
                            var existingScore = PanelScores[hazard.Code]
                                .FirstOrDefault(s => s.MemberId == panel.SMSUserCode);

                            if (existingScore != null)
                            {
                                existingScore.SeverityScore = panel.Severity.Value;
                                existingScore.LikelihoodScore = panel.Likelihood.Value;
                                existingScore.SubmittedDate = (panel.UpdatedDate ?? panel.CreatedDate) ?? DateTime.UtcNow;
                            }
                            else
                            {
                                PanelScores[hazard.Code].Add(new PanelMemberScoreData
                                {
                                    HazardId = hazard.Code,
                                    MemberId = panel.SMSUserCode,
                                    MemberName = panel.SMSUserCode,
                                    SeverityScore = panel.Severity.Value,
                                    LikelihoodScore = panel.Likelihood.Value,
                                    SubmittedDate = (panel.UpdatedDate ?? panel.CreatedDate) ?? DateTime.UtcNow
                                });
                            }
                        }
                    }

                    RecalculateHazardAverage(hazard.Code);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading scoring panels for hazard {hazard.Code}: {ex.Message}");
            }
        }
    }

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
}

/// <summary>
/// Step 5: Risk Mitigation & Implementation
/// </summary>
public class Step5Model
{
    private const string NEW_RISK_ANALYSIS_SEED_CODE = "RA-0000";

    public Dictionary<string, List<string>> SavedMitigationStrategies { get; set; } = new();
    public Dictionary<string, List<Mitigation>> HazardMitigations { get; set; } = new();
    public Dictionary<string, List<string>> ResidualRiskPanels { get; set; } = new();
    public Dictionary<string, List<ResidualRiskScoreData>> ResidualRiskScores { get; set; } = new();
    
    // Dictionary mapping hazard codes to their corresponding Residual RiskAnalysis entities
    public Dictionary<string, RiskAnalysis> HazardResidualRiskAnalyses { get; set; } = new();

    /// <summary>
    /// Gets the Residual RiskAnalysis entity for a specific hazard, creating a new one if it doesn't exist
    /// </summary>
    public RiskAnalysis GetHazardResidualAnalysis(string hazardCode)
    {
        if (string.IsNullOrEmpty(hazardCode))
        {
            return CreateNewRiskAnalysis(hazardCode ?? string.Empty, string.Empty);
        }

        if (!HazardResidualRiskAnalyses.ContainsKey(hazardCode))
        {
            HazardResidualRiskAnalyses[hazardCode] = CreateNewRiskAnalysis(hazardCode, string.Empty);
        }

        return HazardResidualRiskAnalyses[hazardCode];
    }

    private RiskAnalysis CreateNewRiskAnalysis(string hazardCode, string riskAssessmentCode)
    {
        return new RiskAnalysis(new RiskAnalysisID(NEW_RISK_ANALYSIS_SEED_CODE))
        {
            Code = NEW_RISK_ANALYSIS_SEED_CODE,
            HazardCode = hazardCode,
            RiskAssessmentCode = riskAssessmentCode,
            WorstCredibleOutcome = string.Empty,
            RootCause = string.Empty,
            AdditionalComments = string.Empty
        };
    }

    /// <summary>
    /// Load existing Residual RiskAnalysis entities from the database for each hazard
    /// </summary>
    public async Task LoadExistingResidualRiskAnalysesAsync(IMediator mediator, List<Hazard> availableHazards)
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

        var residualAssessmentCodes = assessmentsResult.Value
            .Where(a => a.AssessmentType == RiskAssessmentType.Residual)
            .Select(a => a.Code)
            .ToHashSet();

        var residualAnalyses = allAnalysisResult.Value
            .Where(ra => hazardCodes.Contains(ra.HazardCode) && 
                        residualAssessmentCodes.Contains(ra.RiskAssessmentCode))
            .ToList();

        foreach (var analysis in residualAnalyses)
        {
            // Ensure the RiskAssessmentCode is set correctly
            if (string.IsNullOrEmpty(analysis.RiskAssessmentCode) || analysis.RiskAssessmentCode == "RA-0000")
            {
                // Find the correct Residual assessment code for this analysis
                var residualAssessment = assessmentsResult.Value
                    .FirstOrDefault(a => a.AssessmentType == RiskAssessmentType.Residual && 
                                        hazardCodes.Contains(a.HazardCode ?? string.Empty));
                
                if (residualAssessment != null)
                {
                    analysis.RiskAssessmentCode = residualAssessment.Code;
                }
            }
            
            HazardResidualRiskAnalyses[analysis.HazardCode] = analysis;
        }
    }

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
                
                var updateCommand = new UpdateRiskAnalysisCommand(analysis);
                var updateResult = await mediator.SendAsync(updateCommand, CancellationToken.None);
                
                if (updateResult.IsSuccess)
                {
                    HazardResidualRiskAnalyses[hazardCode] = updateResult.Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating Residual RiskAnalysis for hazard {analysisKvp.Key}: {ex.Message}");
            }
        }
    }

    public async Task LoadFromAssessmentAsync(RiskAssessment assessment, IMediator mediator, List<Hazard> availableHazards)
    {
        if (assessment == null) return;

        LoadFromAssessment(assessment);

        if (mediator != null && availableHazards?.Any() == true)
        {
            await LoadMitigationEntitiesAsync(mediator, availableHazards);
            await LoadExistingResidualRiskAnalysesAsync(mediator, availableHazards);
        }
    }

    public void LoadFromAssessment(RiskAssessment assessment)
    {
        if (assessment == null) return;
    }

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
}
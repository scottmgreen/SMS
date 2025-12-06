using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Shared.Common;

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

        HazardIds = assessment.IdentifiedHazardIds.ToList();
        HazardDescriptions = assessment.IdentifiedHazardIds.Select(id => $"Hazard {id}").ToList();
        HazardCategories = assessment.IdentifiedHazardIds.Select(_ => string.Empty).ToList();
    }

    public void ApplyToAssessment(RiskAssessment assessment)
    {
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

    public Dictionary<string, HazardRiskAnalysis> HazardAnalyses { get; set; } = new();

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
            var hazardCode = hazard.Code;

            if (HazardAnalyses.TryGetValue(hazardCode, out var analysis))
            {
                var worstOutcomeValid = !string.IsNullOrWhiteSpace(analysis.WorstCredibleOutcome) && analysis.WorstCredibleOutcome.Length >= 10;
                var rootCauseValid = !string.IsNullOrWhiteSpace(analysis.RootCauseAnalysis) && analysis.RootCauseAnalysis.Length >= 10;

                if (!worstOutcomeValid && !rootCauseValid)
                {
                    incompleteHazards.Add($"{hazardCode} (missing both worst outcome and root cause analysis)");
                }
                else if (!worstOutcomeValid)
                {
                    incompleteHazards.Add($"{hazardCode} (worst credible outcome incomplete)");
                }
                else if (!rootCauseValid)
                {
                    incompleteHazards.Add($"{hazardCode} (root cause analysis incomplete)");
                }
                else
                {
                    analysisCount++;
                }
            }
            else
            {
                incompleteHazards.Add($"{hazardCode} (no analysis data found)");
            }
        }

        if (incompleteHazards.Any())
        {
            return (false, $"Risk analysis incomplete for {incompleteHazards.Count}/{availableHazards.Count} hazards: {string.Join("; ", incompleteHazards)}");
        }

        return (true, $"Step 3 validation passed - {analysisCount}/{availableHazards.Count} hazards have complete risk analysis");
    }

    public HazardRiskAnalysis GetHazardAnalysis(string hazardCode)
    {
        if (string.IsNullOrEmpty(hazardCode))
        {
            return new HazardRiskAnalysis { HazardId = hazardCode ?? string.Empty };
        }

        if (!HazardAnalyses.ContainsKey(hazardCode))
        {
            HazardAnalyses[hazardCode] = new HazardRiskAnalysis
            {
                HazardId = hazardCode,
                HazardDescription = $"Analysis for {hazardCode}",
                HazardCategory = "General"
            };
        }

        return HazardAnalyses[hazardCode];
    }

    public void InitializeHazardAnalyses(List<Hazard> availableHazards)
    {
        if (availableHazards == null) return;

        foreach (var hazard in availableHazards)
        {
            if (!HazardAnalyses.ContainsKey(hazard.Code))
            {
                HazardAnalyses[hazard.Code] = new HazardRiskAnalysis
                {
                    HazardId = hazard.Code,
                    HazardDescription = hazard.Description,
                    HazardCategory = hazard.HazardType
                };
            }
        }
    }

    public void ApplyToAssessment(RiskAssessment assessment)
    {
        assessment.CompleteStep(3);
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

    #endregion

    #region Helper Classes

    public class HazardRiskAnalysis
    {
        public string HazardId { get; set; } = string.Empty;
        public string HazardDescription { get; set; } = string.Empty;
        public string HazardCategory { get; set; } = string.Empty;

        [Required(ErrorMessage = "Worst credible outcome is required for risk analysis.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Worst credible outcome must be between 10 and 1000 characters.")]
        public string WorstCredibleOutcome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Root cause analysis is required for risk analysis.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Root cause analysis must be between 10 and 1000 characters.")]
        public string RootCauseAnalysis { get; set; } = string.Empty;

        public string AdditionalComments { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;

        public bool IsComplete =>
            !string.IsNullOrWhiteSpace(WorstCredibleOutcome) && WorstCredibleOutcome.Length >= 10 &&
            !string.IsNullOrWhiteSpace(RootCauseAnalysis) && RootCauseAnalysis.Length >= 10;
    }

    #endregion
}

/// <summary>
/// Step 4: Risk Assessment & Scoring Panel
/// </summary>
public class Step4Model
{
    #region Risk Assessment Properties

    public string TolerabilityFramework { get; set; } = "PDX-SMS Default";
    public string RiskAcceptanceCriteria { get; set; } = string.Empty;

    #endregion

    #region Scoring Panel Properties

    public List<string> SelectedPanelMembers { get; set; } = new();
    public Dictionary<string, List<string>> HazardPanelMembers { get; set; } = new();
    public Dictionary<string, List<PanelMemberScoreData>> PanelScores { get; set; } = new();
    public Dictionary<string, double> HazardAverageScores { get; set; } = new();
    public Dictionary<string, string> HazardRiskLevels { get; set; } = new();

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
        if (string.IsNullOrWhiteSpace(TolerabilityFramework))
        {
            return (false, "Tolerability framework is required");
        }

        return (true, "Step 4 validation passed");
    }

    public void ApplyToAssessment(RiskAssessment assessment)
    {
        assessment.CompleteStep(4);
    }

    public void LoadFromAssessment(RiskAssessment assessment)
    {
        if (assessment == null) return;

        if (string.IsNullOrEmpty(TolerabilityFramework))
        {
            TolerabilityFramework = "PDX-SMS Default";
        }
    }

    public void AddPanelMemberScore(string hazardId, PanelMemberScoreData score)
    {
        if (!PanelScores.ContainsKey(hazardId))
        {
            PanelScores[hazardId] = new List<PanelMemberScoreData>();
        }

        PanelScores[hazardId].RemoveAll(s => s.MemberId == score.MemberId);
        PanelScores[hazardId].Add(score);
        RecalculateHazardAverage(hazardId);
    }

    private void RecalculateHazardAverage(string hazardId)
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

    public void AssignPanelMembersToHazard(string hazardId, List<string> memberIds)
    {
        HazardPanelMembers[hazardId] = memberIds.ToList();
    }

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

    public string ImplementationStrategy { get; set; } = string.Empty;
    public DateTime? OverallTargetDate { get; set; }
    public string ImplementationNotes { get; set; } = string.Empty;
    public Dictionary<string, List<string>> SavedMitigationStrategies { get; set; } = new();

    #endregion

    #region Hazard Mitigation Strategies

    // Dictionary of HazardCode to List of Mitigation Strategies (using proper domain entities)
    public Dictionary<string, List<MitigationStrategy>> HazardMitigations { get; set; } = new();
    
    // Dictionary of HazardCode to List of Panel Member IDs for residual risk assessment
    public Dictionary<string, List<string>> ResidualRiskPanels { get; set; } = new();
    
    // Dictionary of HazardCode to List of Residual Risk Scores
    public Dictionary<string, List<ResidualRiskScoreData>> ResidualRiskScores { get; set; } = new();

    #endregion

    #region Helper Methods

    public void AddResidualRiskScore(string hazardCode, ResidualRiskScoreData scoreData)
    {
        if (!ResidualRiskScores.ContainsKey(hazardCode))
        {
            ResidualRiskScores[hazardCode] = new List<ResidualRiskScoreData>();
        }

        // Remove existing score from same member
        ResidualRiskScores[hazardCode].RemoveAll(s => s.MemberId == scoreData.MemberId);
        
        // Add new score
        ResidualRiskScores[hazardCode].Add(scoreData);
    }

    public void AddMitigationStrategy(string hazardCode, MitigationStrategy strategy)
    {
        if (!HazardMitigations.ContainsKey(hazardCode))
        {
            HazardMitigations[hazardCode] = new List<MitigationStrategy>();
        }

        HazardMitigations[hazardCode].Add(strategy);
    }

    #endregion

    #region Validation and Application Methods

    public (bool isValid, string message) Validate()
    {
        bool hasImplementation = !string.IsNullOrWhiteSpace(ImplementationStrategy);

        if (!hasImplementation)
        {
            return (false, "Implementation strategy is required");
        }

        return (true, "Step 5 validation passed");
    }

    public void ApplyToAssessment(RiskAssessment assessment)
    {
        assessment.CompleteStep(5);
    }

    public void LoadFromAssessment(RiskAssessment assessment)
    {
        if (assessment == null) return;

        // Load implementation data if available
        if (string.IsNullOrEmpty(ImplementationStrategy))
        {
            ImplementationStrategy = assessment.ImplementationStrategy ?? string.Empty;
        }

        if (OverallTargetDate == null && assessment.OverallTargetDate.HasValue)
        {
            OverallTargetDate = assessment.OverallTargetDate;
        }

        if (string.IsNullOrEmpty(ImplementationNotes))
        {
            ImplementationNotes = assessment.ImplementationNotes ?? string.Empty;
        }
    }

    #endregion

    #region Apply To Assessment String Helper

    public string ApplyToAssessmentString(List<string> appliedMitigations)
    {
        return string.Join("; ", appliedMitigations ?? new List<string>());
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
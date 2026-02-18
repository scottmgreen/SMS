using SMS3.Components.Shared;

namespace SMS3.Components.Pages.SMSRiskManagement.Models;

/// <summary>
/// Step 4: Risk Assessment & Scoring Panel
/// </summary>
public class Step4Model
{
    public List<string> SelectedPanelMembers { get; set; } = new();
    public Dictionary<string, List<string>> HazardPanelMembers { get; set; } = new();
    public Dictionary<string, List<PanelMemberScoreData>> PanelScores { get; set; } = new();
    public Dictionary<string, double> HazardAverageScores { get; set; } = new();
    public Dictionary<string, RiskLevel> HazardRiskLevels { get; set; } = new();
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

    public async Task ApplyToAssessmentAsync(AuthenticationService AuthService,RiskAssessment assessment, IMediator mediator, List<Hazard> availableHazards)
    {
        await SaveStep4RiskAssessmentAsync(AuthService,assessment, mediator, availableHazards);
        assessment.CompleteStep(4);
    }

    

    private async Task SaveStep4RiskAssessmentAsync(AuthenticationService AuthService,RiskAssessment assessment, IMediator mediator, List<Hazard> availableHazards)
    {
        if (mediator == null || assessment == null || availableHazards == null) return;

        try
        {
            var (finalSeverity, finalLikelihood, finalRiskLevel) = CalculateOverallRiskAssessment(availableHazards);
            var riskAssessmentId = new RiskAssessmentID(assessment.Code);

            var saveStep4Command = new SaveStep4Command(
                riskAssessmentId,
                finalSeverity,
                finalLikelihood,
                finalRiskLevel);

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

    private (int? finalSeverity, int? finalLikelihood, RiskLevel finalRiskLevel) CalculateOverallRiskAssessment(List<Hazard> availableHazards)
    {
        var completedHazards = HazardAverageScores.Keys.ToList();

        if (!completedHazards.Any())
        {
            return (null, null, RiskLevel.Unkonwn);
        }

        var avgScores = HazardAverageScores.Values.ToList();
        var maxScore = avgScores.Max();
        var avgScore = avgScores.Average();

        var highestRiskHazard = HazardAverageScores.OrderByDescending(kvp => kvp.Value).First();
        var highestRiskHazardCode = highestRiskHazard.Key;

        var highestRiskMatrixCode = HazardMatrixCodes.ContainsKey(highestRiskHazardCode)
            ? HazardMatrixCodes[highestRiskHazardCode]
            : "Unknown";

        var (severity, likelihood) = AviationRiskMatrixCalculator.ParseMatrixCode(highestRiskMatrixCode);

        var finalRiskLevel = severity.HasValue && likelihood.HasValue
            ? AviationRiskMatrixCalculator.GetAviationRiskLevel(severity.Value, likelihood.Value)
            : RiskLevel.Unkonwn;

        var rationale = $"Risk assessment based on {completedHazards.Count} hazard(s). " +
               $"Highest risk: {highestRiskHazardCode} ({highestRiskMatrixCode}, Risk Level: {GetRiskLevelDisplayText(highestRiskHazardCode)}). " +
               $"Average risk score: {avgScore:F2}. " +
               $"Matrix codes assessed: {string.Join(", ", HazardMatrixCodes.Select(kvp => $"{kvp.Key}:{kvp.Value}"))}. " +
               $"Assessment completed on {DateTime.UtcNow:yyyy-MM-dd HH:mm}.";


        return (severity, likelihood, finalRiskLevel);
    }


    // Helper method to get display text for UI
    public string GetRiskLevelDisplayText(string hazardId)
    {
        return HazardRiskLevels.ContainsKey(hazardId)
            ? HazardRiskLevels[hazardId].Name
            : RiskLevel.Unkonwn.Name;
    }

    // Helper method to get risk level value for database storage
    public string GetRiskLevelValue(string hazardId)
    {
        return HazardRiskLevels.ContainsKey(hazardId)
            ? HazardRiskLevels[hazardId].Value
            : RiskLevel.Unkonwn.Value;
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

    private RiskLevel DetermineRiskLevel(double score)
    {
        return score switch
        {
            >= 15 => RiskLevel.High,
            >= 8 => RiskLevel.Medium,
            _ => RiskLevel.Low
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
        public RiskLevel RiskLevel { get; set; } = RiskLevel.Unkonwn;
        public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;
        public bool IsComplete => SeverityScore > 0 && LikelihoodScore > 0;

        public PanelMemberScoreData()
        {
            MemberId = PanelMemberId;
        }
    }
}

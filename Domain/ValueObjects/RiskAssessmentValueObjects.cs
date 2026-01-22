namespace SMS_Domain.ValueObjects;

/// <summary>
/// Risk Score Value Object for Panel Scoring
/// This stays as Value Object since it's just scoring data, not an entity with lifecycle
/// </summary>
public sealed class RiskScore
{
    public string PanelMemberId { get; private set; }
    public int SeverityScore { get; private set; }
    public int LikelihoodScore { get; private set; }
    public double CalculatedScore => SeverityScore * LikelihoodScore;
    public DateTime ScoredDate { get; private set; }
    public bool IsComplete => SeverityScore > 0 && LikelihoodScore > 0;

    private RiskScore(string panelMemberId, int severityScore, int likelihoodScore)
    {
        PanelMemberId = panelMemberId;
        SeverityScore = severityScore;
        LikelihoodScore = likelihoodScore;
        ScoredDate = DateTime.UtcNow;
    }

    public static Result<RiskScore> Create(string panelMemberId, int severityScore, int likelihoodScore)
    {
        if (string.IsNullOrWhiteSpace(panelMemberId))
        {
            return Result<RiskScore>.Failure<RiskScore>(DomainErrors.RiskAssessmentError.InvalidPanelMember);
        }

        if (severityScore < 1 || severityScore > 5)
        {
            return Result<RiskScore>.Failure<RiskScore>(DomainErrors.RiskAssessmentError.InvalidSeverityScore);
        }

        if (likelihoodScore < 1 || likelihoodScore > 5)
        {
            return Result<RiskScore>.Failure<RiskScore>(DomainErrors.RiskAssessmentError.InvalidLikelihoodScore);
        }

        return Result<RiskScore>.Success(new RiskScore(panelMemberId, severityScore, likelihoodScore));
    }
}

/// <summary>
/// Monitoring Requirement Value Object
/// This stays as Value Object since it's just configuration data, not an entity with lifecycle
/// </summary>
public sealed class MonitoringRequirement
{
    public string MonitoringFrequency { get; private set; }
    public string ResponsibleDepartment { get; private set; }
    public string ReviewTrigger { get; private set; }
    public DateTime EstablishedDate { get; private set; }

    private MonitoringRequirement(string frequency, string department, string trigger)
    {
        MonitoringFrequency = frequency;
        ResponsibleDepartment = department;
        ReviewTrigger = trigger;
        EstablishedDate = DateTime.UtcNow;
    }

    public static Result<MonitoringRequirement> Create(string frequency, string department, string trigger)
    {
        if (string.IsNullOrWhiteSpace(frequency))
        {
            return Result<MonitoringRequirement>.Failure<MonitoringRequirement>(DomainErrors.RiskAssessmentError.InvalidAssessmentType);
        }

        return Result<MonitoringRequirement>.Success(new MonitoringRequirement(
            frequency,
            department ?? "Safety Department",
            trigger ?? "Annual"));
    }
}
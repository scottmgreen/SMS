namespace SMS_Application.Messaging.Queries;

// =============================================
// SCORING PANEL QUERIES
// =============================================

public class GetScoringPanelByIdQuery : BaseQueryBundle, IRequest<Result<ScoringPanel>>
{
    public ScoringPanelID ScoringPanelId { get; set; }

    public GetScoringPanelByIdQuery(ScoringPanelID scoringPanelId)
    {
        ScoringPanelId = scoringPanelId ?? throw new ArgumentNullException(nameof(scoringPanelId));
    }
}

public class GetAllScoringPanelsQuery : BaseQueryBundle, IRequest<Result<List<ScoringPanel>>>
{
    public GetAllScoringPanelsQuery()
    {
    }
}

public class GetScoringPanelsByHazardCodeQuery : BaseQueryBundle, IRequest<Result<List<ScoringPanel>>>
{
    public string HazardCode { get; set; }

    public GetScoringPanelsByHazardCodeQuery(string hazardCode)
    {
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
    }
}
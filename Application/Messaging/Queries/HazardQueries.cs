namespace SMS_Application.Messaging.Queries;

// =============================================
// HAZARD QUERIES
// =============================================

public class GetHazardByIdQuery : BaseQueryBundle, IRequest<Result<Hazard>>
{
    public HazardID HazardId { get; set; }

    public GetHazardByIdQuery(HazardID hazardId)
    {
        HazardId = hazardId ?? throw new ArgumentNullException(nameof(hazardId));
    }
}

public class GetAllHazardsQuery : BaseQueryBundle, IRequest<Result<List<Hazard>>>
{
    public GetAllHazardsQuery()
    {
    }
}
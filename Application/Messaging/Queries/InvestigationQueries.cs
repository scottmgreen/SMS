namespace SMS_Application.Messaging.Queries;

// =============================================
// INVESTIGATION QUERIES
// =============================================

public class GetInvestigationByIdQuery : BaseQueryBundle, IRequest<Result<Investigation>>
{
    public InvestigationID InvestigationId { get; set; }

    public GetInvestigationByIdQuery(InvestigationID investigationId)
    {
        InvestigationId = investigationId ?? throw new ArgumentNullException(nameof(investigationId));
    }
}

public class GetAllInvestigationsQuery : BaseQueryBundle, IRequest<Result<List<Investigation>>>
{
    public GetAllInvestigationsQuery()
    {
    }
}
namespace SMS_Application.Messaging.Queries;

// =============================================
// INVESTIGATION QUERIES
// =============================================

public class GetInvestigationByCodeQuery : BaseQueryBundle, IRequest<Result<Investigation>>
{
    public InvestigationID InvestigationId { get; set; }

    public GetInvestigationByCodeQuery(InvestigationID code)
    {
        InvestigationId = code ?? throw new ArgumentNullException(nameof(code));
    }
}

public class GetAllInvestigationsQuery : BaseQueryBundle, IRequest<Result<List<Investigation>>>
{
    public GetAllInvestigationsQuery()
    {
    }
}
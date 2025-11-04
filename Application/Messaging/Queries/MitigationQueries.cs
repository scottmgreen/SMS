namespace SMS_Application.Messaging.Queries;

// =============================================
// MITIGATION QUERIES
// =============================================

public class GetMitigationByIdQuery : BaseQueryBundle, IRequest<Result<Mitigation>>
{
    public MitigationID MitigationId { get; set; }

    public GetMitigationByIdQuery(MitigationID mitigationId)
    {
        MitigationId = mitigationId ?? throw new ArgumentNullException(nameof(mitigationId));
    }
}

public class GetAllMitigationsQuery : BaseQueryBundle, IRequest<Result<List<Mitigation>>>
{
    public GetAllMitigationsQuery()
    {
    }
}
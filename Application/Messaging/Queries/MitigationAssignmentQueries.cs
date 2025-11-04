namespace SMS_Application.Messaging.Queries;

// =============================================
// MITIGATION ASSIGNMENT QUERIES
// =============================================

public class GetMitigationAssignmentByIdQuery : BaseQueryBundle, IRequest<Result<MitigationAssignment>>
{
    public MitigationAssignmentID MitigationAssignmentId { get; set; }

    public GetMitigationAssignmentByIdQuery(MitigationAssignmentID mitigationAssignmentId)
    {
        MitigationAssignmentId = mitigationAssignmentId ?? throw new ArgumentNullException(nameof(mitigationAssignmentId));
    }
}

public class GetAllMitigationAssignmentsQuery : BaseQueryBundle, IRequest<Result<List<MitigationAssignment>>>
{
    public GetAllMitigationAssignmentsQuery()
    {
    }
}
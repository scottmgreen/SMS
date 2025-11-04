namespace SMS_Application.Messaging.Commands;

public class CreateMitigationAssignmentCommand : BaseCommandBundle, IRequest<Result<MitigationAssignment>>
{
    public MitigationAssignment MitigationAssignment { get; set; }

    public CreateMitigationAssignmentCommand(MitigationAssignment mitigationAssignment)
    {
        MitigationAssignment = mitigationAssignment ?? throw new ArgumentNullException(nameof(mitigationAssignment));
    }
}

public class UpdateMitigationAssignmentCommand : BaseCommandBundle, IRequest<Result<MitigationAssignment>>
{
    public MitigationAssignment MitigationAssignment { get; set; }

    public UpdateMitigationAssignmentCommand(MitigationAssignment mitigationAssignment)
    {
        MitigationAssignment = mitigationAssignment ?? throw new ArgumentNullException(nameof(mitigationAssignment));
    }
}

public class DeleteMitigationAssignmentCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    public MitigationAssignmentID MitigationAssignmentId { get; set; }

    public DeleteMitigationAssignmentCommand(MitigationAssignmentID mitigationAssignmentId)
    {
        MitigationAssignmentId = mitigationAssignmentId ?? throw new ArgumentNullException(nameof(mitigationAssignmentId));
    }
}
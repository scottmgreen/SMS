namespace SMS_Application.Messaging.Commands;

public class CreateMitigationCommand : BaseCommandBundle, IRequest<Result<Mitigation>>
{
    public Mitigation Mitigation { get; set; }

    public CreateMitigationCommand(Mitigation mitigation)
    {
        Mitigation = mitigation ?? throw new ArgumentNullException(nameof(mitigation));
    }
}

public class UpdateMitigationCommand : BaseCommandBundle, IRequest<Result<Mitigation>>
{
    public Mitigation Mitigation { get; set; }

    public UpdateMitigationCommand(Mitigation mitigation)
    {
        Mitigation = mitigation ?? throw new ArgumentNullException(nameof(mitigation));
    }
}

public class DeleteMitigationCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    public MitigationID MitigationId { get; set; }

    public DeleteMitigationCommand(MitigationID mitigationId)
    {
        MitigationId = mitigationId ?? throw new ArgumentNullException(nameof(mitigationId));
    }
}
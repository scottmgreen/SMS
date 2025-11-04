namespace SMS_Application.Messaging.Commands;

public class CreateInvestigationCommand : BaseCommandBundle, IRequest<Result<Investigation>>
{
    public Investigation Investigation { get; set; }

    public CreateInvestigationCommand(Investigation investigation)
    {
        Investigation = investigation ?? throw new ArgumentNullException(nameof(investigation));
    }
}

public class UpdateInvestigationCommand : BaseCommandBundle, IRequest<Result<Investigation>>
{
    public Investigation Investigation { get; set; }

    public UpdateInvestigationCommand(Investigation investigation)
    {
        Investigation = investigation ?? throw new ArgumentNullException(nameof(investigation));
    }
}

public class DeleteInvestigationCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    public InvestigationID InvestigationId { get; set; }

    public DeleteInvestigationCommand(InvestigationID investigationId)
    {
        InvestigationId = investigationId ?? throw new ArgumentNullException(nameof(investigationId));
    }
}
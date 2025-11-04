namespace SMS_Application.Messaging.Commands;

public class CreateScoringPanelCommand : BaseCommandBundle, IRequest<Result<ScoringPanel>>
{
    public ScoringPanel ScoringPanel { get; set; }

    public CreateScoringPanelCommand(ScoringPanel scoringPanel)
    {
        ScoringPanel = scoringPanel ?? throw new ArgumentNullException(nameof(scoringPanel));
    }
}

public class UpdateScoringPanelCommand : BaseCommandBundle, IRequest<Result<ScoringPanel>>
{
    public ScoringPanel ScoringPanel { get; set; }

    public UpdateScoringPanelCommand(ScoringPanel scoringPanel)
    {
        ScoringPanel = scoringPanel ?? throw new ArgumentNullException(nameof(scoringPanel));
    }
}

public class DeleteScoringPanelCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    public ScoringPanelID ScoringPanelId { get; set; }

    public DeleteScoringPanelCommand(ScoringPanelID scoringPanelId)
    {
        ScoringPanelId = scoringPanelId ?? throw new ArgumentNullException(nameof(scoringPanelId));
    }
}
namespace SMS_Application.Messaging.Commands;

public class CreateRiskAssessmentCommand : BaseCommandBundle, IRequest<Result<RiskAssessment>>
{
    public RiskAssessment RiskAssessment { get; set; }

    public CreateRiskAssessmentCommand(RiskAssessment riskAssessment)
    {
        RiskAssessment = riskAssessment ?? throw new ArgumentNullException(nameof(riskAssessment));
    }
}

public class UpdateRiskAssessmentCommand : BaseCommandBundle, IRequest<Result<RiskAssessment>>
{
    public RiskAssessment RiskAssessment { get; set; }

    public UpdateRiskAssessmentCommand(RiskAssessment riskAssessment)
    {
        RiskAssessment = riskAssessment ?? throw new ArgumentNullException(nameof(riskAssessment));
    }
}

public class DeleteRiskAssessmentCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    public RiskAssessmentID RiskAssessmentId { get; set; }

    public DeleteRiskAssessmentCommand(RiskAssessmentID riskAssessmentId)
    {
        RiskAssessmentId = riskAssessmentId ?? throw new ArgumentNullException(nameof(riskAssessmentId));
    }
}
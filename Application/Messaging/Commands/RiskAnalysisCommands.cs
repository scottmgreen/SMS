namespace SMS_Application.Messaging.Commands;

public class CreateRiskAnalysisCommand : BaseCommandBundle, IRequest<Result<RiskAnalysis>>
{
    public RiskAnalysis RiskAnalysis { get; set; }

    public CreateRiskAnalysisCommand(RiskAnalysis riskAnalysis)
    {
        RiskAnalysis = riskAnalysis ?? throw new ArgumentNullException(nameof(riskAnalysis));
    }
}

public class UpdateRiskAnalysisCommand : BaseCommandBundle, IRequest<Result<RiskAnalysis>>
{
    public RiskAnalysis RiskAnalysis { get; set; }

    public UpdateRiskAnalysisCommand(RiskAnalysis riskAnalysis)
    {
        RiskAnalysis = riskAnalysis ?? throw new ArgumentNullException(nameof(riskAnalysis));
    }
}

public class DeleteRiskAnalysisCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    public RiskAnalysisID RiskAnalysisId { get; set; }

    public DeleteRiskAnalysisCommand(RiskAnalysisID riskAnalysisId)
    {
        RiskAnalysisId = riskAnalysisId ?? throw new ArgumentNullException(nameof(riskAnalysisId));
    }
}
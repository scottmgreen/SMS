namespace SMS_Application.Messaging.Commands;

public class CreateRiskAnalysisCommand : BaseCommandBundle, IRequest<Result<RiskAnalysis>>, ICreateCommand
{
    public RiskAnalysis RiskAnalysis { get; set; }

    public CreateRiskAnalysisCommand(RiskAnalysis riskAnalysis)
    {
        RiskAnalysis = riskAnalysis ?? throw new ArgumentNullException(nameof(riskAnalysis));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        //RiskAnalysis.CreatedBy = userId;
        //RiskAnalysis.CreatedDate = timestamp;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // For create commands, we typically don't set UpdatedBy
    }
}

public class UpdateRiskAnalysisCommand : BaseCommandBundle, IRequest<Result<RiskAnalysis>>, IUpdateCommand
{
    public RiskAnalysis RiskAnalysis { get; set; }

    public UpdateRiskAnalysisCommand(RiskAnalysis riskAnalysis)
    {
        RiskAnalysis = riskAnalysis ?? throw new ArgumentNullException(nameof(riskAnalysis));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // For update commands, we typically don't modify CreatedBy
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        RiskAnalysis.UpdatedBy = userId;
        RiskAnalysis.UpdatedDate = timestamp;
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
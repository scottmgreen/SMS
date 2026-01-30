namespace SMS_Domain.Enums;

public abstract class RiskAnalysisType : BaseEnum<RiskAnalysisType>
{
    protected RiskAnalysisType(string value, string name) : base(value, name)
    {

    }



    #region Risk Assessment Types

    /// <summary>Original risk assessment before mitigations - PrimaryHazardId MUST be set</summary>
    public static readonly RiskAnalysisType Initial = new InitialType();

    /// <summary>Risk assessment after mitigations have been implemented</summary>
    public static readonly RiskAnalysisType Residual = new ResidualType();

    #endregion

    #region Implementations

    private sealed class InitialType : RiskAnalysisType
    {
        public InitialType() : base("Initial", "Initial")
        {
        }
    }

    private sealed class ResidualType : RiskAnalysisType
    {
        public ResidualType() : base("Residual", "Residual")
        {
        }
    }

    #endregion


}
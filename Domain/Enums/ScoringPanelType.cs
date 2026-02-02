namespace SMS_Domain.Enums;

/// <summary>
/// Risk Assessment Type Enumeration - BUSINESS RULE ENFORCED
/// ONLY these values are allowed: Initial, Residual
/// </summary>
public abstract class ScoringPanelType : BaseEnum<ScoringPanelType>
{
    protected ScoringPanelType(string value, string name) : base(value, name)
    {

    }



    #region Risk Assessment Types

    /// <summary>Original risk assessment before mitigations - PrimaryHazardId MUST be set</summary>
    public static readonly ScoringPanelType Initial = new InitialType();

    /// <summary>Risk assessment after mitigations have been implemented</summary>
    public static readonly ScoringPanelType Residual = new ResidualType();

    #endregion

    #region Implementations

    private sealed class InitialType : ScoringPanelType
    {
        public InitialType() : base("Initial", "Initial")
        {
        }
    }

    private sealed class ResidualType : ScoringPanelType
    {
        public ResidualType() : base("Residual", "Residual")
        {
        }
    }

    #endregion


}

using System.Reflection;
using SMS_Domain.Common;

namespace SMS_Domain.Enums;

/// <summary>
/// Risk Assessment Type Enumeration - BUSINESS RULE ENFORCED
/// ONLY these values are allowed: Initial, Residual
/// </summary>
public abstract class RiskAssessmentType : BaseEnum<RiskAssessmentType>
{
    protected RiskAssessmentType(string value, string name, string description, bool requiresPrimaryHazard, bool requiresParentAssessment) : base(value, name)
    {
        Description = description;
        RequiresPrimaryHazard = requiresPrimaryHazard;
        RequiresParentAssessment = requiresParentAssessment;
    }

    public string Description { get; }
    public bool RequiresPrimaryHazard { get; }
    public bool RequiresParentAssessment { get; }

    #region Risk Assessment Types

    /// <summary>Original risk assessment before mitigations - PrimaryHazardId MUST be set</summary>
    public static readonly RiskAssessmentType Initial = new InitialType();

    /// <summary>Risk assessment after mitigations have been implemented</summary>
    public static readonly RiskAssessmentType Residual = new ResidualType();

    #endregion

    #region Implementations

    private sealed class InitialType : RiskAssessmentType
    {
        public InitialType() : base("Initial", "Initial",
            "Original risk assessment before any mitigations are implemented", true, false)
        {
        }
    }

    private sealed class ResidualType : RiskAssessmentType
    {
        public ResidualType() : base("Residual", "Residual",
            "Risk assessment after mitigations have been implemented", false, true)
        {
        }
    }

    #endregion

    /// <summary>
    /// Gets types that require a primary hazard
    /// </summary>
    public static IEnumerable<RiskAssessmentType> GetTypesRequiringPrimaryHazard()
    {
        return GetAllValues().Where(rat => rat.RequiresPrimaryHazard);
    }

    /// <summary>
    /// Gets types that require a parent assessment
    /// </summary>
    public static IEnumerable<RiskAssessmentType> GetTypesRequiringParentAssessment()
    {
        return GetAllValues().Where(rat => rat.RequiresParentAssessment);
    }

    /// <summary>
    /// Checks if this is an initial assessment
    /// </summary>
    public bool IsInitial => this == Initial;

    /// <summary>
    /// Checks if this is a residual assessment
    /// </summary>
    public bool IsResidual => this == Residual;
}
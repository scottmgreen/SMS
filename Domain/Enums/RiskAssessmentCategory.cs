//-----------------------------------------------------------------------
// <copyright file="RiskAssessmentCategory.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enumeration defining category classifications for SMS riskassessment management.
//                  Domain enumeration defining valid states and classifications
//                  for business entities and processes.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Enums;

/// <summary>
/// Risk Assessment HazardCategory Enumeration - BUSINESS RULE ENFORCED
/// ONLY these values are allowed: Technical, Risk Registry Only
/// </summary>
public abstract class RiskAssessmentCategory : BaseEnum<RiskAssessmentCategory>
{
    protected RiskAssessmentCategory(string value, string name, string description, int maxSteps, bool isFullProcess) : base(value, name)
    {
        Description = description;
        MaxSteps = maxSteps;
        IsFullProcess = isFullProcess;
    }

    public string Description { get; }
    public int MaxSteps { get; }
    public bool IsFullProcess { get; }

    #region Risk Assessment Categories

    /// <summary>Full 5-step SMS Technical Risk Assessment (TRA) - CurrentStep reflects last step finished (1-5)</summary>
    public static readonly RiskAssessmentCategory Technical = new TechnicalCategory();

    /// <summary>Streamlined 1-step Preliminary Risk Assessment - CurrentStep is always 1</summary>
    public static readonly RiskAssessmentCategory RiskRegistryOnly = new RiskRegistryOnlyCategory();

    #endregion

    #region Implementations

    private sealed class TechnicalCategory : RiskAssessmentCategory
    {
        public TechnicalCategory() : base("TECHNICAL", "Technical",
            "Full 5-step SMS Technical Risk Assessment (TRA) process", 5, true)
        {
        }
    }

    private sealed class RiskRegistryOnlyCategory : RiskAssessmentCategory
    {
        public RiskRegistryOnlyCategory() : base("RISK_REGISTRY_ONLY", "RiskRegistryOnly",
            "Streamlined 1-step Risk Registry Only Assessment process", 1, false)
        {
        }
    }

    #endregion

    

    

    /// <summary>
    /// Validates if a step number is valid for this category
    /// </summary>
    public bool IsValidStep(int stepNumber)
    {
        return stepNumber >= 1 && stepNumber <= MaxSteps;
    }

    /// <summary>
    /// Checks if this is a technical assessment
    /// </summary>
    public bool IsTechnical => this == Technical;

    /// <summary>
    /// Checks if this is a risk registry only assessment
    /// </summary>
    public bool IsRiskRegistryOnly => this == RiskRegistryOnly;

    /// <summary>
    /// Gets the maximum allowed step for this category
    /// </summary>
    public int GetMaxStep() => MaxSteps;

    /// <summary>
    /// Checks if this category requires multiple steps
    /// </summary>
    public bool IsMultiStep => MaxSteps > 1;
}

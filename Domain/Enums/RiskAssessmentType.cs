//-----------------------------------------------------------------------
// <copyright file="RiskAssessmentType.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enumeration defining classification types for SMS riskassessment entities.
//                  Domain enumeration defining valid states and classifications
//                  for business entities and processes.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Enums;

/// <summary>
/// Risk Assessment Type Enumeration - BUSINESS RULE ENFORCED
/// ONLY these values are allowed: Technical, Residual
/// </summary>
public abstract class RiskAssessmentType : BaseEnum<RiskAssessmentType>
{
    protected RiskAssessmentType(string value, string name) : base(value, name)
    {
        
    }

    

    #region Risk Assessment Types

    /// <summary>Original risk assessment before mitigations - PrimaryHazardId MUST be set</summary>
    public static readonly RiskAssessmentType Technical = new TechnicalType();

    /// <summary>Risk assessment after mitigations have been implemented</summary>
    public static readonly RiskAssessmentType RiskRegistryOnly = new RiskRegistryOnlyType();

    #endregion

    #region Implementations

    private sealed class TechnicalType : RiskAssessmentType
    {
        public TechnicalType() : base("TECHNICAL", "Technical")
        {
        }
    }

    private sealed class RiskRegistryOnlyType : RiskAssessmentType
    {
        public RiskRegistryOnlyType() : base("RISK_REGISTRY_ONLY", "Risk Registry Only")
        {
        }
    }

    #endregion


}


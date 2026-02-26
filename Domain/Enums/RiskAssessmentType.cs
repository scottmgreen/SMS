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
/// ONLY these values are allowed: Initial, Residual
/// </summary>
public abstract class RiskAssessmentType : BaseEnum<RiskAssessmentType>
{
    protected RiskAssessmentType(string value, string name) : base(value, name)
    {
        
    }

    

    #region Risk Assessment Types

    /// <summary>Original risk assessment before mitigations - PrimaryHazardId MUST be set</summary>
    public static readonly RiskAssessmentType Initial = new InitialType();

    /// <summary>Risk assessment after mitigations have been implemented</summary>
    //public static readonly RiskAssessmentType Residual = new ResidualType();

    #endregion

    #region Implementations

    private sealed class InitialType : RiskAssessmentType
    {
        public InitialType() : base("Initial", "Initial")
        {
        }
    }

    //private sealed class ResidualType : RiskAssessmentType
    //{
    //    public ResidualType() : base("Residual", "Residual")
    //    {
    //    }
    //}

    #endregion

    
}


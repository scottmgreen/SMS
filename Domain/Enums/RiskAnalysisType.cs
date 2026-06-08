//-----------------------------------------------------------------------
// <copyright file="RiskAnalysisType.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enumeration defining classification types for SMS riskanalysis entities.
//                  Domain enumeration defining valid states and classifications
//                  for business entities and processes.
// </copyright>
//-----------------------------------------------------------------------

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
        public InitialType() : base("Technical", "Technical")
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

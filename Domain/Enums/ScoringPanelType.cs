//-----------------------------------------------------------------------
// <copyright file="ScoringPanelType.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enumeration defining classification types for SMS scoringpanel entities.
//                  Domain enumeration defining valid states and classifications
//                  for business entities and processes.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Enums;

/// <summary>
/// Risk Assessment Type Enumeration - BUSINESS RULE ENFORCED
/// ONLY these values are allowed: Technical, Residual
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
        public InitialType() : base("Technical", "Technical")
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


//-----------------------------------------------------------------------
// <copyright file="HazardStatus.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enumeration defining valid status values for SMS hazard workflows.
//                  Domain enumeration defining valid states and classifications
//                  for business entities and processes.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Common;
using System.Reflection;

namespace SMS_Domain.Enums;

/// <summary>
/// Hazard Status Smart Enumeration - REVISED FOR RISK ASSESSMENT WORKFLOW
/// Represents the approved final hazard workflow progression statuses through 5-step Technical Assessment
/// Based on approved final "Hazard Status REVISED" list from StatusList.txt
/// This tracks progression through Technical Risk Assessment Steps 1-5, not validation decisions
/// </summary>
public abstract class HazardStatus : BaseEnum<HazardStatus>
{
    protected HazardStatus(string value, string name, string description, int stepNumber, string stage, int workflowOrder) : base(value, name)
    {
        Description = description;
        //StepNumber = stepNumber;
        //Stage = stage;
        //WorkflowOrder = workflowOrder;
    }

    public string Description { get; }
    //public int StepNumber { get; }
    //public string Stage { get; }
    //public int WorkflowOrder { get; }

    #region APPROVED FINAL HAZARD STATUS REVISED VALUES FROM StatusList.txt

    /// <summary>Step 1 & Step 2 - Technical Risk Assessment phase (System Description & Hazard Identification)</summary>
    public static readonly HazardStatus Unknown = new UnknownStatus();

    public static readonly HazardStatus HazardValidationRequired = new HazardRequiresValidation();

    public static readonly HazardStatus HazardValidated = new HazardValidationStatus();

    /// <summary>Step 1 & Step 2 - Technical Risk Assessment phase (System Description & Hazard Identification)</summary>
    public static readonly HazardStatus InitialRiskAssessment = new InitialRiskAssessmentStatus();

    /// <summary>Step 3 - Technical Risk Analysis phase (Risk Analysis)</summary>
    public static readonly HazardStatus InitialRiskAnalysis = new InitialRiskAnalysisStatus();

    /// <summary>Step 4 - Technical Hazard Scoring phase (Risk Assessment & Scoring)</summary>
    public static readonly HazardStatus InitialHazardScoring = new InitialHazardScoringStatus();

    /// <summary>Step 5 - Residual Risk Assessment phase (Post-Mitigation Assessment)</summary>
    public static readonly HazardStatus ResidualRiskAssessment = new ResidualRiskAssessmentStatus();

    /// <summary>Step 5 - Residual Risk Mitigation phase (Mitigation Implementation)</summary>
    public static readonly HazardStatus ResidualRiskMitigation = new ResidualRiskMitigationStatus();

    /// <summary>Step 5 - Residual Hazard Scoring phase (Final Scoring Post-Mitigation)</summary>
    public static readonly HazardStatus ResidualHazardScoring = new ResidualHazardScoringStatus();

    public static readonly HazardStatus ResidualRiskAnalysis = new ResidualRiskAnalysisStatus();

    #endregion

    #region Implementations
    private sealed class HazardValidationStatus : HazardStatus
    {
        public HazardValidationStatus() : base("HAZARD_VALIDATION_STATUS", "Hazard Validation Status", "Hazard validated", 1, "Technical", 1)
        {
        }
    }
    private sealed class HazardRequiresValidation : HazardStatus
    {
        public HazardRequiresValidation() : base("HAZARD_REQUIRES_VALIDATION", "Hazard Requires Validation", "Hazard requires validation", 1, "Technical", 1)
        {
        }
    }
    private sealed class InitialRiskAssessmentStatus : HazardStatus
    {
        public InitialRiskAssessmentStatus() : base("INITIAL_RISK_ASSESSMENT", "Technical Risk Assessment", "Hazard is in initial risk assessment phase covering system description and hazard identification (Steps 1-2)", 2, "Technical", 1)
        {
        }
    }





    private sealed class UnknownStatus : HazardStatus
    {
        public UnknownStatus() : base("UNKNOWN_STATUS", "Unknown Status", "Unknown Status", 2, "Technical", 1)
        {
        }
    }
    private sealed class InitialRiskAnalysisStatus : HazardStatus
    {
        public InitialRiskAnalysisStatus() : base("INITIAL_RISK_ANALYSIS", "Technical Risk Analysis", "Hazard is undergoing initial risk analysis to identify contributing factors and consequences (Step 3)", 3, "Technical", 2)
        {
        }
    }

    private sealed class InitialHazardScoringStatus : HazardStatus
    {
        public InitialHazardScoringStatus() : base("INITIAL_HAZARD_SCORING", "Technical Hazard Scoring",
            "Hazard is being scored for initial risk assessment with stakeholder input (Step 4)", 4, "Technical", 3)
        {
        }
    }

    private sealed class ResidualRiskAssessmentStatus : HazardStatus
    {
        public ResidualRiskAssessmentStatus() : base("RESIDUAL_RISK_ASSESSMENT", "Residual Risk Assessment",
            "Hazard is in residual risk assessment phase after mitigation planning (Step 5)", 5, "Residual", 4)
        {
        }
    }
    private sealed class ResidualRiskAnalysisStatus : HazardStatus
    {
        public ResidualRiskAnalysisStatus() : base("RESIDUAL_RISK_ANALYSIS", "Residual Risk Analysis",
            "Hazard is undergoing residual risk analysis to identify contributing factors and consequences (Step 5)", 5, "Residual", 5)
        {
        }
    }
    private sealed class ResidualRiskMitigationStatus : HazardStatus
    {
        public ResidualRiskMitigationStatus() : base("RESIDUAL_RISK_MITIGATION", "Residual Risk Mitigation",
            "Hazard mitigations are being implemented and managed (Step 5)", 5, "Residual", 5)
        {
        }
    }

    private sealed class ResidualHazardScoringStatus : HazardStatus
    {
        public ResidualHazardScoringStatus() : base("RESIDUAL_HAZARD_SCORING", "Residual Hazard Scoring",
            "Hazard is being scored for residual risk after mitigation implementation (Step 5)", 5, "Residual", 6)
        {
        }
    }

    #endregion

    /// <summary>
    /// Gets all available hazard status values
    /// </summary>
    public static IEnumerable<HazardStatus> GetAllValues()
    {
        return typeof(HazardStatus)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(HazardStatus))
            .Select(f => (HazardStatus)f.GetValue(null)!)
            .Where(hs => hs != null);
            //.OrderBy(hs => hs.WorkflowOrder);
    }

    

    

    

       
    

    
    

    

    

    
}


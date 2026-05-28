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
        StepNumber = stepNumber;
        Stage = stage;
        WorkflowOrder = workflowOrder;
    }

    public string Description { get; }
    public int StepNumber { get; }
    public string Stage { get; }
    public int WorkflowOrder { get; }

    #region APPROVED FINAL HAZARD STATUS REVISED VALUES FROM StatusList.txt

    /// <summary>Step 1 & Step 2 - Initial Risk Assessment phase (System Description & Hazard Identification)</summary>
    public static readonly HazardStatus StatusUnknown = new InitialRiskAssessmentStatus();



    /// <summary>Step 1 & Step 2 - Initial Risk Assessment phase (System Description & Hazard Identification)</summary>
    public static readonly HazardStatus InitialRiskAssessment = new InitialRiskAssessmentStatus();

    /// <summary>Step 3 - Initial Risk Analysis phase (Risk Analysis)</summary>
    public static readonly HazardStatus InitialRiskAnalysis = new InitialRiskAnalysisStatus();

    /// <summary>Step 4 - Initial Hazard Scoring phase (Risk Assessment & Scoring)</summary>
    public static readonly HazardStatus InitialHazardScoring = new InitialHazardScoringStatus();

    /// <summary>Step 5 - Residual Risk Assessment phase (Post-Mitigation Assessment)</summary>
    public static readonly HazardStatus ResidualRiskAssessment = new ResidualRiskAssessmentStatus();

    /// <summary>Step 5 - Residual Risk Mitigation phase (Mitigation Implementation)</summary>
    public static readonly HazardStatus ResidualRiskMitigation = new ResidualRiskMitigationStatus();

    /// <summary>Step 5 - Residual Hazard Scoring phase (Final Scoring Post-Mitigation)</summary>
    public static readonly HazardStatus ResidualHazardScoring = new ResidualHazardScoringStatus();

    public static readonly HazardStatus ResidualRiskAnalysis = new ResidualRiskAssessmentStatus();

    #endregion

    #region Implementations

    private sealed class InitialRiskAssessmentStatus : HazardStatus
    {
        public InitialRiskAssessmentStatus() : base("INITIAL_RISK_ASSESSMENT", "Initial Risk Assessment",
            "Hazard is in initial risk assessment phase covering system description and hazard identification (Steps 1-2)", 2, "Initial", 1)
        {
        }
    }
    private sealed class UnknownStatus : HazardStatus
    {
        public UnknownStatus() : base("UNKNOWN_STATUS", "Unknown Status",
            "Hazard is Unknown", 2, "Initial", 1)
        {
        }
    }
    private sealed class InitialRiskAnalysisStatus : HazardStatus
    {
        public InitialRiskAnalysisStatus() : base("INITIAL_RISK_ANALYSIS", "Initial Risk Analysis",
            "Hazard is undergoing initial risk analysis to identify contributing factors and consequences (Step 3)", 3, "Initial", 2)
        {
        }
    }

    private sealed class InitialHazardScoringStatus : HazardStatus
    {
        public InitialHazardScoringStatus() : base("INITIAL_HAZARD_SCORING", "Initial Hazard Scoring",
            "Hazard is being scored for initial risk assessment with stakeholder input (Step 4)", 4, "Initial", 3)
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
            .Where(hs => hs != null)
            .OrderBy(hs => hs.WorkflowOrder);
    }

    /// <summary>
    /// Gets status values in the Initial risk assessment stage
    /// </summary>
    public static IEnumerable<HazardStatus> GetInitialStageStatuses()
    {
        return GetAllValues().Where(hs => hs.Stage == "Initial");
    }

    /// <summary>
    /// Gets status values in the Residual risk assessment stage
    /// </summary>
    public static IEnumerable<HazardStatus> GetResidualStageStatuses()
    {
        return GetAllValues().Where(hs => hs.Stage == "Residual");
    }

    /// <summary>
    /// Gets the appropriate status for a given Technical Assessment step
    /// </summary>
    public static HazardStatus GetStatusForAssessmentStep(int stepNumber, string substep = "")
    {
        return stepNumber switch
        {
            1 or 2 => InitialRiskAssessment, // Steps 1-2 are combined
            3 => InitialRiskAnalysis,
            4 => InitialHazardScoring,
            5 => substep.ToLowerInvariant() switch
            {
                "mitigation" or "mitigations" => ResidualRiskMitigation,
                "scoring" or "score" => ResidualHazardScoring,
                _ => ResidualRiskAssessment // Default for Step 5
            },
            _ => InitialRiskAssessment
        };
    }

    /// <summary>
    /// Gets the next logical workflow status
    /// </summary>
    public HazardStatus? GetNextWorkflowStatus()
    {
        return this switch
        {
            var s when s == InitialRiskAssessment => InitialRiskAnalysis,
            var s when s == InitialRiskAnalysis => InitialHazardScoring,
            var s when s == InitialHazardScoring => ResidualRiskAssessment,
            var s when s == ResidualRiskAssessment => ResidualRiskMitigation,
            var s when s == ResidualRiskMitigation => ResidualHazardScoring,
            _ => null // ResidualHazardScoring is final
        };
    }

    /// <summary>
    /// Validates if transition to target status is allowed
    /// </summary>
    public bool CanTransitionTo(HazardStatus targetStatus)
    {
        // Allow progression forward in workflow
        var nextStatus = GetNextWorkflowStatus();
        if (nextStatus != null && targetStatus == nextStatus)
            return true;

        // Allow movement within Step 5 substeps
        if (StepNumber == 5 && targetStatus.StepNumber == 5)
            return true;

        // Allow backward movement for corrections
        if (targetStatus.WorkflowOrder < WorkflowOrder)
            return true;

        return false;
    }

    /// <summary>
    /// Determines if this status is in the Initial assessment stage
    /// </summary>
    public bool IsInitialStage => Stage == "Initial";

    /// <summary>
    /// Determines if this status is in the Residual assessment stage  
    /// </summary>
    public bool IsResidualStage => Stage == "Residual";

    /// <summary>
    /// Determines if this status indicates assessment is complete
    /// </summary>
    public bool IsAssessmentComplete => this == ResidualHazardScoring;

    /// <summary>
    /// Gets the UI color for this status based on stage and progress
    /// </summary>
    public string GetDisplayColor()
    {
        return this switch
        {
            var s when s == InitialRiskAssessment => "#17a2b8", // Info blue
            var s when s == InitialRiskAnalysis => "#007bff", // Primary blue  
            var s when s == InitialHazardScoring => "#6f42c1", // Purple
            var s when s == ResidualRiskAssessment => "#fd7e14", // Orange
            var s when s == ResidualRiskMitigation => "#ffc107", // Warning yellow
            var s when s == ResidualHazardScoring => "#28a745", // Success green
            _ => "#6c757d"
        };
    }

    /// <summary>
    /// Gets the workflow description for this status
    /// </summary>
    public string GetWorkflowDescription()
    {
        return this switch
        {
            var s when s == InitialRiskAssessment => "System description and hazard identification in progress",
            var s when s == InitialRiskAnalysis => "Analyzing risk factors and potential consequences", 
            var s when s == InitialHazardScoring => "Stakeholders scoring initial risk levels",
            var s when s == ResidualRiskAssessment => "Assessing residual risk after mitigation planning",
            var s when s == ResidualRiskMitigation => "Implementing and managing risk mitigation strategies",
            var s when s == ResidualHazardScoring => "Final scoring of residual risk levels",
            _ => Description
        };
    }

    /// <summary>
    /// Gets the percentage completion for this status in the overall workflow
    /// </summary>
    public int GetProgressPercentage()
    {
        return this switch
        {
            var s when s == InitialRiskAssessment => 20,  // Steps 1-2 complete
            var s when s == InitialRiskAnalysis => 40,    // Step 3 complete
            var s when s == InitialHazardScoring => 60,   // Step 4 complete
            var s when s == ResidualRiskAssessment => 75, // Step 5 started
            var s when s == ResidualRiskMitigation => 90, // Step 5 mitigation phase
            var s when s == ResidualHazardScoring => 100, // Step 5 final phase
            _ => 0
        };
    }

    /// <summary>
    /// Gets the step range description for display
    /// </summary>
    public string GetStepRangeDescription()
    {
        return this switch
        {
            var s when s == InitialRiskAssessment => "Steps 1-2",
            var s when s == InitialRiskAnalysis => "Step 3",
            var s when s == InitialHazardScoring => "Step 4", 
            var s when s == ResidualRiskAssessment => "Step 5a",
            var s when s == ResidualRiskMitigation => "Step 5b",
            var s when s == ResidualHazardScoring => "Step 5c",
            _ => $"Step {StepNumber}"
        };
    }
}


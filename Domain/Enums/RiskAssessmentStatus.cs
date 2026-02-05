using SMS_Domain.Common;
using System.Reflection;

namespace SMS_Domain.Enums;

/// <summary>
/// Risk Assessment Status Smart Enumeration - COMPLETELY REPLACED
/// Represents the approved final risk assessment workflow statuses
/// Based on approved final status list from StatusList.txt
/// </summary>
public abstract class RiskAssessmentStatus : BaseEnum<RiskAssessmentStatus>
{
    protected RiskAssessmentStatus(string value, string name, string description, bool allowsModification, bool isFinal, int workflowOrder) : base(value, name)
    {
        Description = description;
        AllowsModification = allowsModification;
        IsFinal = isFinal;
        WorkflowOrder = workflowOrder;
    }

    public string Description { get; }
    public bool AllowsModification { get; }
    public bool IsFinal { get; }
    public int WorkflowOrder { get; }

    #region ✅ APPROVED FINAL RISK ASSESSMENT STATUS VALUES FROM StatusList.txt

    /// <summary>Risk assessor has been assigned to the assessment</summary>
    public static readonly RiskAssessmentStatus AssignedToAssessor = new AssignedToAssessorStatus();

    /// <summary>Assessment has been scheduled with assessor and stakeholders</summary>
    public static readonly RiskAssessmentStatus AssessmentScheduled = new AssessmentScheduledStatus();

    /// <summary>Assessment is currently underway with active evaluation</summary>
    public static readonly RiskAssessmentStatus AssessmentUnderway = new AssessmentUnderwayStatus();

    /// <summary>Assessment has been completed with final results</summary>
    public static readonly RiskAssessmentStatus AssessmentComplete = new AssessmentCompleteStatus();

    /// <summary>Assessment has been completed with final results</summary>
    public static readonly RiskAssessmentStatus AssessmentCreate = new AssessmentCreated();

    #endregion

    #region Implementations

    private sealed class AssignedToAssessorStatus : RiskAssessmentStatus
    {
        public AssignedToAssessorStatus() : base("ASSIGNED_TO_ASSESSOR", "Assigned to Assessor",
            "Risk assessor has been assigned to conduct the assessment", true, false, 1)
        {
        }
    }

    private sealed class AssessmentScheduledStatus : RiskAssessmentStatus
    {
        public AssessmentScheduledStatus() : base("ASSESSMENT_SCHEDULED", "Assessment Scheduled",
            "Assessment has been scheduled with assessor and relevant stakeholders", true, false, 2)
        {
        }
    }

    private sealed class AssessmentUnderwayStatus : RiskAssessmentStatus
    {
        public AssessmentUnderwayStatus() : base("ASSESSMENT_UNDERWAY", "Assessment Underway",
            "Assessment is currently in progress with active risk evaluation", true, false, 3)
        {
        }
    }

    private sealed class AssessmentCompleteStatus : RiskAssessmentStatus
    {
        public AssessmentCompleteStatus() : base("ASSESSMENT_COMPLETE", "Assessment Complete",
            "Assessment has been completed with final risk evaluation and recommendations", false, true, 4)
        {
        }
    }
    private sealed class AssessmentCreated : RiskAssessmentStatus
    {
        public AssessmentCreated() : base("ASSESSMENT_CREATED", "Assessment Created",
            "Assessment has been Created", false, true, 4)
        {
        }
    }
    #endregion

    /// <summary>
    /// Gets all available risk assessment status values
    /// </summary>
    public static IEnumerable<RiskAssessmentStatus> GetAllValues()
    {
        return typeof(RiskAssessmentStatus)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(RiskAssessmentStatus))
            .Select(f => (RiskAssessmentStatus)f.GetValue(null)!)
            .Where(ras => ras != null)
            .OrderBy(ras => ras.WorkflowOrder);
    }

    /// <summary>
    /// Gets status values that allow modifications
    /// </summary>
    public static IEnumerable<RiskAssessmentStatus> GetModifiableStatuses()
    {
        return GetAllValues().Where(ras => ras.AllowsModification);
    }

    /// <summary>
    /// Gets status values that indicate work in progress
    /// </summary>
    public static IEnumerable<RiskAssessmentStatus> GetActiveStatuses()
    {
        return GetAllValues().Where(ras => !ras.IsFinal);
    }

    /// <summary>
    /// Gets status values that are final states
    /// </summary>
    public static IEnumerable<RiskAssessmentStatus> GetFinalStatuses()
    {
        return GetAllValues().Where(ras => ras.IsFinal);
    }

    /// <summary>
    /// Checks if this status allows step completion
    /// </summary>
    public bool CanCompleteSteps => AllowsModification;

    /// <summary>
    /// Checks if this status is final
    /// </summary>
    public bool IsCompleted => IsFinal;

    /// <summary>
    /// Checks if work can be done in this status
    /// </summary>
    public bool IsWorkable => AllowsModification;

    /// <summary>
    /// Determines if this status indicates assessment is assigned
    /// </summary>
    public bool IsAssigned => this == AssignedToAssessor;

    /// <summary>
    /// Determines if this status indicates assessment is scheduled
    /// </summary>
    public bool IsScheduled => this == AssessmentScheduled;

    /// <summary>
    /// Determines if this status indicates assessment is underway
    /// </summary>
    public bool IsUnderway => this == AssessmentUnderway;

    /// <summary>
    /// Determines if this status indicates assessment is complete
    /// </summary>
    public bool IsComplete => this == AssessmentComplete;

    /// <summary>
    /// Gets the next possible statuses from this status
    /// </summary>
    public IEnumerable<RiskAssessmentStatus> GetPossibleNextStatuses()
    {
        return Value switch
        {
            "ASSIGNED_TO_ASSESSOR" => new[] { AssessmentScheduled },
            "ASSESSMENT_SCHEDULED" => new[] { AssessmentUnderway },
            "ASSESSMENT_UNDERWAY" => new[] { AssessmentComplete },
            "ASSESSMENT_COMPLETE" => new RiskAssessmentStatus[] { }, // Final state
            _ => new RiskAssessmentStatus[] { }
        };
    }

    /// <summary>
    /// Validates if transition to target status is allowed
    /// </summary>
    public bool CanTransitionTo(RiskAssessmentStatus targetStatus)
    {
        var possibleStatuses = GetPossibleNextStatuses();
        return possibleStatuses.Contains(targetStatus);
    }

    /// <summary>
    /// Gets the UI color for this status
    /// </summary>
    public string GetDisplayColor()
    {
        return this switch
        {
            var s when s == AssignedToAssessor => "#17a2b8", // Info blue
            var s when s == AssessmentScheduled => "#ffc107", // Warning yellow
            var s when s == AssessmentUnderway => "#007bff", // Primary blue
            var s when s == AssessmentComplete => "#28a745", // Success green
            _ => "#6c757d"
        };
    }

    /// <summary>
    /// Gets the status description for workflow display
    /// </summary>
    public string GetWorkflowDescription()
    {
        return this switch
        {
            var s when s == AssignedToAssessor => "Waiting for assessment to be scheduled",
            var s when s == AssessmentScheduled => "Scheduled and ready to begin assessment",
            var s when s == AssessmentUnderway => "Assessment in active progress",
            var s when s == AssessmentComplete => "Assessment completed with final results",
            _ => Description
        };
    }
}
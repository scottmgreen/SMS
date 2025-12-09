using System.Reflection;
using SMS_Domain.Common;

namespace SMS_Domain.Enums;

/// <summary>
/// Risk Assessment Status for workflow management - BUSINESS RULE ENFORCED
/// ONLY these values are allowed: Created, InProgress, Completed
/// </summary>
public abstract class RiskAssessmentStatus : BaseEnum<RiskAssessmentStatus>
{
    protected RiskAssessmentStatus(string value, string name, string description, bool allowsModification, bool isFinal) : base(value, name)
    {
        Description = description;
        AllowsModification = allowsModification;
        IsFinal = isFinal;
    }

    public string Description { get; }
    public bool AllowsModification { get; }
    public bool IsFinal { get; }

    #region Risk Assessment Status Types

    /// <summary>Assessment just created, no work started</summary>
    public static readonly RiskAssessmentStatus Created = new CreatedStatus();

    /// <summary>Assessment work in progress, not finished</summary>
    public static readonly RiskAssessmentStatus InProgress = new InProgressStatus();

    /// <summary>Assessment work completed</summary>
    public static readonly RiskAssessmentStatus Completed = new CompletedStatus();

    #endregion

    #region Implementations

    private sealed class CreatedStatus : RiskAssessmentStatus
    {
        public CreatedStatus() : base("Created", "Created",
            "Assessment just created, no work started", true, false)
        {
        }
    }

    private sealed class InProgressStatus : RiskAssessmentStatus
    {
        public InProgressStatus() : base("InProgress", "In Progress",
            "Assessment work in progress, not finished", true, false)
        {
        }
    }

    private sealed class CompletedStatus : RiskAssessmentStatus
    {
        public CompletedStatus() : base("Completed", "Completed",
            "Assessment work completed", false, true)
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
            .Where(ras => ras != null);
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
    /// Checks if this status allows step completion
    /// </summary>
    public bool AllowsStepCompletion => this != Completed;

    /// <summary>
    /// Checks if this status indicates completion
    /// </summary>
    public bool IsCompleted => this == Completed;

    /// <summary>
    /// Checks if this status indicates active work
    /// </summary>
    public bool IsActive => this == Created || this == InProgress;
}
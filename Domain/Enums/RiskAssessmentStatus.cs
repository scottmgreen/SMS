using SMS_Domain.Common;
using System.Reflection;

namespace SMS_Domain.Enums;

/// <summary>
/// Risk Assessment Status Smart Enumeration -
/// </summary>
public abstract class RiskAssessmentStatus : BaseEnum<RiskAssessmentStatus>
{
    protected RiskAssessmentStatus(string value, string name) : base(value, name)
    {
        
    }

    
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
        public AssignedToAssessorStatus() : base("ASSIGNED_TO_ASSESSOR", "Assigned to Assessor")
        {
        }
    }
    private sealed class AssessmentScheduledStatus : RiskAssessmentStatus
    {
        public AssessmentScheduledStatus() : base("ASSESSMENT_SCHEDULED", "Assessment Scheduled")
        {
        }
    }
    private sealed class AssessmentUnderwayStatus : RiskAssessmentStatus
    {
        public AssessmentUnderwayStatus() : base("ASSESSMENT_UNDERWAY", "Assessment Underway")
        {
        }
    }
    private sealed class AssessmentCompleteStatus : RiskAssessmentStatus
    {
        public AssessmentCompleteStatus() : base("ASSESSMENT_COMPLETE", "Assessment Complete")
        {
        }
    }
    private sealed class AssessmentCreated : RiskAssessmentStatus
    {
        public AssessmentCreated() : base("ASSESSMENT_CREATED", "Assessment Created")
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

}

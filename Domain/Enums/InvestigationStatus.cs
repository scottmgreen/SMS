using SMS_Domain.Common;
using System.Reflection;

namespace SMS_Domain.Enums;

/// <summary>
/// Investigation Status Smart Enumeration - COMPLETELY REWRITTEN
/// Represents the approved final investigation workflow statuses
/// Based on approved final status list from StatusList.txt
/// </summary>
public abstract class InvestigationStatus : BaseEnum<InvestigationStatus>
{
    protected InvestigationStatus(string value, string name, string description, bool allowsModification, int workflowOrder) : base(value, name)
    {
        Description = description;
        AllowsModification = allowsModification;
        WorkflowOrder = workflowOrder;
    }

    public string Description { get; }
    public bool AllowsModification { get; }
    public int WorkflowOrder { get; }

    #region ✅ APPROVED FINAL INVESTIGATION STATUS VALUES FROM StatusList.txt

    /// <summary>Default status when investigation status is not yet determined</summary>
    public static readonly InvestigationStatus StatusUnknown = new StatusUnknownStatus();

    /// <summary>Investigator has been assigned to the investigation</summary>
    public static readonly InvestigationStatus InvestigatorAssigned = new InvestigatorAssignedStatus();

    /// <summary>Investigation is actively underway with ongoing work</summary>
    public static readonly InvestigationStatus InvestigationUnderway = new InvestigationUnderwayStatus();

    /// <summary>Investigation has been completed with final findings</summary>
    public static readonly InvestigationStatus InvestigationComplete = new InvestigationCompleteStatus();

    #endregion

    #region Implementations

    private sealed class StatusUnknownStatus : InvestigationStatus
    {
        public StatusUnknownStatus() : base("STATUS_UNKNOWN", "Status Unknown",
            "Investigation status has not yet been determined", true, 0)
        {
        }
    }

    private sealed class InvestigatorAssignedStatus : InvestigationStatus
    {
        public InvestigatorAssignedStatus() : base("INVESTIGATOR_ASSIGNED", "Investigator Assigned",
            "Investigator has been assigned and ready to begin investigation", true, 1)
        {
        }
    }

    private sealed class InvestigationUnderwayStatus : InvestigationStatus
    {
        public InvestigationUnderwayStatus() : base("INVESTIGATION_UNDERWAY", "Investigation Underway",
            "Investigation is actively in progress", true, 2)
        {
        }
    }

    private sealed class InvestigationCompleteStatus : InvestigationStatus
    {
        public InvestigationCompleteStatus() : base("INVESTIGATION_COMPLETE", "Investigation Complete",
            "Investigation has been completed with final findings", false, 3)
        {
        }
    }

    #endregion

    /// <summary>
    /// Gets all available investigation status values
    /// </summary>
    public static IEnumerable<InvestigationStatus> GetAllValues()
    {
        return typeof(InvestigationStatus)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(InvestigationStatus))
            .Select(f => (InvestigationStatus)f.GetValue(null))
            .Where(ins => ins != null)
            .OrderBy(ins => ins.WorkflowOrder);
    }

    /// <summary>
    /// Determines if this status is a final state
    /// </summary>
    public bool IsFinalStatus => this == InvestigationComplete;

    /// <summary>
    /// Determines if this status is an active processing state
    /// </summary>
    public bool IsActiveStatus => this == InvestigatorAssigned || this == InvestigationUnderway;
}



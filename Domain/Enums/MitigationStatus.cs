using SMS_Domain.Common;

namespace SMS_Domain.Enums;

/// <summary>
/// Mitigation Status Smart Enumeration - NEWLY CREATED
/// Represents the various stages of mitigation implementation in the SMS mitigation process
/// Based on approved final status list from StatusList.txt
/// </summary>
public sealed class MitigationStatus : BaseEnum<MitigationStatus>
{
    // ? APPROVED FINAL STATUS VALUES FROM StatusList.txt
    public static readonly MitigationStatus PendingApproval = new("PENDING_APPROVAL", "Mitigation Pending Approval");
    public static readonly MitigationStatus Approved = new("APPROVED", "Mitigation Approved");
    public static readonly MitigationStatus InProgressDueDate = new("IN_PROGRESS_DUE_DATE", "Mitigation in Progress – Due Date");
    public static readonly MitigationStatus Rejected = new("REJECTED", "Mitigation Rejected");
    public static readonly MitigationStatus Complete = new("COMPLETE", "Mitigation Complete");
    public static readonly MitigationStatus MonitoringHazard = new("MONITORING_HAZARD", "Monitoring Hazard");

    private MitigationStatus(string value, string name) : base(value, name)
    {
    }

 
    

}
//-----------------------------------------------------------------------
// <copyright file="MitigationStatus.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enumeration defining valid status values for SMS mitigation workflows.
//                  Domain enumeration defining valid states and classifications
//                  for business entities and processes.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Common;
using System.Reflection;

namespace SMS_Domain.Enums;

/// <summary>
/// Mitigation Status Smart Enumeration - NEWLY CREATED
/// Represents the various stages of mitigation implementation in the SMS mitigation process
/// Based on approved final status list from StatusList.txt
/// </summary>
public abstract class MitigationStatus : BaseEnum<MitigationStatus>
{
    // ? APPROVED FINAL STATUS VALUES FROM StatusList.txt
    public static readonly MitigationStatus PendingApproval = new PendingApprovalStatus();
    public static readonly MitigationStatus Approved = new ApprovedStatus();
    public static readonly MitigationStatus InProgress = new InProgressStatus();
    public static readonly MitigationStatus PastExpectedTargetDate = new PastExpectedTargetDateStatus();
    public static readonly MitigationStatus Rejected = new RejectedStatus();
    public static readonly MitigationStatus MitigationImplemented = new MitigationImplementedStatus ();
    public static readonly MitigationStatus MonitoringHazard = new MonitoringHazardStatus();
    public static readonly MitigationStatus HazardEliminated = new HazardEliminatedStatus();

    protected MitigationStatus(string value, string name, string description) : base(value, name)   
    {
        Description = description;
    }

    public string Description { get; }

    private sealed class PendingApprovalStatus : MitigationStatus
    {
        public PendingApprovalStatus() : base("PENDING_APPROVAL", "Mitigation Pending Approval", "Mitigation is pending approval")
        {
        }
    }

    private sealed class ApprovedStatus : MitigationStatus
    {
        public ApprovedStatus() : base("APPROVED", "Mitigation Approved", "Mitigation has been approved")
        {
        }
    }

    private sealed class InProgressStatus : MitigationStatus
    {
        public InProgressStatus() : base("IN_PROGRESS", "Mitigation in Progress", "Mitigation work is in progress")
        {
        }
    }

    private sealed class PastExpectedTargetDateStatus : MitigationStatus
    {
        public PastExpectedTargetDateStatus() : base("PAST_EXPECTED_TARGET_DATE", "Mitigation Completion Past Expected Target Date", "Mitigation has passed the expected target completion date")
        {
        }
    }

    private sealed class RejectedStatus : MitigationStatus
    {
        public RejectedStatus() : base("REJECTED", "Mitigation Rejected", "Mitigation has been rejected")
        {
        }
    }

    private sealed class MitigationImplementedStatus : MitigationStatus
    {
        public MitigationImplementedStatus() : base("IMPLEMENTED", "Mitigation Implemented", "Mitigation has been Implemented")
        {
        }
    }

    private sealed class MonitoringHazardStatus : MitigationStatus
    {
        public MonitoringHazardStatus() : base("MONITORING_HAZARD", "Monitoring Hazard", "Hazard is being monitored after mitigation action")
        {
        }
    }

    private sealed class HazardEliminatedStatus : MitigationStatus
    {
        public HazardEliminatedStatus() : base("HAZARD_ELIMINATED", "Hazard Eliminated", "Hazard has been eliminated, Report will be closed")
        {
        }
    }

    public static IEnumerable<MitigationStatus> GetAllValues()
    {
        return typeof(MitigationStatus)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(MitigationStatus))
            .Select(f => (MitigationStatus)f.GetValue(null)!)
            .Where(ms => ms != null);
    }

 
    

}

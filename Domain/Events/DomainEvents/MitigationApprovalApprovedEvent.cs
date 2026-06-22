// <copyright file="MitigationApprovalApprovedEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain event triggered when mitigation approval is approved in the SMS system.
// </copyright>
//-----------------------------------------------------------------------


using SMS_Domain.Entities;

namespace SMS_Domain.Events;

/// <summary>
/// Domain event triggered when mitigation approval is approved
/// </summary>
public class MitigationApprovalApprovedEvent : BaseDomainEvent
{
    public override string EventType => SMS_Domain.Enums.EventType.MitigationApprovalApproved.Value;
    public string EventCategory => SMS_Domain.Enums.EventCategory.DomainEvent.Value;

    public string MitigationId { get; private set; }
    public string ApprovedBy { get; private set; }
    public DateTime ApprovedDate { get; private set; }

    public MitigationApprovalApprovedEvent(SMSEventID id,string mitigationId, string approvedBy, DateTime approvedDate):base(id)
    {
        MitigationId = mitigationId ?? throw new ArgumentNullException(nameof(mitigationId));
        ApprovedBy = approvedBy ?? "SYSTEM";
        ApprovedDate = approvedDate;
    }
}

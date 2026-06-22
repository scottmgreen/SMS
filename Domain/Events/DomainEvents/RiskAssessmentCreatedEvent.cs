// <copyright file="RiskAssessmentCreatedEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain event triggered when a risk assessment is created in the SMS system.
// </copyright>
//-----------------------------------------------------------------------


using SMS_Domain.Common;
using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS_Domain.Events;

/// <summary>
/// Domain event triggered when a risk assessment is created
/// </summary>
public class RiskAssessmentCreatedEvent : BaseDomainEvent
{
    public override string EventType => SMS_Domain.Enums.EventType.RiskAssessmentCreated.Value;
    public string EventCategory => SMS_Domain.Enums.EventCategory.DomainEvent.Value;

    public string RiskAssessmentId { get; private set; }
    public string CreatedBy { get; private set; }
    public DateTime CreatedDate { get; private set; }

    public RiskAssessmentCreatedEvent(SMSEventID id,string riskAssessmentId, string createdBy, DateTime createdDate) : base(id)
    {
        RiskAssessmentId = riskAssessmentId ?? throw new ArgumentNullException(nameof(riskAssessmentId));
        CreatedBy = createdBy ?? "SYSTEM";
        CreatedDate = createdDate;
    }
}

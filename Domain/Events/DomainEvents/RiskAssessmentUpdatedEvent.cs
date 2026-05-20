// <copyright file="RiskAssessmentUpdatedEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain event triggered when a risk assessment is updated in the SMS system.
// </copyright>
//-----------------------------------------------------------------------

using Domain.Enums;
using SMS_Domain.Common;
using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS_Domain.Events;

/// <summary>
/// Domain event triggered when a risk assessment is updated
/// </summary>
public class RiskAssessmentUpdatedEvent : BaseDomainEvent
{
    public override string EventType => Domain.Enums.EventType.RiskAssessmentUpdated.Value;
    public string EventCategory => Domain.Enums.EventCategogy.DomainEvent.Value;

    public string RiskAssessmentId { get; private set; }
    public string UpdatedBy { get; private set; }
    public DateTime UpdatedDate { get; private set; }

    public RiskAssessmentUpdatedEvent(SMSEventID id,string riskAssessmentId, string updatedBy, DateTime updatedDate) : base(id)
    {
        RiskAssessmentId = riskAssessmentId ?? throw new ArgumentNullException(nameof(riskAssessmentId));
        UpdatedBy = updatedBy ?? "SYSTEM";
        UpdatedDate = updatedDate;
    }
}

//-----------------------------------------------------------------------
// <copyright file="ValidationDecisionMadeEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain event triggered when a report validation decision is recorded.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Common;
using SMS_Domain.Entities;

namespace SMS_Domain.Events;

public class ValidationDecisionMadeEvent : BaseDomainEvent
{
    public override string EventType => "Report_ValidationDecision_Made";

    public string ReportCode { get; set; } = string.Empty;
    public string ValidationDecision { get; set; } = string.Empty;
    public DateTime ValidatedDate { get; set; }
    public string ValidatedBy { get; set; } = string.Empty;
    public string ValidationComments { get; set; } = string.Empty;

    public ValidationDecisionMadeEvent(SMSEventID id, string reportCode, string validationDecision, DateTime validatedDate)
        : base(id)
    {
        ReportCode = reportCode;
        ValidationDecision = validationDecision;
        ValidatedDate = validatedDate;
        ReportId = reportCode;
    }
}

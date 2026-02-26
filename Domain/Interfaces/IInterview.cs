//-----------------------------------------------------------------------
// <copyright file="IInterview.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain contract defining operations and ensuring clean architecture boundaries for SMS business logic.
//                  Domain service contract defining business operations
//                  and ensuring clean architecture boundaries.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Interfaces;

public interface IInterview
{
    InterviewID Id { get; set; }
    string? Code { get; set; }
    string? InvestigationCode { get; set; }
    string? SMSInvestigatorCode { get; set; }
    string? PersonInterviewed { get; set; }
    string? PersonInterviewedNotes { get; set; }
    string? InvestigatorNotes { get; set; }
}


//-----------------------------------------------------------------------
// <copyright file="IReportValidation.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain contract defining operations and ensuring clean architecture boundaries for SMS business logic.
//                  Domain service contract defining business operations
//                  and ensuring clean architecture boundaries.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Interfaces;

public interface IReportValidation
{
    ReportValidationID Id { get; set; }
    string? Code { get; set; }
    string? ReportCode { get; set; }
    string? ValidationDecision { get; set; }
    ReportValidationStatus? Status { get; set; }
    string? Stage { get; set; }
}


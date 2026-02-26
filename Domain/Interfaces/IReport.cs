//-----------------------------------------------------------------------
// <copyright file="IReport.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain contract defining operations and ensuring clean architecture boundaries for SMS business logic.
//                  Domain service contract defining business operations
//                  and ensuring clean architecture boundaries.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Interfaces;

public interface IReport
{
    ReportID Id { get; set; }
    string Code { get; set; }
    string? Name { get; set; }
    string? Description { get; set; }
    string? Status { get; set; }
    string? Stage { get; set; }
}


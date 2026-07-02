//-----------------------------------------------------------------------
// <copyright file="IReportService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS Report management service handling report creation, validation, and processing.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Interfaces;

public interface IReportService
{
    Task<Result<Report>> CreateReportAsync(Report report, CancellationToken ct = default);
    Task<Result<bool>> DeleteReportAsync(ReportID id, CancellationToken ct = default);
    Task<Result<List<Report>>> GetAllReportsAsync(CancellationToken ct = default);
    Task<Result<Report>> GetReportByCodeAsync(ReportID id, CancellationToken ct = default);
    Task<Result<string>> GetTrackingIDByReportCodeAsync(ReportID id, CancellationToken ct = default);
    Task<Result<Report>> UpdateReportAsync(Report report, CancellationToken ct = default);
}

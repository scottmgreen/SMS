//-----------------------------------------------------------------------
// <copyright file="IReportValidationService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS Report management service handling report creation, validation, and processing.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

namespace Application.Interfaces;

public interface IReportValidationService
{
    Task<Result<ReportValidation>> CreateReportValidationAsync(ReportValidation reportValidation, CancellationToken ct = default);
    Task<Result<bool>> DeleteReportValidationAsync(ReportValidationID id, CancellationToken ct = default);
    Task<Result<List<ReportValidation>>> GetAllReportValidationsAsync(CancellationToken ct = default);
    Task<Result<ReportValidation>> GetReportValidationByIdAsync(ReportValidationID id, CancellationToken ct = default);
    Task<Result<ReportValidation>> UpdateReportValidationAsync(ReportValidation reportValidation, CancellationToken ct = default);
}

//-----------------------------------------------------------------------
// <copyright file="IReportDataService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Data service contracts defining business-focused data operations for SMS domain entities.
//                  Infrastructure service contract defining data access operations
//                  and external system integration interfaces.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// Report Data Service Interface
/// </summary>
public interface IReportDataService
{
    /// <summary>
    /// Creates a new report asynchronously
    /// </summary>
    /// <param name="report">The report entity to create</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result containing the created report or error</returns>
    Task<Result<Report>> CreateReportAsync(Report report, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a report by its unique identifier asynchronously
    /// </summary>
    /// <param name="id">The report identifier</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result containing the report or error</returns>
    Task<Result<Report>> GetReportByCodeAsync(ReportID id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all reports asynchronously
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result containing list of reports or error</returns>
    Task<Result<List<Report>>> GetAllReportsAsync(CancellationToken ct = default);

    /// <summary>
    /// Updates an existing report asynchronously
    /// </summary>
    /// <param name="report">The report entity to update</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result containing the updated report or error</returns>
    Task<Result<Report>> UpdateReportAsync(Report report, CancellationToken ct = default);

    /// <summary>
    /// Deletes a report by its unique identifier asynchronously
    /// </summary>
    /// <param name="id">The report identifier</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result containing success status or error</returns>
    Task<Result<bool>> DeleteReportAsync(ReportID id, CancellationToken ct = default);
}

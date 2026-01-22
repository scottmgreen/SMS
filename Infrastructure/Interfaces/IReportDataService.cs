namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// Interface for Report Data Service operations
/// Provides CRUD operations for Report entities
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
    Task<Result<Report>> GetReportByIdAsync(ReportID id, CancellationToken ct = default);

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
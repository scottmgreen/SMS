using SMS_Domain.Entities;

namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// Interface for Airport Shared Dataset Data Service operations
/// Provides CRUD operations for Airport Shared Dataset entities
/// This critical SMS compliance interface manages all regulatory-required data elements
/// for SMS Risk validation and hazard processing
/// </summary>
public interface IAirportSharedDatasetDataService
{
    /// <summary>
    /// Creates a new airport shared dataset asynchronously
    /// IMPORTANT: Requires ReportID - cannot create dataset without parent report
    /// </summary>
    /// <param name="airportSharedDataset">The airport shared dataset entity to create</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result containing the created airport shared dataset or error</returns>
    Task<Result<AirportSharedDataset>> CreateAirportSharedDatasetAsync(AirportSharedDataset airportSharedDataset, CancellationToken ct = default);

    /// <summary>
    /// Retrieves an airport shared dataset by its unique identifier asynchronously
    /// </summary>
    /// <param name="id">The airport shared dataset identifier</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result containing the airport shared dataset or error</returns>
    Task<Result<AirportSharedDataset>> GetAirportSharedDatasetByCodeAsync(AirportSharedDatasetID code, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all airport shared datasets asynchronously
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result containing list of airport shared datasets or error</returns>
    Task<Result<List<AirportSharedDataset>>> GetAllAirportSharedDatasetsAsync(CancellationToken ct = default);

    /// <summary>
    /// Updates an existing airport shared dataset asynchronously
    /// </summary>
    /// <param name="airportSharedDataset">The airport shared dataset entity to update</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result containing the updated airport shared dataset or error</returns>
    Task<Result<AirportSharedDataset>> UpdateAirportSharedDatasetAsync(AirportSharedDataset airportSharedDataset, CancellationToken ct = default);

    /// <summary>
    /// Deletes an airport shared dataset by its unique identifier asynchronously
    /// </summary>
    /// <param name="id">The airport shared dataset identifier</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result containing success status or error</returns>
    Task<Result<bool>> DeleteAirportSharedDatasetAsync(AirportSharedDatasetID code, CancellationToken ct = default);
}
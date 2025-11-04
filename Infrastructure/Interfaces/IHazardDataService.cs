using SMS_Domain.Entities;

namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// Interface for Hazard Data Service operations
/// Provides CRUD operations for Hazard entities
/// </summary>
public interface IHazardDataService
{
    /// <summary>
    /// Creates a new hazard asynchronously
    /// </summary>
    /// <param name="hazard">The hazard entity to create</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result containing the created hazard or error</returns>
    Task<Result<Hazard>> CreateHazardAsync(Hazard hazard, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a hazard by its unique identifier asynchronously
    /// </summary>
    /// <param name="id">The hazard identifier</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result containing the hazard or error</returns>
    Task<Result<Hazard>> GetHazardByIdAsync(HazardID id, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all hazards asynchronously
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result containing list of hazards or error</returns>
    Task<Result<List<Hazard>>> GetAllHazardsAsync(CancellationToken ct = default);

    /// <summary>
    /// Updates an existing hazard asynchronously
    /// </summary>
    /// <param name="hazard">The hazard entity to update</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result containing the updated hazard or error</returns>
    Task<Result<Hazard>> UpdateHazardAsync(Hazard hazard, CancellationToken ct = default);

    /// <summary>
    /// Deletes a hazard by its unique identifier asynchronously
    /// </summary>
    /// <param name="id">The hazard identifier</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result containing success status or error</returns>
    Task<Result<bool>> DeleteHazardAsync(HazardID id, CancellationToken ct = default);
}
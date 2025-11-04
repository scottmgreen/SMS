namespace SMS_Domain.Entities;

/// <summary>
/// Represents a unique identifier for an Airport Shared Dataset entity.
/// This critical SMS compliance entity contains all regulatory-required data elements
/// for SMS Risk validation and hazard processing.
/// </summary>
public class AirportSharedDatasetID : BaseID<string>
{
    /// <summary>
    /// Initializes a new instance of the AirportSharedDatasetID class.
    /// </summary>
    /// <param name="id">The string identifier value</param>
    public AirportSharedDatasetID(string id) : base(id)
    {
    }
}
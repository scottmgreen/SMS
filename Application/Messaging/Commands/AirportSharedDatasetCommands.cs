namespace SMS_Application.Messaging.Commands;

/// <summary>
/// Command to create a new Airport Shared Dataset.
/// IMPORTANT: Requires ReportID - cannot create dataset without parent report.
/// </summary>
public class CreateAirportSharedDatasetCommand : BaseCommandBundle, IRequest<Result<AirportSharedDataset>>, ICreateCommand
{
    /// <summary>
    /// The Airport Shared Dataset entity to create
    /// </summary>
    public AirportSharedDataset AirportSharedDataset { get; set; }

    /// <summary>
    /// Initializes a new instance of the CreateAirportSharedDatasetCommand class.
    /// </summary>
    /// <param name="airportSharedDataset">The airport shared dataset to create</param>
    /// <exception cref="ArgumentNullException">Thrown when airportSharedDataset is null</exception>
    public CreateAirportSharedDatasetCommand(AirportSharedDataset airportSharedDataset)
    {
        AirportSharedDataset = airportSharedDataset ?? throw new ArgumentNullException(nameof(airportSharedDataset));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        AirportSharedDataset.CreatedBy = userId;
        AirportSharedDataset.CreatedDate = timestamp;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // For create commands, we typically don't set UpdatedBy
    }
}

/// <summary>
/// Command to update an existing Airport Shared Dataset.
/// </summary>
public class UpdateAirportSharedDatasetCommand : BaseCommandBundle, IRequest<Result<AirportSharedDataset>>, IUpdateCommand
{
    /// <summary>
    /// The Airport Shared Dataset entity to update
    /// </summary>
    public AirportSharedDataset AirportSharedDataset { get; set; }

    /// <summary>
    /// Initializes a new instance of the UpdateAirportSharedDatasetCommand class.
    /// </summary>
    /// <param name="airportSharedDataset">The airport shared dataset to update</param>
    /// <exception cref="ArgumentNullException">Thrown when airportSharedDataset is null</exception>
    public UpdateAirportSharedDatasetCommand(AirportSharedDataset airportSharedDataset)
    {
        AirportSharedDataset = airportSharedDataset ?? throw new ArgumentNullException(nameof(airportSharedDataset));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // For update commands, we typically don't modify CreatedBy
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        AirportSharedDataset.UpdatedBy = userId;
        AirportSharedDataset.UpdatedDate = timestamp;
    }
}

/// <summary>
/// Command to delete an Airport Shared Dataset by ID.
/// </summary>
public class DeleteAirportSharedDatasetCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    /// <summary>
    /// The ID of the Airport Shared Dataset to delete
    /// </summary>
    public AirportSharedDatasetID AirportSharedDatasetId { get; set; }

    /// <summary>
    /// Initializes a new instance of the DeleteAirportSharedDatasetCommand class.
    /// </summary>
    /// <param name="airportSharedDatasetId">The ID of the airport shared dataset to delete</param>
    /// <exception cref="ArgumentNullException">Thrown when airportSharedDatasetId is null</exception>
    public DeleteAirportSharedDatasetCommand(AirportSharedDatasetID airportSharedDatasetId)
    {
        AirportSharedDatasetId = airportSharedDatasetId ?? throw new ArgumentNullException(nameof(airportSharedDatasetId));
    }
}

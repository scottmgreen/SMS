namespace SMS_Application.Messaging.Queries;

// =============================================
// AIRPORT SHARED DATASET QUERIES
// =============================================

public class GetAirportSharedDatasetByIdQuery : BaseQueryBundle, IRequest<Result<AirportSharedDataset>>
{
    public AirportSharedDatasetID AirportSharedDatasetId { get; set; }

    public GetAirportSharedDatasetByIdQuery(AirportSharedDatasetID airportSharedDatasetId)
    {
        AirportSharedDatasetId = airportSharedDatasetId ?? throw new ArgumentNullException(nameof(airportSharedDatasetId));
    }
}

public class GetAllAirportSharedDatasetsQuery : BaseQueryBundle, IRequest<Result<List<AirportSharedDataset>>>
{
    public GetAllAirportSharedDatasetsQuery()
    {
    }
}
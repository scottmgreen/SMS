namespace SMS_Application.Messaging.Queries;

// =============================================
// AIRPORT SHARED DATASET QUERIES
// =============================================

public class GetAirportSharedDatasetByCodeQuery : BaseQueryBundle, IRequest<Result<AirportSharedDataset>>
{
    public AirportSharedDatasetID AirportSharedDatasetCode { get; set; }

    public GetAirportSharedDatasetByCodeQuery(AirportSharedDatasetID airportSharedDatasetCode)
    {
        AirportSharedDatasetCode = airportSharedDatasetCode ?? throw new ArgumentNullException(nameof(airportSharedDatasetCode));
    }
}

public class GetAllAirportSharedDatasetsQuery : BaseQueryBundle, IRequest<Result<List<AirportSharedDataset>>>
{
    public GetAllAirportSharedDatasetsQuery()
    {
    }
}
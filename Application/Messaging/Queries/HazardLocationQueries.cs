using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Application.Common;
using SMS_Application.Interfaces;
using SMS_Shared.Common;

namespace SMS_Application.Messaging.Queries;

// =============================================
// HAZARD LOCATION QUERIES
// =============================================

public class GetHazardLocationByCodeQuery : BaseQueryBundle, IRequest<Result<HazardLocation>>
{
    public HazardLocationID HazardLocationId { get; set; }

    public GetHazardLocationByCodeQuery(HazardLocationID hazardLocationId)
    {
        HazardLocationId = hazardLocationId ?? throw new ArgumentNullException(nameof(hazardLocationId));
    }
}

public class GetAllHazardLocationsQuery : BaseQueryBundle, IRequest<Result<List<HazardLocation>>>
{
    public GetAllHazardLocationsQuery()
    {
    }
}

public class GetHazardLocationsByHazardCodeQuery : BaseQueryBundle, IRequest<Result<List<HazardLocation>>>
{
    public string HazardCode { get; set; }

    public GetHazardLocationsByHazardCodeQuery(string hazardCode)
    {
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
    }
}
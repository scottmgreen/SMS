//-----------------------------------------------------------------------
// <copyright file="AirportSharedDatasetQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for read operations in the SMS CQRS architecture.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Messaging.Queries;

// =============================================
// AIRPORT SHARED DATASET QUERIES
// =============================================

public class GetAirportSharedDatasetByCodeQuery : BaseQueryBundle, IRequest<Result<AirportSharedDataset>>, IReadQuery
{
    public AirportSharedDatasetID AirportSharedDatasetCode { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetAirportSharedDatasetByCodeQuery(AirportSharedDatasetID airportSharedDatasetCode)
    {
        AirportSharedDatasetCode = airportSharedDatasetCode ?? throw new ArgumentNullException(nameof(airportSharedDatasetCode));
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"AirportSharedDataset:{AirportSharedDatasetCode?.Value ?? "Unknown"}";
    }

    public string GetAccessType()
    {
        return "GetById";
    }
}

public class GetAllAirportSharedDatasetsQuery : BaseQueryBundle, IRequest<Result<List<AirportSharedDataset>>>, IReadQuery
{
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetAllAirportSharedDatasetsQuery()
    {
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return "AirportSharedDataset:All";
    }

    public string GetAccessType()
    {
        return "GetAll";
    }
}

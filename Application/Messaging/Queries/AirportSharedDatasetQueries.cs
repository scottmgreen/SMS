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

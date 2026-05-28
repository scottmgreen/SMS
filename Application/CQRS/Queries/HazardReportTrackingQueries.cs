//-----------------------------------------------------------------------
// <copyright file="HazardReportTrackingQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for SMS hazard data retrieval and analysis operations.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using SMS_Domain.Entities;

namespace SMS_Application.Queries;

// =============================================
// HAZARD REPORT TRACKING QUERIES
// =============================================

public class GetHazardReportTrackingByIdQuery : BaseQueryBundle, IRequest<Result<HazardReportTracking>>, IReadQuery
{
    public HazardReportTrackingID HazardReportTrackingId { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetHazardReportTrackingByIdQuery(HazardReportTrackingID hazardReportTrackingId)
    {
        HazardReportTrackingId = hazardReportTrackingId ?? throw new ArgumentNullException(nameof(hazardReportTrackingId));
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"HazardReportTracking:{HazardReportTrackingId?.Value ?? "Unknown"}";
    }

    public string GetAccessType()
    {
        return "GetById";
    }
}

public class GetAllHazardReportTrackingQuery : BaseQueryBundle, IRequest<Result<List<HazardReportTracking>>>, IReadQuery
{
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetAllHazardReportTrackingQuery()
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
        return "HazardReportTracking:All";
    }

    public string GetAccessType()
    {
        return "GetAll";
    }
}

/// <summary>
/// Query to get hazard report tracking by tracking code - Primary use case for users
/// </summary>
public class GetHazardReportTrackingByTrackingCodeQuery : BaseQueryBundle, IRequest<Result<HazardReportTrackingDetails>>, IReadQuery
{
    public string TrackingCode { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetHazardReportTrackingByTrackingCodeQuery(string trackingCode)
    {
        TrackingCode = trackingCode ?? throw new ArgumentNullException(nameof(trackingCode));
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"HazardReportTracking:ByTrackingCode:{TrackingCode}";
    }

    public string GetAccessType()
    {
        return "GetByTrackingCode";
    }
}

/// <summary>
/// Query to get all tracking records for a specific hazard
/// </summary>
public class GetHazardReportTrackingByHazardCodeQuery : BaseQueryBundle, IRequest<Result<List<HazardReportTracking>>>, IReadQuery
{
    public string HazardCode { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetHazardReportTrackingByHazardCodeQuery(string hazardCode)
    {
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"HazardReportTracking:ByHazard:{HazardCode}";
    }

    public string GetAccessType()
    {
        return "GetByHazard";
    }
}

/// <summary>
/// Query to get all tracking records for a specific report
/// </summary>
public class GetHazardReportTrackingByReportCodeQuery : BaseQueryBundle, IRequest<Result<List<HazardReportTracking>>>, IReadQuery
{
    public string ReportCode { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetHazardReportTrackingByReportCodeQuery(string reportCode)
    {
        ReportCode = reportCode ?? throw new ArgumentNullException(nameof(reportCode));
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"HazardReportTracking:ByReport:{ReportCode}";
    }

    public string GetAccessType()
    {
        return "GetByReport";
    }
}

/// <summary>
/// Detailed tracking information including current status and processing details
/// </summary>
public class HazardReportTrackingDetails
{
    public HazardReportTracking HazardReportTracking { get; set; } = null!;
    public Hazard? Hazard { get; set; }
    public Report? Report { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
    public string ProcessingStage { get; set; } = string.Empty;
    public DateTime? LastUpdated { get; set; }
    public List<string> ProcessingNotes { get; set; } = new();

    // Convenience properties for easier access
    public string TrackingCode => HazardReportTracking?.TrackingCode ?? string.Empty;
    public string HazardCode => HazardReportTracking?.HazardCode ?? string.Empty;
    public string ReportCode => HazardReportTracking?.ReportCode ?? string.Empty;
    public DateTime CreatedDate => HazardReportTracking?.CreatedDate ?? DateTime.MinValue;
}

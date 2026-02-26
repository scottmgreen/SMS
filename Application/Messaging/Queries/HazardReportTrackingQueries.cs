//-----------------------------------------------------------------------
// <copyright file="HazardReportTrackingQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for SMS hazard data retrieval and analysis operations.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Messaging.Queries;

// =============================================
// HAZARD REPORT TRACKING QUERIES
// =============================================

public class GetHazardReportTrackingByIdQuery : BaseQueryBundle, IRequest<Result<HazardReportTracking>>
{
    public HazardReportTrackingID HazardReportTrackingId { get; set; }

    public GetHazardReportTrackingByIdQuery(HazardReportTrackingID hazardReportTrackingId)
    {
        HazardReportTrackingId = hazardReportTrackingId ?? throw new ArgumentNullException(nameof(hazardReportTrackingId));
    }
}

public class GetAllHazardReportTrackingQuery : BaseQueryBundle, IRequest<Result<List<HazardReportTracking>>>
{
    public GetAllHazardReportTrackingQuery()
    {
    }
}

/// <summary>
/// Query to get hazard report tracking by tracking code - Primary use case for users
/// </summary>
public class GetHazardReportTrackingByTrackingCodeQuery : BaseQueryBundle, IRequest<Result<HazardReportTrackingDetails>>
{
    public string TrackingCode { get; set; }

    public GetHazardReportTrackingByTrackingCodeQuery(string trackingCode)
    {
        TrackingCode = trackingCode ?? throw new ArgumentNullException(nameof(trackingCode));
    }
}

/// <summary>
/// Query to get all tracking records for a specific hazard
/// </summary>
public class GetHazardReportTrackingByHazardCodeQuery : BaseQueryBundle, IRequest<Result<List<HazardReportTracking>>>
{
    public string HazardCode { get; set; }

    public GetHazardReportTrackingByHazardCodeQuery(string hazardCode)
    {
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
    }
}

/// <summary>
/// Query to get all tracking records for a specific report
/// </summary>
public class GetHazardReportTrackingByReportCodeQuery : BaseQueryBundle, IRequest<Result<List<HazardReportTracking>>>
{
    public string ReportCode { get; set; }

    public GetHazardReportTrackingByReportCodeQuery(string reportCode)
    {
        ReportCode = reportCode ?? throw new ArgumentNullException(nameof(reportCode));
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

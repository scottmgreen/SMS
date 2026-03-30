//-----------------------------------------------------------------------
// <copyright file="IHazardReportTrackingService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS Hazard management service handling hazard identification and lifecycle.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using SMS_Application.Messaging.Queries;

namespace SMS_Application.Interfaces;

/// <summary>
/// Interface for HazardReportTracking service operations
/// </summary>
public interface IHazardReportTrackingService
{
    /// <summary>
    /// Create a new hazard report tracking record
    /// </summary>
    Task<Result<HazardReportTracking>> CreateHazardReportTrackingAsync(HazardReportTracking hazardReportTracking, CancellationToken ct = default);

    /// <summary>
    /// Create a new hazard report with automatic tracking code generation
    /// </summary>
    Task<Result<HazardReportTrackingResult>> CreateHazardReportWithTrackingAsync(string hazardCode, string reportCode, CancellationToken ct = default);

    /// <summary>
    /// Get hazard report tracking by ID
    /// </summary>
    Task<Result<HazardReportTracking>> GetHazardReportTrackingByIdAsync(HazardReportTrackingID id, CancellationToken ct = default);

    /// <summary>
    /// Get detailed tracking information by tracking code - Primary method for users tracking their reports
    /// </summary>
    Task<Result<HazardReportTrackingDetails>> GetHazardReportTrackingDetailsByTrackingCodeAsync(string trackingCode, CancellationToken ct = default);

    /// <summary>
    /// Get all hazard report tracking records
    /// </summary>
    Task<Result<List<HazardReportTracking>>> GetAllHazardReportTrackingAsync(CancellationToken ct = default);

    /// <summary>
    /// Get tracking records by hazard code
    /// </summary>
    Task<Result<List<HazardReportTracking>>> GetHazardReportTrackingByHazardCodeAsync(string hazardCode, CancellationToken ct = default);

    /// <summary>
    /// Get tracking records by report code
    /// </summary>
    Task<Result<List<HazardReportTracking>>> GetHazardReportTrackingByReportCodeAsync(string reportCode, CancellationToken ct = default);

    /// <summary>
    /// Update hazard report tracking
    /// </summary>
    Task<Result<HazardReportTracking>> UpdateHazardReportTrackingAsync(HazardReportTracking hazardReportTracking, CancellationToken ct = default);

    /// <summary>
    /// Delete hazard report tracking
    /// </summary>
    Task<Result<bool>> DeleteHazardReportTrackingAsync(HazardReportTrackingID id, CancellationToken ct = default);
}

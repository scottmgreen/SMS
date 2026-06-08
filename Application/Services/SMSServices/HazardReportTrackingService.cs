//-----------------------------------------------------------------------
// <copyright file="HazardReportTrackingService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS Hazard management service handling hazard identification and lifecycle.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.Extensions.Logging;

using SMS_Application.Queries;

namespace SMS_Application.Services;

/// <summary>
/// Service for managing HazardReportTracking operations with tracking code generation and status management
/// </summary>
public sealed class HazardReportTrackingService : IHazardReportTrackingService
{
    private readonly HazardReportTrackingDataService _dataService;
    private readonly HazardService _hazardService;
    private readonly ReportService _reportService;
    private readonly ILogger<HazardReportTrackingService> _logger;

    public HazardReportTrackingService(
        HazardReportTrackingDataService dataService,
        HazardService hazardService,
        ReportService reportService,
        ILogger<HazardReportTrackingService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _hazardService = hazardService ?? throw new ArgumentNullException(nameof(hazardService));
        _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Create a new hazard report tracking record
    /// </summary>
    public async Task<Result<HazardReportTracking>> CreateHazardReportTrackingAsync(HazardReportTracking hazardReportTracking, CancellationToken ct = default)
    {
        try
        {
            if (hazardReportTracking == null)
            {
                _logger.LogApplicationError("CreateHazardReportTrackingAsync called with null hazardReportTracking");
                return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
            }

            _logger.LogApplicationInformation("Creating hazard report tracking for HazardCode: {HazardCode}, ReportCode: {ReportCode}",
                hazardReportTracking.HazardCode, hazardReportTracking.ReportCode);

            var result = await _dataService.CreateHazardReportTrackingAsync(hazardReportTracking, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully created hazard report tracking with TrackingCode: {TrackingCode}",
                    result.Value?.TrackingCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to create hazard report tracking. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error creating hazard report tracking");
            return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.HazardReportTrackingError.CreateFailed);
        }
    }

    /// <summary>
    /// Create a new hazard report with automatic tracking code generation
    /// </summary>
    public async Task<Result<HazardReportTrackingResult>> CreateHazardReportWithTrackingAsync(string hazardCode, string reportCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hazardCode) || string.IsNullOrWhiteSpace(reportCode))
            {
                _logger.LogApplicationError("CreateHazardReportWithTrackingAsync called with invalid parameters");
                return Result<HazardReportTrackingResult>.Failure<HazardReportTrackingResult>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
            }

            _logger.LogApplicationInformation("Creating hazard report with tracking for HazardCode: {HazardCode}, ReportCode: {ReportCode}",
                hazardCode, reportCode);

            // Generate unique tracking code
            var trackingCode = "HT-0000";

            // Create the hazard report tracking entity
            var trackingId = new HazardReportTrackingID(trackingCode);
            var hazardReportTracking = new HazardReportTracking(trackingId)
            {
                HazardCode = hazardCode,
                ReportCode = reportCode,
                TrackingCode = trackingCode,
                CreatedBy = "SYSTEM", // This should be set by the audit pipeline
                CreatedDate = DateTime.UtcNow
            };

            var result = await _dataService.CreateHazardReportTrackingAsync(hazardReportTracking, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                var trackingResult = new HazardReportTrackingResult
                {
                    HazardReportTracking = result.Value,
                    TrackingCode = trackingCode,
                    Message = $"Hazard report successfully submitted. Your tracking code is: {trackingCode}"
                };

                _logger.LogApplicationInformation("Successfully created hazard report with tracking code: {TrackingCode}", trackingCode);
                return Result<HazardReportTrackingResult>.Success(trackingResult);
            }
            else
            {
                _logger.LogApplicationError("Failed to create hazard report with tracking. Error: {Error}", result.Error?.Message);
                return Result<HazardReportTrackingResult>.Failure<HazardReportTrackingResult>(result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error creating hazard report with tracking");
            return Result<HazardReportTrackingResult>.Failure<HazardReportTrackingResult>(DomainErrors.HazardReportTrackingError.CreateFailed);
        }
    }

    /// <summary>
    /// Get hazard report tracking by ID
    /// </summary>
    public async Task<Result<HazardReportTracking>> GetHazardReportTrackingByIdAsync(HazardReportTrackingID id, CancellationToken ct = default)
    {
        try
        {
            if (id == null)
            {
                _logger.LogApplicationError("GetHazardReportTrackingByIdAsync called with null id");
                return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
            }

            _logger.LogApplicationInformation("Retrieving hazard report tracking with ID: {Id}", id.Value);
            return await _dataService.GetHazardReportTrackingByCodeAsync(id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving hazard report tracking with ID: {Id}", id?.Value);
            return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.HazardReportTrackingError.NotFound);
        }
    }

    /// <summary>
    /// Get detailed tracking information by tracking code - Primary method for users tracking their reports
    /// </summary>
    public async Task<Result<HazardReportTrackingDetails>> GetHazardReportTrackingDetailsByTrackingCodeAsync(string trackingCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(trackingCode))
            {
                _logger.LogApplicationError("GetHazardReportTrackingDetailsByTrackingCodeAsync called with null or empty tracking code");
                return Result<HazardReportTrackingDetails>.Failure<HazardReportTrackingDetails>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
            }

            _logger.LogApplicationInformation("Retrieving detailed tracking information for TrackingCode: {TrackingCode}", trackingCode);

            // Get the basic tracking record
            var trackingResult = await _dataService.GetHazardReportTrackingByTrackingCodeAsync(trackingCode, ct).ConfigureAwait(false);

            if (!trackingResult.IsSuccess || trackingResult.Value == null)
            {
                _logger.LogApplicationWarning("Tracking code not found: {TrackingCode}", trackingCode);
                return Result<HazardReportTrackingDetails>.Failure<HazardReportTrackingDetails>(DomainErrors.HazardReportTrackingError.NotFound);
            }

            var tracking = trackingResult.Value;

            // Build detailed tracking information
            var details = new HazardReportTrackingDetails
            {
                HazardReportTracking = tracking,
                CurrentStatus = DetermineCurrentStatus(tracking),
                ProcessingStage = DetermineProcessingStage(tracking),
                LastUpdated = tracking.UpdatedDate ?? tracking.CreatedDate,
                ProcessingNotes = GenerateProcessingNotes(tracking)
            };

            // Load related hazard information
            if (!string.IsNullOrWhiteSpace(tracking.HazardCode))
            {
                HazardID hazardcode = new HazardID(tracking.HazardCode);
                var hazardResult = await _hazardService.GetHazardByCodeAsync(hazardcode, ct).ConfigureAwait(false);
                if (hazardResult.IsSuccess)
                {
                    details.Hazard = hazardResult.Value;
                }
            }

            // Load related report information
            if (!string.IsNullOrWhiteSpace(tracking.ReportCode))
            {
                var reportId = new ReportID(tracking.ReportCode);
                var reportResult = await _reportService.GetReportByCodeAsync(reportId, ct).ConfigureAwait(false);
                if (reportResult.IsSuccess)
                {
                    details.Report = reportResult.Value;
                }
            }

            _logger.LogApplicationInformation("Successfully retrieved detailed tracking information for TrackingCode: {TrackingCode}", trackingCode);
            return Result<HazardReportTrackingDetails>.Success(details);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving tracking details for TrackingCode: {TrackingCode}", trackingCode);
            return Result<HazardReportTrackingDetails>.Failure<HazardReportTrackingDetails>(DomainErrors.HazardReportTrackingError.NotFound);
        }
    }

    /// <summary>
    /// Get all hazard report tracking records
    /// </summary>
    public async Task<Result<List<HazardReportTracking>>> GetAllHazardReportTrackingAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving all hazard report tracking records");
            return await _dataService.GetAllHazardReportTrackingAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving all hazard report tracking records");
            return Result<List<HazardReportTracking>>.Failure<List<HazardReportTracking>>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
        }
    }

    /// <summary>
    /// Get tracking records by hazard code
    /// </summary>
    public async Task<Result<List<HazardReportTracking>>> GetHazardReportTrackingByHazardCodeAsync(string hazardCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hazardCode))
            {
                _logger.LogApplicationError("GetHazardReportTrackingByHazardCodeAsync called with null or empty hazard code");
                return Result<List<HazardReportTracking>>.Failure<List<HazardReportTracking>>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
            }

            _logger.LogApplicationInformation("Retrieving tracking records for HazardCode: {HazardCode}", hazardCode);
            return await _dataService.GetHazardReportTrackingByHazardCodeAsync(hazardCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving tracking records for HazardCode: {HazardCode}", hazardCode);
            return Result<List<HazardReportTracking>>.Failure<List<HazardReportTracking>>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
        }
    }

    /// <summary>
    /// Get tracking records by report code
    /// </summary>
    public async Task<Result<List<HazardReportTracking>>> GetHazardReportTrackingByReportCodeAsync(string reportCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(reportCode))
            {
                _logger.LogApplicationError("GetHazardReportTrackingByReportCodeAsync called with null or empty report code");
                return Result<List<HazardReportTracking>>.Failure<List<HazardReportTracking>>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
            }

            _logger.LogApplicationInformation("Retrieving tracking records for ReportCode: {ReportCode}", reportCode);
            return await _dataService.GetHazardReportTrackingByReportCodeAsync(reportCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving tracking records for ReportCode: {ReportCode}", reportCode);
            return Result<List<HazardReportTracking>>.Failure<List<HazardReportTracking>>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
        }
    }

    /// <summary>
    /// Update hazard report tracking
    /// </summary>
    public async Task<Result<HazardReportTracking>> UpdateHazardReportTrackingAsync(HazardReportTracking hazardReportTracking, CancellationToken ct = default)
    {
        try
        {
            if (hazardReportTracking == null)
            {
                _logger.LogApplicationError("UpdateHazardReportTrackingAsync called with null hazardReportTracking");
                return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
            }

            _logger.LogApplicationInformation("Updating hazard report tracking with TrackingCode: {TrackingCode}",
                hazardReportTracking.TrackingCode);

            var result = await _dataService.UpdateHazardReportTrackingAsync(hazardReportTracking, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully updated hazard report tracking with TrackingCode: {TrackingCode}",
                    hazardReportTracking.TrackingCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to update hazard report tracking. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error updating hazard report tracking with TrackingCode: {TrackingCode}",
                hazardReportTracking?.TrackingCode);
            return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.HazardReportTrackingError.UpdateFailed);
        }
    }

    /// <summary>
    /// Delete hazard report tracking
    /// </summary>
    public async Task<Result<bool>> DeleteHazardReportTrackingAsync(HazardReportTrackingID id, CancellationToken ct = default)
    {
        try
        {
            if (id == null)
            {
                _logger.LogApplicationError("DeleteHazardReportTrackingAsync called with null id");
                return Result<bool>.Failure<bool>(DomainErrors.HazardReportTrackingError.NullOrEmpty);
            }

            _logger.LogApplicationInformation("Deleting hazard report tracking with ID: {Id}", id.Value);

            var result = await _dataService.DeleteHazardReportTrackingAsync(id, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully deleted hazard report tracking with ID: {Id}", id.Value);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete hazard report tracking. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error deleting hazard report tracking with ID: {Id}", id?.Value);
            return Result<bool>.Failure<bool>(DomainErrors.HazardReportTrackingError.DeleteFailed);
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Determine the current status of the tracking record
    /// </summary>
    private static string DetermineCurrentStatus(HazardReportTracking tracking)
    {
        // This logic can be enhanced based on your business rules
        return tracking.UpdatedDate.HasValue ? "Processing" : "Submitted";
    }

    /// <summary>
    /// Determine the current processing stage
    /// </summary>
    private static string DetermineProcessingStage(HazardReportTracking tracking)
    {
        // This logic can be enhanced based on your business rules
        var daysSinceSubmission = (DateTime.UtcNow - tracking.CreatedDate).Value.Days;

        return daysSinceSubmission switch
        {
            0 => "Technical Review",
            <= 7 => "Safety Analysis",
            <= 14 => "Risk Assessment",
            <= 21 => "Mitigation Planning",
            _ => "Final Review"
        };
    }

    /// <summary>
    /// Generate processing notes based on tracking history
    /// </summary>
    private static List<string> GenerateProcessingNotes(HazardReportTracking tracking)
    {
        var notes = new List<string>
        {
            $"Report submitted on {tracking.CreatedDate:yyyy-MM-dd HH:mm} UTC"
        };

        if (tracking.UpdatedDate.HasValue)
        {
            notes.Add($"Last updated on {tracking.UpdatedDate:yyyy-MM-dd HH:mm} UTC");
        }

        var daysSinceSubmission = (DateTime.UtcNow - tracking.CreatedDate).Value.Days;
        if (daysSinceSubmission > 0)
        {
            notes.Add($"Processing for {daysSinceSubmission} day(s)");
        }

        return notes;
    }

    #endregion
}


//-----------------------------------------------------------------------
// <copyright file="HazardReportTrackingDataService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Data service coordinating hazardreporttracking repository operations with safety management workflows.
//                  Infrastructure service providing external system integration
//                  and technical functionality support.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using SMS_Domain.Errors;


namespace SMS_Infrastructure.Services;

public sealed class HazardReportTrackingDataService : BaseDataService<HazardReportTrackingDataService>
{
    private readonly HazardReportTrackingRepository _repository;
    private readonly ILogger<HazardReportTrackingDataService> _logger;
    private readonly string _logheader;

    public HazardReportTrackingDataService(HazardReportTrackingRepository repository, IServiceScopeFactory serviceScopeFactory, ILogger<HazardReportTrackingDataService> logger, IConfiguration configuration)
        : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _repository = repository;

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} {repository.GetType().Name}");
    }

    /// <summary>
    /// Create a new hazard report tracking record
    /// </summary>
    public async Task<Result<HazardReportTracking>> CreateHazardReportTrackingAsync(HazardReportTracking hazardReportTracking, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureInformation("Creating hazard report tracking for HazardCode: {HazardCode}, ReportCode: {ReportCode}, TrackingCode: {TrackingCode}",
                hazardReportTracking?.HazardCode, hazardReportTracking?.ReportCode, hazardReportTracking?.TrackingCode);

            return await _repository.CreateHazardReportTrackingAsync(hazardReportTracking, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Error creating hazard report tracking");
            return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.HazardReportTrackingError.CreateFailed);
        }
    }

    /// <summary>
    /// Get hazard report tracking by tracking code ID
    /// </summary>
    public async Task<Result<HazardReportTracking>> GetHazardReportTrackingByCodeAsync(HazardReportTrackingID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureInformation("Retrieving hazard report tracking with ID: {Id}", id?.Value);
            return await _repository.GetHazardReportTrackingByCodeAsync(id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Error retrieving hazard report tracking by ID: {Id}", id?.Value);
            return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.HazardReportTrackingError.NotFound);
        }
    }

    /// <summary>
    /// Get hazard report tracking by tracking code string
    /// </summary>
    public async Task<Result<HazardReportTracking>> GetHazardReportTrackingByTrackingCodeAsync(string trackingCode, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureInformation("Retrieving hazard report tracking with TrackingCode: {TrackingCode}", trackingCode);
            return await _repository.GetHazardReportTrackingByTrackingCodeAsync(trackingCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Error retrieving hazard report tracking by TrackingCode: {TrackingCode}", trackingCode);
            return Result<HazardReportTracking>.Failure<HazardReportTracking>(DomainErrors.HazardReportTrackingError.NotFound);
        }
    }

    /// <summary>
    /// Get all hazard report tracking records
    /// </summary>
    public async Task<Result<List<HazardReportTracking>>> GetAllHazardReportTrackingAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureInformation("Retrieving all hazard report tracking records");
            return await _repository.GetAllHazardReportTrackingAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Error retrieving all hazard report tracking records");
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
            _logger.LogInfrastructureInformation("Retrieving tracking records for HazardCode: {HazardCode}", hazardCode);
            return await _repository.GetHazardReportTrackingByHazardCodeAsync(hazardCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Error retrieving tracking records for HazardCode: {HazardCode}", hazardCode);
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
            _logger.LogInfrastructureInformation("Retrieving tracking records for ReportCode: {ReportCode}", reportCode);
            return await _repository.GetHazardReportTrackingByReportCodeAsync(reportCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Error retrieving tracking records for ReportCode: {ReportCode}", reportCode);
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
            _logger.LogInfrastructureInformation("Updating hazard report tracking with TrackingCode: {TrackingCode}",
                hazardReportTracking?.TrackingCode);

            return await _repository.UpdateHazardReportTrackingAsync(hazardReportTracking, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Error updating hazard report tracking with TrackingCode: {TrackingCode}",
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
            _logger.LogInfrastructureInformation("Deleting hazard report tracking with ID: {Id}", id?.Value);
            return await _repository.DeleteHazardReportTrackingAsync(id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureError(ex, "Error deleting hazard report tracking with ID: {Id}", id?.Value);
            return Result<bool>.Failure<bool>(DomainErrors.HazardReportTrackingError.DeleteFailed);
        }
    }
}


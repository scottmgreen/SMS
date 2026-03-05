//-----------------------------------------------------------------------
// <copyright file="SafetyPerformanceIndicatorService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application service providing business logic operations for SMS domain entities.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;

namespace SMS_Application.Services;

/// <summary>
/// Application service for Safety Performance Indicator business operations
/// Provides high-level business logic and coordination between domain and infrastructure
/// </summary>
public class SafetyPerformanceIndicatorService : ISafetyPerformanceIndicatorService
{
    private readonly SafetyPerformanceIndicatorDataService _dataService;
    private readonly ILogger<SafetyPerformanceIndicatorService> _logger;

    public SafetyPerformanceIndicatorService(
        SafetyPerformanceIndicatorDataService dataService,
        ILogger<SafetyPerformanceIndicatorService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region ISafetyPerformanceIndicatorService Implementation

    /// <summary>
    /// Creates a new safety performance indicator
    /// </summary>
    public async Task<Result<SafetyPerformanceIndicator>> CreateSafetyPerformanceIndicatorAsync(SafetyPerformanceIndicator spi, CancellationToken ct = default)
    {
        try
        {
            if (spi is null)
            {
                _logger.LogError("CreateSafetyPerformanceIndicatorAsync received null SPI");
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NullOrEmpty);
            }

            _logger.LogInformation("Creating safety performance indicator: {Name} (Code: {Code})", spi.Name, spi.Code);
            return await _dataService.CreateSafetyPerformanceIndicatorAsync(spi, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating safety performance indicator");
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.CreateFailed);
        }
    }

    /// <summary>
    /// Gets safety performance indicator by ID
    /// </summary>
    public async Task<Result<SafetyPerformanceIndicator>> GetSafetyPerformanceIndicatorByIdAsync(SafetyPerformanceIndicatorID id, CancellationToken ct = default)
    {
        try
        {
            if (id is null)
            {
                _logger.LogError("GetSafetyPerformanceIndicatorByIdAsync received null ID");
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NullOrEmpty);
            }

            _logger.LogInformation("Retrieving safety performance indicator with ID: {Id}", id.Value);
            return await _dataService.GetSafetyPerformanceIndicatorByIdAsync(id, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving safety performance indicator with ID: {Id}", id?.Value);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NotFound);
        }
    }

    /// <summary>
    /// Gets safety performance indicator by code
    /// </summary>
    public async Task<Result<SafetyPerformanceIndicator>> GetSafetyPerformanceIndicatorByCodeAsync(string code, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                _logger.LogError("GetSafetyPerformanceIndicatorByCodeAsync received null or empty code");
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NullOrEmpty);
            }

            _logger.LogInformation("Retrieving safety performance indicator with Code: {Code}", code);
            return await _dataService.GetSafetyPerformanceIndicatorByCodeAsync(code, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving safety performance indicator with Code: {Code}", code);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NotFound);
        }
    }

    /// <summary>
    /// Gets all safety performance indicators
    /// </summary>
    public async Task<Result<IEnumerable<SafetyPerformanceIndicator>>> GetAllSafetyPerformanceIndicatorsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all safety performance indicators");
            var result = await _dataService.GetAllSafetyPerformanceIndicatorsAsync(ct);
            
            if (result.IsSuccess)
            {
                return Result<IEnumerable<SafetyPerformanceIndicator>>.Success(result.Value.AsEnumerable());
            }
            else
            {
                return Result<IEnumerable<SafetyPerformanceIndicator>>.Failure<IEnumerable<SafetyPerformanceIndicator>>(result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving all safety performance indicators");
            return Result<IEnumerable<SafetyPerformanceIndicator>>.Failure<IEnumerable<SafetyPerformanceIndicator>>(DomainErrors.SPIError.NotFound);
        }
    }

    /// <summary>
    /// Updates an existing safety performance indicator
    /// </summary>
    public async Task<Result<SafetyPerformanceIndicator>> UpdateSafetyPerformanceIndicatorAsync(SafetyPerformanceIndicator spi, CancellationToken ct = default)
    {
        try
        {
            if (spi is null)
            {
                _logger.LogError("UpdateSafetyPerformanceIndicatorAsync received null SPI");
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NullOrEmpty);
            }

            _logger.LogInformation("Updating safety performance indicator with ID: {Id}", spi.Id);
            return await _dataService.UpdateSafetyPerformanceIndicatorAsync(spi, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating safety performance indicator with ID: {Id}", spi?.Id);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.UpdateFailed);
        }
    }

    /// <summary>
    /// Deletes a safety performance indicator
    /// </summary>
    public async Task<Result<bool>> DeleteSafetyPerformanceIndicatorAsync(SafetyPerformanceIndicatorID id, CancellationToken ct = default)
    {
        try
        {
            if (id is null)
            {
                _logger.LogError("DeleteSafetyPerformanceIndicatorAsync received null ID");
                return Result<bool>.Failure<bool>(DomainErrors.SPIError.NullOrEmpty);
            }

            _logger.LogInformation("Deleting safety performance indicator with ID: {Id}", id.Value);
            return await _dataService.DeleteSafetyPerformanceIndicatorAsync(id, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting safety performance indicator with ID: {Id}", id?.Value);
            return Result<bool>.Failure<bool>(DomainErrors.SPIError.DeleteFailed);
        }
    }

    /// <summary>
    /// Adds a data point to an SPI
    /// </summary>
    public async Task<Result<SafetyPerformanceIndicator>> AddSPIDataPointAsync(string spiCode, SPIDataPoint dataPoint, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(spiCode) || dataPoint is null)
            {
                _logger.LogError("AddSPIDataPointAsync received null or empty parameters");
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NullOrEmpty);
            }

            _logger.LogInformation("Adding data point to SPI: {SPICode}", spiCode);

            // Get the SPI first
            var spiResult = await GetSafetyPerformanceIndicatorByCodeAsync(spiCode, ct);
            if (spiResult.IsFailure)
            {
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(spiResult.Error);
            }

            var spi = spiResult.Value;

            // Add the data point (assuming the SPI entity has an AddDataPoint method)
            if (spi.DataPoints == null)
                spi.DataPoints = new List<SPIDataPoint>();

            spi.DataPoints.Add(dataPoint);

            // Update the SPI
            return await UpdateSafetyPerformanceIndicatorAsync(spi, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error adding data point to SPI: {SPICode}", spiCode);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.UpdateFailed);
        }
    }

    /// <summary>
    /// Updates an SPI data point
    /// </summary>
    public async Task<Result<SafetyPerformanceIndicator>> UpdateSPIDataPointAsync(SPIDataPoint dataPoint, CancellationToken ct = default)
    {
        try
        {
            if (dataPoint is null)
            {
                _logger.LogError("UpdateSPIDataPointAsync received null data point");
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NullOrEmpty);
            }

            _logger.LogInformation("Updating SPI data point: {Code}", dataPoint.Code);

            // Get the SPI that contains this data point
            var spiResult = await GetSafetyPerformanceIndicatorByCodeAsync(dataPoint.SPIId, ct);
            if (spiResult.IsFailure)
            {
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(spiResult.Error);
            }

            var spi = spiResult.Value;

            // Update the data point
            var existingDataPoint = spi.DataPoints?.FirstOrDefault(dp => dp.Code == dataPoint.Code);
            if (existingDataPoint != null)
            {
                var index = spi.DataPoints.IndexOf(existingDataPoint);
                spi.DataPoints[index] = dataPoint;
            }

            // Update the SPI
            return await UpdateSafetyPerformanceIndicatorAsync(spi, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating SPI data point: {Code}", dataPoint?.Code);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.UpdateFailed);
        }
    }

    /// <summary>
    /// Deletes an SPI data point
    /// </summary>
    public async Task<Result<SafetyPerformanceIndicator>> DeleteSPIDataPointAsync(string dataPointCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dataPointCode))
            {
                _logger.LogError("DeleteSPIDataPointAsync received null or empty data point code");
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NullOrEmpty);
            }

            _logger.LogInformation("Deleting SPI data point: {Code}", dataPointCode);

            // Get all SPIs and find the one containing the data point
            var allSPIsResult = await GetAllSafetyPerformanceIndicatorsAsync(ct);
            if (allSPIsResult.IsFailure)
            {
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(allSPIsResult.Error);
            }

            var targetSPI = allSPIsResult.Value?.FirstOrDefault(spi => 
                spi.DataPoints?.Any(dp => dp.Code == dataPointCode) == true);

            if (targetSPI == null)
            {
                _logger.LogWarning("Data point {Code} not found in any SPI", dataPointCode);
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NotFound);
            }

            // Remove the data point
            var dataPointToRemove = targetSPI.DataPoints.FirstOrDefault(dp => dp.Code == dataPointCode);
            if (dataPointToRemove != null)
            {
                targetSPI.DataPoints.Remove(dataPointToRemove);
            }

            // Update the SPI
            return await UpdateSafetyPerformanceIndicatorAsync(targetSPI, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting SPI data point: {Code}", dataPointCode);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.DeleteFailed);
        }
    }

    #endregion

    /// <summary>
    /// Updates SPI configuration with business validation
    /// </summary>
    public async Task<Result<SafetyPerformanceIndicator>> UpdateConfigurationAsync(
        string spiId, string name, string description, string indicatorType,
        string measurementUnit, string measurementFrequency, string calculationMethod,
        string dataSource, string updatedBy, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating SPI configuration for ID: {Id}", spiId);

            // Get existing SPI
            var existingResult = await _dataService.GetSafetyPerformanceIndicatorByCodeAsync(spiId, ct);
            if (existingResult.IsFailure)
            {
                return existingResult;
            }

            var spi = existingResult.Value;

            // Map string values to enum types
            var spiType = SPIType.GetAllValues().FirstOrDefault(t =>
                t.Value.Equals(indicatorType, StringComparison.OrdinalIgnoreCase)) ?? SPIType.IncidentRate;

            var frequency = SPIMeasurementFrequency.GetAllValues().FirstOrDefault(f =>
                f.Value.Equals(measurementFrequency, StringComparison.OrdinalIgnoreCase)) ?? SPIMeasurementFrequency.Monthly;

            // Update configuration using domain method
            var updateResult = spi.UpdateConfiguration(name, description, spiType, measurementUnit, frequency, updatedBy);
            if (updateResult.IsFailure)
            {
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(updateResult.Error);
            }

            // Set additional properties
            spi.CalculationMethod = calculationMethod;
            spi.DataSource = dataSource;

            // Save to database
            return await _dataService.UpdateSafetyPerformanceIndicatorAsync(spi, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating SPI configuration for ID: {Id}", spiId);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.UpdateFailed);
        }
    }

    /// <summary>
    /// Sets SPI targets with validation
    /// </summary>
    public async Task<Result<SafetyPerformanceIndicator>> SetTargetsAsync(
        string spiId, decimal? targetValue, decimal? acceptableRange,
        decimal? warningThreshold, decimal? criticalThreshold, string updatedBy, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Setting SPI targets for ID: {Id}", spiId);

            // Get existing SPI
            var existingResult = await _dataService.GetSafetyPerformanceIndicatorByCodeAsync(spiId, ct);
            if (existingResult.IsFailure)
            {
                return existingResult;
            }

            var spi = existingResult.Value;

            // Update targets using domain method
            var updateResult = spi.SetTargets(targetValue, acceptableRange, warningThreshold, criticalThreshold, updatedBy);
            if (updateResult.IsFailure)
            {
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(updateResult.Error);
            }

            // Save to database
            return await _dataService.UpdateSafetyPerformanceIndicatorAsync(spi, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting SPI targets for ID: {Id}", spiId);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.TargetUpdateFailed);
        }
    }

    /// <summary>
    /// Updates SPI status
    /// </summary>
    public async Task<Result<SafetyPerformanceIndicator>> UpdateStatusAsync(
        string spiId, string status, string updatedBy, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating SPI status for ID: {Id} to {Status}", spiId, status);

            // Get existing SPI
            var existingResult = await _dataService.GetSafetyPerformanceIndicatorByCodeAsync(spiId, ct);
            if (existingResult.IsFailure)
            {
                return existingResult;
            }

            var spi = existingResult.Value;

            // Map string to enum
            var spiStatus = SPIStatus.GetAllValues().FirstOrDefault(s =>
                s.Value.Equals(status, StringComparison.OrdinalIgnoreCase)) ?? SPIStatus.Active;

            // Update status using domain method
            var updateResult = spi.UpdateStatus(spiStatus, updatedBy);
            if (updateResult.IsFailure)
            {
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(updateResult.Error);
            }

            // Save to database
            return await _dataService.UpdateSafetyPerformanceIndicatorAsync(spi, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating SPI status for ID: {Id}", spiId);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.StatusUpdateFailed);
        }
    }

    /// <summary>
    /// Adds data point to SPI
    /// </summary>
    //public async Task<Result<SafetyPerformanceIndicator>> AddDataPointAsync(
    //    string spiId, decimal value, DateTime measurementDate, string dataSource,
    //    string enteredBy, string? notes = null, CancellationToken ct = default)
    //{
    //    try
    //    {
    //        _logger.LogInformation("Adding data point to SPI ID: {Id}, Value: {Value}", spiId, value);

    //        // Get existing SPI
    //        var existingResult = await _dataService.GetSafetyPerformanceIndicatorByCodeAsync(spiId, ct);
    //        if (existingResult.IsFailure)
    //        {
    //            return existingResult;
    //        }

    //        var spi = existingResult.Value;

    //        // Add data point using domain method
    //        var addResult = _dataService.AddSPIDataPointAsync(spi.Code,       //spi.AddDataPoint(value, measurementDate, dataSource, enteredBy);
    //        if (addResult.IsFailure)
    //        {
    //            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(addResult.Error);
    //        }

    //        // Set notes if provided
    //        if (!string.IsNullOrEmpty(notes) && spi.DataPoints.Any())
    //        {
    //            spi.DataPoints.Last().Notes = notes;
    //        }

    //        // Save to database
    //        return await _dataService.UpdateSafetyPerformanceIndicatorAsync(spi, ct);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error adding data point to SPI ID: {Id}", spiId);
    //        return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.DataPointAddFailed);
    //    }
    //}

    /// <summary>
    /// Schedules SPI review
    /// </summary>
    public async Task<Result<SafetyPerformanceIndicator>> ScheduleReviewAsync(
        string spiId, DateTime reviewDate, string scheduledBy, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Scheduling SPI review for ID: {Id}", spiId);

            // Get existing SPI
            var existingResult = await _dataService.GetSafetyPerformanceIndicatorByCodeAsync(spiId, ct);
            if (existingResult.IsFailure)
            {
                return existingResult;
            }

            var spi = existingResult.Value;

            // Schedule review using domain method
            var scheduleResult = spi.ScheduleReview(reviewDate, scheduledBy);
            if (scheduleResult.IsFailure)
            {
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(scheduleResult.Error);
            }

            // Save to database
            return await _dataService.UpdateSafetyPerformanceIndicatorAsync(spi, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling SPI review for ID: {Id}", spiId);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.ReviewScheduleFailed);
        }
    }

    /// <summary>
    /// Completes SPI review
    /// </summary>
    public async Task<Result<SafetyPerformanceIndicator>> CompleteReviewAsync(
        string spiId, string reviewNotes, DateTime? nextReviewDate, string reviewedBy, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Completing SPI review for ID: {Id}", spiId);

            // Get existing SPI
            var existingResult = await _dataService.GetSafetyPerformanceIndicatorByCodeAsync(spiId, ct);
            if (existingResult.IsFailure)
            {
                return existingResult;
            }

            var spi = existingResult.Value;

            // Complete review using domain method
            var reviewResult = spi.CompleteReview(reviewNotes, nextReviewDate, reviewedBy);
            if (reviewResult.IsFailure)
            {
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(reviewResult.Error);
            }

            // Save to database
            return await _dataService.UpdateSafetyPerformanceIndicatorAsync(spi, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing SPI review for ID: {Id}", spiId);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.ReviewCompletionFailed);
        }
    }

    /// <summary>
    /// Gets comprehensive dashboard data with analytics
    /// </summary>
    public async Task<Result<SPIDashboardData>> GetDashboardDataAsync(
        DateTime? startDate = null, DateTime? endDate = null,
        List<string>? spiIds = null, List<string>? departmentFilters = null,
        List<string>? typeFilters = null, bool includeTrends = true,
        bool includeAlerts = true, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting SPI dashboard data");

            // Get all SPIs
            var allSpisResult = await _dataService.GetAllSafetyPerformanceIndicatorsAsync(ct);
            if (allSpisResult.IsFailure)
            {
                return Result<SPIDashboardData>.Failure<SPIDashboardData>(allSpisResult.Error);
            }

            var spis = allSpisResult.Value.ToList();

            // Apply filters
            if (spiIds?.Any() == true)
            {
                spis = spis.Where(spi => spiIds.Contains(spi.Code)).ToList();
            }

            if (departmentFilters?.Any() == true)
            {
                spis = spis.Where(spi => departmentFilters.Contains(spi.ResponsibleDepartment)).ToList();
            }

            if (typeFilters?.Any() == true)
            {
                spis = spis.Where(spi => typeFilters.Contains(spi.IndicatorType.Value)).ToList();
            }

            var dashboardData = new SPIDashboardData
            {
                TotalSPIs = spis.Count,
                ActiveSPIs = spis.Count(spi => spi.Status == SPIStatus.Active),
                SPIsOverThreshold = spis.Count(spi => spi.IsOverThreshold()),
                SPIsRequiringReview = spis.Count(spi => spi.RequiresReview()),
                LastUpdateDate = DateTime.UtcNow
            };

            // Build SPI cards
            dashboardData.SPICards = spis.Select(spi => new SPIDashboardCard
            {
                SPIId = spi.Id.Value,
                Code = spi.Code,
                Name = spi.Name,
                Description = spi.Description,
                IndicatorType = spi.IndicatorType.Name,
                Status = spi.Status.Name,
                MeasurementUnit = spi.MeasurementUnit,
                MeasurementFrequency = spi.MeasurementFrequency.Name,
                CurrentValue = spi.GetCurrentValue(),
                TargetValue = spi.TargetValue,
                WarningThreshold = spi.WarningThreshold,
                CriticalThreshold = spi.CriticalThreshold,
                TrendDirection = spi.GetTrendDirection().Name,
                IsOverThreshold = spi.IsOverThreshold(),
                IsAtWarningLevel = spi.IsAtWarningLevel(),
                RequiresReview = spi.RequiresReview(),
                LastMeasurementDate = spi.DataPoints?.OrderByDescending(dp => dp.MeasurementDate).FirstOrDefault()?.MeasurementDate,
                NextReviewDate = spi.NextReviewDate,
                ResponsibleDepartment = spi.ResponsibleDepartment,
                DataOwner = spi.DataOwner
            }).ToList();

            // Build performance summary
            dashboardData.PerformanceSummary = new SPIPerformanceSummary
            {
                StartDate = startDate ?? DateTime.UtcNow.AddMonths(-12),
                EndDate = endDate ?? DateTime.UtcNow,
                TotalSPIs = spis.Count,
                SPIsMeetingTarget = spis.Count(spi => spi.GetCurrentValue() >= spi.TargetValue),
                SPIsAboveWarning = spis.Count(spi => spi.IsAtWarningLevel()),
                SPIsAboveCritical = spis.Count(spi => spi.IsOverThreshold()),
                OverallComplianceRate = spis.Count > 0 ? (decimal)spis.Count(spi => spi.GetCurrentValue() >= spi.TargetValue) / spis.Count * 100 : 0,
                SPIsByType = spis.GroupBy(spi => spi.IndicatorType.Name).ToDictionary(g => g.Key, g => g.Count()),
                SPIsByDepartment = spis.GroupBy(spi => spi.ResponsibleDepartment).ToDictionary(g => g.Key, g => g.Count()),
                SPIsImproving = spis.Count(spi => spi.GetTrendDirection() == SPITrendDirection.Improving),
                SPIsStable = spis.Count(spi => spi.GetTrendDirection() == SPITrendDirection.Stable),
                SPIsDeclining = spis.Count(spi => spi.GetTrendDirection() == SPITrendDirection.Declining)
            };

            // Generate alerts if requested
            if (includeAlerts)
            {
                dashboardData.ActiveAlerts = await GenerateAlertsAsync(spis, ct);
            }

            // Generate trend analysis if requested
            if (includeTrends)
            {
                dashboardData.TrendAnalysis = await GenerateTrendAnalysisAsync(spis, ct);
            }

            return Result<SPIDashboardData>.Success(dashboardData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SPI dashboard data");
            return Result<SPIDashboardData>.Failure<SPIDashboardData>(DomainErrors.SPIError.NotFound);
        }
    }

    /// <summary>
    /// Gets SPI trend analysis
    /// </summary>
    public async Task<Result<List<SPITrendAnalysis>>> GetTrendAnalysisAsync(
        List<string>? spiIds = null, int periods = 12, DateTime? endDate = null, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting SPI trend analysis");

            // Get SPIs
            var allSpisResult = await _dataService.GetAllSafetyPerformanceIndicatorsAsync(ct);
            if (allSpisResult.IsFailure)
            {
                return Result<List<SPITrendAnalysis>>.Failure<List<SPITrendAnalysis>>(allSpisResult.Error);
            }

            var spis = allSpisResult.Value.ToList();

            if (spiIds?.Any() == true)
            {
                spis = spis.Where(spi => spiIds.Contains(spi.Code)).ToList();
            }

            return Result<List<SPITrendAnalysis>>.Success(await GenerateTrendAnalysisAsync(spis, ct));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SPI trend analysis");
            return Result<List<SPITrendAnalysis>>.Failure<List<SPITrendAnalysis>>(DomainErrors.SPIError.NotFound);
        }
    }

    /// <summary>
    /// Gets SPI performance summary
    /// </summary>
    public async Task<Result<SPIPerformanceSummary>> GetPerformanceSummaryAsync(
        DateTime startDate, DateTime endDate, List<string>? departmentFilters = null,
        List<string>? typeFilters = null, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting SPI performance summary");

            var dashboardResult = await GetDashboardDataAsync(startDate, endDate, null, departmentFilters, typeFilters, false, false, ct);
            if (dashboardResult.IsFailure)
            {
                return Result<SPIPerformanceSummary>.Failure<SPIPerformanceSummary>(dashboardResult.Error);
            }

            return Result<SPIPerformanceSummary>.Success(dashboardResult.Value.PerformanceSummary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SPI performance summary");
            return Result<SPIPerformanceSummary>.Failure<SPIPerformanceSummary>(DomainErrors.SPIError.NotFound);
        }
    }

    /// <summary>
    /// Gets SPI alerts
    /// </summary>
    public async Task<Result<List<SPIAlert>>> GetAlertsAsync(
        List<string>? spiIds = null, bool activeAlertsOnly = true,
        DateTime? startDate = null, DateTime? endDate = null, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting SPI alerts");

            var allSpisResult = await _dataService.GetAllSafetyPerformanceIndicatorsAsync(ct);
            if (allSpisResult.IsFailure)
            {
                return Result<List<SPIAlert>>.Failure<List<SPIAlert>>(allSpisResult.Error);
            }

            var spis = allSpisResult.Value.ToList();

            if (spiIds?.Any() == true)
            {
                spis = spis.Where(spi => spiIds.Contains(spi.Code)).ToList();
            }

            if (activeAlertsOnly)
            {
                spis = spis.Where(spi => spi.Status == SPIStatus.Active).ToList();
            }

            return Result<List<SPIAlert>>.Success(await GenerateAlertsAsync(spis, ct));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SPI alerts");
            return Result<List<SPIAlert>>.Failure<List<SPIAlert>(DomainErrors.SPIError.NotFound);
        }
    }

    /// <summary>
    /// Gets SPI compliance status
    /// </summary>
    public async Task<Result<List<SPIComplianceStatus>>> GetComplianceStatusAsync(
        List<string>? spiIds = null, DateTime? asOfDate = null, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting SPI compliance status");

            var allSpisResult = await _dataService.GetAllSafetyPerformanceIndicatorsAsync(ct);
            if (allSpisResult.IsFailure)
            {
                return Result<List<SPIComplianceStatus>>.Failure<List<SPIComplianceStatus>>(allSpisResult.Error);
            }

            var spis = allSpisResult.Value.ToList();

            if (spiIds?.Any() == true)
            {
                spis = spis.Where(spi => spiIds.Contains(spi.Code)).ToList();
            }

            var complianceStatuses = spis.Select(spi =>
            {
                var currentValue = spi.GetCurrentValue();
                var lastMeasurement = spi.DataPoints?.OrderByDescending(dp => dp.MeasurementDate).FirstOrDefault();

                return new SPIComplianceStatus
                {
                    SPIId = spi.Id.Value,
                    SPIName = spi.Name,
                    InCompliance = currentValue >= spi.TargetValue,
                    ComplianceStatus = GetComplianceStatusText(spi),
                    CurrentValue = currentValue,
                    ComplianceThreshold = spi.TargetValue,
                    LastMeasurementDate = lastMeasurement?.MeasurementDate,
                    DaysWithoutData = lastMeasurement != null ? (int)(DateTime.UtcNow - lastMeasurement.MeasurementDate).TotalDays : 9999
                };
            }).ToList();

            return Result<List<SPIComplianceStatus>>.Success(complianceStatuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SPI compliance status");
            return Result<List<SPIComplianceStatus>>.Failure<List<SPIComplianceStatus>(DomainErrors.SPIError.NotFound);
        }
    }

    /// <summary>
    /// Gets SPI review schedule
    /// </summary>
    public async Task<Result<List<SPIReviewItem>>> GetReviewScheduleAsync(
        DateTime? startDate = null, DateTime? endDate = null, bool overdueOnly = false, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Getting SPI review schedule");

            var allSpisResult = await _dataService.GetAllSafetyPerformanceIndicatorsAsync(ct);
            if (allSpisResult.IsFailure)
            {
                return Result<List<SPIReviewItem>>.Failure<List<SPIReviewItem>>(allSpisResult.Error);
            }

            var spis = allSpisResult.Value.ToList();

            var reviewItems = spis
                .Where(spi => spi.NextReviewDate.HasValue)
                .Select(spi =>
                {
                    var isOverdue = spi.NextReviewDate.HasValue && spi.NextReviewDate.Value < DateTime.UtcNow;
                    var daysOverdue = isOverdue ? (int)(DateTime.UtcNow - spi.NextReviewDate.Value).TotalDays : 0;

                    return new SPIReviewItem
                    {
                        SPIId = spi.Id.Value,
                        SPIName = spi.Name,
                        ResponsibleDepartment = spi.ResponsibleDepartment,
                        ReviewAuthority = spi.ReviewAuthority,
                        NextReviewDate = spi.NextReviewDate,
                        LastReviewDate = spi.LastReviewDate,
                        IsOverdue = isOverdue,
                        DaysOverdue = daysOverdue,
                        Priority = GetReviewPriority(daysOverdue, spi.IsOverThreshold())
                    };
                });

            // Apply filters
            if (overdueOnly)
            {
                reviewItems = reviewItems.Where(ri => ri.IsOverdue);
            }

            if (startDate.HasValue)
            {
                reviewItems = reviewItems.Where(ri => ri.NextReviewDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                reviewItems = reviewItems.Where(ri => ri.NextReviewDate <= endDate.Value);
            }

            return Result<List<SPIReviewItem>>.Success(reviewItems.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SPI review schedule");
            return Result<List<SPIReviewItem>>.Failure<List<SPIReviewItem>>(DomainErrors.SPIError.NotFound);
        }
    }

    #region Private Helper Methods

    private async Task<List<SPIAlert>> GenerateAlertsAsync(List<SafetyPerformanceIndicator> spis, CancellationToken ct = default)
    {
        var alerts = new List<SPIAlert>();

        foreach (var spi in spis.Where(s => s.AlertsEnabled))
        {
            if (spi.IsOverThreshold())
            {
                alerts.Add(new SPIAlert
                {
                    SPIId = spi.Id.Value,
                    SPIName = spi.Name,
                    AlertType = "Critical",
                    CurrentValue = spi.GetCurrentValue() ?? 0,
                    ThresholdValue = spi.CriticalThreshold,
                    AlertMessage = $"{spi.Name} has exceeded the critical threshold",
                    AlertDate = DateTime.UtcNow,
                    TrendDirection = spi.GetTrendDirection().Name
                });
            }
            else if (spi.IsAtWarningLevel())
            {
                alerts.Add(new SPIAlert
                {
                    SPIId = spi.Id.Value,
                    SPIName = spi.Name,
                    AlertType = "Warning",
                    CurrentValue = spi.GetCurrentValue() ?? 0,
                    ThresholdValue = spi.WarningThreshold,
                    AlertMessage = $"{spi.Name} has reached the warning threshold",
                    AlertDate = DateTime.UtcNow,
                    TrendDirection = spi.GetTrendDirection().Name
                });
            }
        }

        return alerts;
    }

    private async Task<List<SPITrendAnalysis>> GenerateTrendAnalysisAsync(List<SafetyPerformanceIndicator> spis, CancellationToken ct = default)
    {
        var trendAnalysis = new List<SPITrendAnalysis>();

        foreach (var spi in spis)
        {
            var analysis = new SPITrendAnalysis
            {
                SPIId = spi.Id.Value,
                SPIName = spi.Name,
                OverallTrend = spi.GetTrendDirection().Name,
                DataPoints = spi.DataPoints?
                    .OrderByDescending(dp => dp.MeasurementDate)
                    .Take(12)
                    .Select(dp => new SPIDataPointSummary
                    {
                        Period = dp.Period,
                        MeasurementDate = dp.MeasurementDate,
                        Value = dp.Value,
                        Target = spi.TargetValue,
                        IsAboveWarning = spi.WarningThreshold.HasValue && dp.Value >= spi.WarningThreshold.Value,
                        IsAboveCritical = spi.CriticalThreshold.HasValue && dp.Value >= spi.CriticalThreshold.Value
                    }).ToList() ?? new List<SPIDataPointSummary>()
            };

            trendAnalysis.Add(analysis);
        }

        return trendAnalysis;
    }

    private string GetComplianceStatusText(SafetyPerformanceIndicator spi)
    {
        if (spi.IsOverThreshold()) return "Critical";
        if (spi.IsAtWarningLevel()) return "Warning";

        var currentValue = spi.GetCurrentValue();
        if (!currentValue.HasValue) return "No Data";
        if (currentValue >= spi.TargetValue) return "Compliant";

        return "Below Target";
    }

    private string GetReviewPriority(int daysOverdue, bool isOverThreshold)
    {
        if (isOverThreshold || daysOverdue > 30) return "High";
        if (daysOverdue > 0) return "Medium";
        return "Low";
    }

    #endregion
}

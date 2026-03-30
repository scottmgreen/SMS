//-----------------------------------------------------------------------
// <copyright file="SafetyPerformanceIndicatorDataService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Safety performance indicator data service providing business operations for SMS domain entities.
//                  Infrastructure layer service implementing data access patterns
//                  through repositories while maintaining clean architecture.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Infrastructure.Services;

/// <summary>
/// Safety Performance Indicator Data Service providing business operations for SafetyPerformanceIndicator entities
/// </summary>
public class SafetyPerformanceIndicatorDataService : BaseDataService<SafetyPerformanceIndicatorDataService>
{
    private readonly ILogger<SafetyPerformanceIndicatorDataService> _logger;
    private readonly string _logheader;
    private readonly SafetyPerformanceIndicatorRepository _repo;

    public SafetyPerformanceIndicatorDataService(
        ILogger<SafetyPerformanceIndicatorDataService> logger,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        SafetyPerformanceIndicatorRepository repo)
        : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _repo = repo;

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} {repo.GetType().Name}");
    }

    /// <summary>
    /// Creates a new Safety Performance Indicator
    /// </summary>
    public Task<Result<SafetyPerformanceIndicator>> CreateSafetyPerformanceIndicatorAsync(SafetyPerformanceIndicator spi, CancellationToken ct = default)
    {
        return _repo.CreateSafetyPerformanceIndicatorAsync(spi, ct);
    }

    /// <summary>
    /// Updates an existing Safety Performance Indicator
    /// </summary>
    public Task<Result<SafetyPerformanceIndicator>> UpdateSafetyPerformanceIndicatorAsync(SafetyPerformanceIndicator spi, CancellationToken ct = default)
    {
        return _repo.UpdateSafetyPerformanceIndicatorAsync(spi, ct);
    }

    /// <summary>
    /// Deletes a Safety Performance Indicator
    /// </summary>
    public Task<Result<bool>> DeleteSafetyPerformanceIndicatorAsync(SafetyPerformanceIndicatorID spiId, CancellationToken ct = default)
    {
        return _repo.DeleteSafetyPerformanceIndicatorAsync(spiId, ct);
    }

    /// <summary>
    /// Gets all Safety Performance Indicators
    /// </summary>
    public Task<Result<List<SafetyPerformanceIndicator>>> GetAllSafetyPerformanceIndicatorsAsync(CancellationToken ct = default)
    {
        return _repo.GetAllSafetyPerformanceIndicatorsAsync(ct);
    }

    /// <summary>
    /// Gets a Safety Performance Indicator by ID
    /// </summary>
    public Task<Result<SafetyPerformanceIndicator>> GetSafetyPerformanceIndicatorByIdAsync(SafetyPerformanceIndicatorID spiId, CancellationToken ct = default)
    {
        return _repo.GetSafetyPerformanceIndicatorByIdAsync(spiId, ct);
    }

    /// <summary>
    /// Gets a Safety Performance Indicator by Code
    /// </summary>
    public Task<Result<SafetyPerformanceIndicator>> GetSafetyPerformanceIndicatorByCodeAsync(string code, CancellationToken ct = default)
    {
        return _repo.GetSafetyPerformanceIndicatorByCodeAsync(code, ct);
    }

    /// <summary>
    /// Gets Safety Performance Indicators by type
    /// </summary>
    public Task<Result<List<SafetyPerformanceIndicator>>> GetSafetyPerformanceIndicatorsByTypeAsync(string indicatorType, CancellationToken ct = default)
    {
        return _repo.GetSafetyPerformanceIndicatorsByTypeAsync(indicatorType, ct);
    }

    /// <summary>
    /// Gets Safety Performance Indicators by department
    /// </summary>
    public Task<Result<List<SafetyPerformanceIndicator>>> GetSafetyPerformanceIndicatorsByDepartmentAsync(string department, CancellationToken ct = default)
    {
        return _repo.GetSafetyPerformanceIndicatorsByDepartmentAsync(department, ct);
    }

    /// <summary>
    /// Adds a new data point to an existing Safety Performance Indicator
    /// </summary>
    public Task<Result<SPIDataPoint>> AddSPIDataPointAsync(string spiId, SPIDataPoint dataPoint, CancellationToken ct = default)
    {
        return _repo.AddSPIDataPointAsync(spiId, dataPoint, ct);
    }

    /// <summary>
    /// Updates an existing SPI data point
    /// </summary>
    public Task<Result<SPIDataPoint>> UpdateSPIDataPointAsync(SPIDataPoint dataPoint, CancellationToken ct = default)
    {
        return _repo.UpdateSPIDataPointAsync(dataPoint, ct);
    }

    /// <summary>
    /// Deletes an SPI data point
    /// </summary>
    public Task<Result<bool>> DeleteSPIDataPointAsync(string dataPointId, CancellationToken ct = default)
    {
        return _repo.DeleteSPIDataPointAsync(dataPointId, ct);
    }
}

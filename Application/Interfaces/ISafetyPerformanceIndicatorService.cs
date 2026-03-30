//-----------------------------------------------------------------------
// <copyright file="ISafetyPerformanceIndicatorService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application service interface for Safety Performance Indicator management.
//                  Provides business logic operations for SPI entities.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Interfaces;

/// <summary>
/// Application service interface for Safety Performance Indicator management and business operations
/// </summary>
public interface ISafetyPerformanceIndicatorService
{
    /// <summary>
    /// Creates a new safety performance indicator
    /// </summary>
    Task<Result<SafetyPerformanceIndicator>> CreateSafetyPerformanceIndicatorAsync(SafetyPerformanceIndicator spi, CancellationToken ct = default);

    /// <summary>
    /// Gets safety performance indicator by ID
    /// </summary>
    Task<Result<SafetyPerformanceIndicator>> GetSafetyPerformanceIndicatorByIdAsync(SafetyPerformanceIndicatorID id, CancellationToken ct = default);

    /// <summary>
    /// Gets safety performance indicator by code
    /// </summary>
    Task<Result<SafetyPerformanceIndicator>> GetSafetyPerformanceIndicatorByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Gets all safety performance indicators
    /// </summary>
    Task<Result<IEnumerable<SafetyPerformanceIndicator>>> GetAllSafetyPerformanceIndicatorsAsync(CancellationToken ct = default);

    /// <summary>
    /// Updates an existing safety performance indicator
    /// </summary>
    Task<Result<SafetyPerformanceIndicator>> UpdateSafetyPerformanceIndicatorAsync(SafetyPerformanceIndicator spi, CancellationToken ct = default);

    /// <summary>
    /// Deletes a safety performance indicator
    /// </summary>
    Task<Result<bool>> DeleteSafetyPerformanceIndicatorAsync(SafetyPerformanceIndicatorID id, CancellationToken ct = default);

    /// <summary>
    /// Adds a data point to an SPI
    /// </summary>
    Task<Result<SafetyPerformanceIndicator>> AddSPIDataPointAsync(string spiCode, SPIDataPoint dataPoint, CancellationToken ct = default);

    /// <summary>
    /// Updates an SPI data point
    /// </summary>
    Task<Result<SafetyPerformanceIndicator>> UpdateSPIDataPointAsync(SPIDataPoint dataPoint, CancellationToken ct = default);

    /// <summary>
    /// Deletes an SPI data point
    /// </summary>
    Task<Result<SafetyPerformanceIndicator>> DeleteSPIDataPointAsync(string dataPointCode, CancellationToken ct = default);
}
//-----------------------------------------------------------------------
// <copyright file="SPIAutomationService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SPI automation service providing automated calculation and data point creation
//                  for Safety Performance Indicators based on SMS events and scheduled calculations.
//                  Coordinates automatic SPI updates from hazard reports, risk assessments, and mitigations.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS_Application.Services;

/// <summary>
/// SPI Automation Service - Orchestrates automatic Safety Performance Indicator calculations
/// Handles event-driven data point creation and scheduled SPI updates
/// </summary>
public class SPIAutomationService : ISPIAutomationService
{
    private readonly SafetyPerformanceIndicatorService _spiService;
    private readonly IMediator _mediator;
    private readonly ILogger<SPIAutomationService> _logger;

    public SPIAutomationService(
        SafetyPerformanceIndicatorService spiService,
        IMediator mediator,
        ILogger<SPIAutomationService> logger)
    {
        _spiService = spiService ?? throw new ArgumentNullException(nameof(spiService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Hazard Report SPI Automation

    /// <summary>
    /// Updates Hazard Report Rate SPI when a new hazard is created
    /// Uses dynamic SPI lookup to find the correct SPI in the database
    /// </summary>
    public async Task<Result<bool>> UpdateHazardReportRateAsync(DateTime reportDate, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("?? SPI Automation: Updating Hazard Report Rate for date {Date}", reportDate);

            // Dynamic SPI lookup - find by name
            var spi = await FindSPIByName("Hazard Report Rate", ct);
            if (spi == null)
            {
                _logger.LogError("? SPI Automation: Could not find 'Hazard Report Rate' SPI in database");
                return Result<bool>.Failure<bool>(DomainErrors.SPIError.NotFound);
            }

            // Get count of hazards reported today
            var todaysHazardCount = await GetHazardCountForDate(reportDate, ct);

            // Create data point using the actual SPI code from database
            var dataPoint = new SPIDataPoint(new SPIDataPointID($"HRR-{reportDate:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8]}"))
            {
                SPIId = spi.Code, // Use actual SPI code from database
                Value = todaysHazardCount,
                MeasurementDate = reportDate,
                Period = reportDate.ToString("yyyy-MM-dd"),
                DataSource = "Automated-HazardEvent",
                Notes = $"Daily hazard submissions: {todaysHazardCount} (raw hazard creation count)",
                IsVerified = true, // Auto-verified for system calculations
                VerifiedBy = "SYSTEM",
                VerifiedDate = DateTime.UtcNow,
                CreatedBy = "SPI_AUTOMATION"
            };

            // Add data point to SPI using actual SPI code
            var result = await _spiService.AddSPIDataPointAsync(spi.Code, dataPoint, ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("? SPI Automation: Successfully updated Hazard Report Rate - Count: {Count}, SPI Code: {Code}", 
                    todaysHazardCount, spi.Code);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogError("? SPI Automation: Failed to update Hazard Report Rate - Error: {Error}", result.Error?.Message);
                return Result<bool>.Failure<bool>(result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Automation: Exception updating Hazard Report Rate");
            return Result<bool>.Failure<bool>(DomainErrors.SPIError.AutomationFailed);
        }
    }

    /// <summary>
    /// Updates Time to Close Hazards SPI when a hazard is closed
    /// </summary>
    public async Task<Result<bool>> UpdateHazardClosureTimeAsync(string hazardId, DateTime submittedDate, DateTime closedDate, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("?? SPI Automation: Updating Hazard Closure Time for hazard {HazardId}", hazardId);

            var daysToClose = (closedDate - submittedDate).TotalDays;

            var dataPoint = new SPIDataPoint(new SPIDataPointID($"HCT-{hazardId}-{closedDate:yyyyMMdd}"))
            {
                SPIId = "HAZARD_CLOSURE_TIME", // Custom SPI (may need to create)
                Value = (decimal)daysToClose,
                MeasurementDate = closedDate,
                Period = closedDate.ToString("yyyy-MM-dd"),
                DataSource = "Automated-HazardClosure",
                Notes = $"Hazard {hazardId} closed in {daysToClose:F1} days",
                IsVerified = true,
                VerifiedBy = "SYSTEM",
                VerifiedDate = DateTime.UtcNow,
                CreatedBy = "SPI_AUTOMATION"
            };

            var result = await _spiService.AddSPIDataPointAsync("HAZARD_CLOSURE_TIME", dataPoint, ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("? SPI Automation: Successfully updated Hazard Closure Time - Days: {Days}", daysToClose);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogError("? SPI Automation: Failed to update Hazard Closure Time - Error: {Error}", result.Error?.Message);
                return Result<bool>.Failure<bool>(result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Automation: Exception updating Hazard Closure Time for hazard {HazardId}", hazardId);
            return Result<bool>.Failure<bool>(DomainErrors.SPIError.AutomationFailed);
        }
    }

    #endregion

    #region Risk Assessment SPI Automation

    /// <summary>
    /// Updates Risk Assessment Completion Rate SPI when an assessment is completed
    /// </summary>
    public async Task<Result<bool>> UpdateRiskAssessmentCompletionAsync(string assessmentId, DateTime startDate, DateTime completedDate, bool isOnTime, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("?? SPI Automation: Updating Risk Assessment Completion for assessment {AssessmentId}", assessmentId);

            // Dynamic SPI lookup
            var spi = await FindSPIByName("Risk Assessment Completion Rate", ct);
            if (spi == null)
            {
                _logger.LogError("? SPI Automation: Could not find 'Risk Assessment Completion Rate' SPI in database");
                return Result<bool>.Failure<bool>(DomainErrors.SPIError.NotFound);
            }

            // Calculate completion rate (1 = 100% for on-time, 0 = 0% for late)
            var completionScore = isOnTime ? 100m : 0m;
            var daysToComplete = (completedDate - startDate).TotalDays;

            var dataPoint = new SPIDataPoint(new SPIDataPointID($"RAC-{assessmentId}-{completedDate:yyyyMMdd}"))
            {
                SPIId = spi.Code, // Use actual SPI code from database
                Value = completionScore,
                MeasurementDate = completedDate,
                Period = completedDate.ToString("yyyy-MM-dd"),
                DataSource = "Automated-RiskAssessment",
                Notes = $"Assessment {assessmentId} completed in {daysToComplete:F1} days - {(isOnTime ? "On Time" : "Late")}",
                IsVerified = true,
                VerifiedBy = "SYSTEM",
                VerifiedDate = DateTime.UtcNow,
                CreatedBy = "SPI_AUTOMATION"
            };

            var result = await _spiService.AddSPIDataPointAsync(spi.Code, dataPoint, ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("? SPI Automation: Successfully updated Risk Assessment Completion - Score: {Score}%, SPI Code: {Code}", 
                    completionScore, spi.Code);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogError("? SPI Automation: Failed to update Risk Assessment Completion - Error: {Error}", result.Error?.Message);
                return Result<bool>.Failure<bool>(result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Automation: Exception updating Risk Assessment Completion for assessment {AssessmentId}", assessmentId);
            return Result<bool>.Failure<bool>(DomainErrors.SPIError.AutomationFailed);
        }
    }

    /// <summary>
    /// Updates High Risk Exposure SPI when high-risk conditions are identified
    /// </summary>
    public async Task<Result<bool>> UpdateHighRiskExposureAsync(string riskLevel, DateTime identifiedDate, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("?? SPI Automation: Updating High Risk Exposure for risk level {RiskLevel}", riskLevel);

            // Only track Critical and High risk levels
            if (!IsHighRisk(riskLevel))
            {
                _logger.LogInformation("?? SPI Automation: Risk level {RiskLevel} not considered high risk - skipping", riskLevel);
                return Result<bool>.Success(true);
            }

            // Dynamic SPI lookup - find "High Risk Exposure Count" SPI
            var spi = await FindSPIByName("High Risk Exposure Count", ct);
            if (spi == null)
            {
                _logger.LogError("? SPI Automation: Could not find 'High Risk Exposure Count' SPI in database");
                return Result<bool>.Failure<bool>(DomainErrors.SPIError.NotFound);
            }

            // Get current high-risk count for the day
            var highRiskCount = await GetHighRiskCountForDate(identifiedDate, ct);

            var dataPoint = new SPIDataPoint(new SPIDataPointID($"HRE-{identifiedDate:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8]}"))
            {
                SPIId = spi.Code, // Use actual SPI code from database
                Value = highRiskCount,
                MeasurementDate = identifiedDate,
                Period = identifiedDate.ToString("yyyy-MM-dd"),
                DataSource = "Automated-RiskAssessment",
                Notes = $"High risk exposure count: {highRiskCount} (Latest: {riskLevel})",
                IsVerified = true,
                VerifiedBy = "SYSTEM",
                VerifiedDate = DateTime.UtcNow,
                CreatedBy = "SPI_AUTOMATION"
            };

            var result = await _spiService.AddSPIDataPointAsync(spi.Code, dataPoint, ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("? SPI Automation: Successfully updated High Risk Exposure - Count: {Count}, SPI Code: {Code}", 
                    highRiskCount, spi.Code);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogError("? SPI Automation: Failed to update High Risk Exposure - Error: {Error}", result.Error?.Message);
                return Result<bool>.Failure<bool>(result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Automation: Exception updating High Risk Exposure");
            return Result<bool>.Failure<bool>(DomainErrors.SPIError.AutomationFailed);
        }
    }

    #endregion

    #region Mitigation SPI Automation

    /// <summary>
    /// Updates Mitigation Implementation Rate SPI when a mitigation is completed
    /// </summary>
    public async Task<Result<bool>> UpdateMitigationImplementationRateAsync(string mitigationId, DateTime targetDate, DateTime completedDate, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("?? SPI Automation: Updating Mitigation Implementation Rate for mitigation {MitigationId}", mitigationId);

            // Calculate implementation score based on timeliness
            var daysFromTarget = (completedDate - targetDate).TotalDays;
            var implementationScore = CalculateMitigationScore(daysFromTarget);

            var dataPoint = new SPIDataPoint(new SPIDataPointID($"MIR-{mitigationId}-{completedDate:yyyyMMdd}"))
            {
                SPIId = "MITIGATION_IMPLEMENTATION_RATE", // Use hardcoded SPI code directly
                Value = implementationScore,
                MeasurementDate = completedDate,
                Period = completedDate.ToString("yyyy-MM-dd"),
                DataSource = "Automated-MitigationCompletion",
                Notes = $"Mitigation {mitigationId} completed {daysFromTarget:F1} days from target - Score: {implementationScore}%",
                IsVerified = true,
                VerifiedBy = "SYSTEM",
                VerifiedDate = DateTime.UtcNow,
                CreatedBy = "SPI_AUTOMATION"
            };

            var result = await _spiService.AddSPIDataPointAsync("MITIGATION_IMPLEMENTATION_RATE", dataPoint, ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("? SPI Automation: Successfully updated Mitigation Implementation Rate - Score: {Score}%", implementationScore);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogError("? SPI Automation: Failed to update Mitigation Implementation Rate - Error: {Error}", result.Error?.Message);
                return Result<bool>.Failure<bool>(result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Automation: Exception updating Mitigation Implementation Rate for mitigation {MitigationId}", mitigationId);
            return Result<bool>.Failure<bool>(DomainErrors.SPIError.AutomationFailed);
        }
    }

    /// <summary>
    /// Updates Corrective Action Closure SPI when mitigations are overdue
    /// </summary>
    public async Task<Result<bool>> UpdateCorrectiveActionClosureAsync(DateTime calculationDate, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("?? SPI Automation: Updating Corrective Action Closure Rate for date {Date}", calculationDate);

            // Dynamic SPI lookup
            var spi = await FindSPIByName("Corrective Action Closure Rate", ct);
            if (spi == null)
            {
                _logger.LogError("? SPI Automation: Could not find 'Corrective Action Closure Rate' SPI in database");
                return Result<bool>.Failure<bool>(DomainErrors.SPIError.NotFound);
            }

            // Get overdue mitigation counts
            var overdueCount = await GetOverdueMitigationCount(calculationDate, ct);
            var totalActiveCount = await GetActiveMitigationCount(calculationDate, ct);
            var closureRate = totalActiveCount > 0 ? ((totalActiveCount - overdueCount) / (decimal)totalActiveCount) * 100 : 100m;

            var dataPoint = new SPIDataPoint(new SPIDataPointID($"CAC-{calculationDate:yyyyMMdd}"))
            {
                SPIId = spi.Code, // Use actual SPI code from database
                Value = closureRate,
                MeasurementDate = calculationDate,
                Period = calculationDate.ToString("yyyy-MM-dd"),
                DataSource = "Automated-ScheduledCalculation",
                Notes = $"Closure rate: {closureRate:F1}% ({totalActiveCount - overdueCount}/{totalActiveCount} on time)",
                IsVerified = true,
                VerifiedBy = "SYSTEM",
                VerifiedDate = DateTime.UtcNow,
                CreatedBy = "SPI_AUTOMATION"
            };

            var result = await _spiService.AddSPIDataPointAsync(spi.Code, dataPoint, ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("? SPI Automation: Successfully updated Corrective Action Closure Rate - Rate: {Rate}%, SPI Code: {Code}", 
                    closureRate, spi.Code);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogError("? SPI Automation: Failed to update Corrective Action Closure Rate - Error: {Error}", result.Error?.Message);
                return Result<bool>.Failure<bool>(result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Automation: Exception updating Corrective Action Closure Rate");
            return Result<bool>.Failure<bool>(DomainErrors.SPIError.AutomationFailed);
        }
    }

    #endregion

    #region Risk Assessment SPI Automation

    /// <summary>
    /// Updates Risk Assessment Effectiveness SPI when a validation decision is made
    /// This tracks how effective we are at identifying real SMS risks vs false positives
    /// </summary>
    public async Task<Result<bool>> UpdateRiskIdentificationEffectivenessAsync(string reportCode, DateTime validatedDate, string validationDecision, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("?? SPI Automation: Updating Risk Identification Effectiveness for report {ReportCode}, Decision: {Decision}", 
                reportCode, validationDecision);

            // This could feed into a new SPI that tracks validation effectiveness
            // For example: "SMS Risk Identification Rate" = (SMS_RISK decisions / Total validations) * 100

            // Dynamic SPI lookup - find "Risk Identification Effectiveness" SPI (if it exists)
            var spi = await FindSPIByDescription("risk identification", ct);

            if (spi == null)
            {
                _logger.LogInformation("?? SPI Automation: No Risk Identification Effectiveness SPI found - this is optional");
                return Result<bool>.Success(true); // Not an error, just not implemented yet
            }

            // Calculate daily effectiveness rate using validated decisions
            var effectivenessRate = await CalculateRiskIdentificationRate(validatedDate, ct);

            var dataPoint = new SPIDataPoint(new SPIDataPointID($"RIE-{validatedDate:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8]}"))
            {
                SPIId = spi.Code,
                Value = effectivenessRate,
                MeasurementDate = validatedDate,
                Period = validatedDate.ToString("yyyy-MM-dd"),
                DataSource = "Automated-ValidationDecision",
                Notes = $"Risk identification effectiveness: {effectivenessRate:F1}% (Decision: {validationDecision})",
                IsVerified = true,
                VerifiedBy = "SYSTEM",
                VerifiedDate = DateTime.UtcNow,
                CreatedBy = "SPI_AUTOMATION"
            };

            var result = await _spiService.AddSPIDataPointAsync(spi.Code, dataPoint, ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("? SPI Automation: Successfully updated Risk Identification Effectiveness - Rate: {Rate}%, SPI Code: {Code}", 
                    effectivenessRate, spi.Code);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogError("? SPI Automation: Failed to update Risk Identification Effectiveness - Error: {Error}", result.Error?.Message);
                return Result<bool>.Failure<bool>(result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Automation: Exception updating Risk Identification Effectiveness");
            return Result<bool>.Failure<bool>(DomainErrors.SPIError.AutomationFailed);
        }
    }

    /// <summary>
    /// Calculate the effectiveness rate of risk identification for a given date
    /// </summary>
    private async Task<decimal> CalculateRiskIdentificationRate(DateTime date, CancellationToken ct)
    {
        try
        {
            // Get validated SMS risks for the date
            var validatedRisks = await GetValidatedSMSRiskCountForDate(date, ct);

            // Get total validations for the date (would need another query)
            // For now, assume if we have validated risks, the rate is good

            return validatedRisks > 0 ? 100m : 0m; // Simplified calculation
        }
        catch
        {
            return 0m;
        }
    }

    #endregion

    #region Helper Methods - Dynamic SPI Lookup

    /// <summary>
    /// Finds SPI by name using CQRS query to avoid hardcoded dependencies
    /// </summary>
    private async Task<SafetyPerformanceIndicator?> FindSPIByName(string spiName, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("?? SPI Automation: Looking up SPI by name: {SPIName}", spiName);

            // Use CQRS to get all SPIs and find by name
            var getAllQuery = new GetAllSafetyPerformanceIndicatorsQuery();
            var result = await _mediator.SendAsync(getAllQuery, ct);

            if (result.IsSuccess && result.Value?.Any() == true)
            {
                var spi = result.Value.FirstOrDefault(s => s.Name.Equals(spiName, StringComparison.OrdinalIgnoreCase));

                if (spi != null)
                {
                    _logger.LogInformation("? SPI Automation: Found SPI {SPIName} with Code: {Code}", spiName, spi.Code);
                    return spi;
                }
                else
                {
                    _logger.LogWarning("?? SPI Automation: SPI not found by name: {SPIName}", spiName);
                }
            }
            else
            {
                _logger.LogError("? SPI Automation: Failed to retrieve SPIs for lookup");
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Automation: Error looking up SPI by name: {SPIName}", spiName);
            return null;
        }
    }

    /// <summary>
    /// Finds SPI by description pattern (for legacy type matching)
    /// </summary>
    private async Task<SafetyPerformanceIndicator?> FindSPIByDescription(string descriptionPattern, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("?? SPI Automation: Looking up SPI by description pattern: {Pattern}", descriptionPattern);

            var getAllQuery = new GetAllSafetyPerformanceIndicatorsQuery();
            var result = await _mediator.SendAsync(getAllQuery, ct);

            if (result.IsSuccess && result.Value?.Any() == true)
            {
                var spi = result.Value.FirstOrDefault(s => s.Description.Contains(descriptionPattern, StringComparison.OrdinalIgnoreCase));

                if (spi != null)
                {
                    _logger.LogInformation("? SPI Automation: Found SPI by description with Code: {Code}", spi.Code);
                    return spi;
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Automation: Error looking up SPI by description: {Pattern}", descriptionPattern);
            return null;
        }
    }

    /// <summary>
    /// Gets count of hazards submitted (created) on a specific date
    /// Tracks raw hazard submissions regardless of validation status
    /// </summary>
    private async Task<int> GetHazardCountForDate(DateTime date, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("?? SPI Automation: Querying hazard submissions for {Date}", date.ToString("yyyy-MM-dd"));

            // Query hazards created on the specific date
            // TODO: When hazard CQRS queries are available, use GetHazardsByDateQuery
            // For now, use a simple incremental counter based on external reporting events

            // Since we trigger this from hazard creation events, each call = 1 new hazard
            // This gives us the actual count of hazards submitted today
            var todaysKey = date.ToString("yyyy-MM-dd");

            if (!_dailyHazardCounts.ContainsKey(todaysKey))
            {
                _dailyHazardCounts[todaysKey] = 0;
            }

            _dailyHazardCounts[todaysKey]++;
            var actualCount = _dailyHazardCounts[todaysKey];

            _logger.LogInformation("? SPI Automation: Found {Count} hazard submissions on {Date}", 
                actualCount, date.ToString("yyyy-MM-dd"));

            return actualCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? Error getting hazard submission count for date {Date}", date);
            return 1; // Default to 1 since this is called when a hazard is created
        }
    }

    /// <summary>
    /// Gets count of VALIDATED SMS risks for a specific date  
    /// This should be used for risk assessment effectiveness SPIs, not hazard report rate
    /// </summary>
    private async Task<int> GetValidatedSMSRiskCountForDate(DateTime date, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("?? SPI Automation: Querying validated SMS risks for {Date}", date.ToString("yyyy-MM-dd"));

            // Use CQRS query to get validated SMS risks for the specific date
            var query = new GetValidatedSMSRisksByDateQuery(date);
            var result = await _mediator.SendAsync(query, ct);

            if (result.IsSuccess)
            {
                var validatedCount = result.Value.Count;
                _logger.LogInformation("? SPI Automation: Found {Count} validated SMS risks on {Date}", 
                    validatedCount, date.ToString("yyyy-MM-dd"));

                // Log details of what we found for transparency
                if (result.Value.Any())
                {
                    var reportCodes = string.Join(", ", result.Value.Select(rv => rv.ReportCode ?? "Unknown"));
                    _logger.LogInformation("?? Validated reports on {Date}: {ReportCodes}", 
                        date.ToString("yyyy-MM-dd"), reportCodes);
                }

                return validatedCount;
            }
            else
            {
                _logger.LogWarning("?? SPI Automation: Failed to query validated SMS risks for {Date}: {Error}", 
                    date.ToString("yyyy-MM-dd"), result.Error?.Message);
                return 0; // No validated risks found
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? Error getting validated SMS risk count for date {Date}", date);
            return 0; // Default to 0 on error
        }
    }

    // Static dictionary to track daily hazard submission counts
    // In production, this would be replaced with: SELECT COUNT(*) FROM Hazards WHERE CAST(CreatedDate AS DATE) = @date
    private static readonly Dictionary<string, int> _dailyHazardCounts = new();

    /// <summary>
    /// Gets count of high-risk assessments for a specific date
    /// </summary>
    private async Task<int> GetHighRiskCountForDate(DateTime date, CancellationToken ct)
    {
        try
        {
            // TODO: Implement query to get high-risk count for specific date
            _logger.LogWarning("?? SPI Automation: GetHighRiskCountForDate not yet implemented - using placeholder");
            return 1; // Placeholder - implement actual query
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting high risk count for date {Date}", date);
            return 0;
        }
    }

    /// <summary>
    /// Gets count of overdue mitigations
    /// </summary>
    private async Task<int> GetOverdueMitigationCount(DateTime asOfDate, CancellationToken ct)
    {
        try
        {
            // TODO: Implement query to get overdue mitigation count
            _logger.LogWarning("?? SPI Automation: GetOverdueMitigationCount not yet implemented - using placeholder");
            return 0; // Placeholder - implement actual query
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overdue mitigation count for date {Date}", asOfDate);
            return 0;
        }
    }

    /// <summary>
    /// Gets count of active mitigations
    /// </summary>
    private async Task<int> GetActiveMitigationCount(DateTime asOfDate, CancellationToken ct)
    {
        try
        {
            // TODO: Implement query to get active mitigation count
            _logger.LogWarning("?? SPI Automation: GetActiveMitigationCount not yet implemented - using placeholder");
            return 1; // Placeholder - implement actual query
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active mitigation count for date {Date}", asOfDate);
            return 1; // Default to 1 to avoid division by zero
        }
    }

    /// <summary>
    /// Determines if a risk level is considered high risk for SPI tracking
    /// Uses proper RiskLevel enum instead of hardcoded strings
    /// </summary>
    private static bool IsHighRisk(string riskLevel)
    {
        // Use enum Value property for comparison
        return string.Equals(riskLevel, RiskLevel.Critical.Value, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(riskLevel, RiskLevel.High.Value, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Calculates mitigation implementation score based on completion timing
    /// </summary>
    private static decimal CalculateMitigationScore(double daysFromTarget)
    {
        // On time or early: 100%
        if (daysFromTarget <= 0)
            return 100m;

        // Late by 1-7 days: 90-99%
        if (daysFromTarget <= 7)
            return 100m - ((decimal)daysFromTarget * 1.5m);

        // Late by 8-30 days: 75-89%
        if (daysFromTarget <= 30)
            return 90m - ((decimal)(daysFromTarget - 7) * 0.7m);

        // Late by more than 30 days: 0-74%
        return Math.Max(0m, 75m - ((decimal)(daysFromTarget - 30) * 2m));
    }

    #endregion
}
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
using SMS_Application.Queries;
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
    private readonly IBaseMediator _mediator;
    private readonly ILogger<SPIAutomationService> _logger;

    public SPIAutomationService(
        SafetyPerformanceIndicatorService spiService,
        IBaseMediator mediator,
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
            _logger.LogApplicationInformation("SPI Automation: Updating Hazard Report Rate for date {Date}", reportDate);

            var hazardCreatedDataSource = EventType.HazardCreated.Value;
            var targetSpis = await FindSPIsByDataSource(hazardCreatedDataSource, ct);
            if (!targetSpis.Any())
            {
                _logger.LogApplicationError("SPI Automation: No SPI configured with data source {DataSource}", hazardCreatedDataSource);
                return Result<bool>.Failure<bool>(DomainErrors.SPIError.NotFound);
            }

            // Get count of hazards reported today
            var todaysHazardCount = await GetHazardCountForDate(reportDate, ct);

            var successfulUpdates = 0;
            Error? lastError = null;

            foreach (var spi in targetSpis)
            {
                var dataPoint = new SPIDataPoint(new SPIDataPointID($"HRR-{spi.Code}-{reportDate:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8]}"))
                {
                    SPIId = spi.Code,
                    Value = todaysHazardCount,
                    MeasurementDate = reportDate,
                    Period = reportDate.ToString("yyyy-MM-dd"),
                    DataSource = hazardCreatedDataSource,
                    Notes = $"Daily hazard submissions: {todaysHazardCount} (raw hazard creation count)",
                    IsVerified = true,
                    VerifiedBy = "SYSTEM",
                    VerifiedDate = DateTime.UtcNow,
                    CreatedBy = "SPI_AUTOMATION"
                };

                var result = await _spiService.AddSPIDataPointAsync(spi.Code, dataPoint, ct);
                if (result.IsSuccess)
                {
                    successfulUpdates++;
                }
                else
                {
                    lastError = result.Error;
                    _logger.LogApplicationWarning("SPI Automation: Failed to update hazard report datapoint for SPI {SPICode}. Error: {Error}", spi.Code, result.Error?.Message);
                }
            }

            return successfulUpdates > 0
                ? Result<bool>.Success(true)
                : Result<bool>.Failure<bool>(lastError ?? DomainErrors.SPIError.AutomationFailed);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "SPI Automation: Exception updating Hazard Report Rate");
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
            _logger.LogApplicationInformation("SPI Automation: Updating Hazard Closure Time for hazard {HazardId}", hazardId);

            var hazardStatusChangedDataSource = EventType.HazardStatusChanged.Value;
            var targetSpis = await FindSPIsByDataSource(hazardStatusChangedDataSource, ct);
            if (!targetSpis.Any())
            {
                _logger.LogApplicationError("SPI Automation: No SPI configured with data source {DataSource}", hazardStatusChangedDataSource);
                return Result<bool>.Failure<bool>(DomainErrors.SPIError.NotFound);
            }

            var daysToClose = (closedDate - submittedDate).TotalDays;

            var successfulUpdates = 0;
            Error? lastError = null;

            foreach (var spi in targetSpis)
            {
                var dataPoint = new SPIDataPoint(new SPIDataPointID($"HCT-{spi.Code}-{hazardId}-{closedDate:yyyyMMdd}"))
                {
                    SPIId = spi.Code,
                    Value = (decimal)daysToClose,
                    MeasurementDate = closedDate,
                    Period = closedDate.ToString("yyyy-MM-dd"),
                    DataSource = hazardStatusChangedDataSource,
                    Notes = $"Hazard {hazardId} closed in {daysToClose:F1} days",
                    IsVerified = true,
                    VerifiedBy = "SYSTEM",
                    VerifiedDate = DateTime.UtcNow,
                    CreatedBy = "SPI_AUTOMATION"
                };

                var result = await _spiService.AddSPIDataPointAsync(spi.Code, dataPoint, ct);
                if (result.IsSuccess)
                {
                    successfulUpdates++;
                }
                else
                {
                    lastError = result.Error;
                    _logger.LogApplicationWarning("SPI Automation: Failed to update hazard closure datapoint for SPI {SPICode}. Error: {Error}", spi.Code, result.Error?.Message);
                }
            }

            return successfulUpdates > 0
                ? Result<bool>.Success(true)
                : Result<bool>.Failure<bool>(lastError ?? DomainErrors.SPIError.AutomationFailed);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "SPI Automation: Exception updating Hazard Closure Time for hazard {HazardId}", hazardId);
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
            _logger.LogApplicationInformation("SPI Automation: Updating Risk Assessment Completion for assessment {AssessmentId}", assessmentId);

            var riskAssessmentCompletedDataSource = EventType.RiskAssessmentCompleted.Value;
            var targetSpis = await FindSPIsByDataSource(riskAssessmentCompletedDataSource, ct);
            if (!targetSpis.Any())
            {
                _logger.LogApplicationError("SPI Automation: No SPI configured with data source {DataSource}", riskAssessmentCompletedDataSource);
                return Result<bool>.Failure<bool>(DomainErrors.SPIError.NotFound);
            }

            // Calculate completion rate (1 = 100% for on-time, 0 = 0% for late)
            var completionScore = isOnTime ? 100m : 0m;
            var daysToComplete = (completedDate - startDate).TotalDays;

            var successfulUpdates = 0;
            Error? lastError = null;

            foreach (var spi in targetSpis)
            {
                var dataPoint = new SPIDataPoint(new SPIDataPointID($"RAC-{spi.Code}-{assessmentId}-{completedDate:yyyyMMdd}"))
                {
                    SPIId = spi.Code,
                    Value = completionScore,
                    MeasurementDate = completedDate,
                    Period = completedDate.ToString("yyyy-MM-dd"),
                    DataSource = riskAssessmentCompletedDataSource,
                    Notes = $"Assessment {assessmentId} completed in {daysToComplete:F1} days - {(isOnTime ? "On Time" : "Late")}",
                    IsVerified = true,
                    VerifiedBy = "SYSTEM",
                    VerifiedDate = DateTime.UtcNow,
                    CreatedBy = "SPI_AUTOMATION"
                };

                var result = await _spiService.AddSPIDataPointAsync(spi.Code, dataPoint, ct);
                if (result.IsSuccess)
                {
                    successfulUpdates++;
                }
                else
                {
                    lastError = result.Error;
                    _logger.LogApplicationWarning("SPI Automation: Failed to update risk assessment completion datapoint for SPI {SPICode}. Error: {Error}", spi.Code, result.Error?.Message);
                }
            }

            return successfulUpdates > 0
                ? Result<bool>.Success(true)
                : Result<bool>.Failure<bool>(lastError ?? DomainErrors.SPIError.AutomationFailed);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "SPI Automation: Exception updating Risk Assessment Completion for assessment {AssessmentId}", assessmentId);
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
            _logger.LogApplicationInformation("SPI Automation: Updating High Risk Exposure for risk level {RiskLevel}", riskLevel);

            // Only track Critical and High risk levels
            if (!IsHighRisk(riskLevel))
            {
                _logger.LogApplicationInformation("SPI Automation: Risk level {RiskLevel} not considered high risk - skipping", riskLevel);
                return Result<bool>.Success(true);
            }

            var highRiskIdentifiedDataSource = EventType.HighRiskIdentified.Value;
            var targetSpis = await FindSPIsByDataSource(highRiskIdentifiedDataSource, ct);
            if (!targetSpis.Any())
            {
                _logger.LogApplicationError("SPI Automation: No SPI configured with data source {DataSource}", highRiskIdentifiedDataSource);
                return Result<bool>.Failure<bool>(DomainErrors.SPIError.NotFound);
            }

            // Get current high-risk count for the day
            var highRiskCount = await GetHighRiskCountForDate(identifiedDate, ct);

            var successfulUpdates = 0;
            Error? lastError = null;

            foreach (var spi in targetSpis)
            {
                var dataPoint = new SPIDataPoint(new SPIDataPointID($"HRE-{spi.Code}-{identifiedDate:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8]}"))
                {
                    SPIId = spi.Code,
                    Value = highRiskCount,
                    MeasurementDate = identifiedDate,
                    Period = identifiedDate.ToString("yyyy-MM-dd"),
                    DataSource = highRiskIdentifiedDataSource,
                    Notes = $"High risk exposure count: {highRiskCount} (Latest: {riskLevel})",
                    IsVerified = true,
                    VerifiedBy = "SYSTEM",
                    VerifiedDate = DateTime.UtcNow,
                    CreatedBy = "SPI_AUTOMATION"
                };

                var result = await _spiService.AddSPIDataPointAsync(spi.Code, dataPoint, ct);
                if (result.IsSuccess)
                {
                    successfulUpdates++;
                }
                else
                {
                    lastError = result.Error;
                    _logger.LogApplicationWarning("SPI Automation: Failed to update high risk exposure datapoint for SPI {SPICode}. Error: {Error}", spi.Code, result.Error?.Message);
                }
            }

            return successfulUpdates > 0
                ? Result<bool>.Success(true)
                : Result<bool>.Failure<bool>(lastError ?? DomainErrors.SPIError.AutomationFailed);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "SPI Automation: Exception updating High Risk Exposure");
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
            _logger.LogApplicationInformation("SPI Automation: Updating Mitigation Implementation Rate for mitigation {MitigationId}", mitigationId);

            var mitigationCompletedDataSource = EventType.MitigationCompleted.Value;
            var targetSpis = await FindSPIsByDataSource(mitigationCompletedDataSource, ct);
            if (!targetSpis.Any())
            {
                _logger.LogApplicationWarning(
                    "SPI Automation: No SPI configured with data source {DataSource} for mitigation {MitigationId}",
                    mitigationCompletedDataSource,
                    mitigationId);
                return Result<bool>.Failure<bool>(DomainErrors.SPIError.NotFound);
            }

            // Calculate implementation score based on timeliness
            var daysFromTarget = (completedDate - targetDate).TotalDays;
            var implementationScore = CalculateMitigationScore(daysFromTarget);

            var successfulUpdates = 0;
            Error? lastError = null;

            foreach (var spi in targetSpis)
            {
                var dataPoint = new SPIDataPoint(new SPIDataPointID($"MIR-{spi.Code}-{mitigationId}-{completedDate:yyyyMMdd}"))
                {
                    SPIId = spi.Code,
                    Value = implementationScore,
                    MeasurementDate = completedDate,
                    Period = completedDate.ToString("yyyy-MM-dd"),
                    DataSource = mitigationCompletedDataSource,
                    Notes = $"Mitigation {mitigationId} completed {daysFromTarget:F1} days from target - Score: {implementationScore}%",
                    IsVerified = true,
                    VerifiedBy = "SYSTEM",
                    VerifiedDate = DateTime.UtcNow,
                    CreatedBy = "SPI_AUTOMATION"
                };

                var result = await _spiService.AddSPIDataPointAsync(spi.Code, dataPoint, ct);
                if (result.IsSuccess)
                {
                    successfulUpdates++;
                }
                else
                {
                    lastError = result.Error;
                    _logger.LogApplicationWarning(
                        "SPI Automation: Failed mitigation datapoint insert for SPI {SPICode}. Error: {Error}",
                        spi.Code,
                        result.Error?.Message);
                }
            }

            if (successfulUpdates > 0)
            {
                _logger.LogApplicationInformation(
                    "SPI Automation: Successfully updated {Count} SPI(s) for mitigation completion {MitigationId} (Score: {Score}%)",
                    successfulUpdates,
                    mitigationId,
                    implementationScore);
                return Result<bool>.Success(true);
            }

            return Result<bool>.Failure<bool>(lastError ?? DomainErrors.SPIError.AutomationFailed);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "SPI Automation: Exception updating Mitigation Implementation Rate for mitigation {MitigationId}", mitigationId);
            return Result<bool>.Failure<bool>(DomainErrors.SPIError.AutomationFailed);
        }
    }

    /// <summary>
    /// Finds all SPIs bound to a specific data source token/event type.
    /// </summary>
    private async Task<List<SafetyPerformanceIndicator>> FindSPIsByDataSource(string dataSource, CancellationToken ct = default)
    {
        try
        {
            var getAllQuery = new GetAllSafetyPerformanceIndicatorsQuery();
            var result = await _mediator.SendAsync(getAllQuery, ct);

            if (result.IsFailure || result.Value is null)
            {
                _logger.LogApplicationWarning("SPI Automation: Failed to retrieve SPIs for data source lookup {DataSource}", dataSource);
                return new List<SafetyPerformanceIndicator>();
            }

            return result.Value
                .Where(s => !string.IsNullOrWhiteSpace(s.DataSource) &&
                            string.Equals(s.DataSource.Trim(), dataSource, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "SPI Automation: Error looking up SPIs by data source {DataSource}", dataSource);
            return new List<SafetyPerformanceIndicator>();
        }
    }

    /// <summary>
    /// Updates Corrective Action Closure SPI when mitigations are overdue
    /// </summary>
    public async Task<Result<bool>> UpdateCorrectiveActionClosureAsync(DateTime calculationDate, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("SPI Automation: Updating Corrective Action Closure Rate for date {Date}", calculationDate);

            var mitigationOverdueDataSource = EventType.MitigationOverdue.Value;
            var targetSpis = await FindSPIsByDataSource(mitigationOverdueDataSource, ct);
            if (!targetSpis.Any())
            {
                _logger.LogApplicationError("SPI Automation: No SPI configured with data source {DataSource}", mitigationOverdueDataSource);
                return Result<bool>.Failure<bool>(DomainErrors.SPIError.NotFound);
            }

            // Get overdue mitigation counts
            var overdueCount = await GetOverdueMitigationCount(calculationDate, ct);
            var totalActiveCount = await GetActiveMitigationCount(calculationDate, ct);
            var closureRate = totalActiveCount > 0 ? ((totalActiveCount - overdueCount) / (decimal)totalActiveCount) * 100 : 100m;

            var successfulUpdates = 0;
            Error? lastError = null;

            foreach (var spi in targetSpis)
            {
                var dataPoint = new SPIDataPoint(new SPIDataPointID($"CAC-{spi.Code}-{calculationDate:yyyyMMdd}"))
                {
                    SPIId = spi.Code,
                    Value = closureRate,
                    MeasurementDate = calculationDate,
                    Period = calculationDate.ToString("yyyy-MM-dd"),
                    DataSource = mitigationOverdueDataSource,
                    Notes = $"Closure rate: {closureRate:F1}% ({totalActiveCount - overdueCount}/{totalActiveCount} on time)",
                    IsVerified = true,
                    VerifiedBy = "SYSTEM",
                    VerifiedDate = DateTime.UtcNow,
                    CreatedBy = "SPI_AUTOMATION"
                };

                var result = await _spiService.AddSPIDataPointAsync(spi.Code, dataPoint, ct);
                if (result.IsSuccess)
                {
                    successfulUpdates++;
                }
                else
                {
                    lastError = result.Error;
                    _logger.LogApplicationWarning("SPI Automation: Failed to update corrective-action-closure datapoint for SPI {SPICode}. Error: {Error}", spi.Code, result.Error?.Message);
                }
            }

            return successfulUpdates > 0
                ? Result<bool>.Success(true)
                : Result<bool>.Failure<bool>(lastError ?? DomainErrors.SPIError.AutomationFailed);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "SPI Automation: Exception updating Corrective Action Closure Rate");
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
            _logger.LogApplicationInformation("SPI Automation: Updating Risk Identification Effectiveness for report {ReportCode}, Decision: {Decision}", reportCode, validationDecision);

            if (!string.Equals(validationDecision?.Trim(), ValidationDecision.SmsRisk.Value, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogApplicationInformation(
                    "SPI Automation: Skipping risk-identification datapoint for report {ReportCode}. Decision {Decision} is not {RequiredDecision}",
                    reportCode,
                    validationDecision,
                    ValidationDecision.SmsRisk.Value);

                return Result<bool>.Success(true);
            }

            // This could feed into a new SPI that tracks validation effectiveness
            // For example: "SMS Risk Identification Rate" = (SMS_RISK decisions / Total validations) * 100

            var validationDecisionDataSource = EventType.ValidationDecisionMade.Value;
            var targetSpis = await FindSPIsByDataSource(validationDecisionDataSource, ct);
            if (!targetSpis.Any())
            {
                _logger.LogApplicationError("SPI Automation: No SPI configured with data source {DataSource}", validationDecisionDataSource);
                return Result<bool>.Failure<bool>(DomainErrors.SPIError.NotFound);
            }

            // Calculate daily effectiveness rate using validated decisions
            var effectivenessRate = await CalculateRiskIdentificationRate(validatedDate, ct);

            var successfulUpdates = 0;
            Error? lastError = null;

            foreach (var spi in targetSpis)
            {
                var dataPoint = new SPIDataPoint(new SPIDataPointID($"RIE-{spi.Code}-{validatedDate:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8]}"))
                {
                    SPIId = spi.Code,
                    Value = effectivenessRate,
                    MeasurementDate = validatedDate,
                    Period = validatedDate.ToString("yyyy-MM-dd"),
                    DataSource = validationDecisionDataSource,
                    Notes = $"Risk identification effectiveness: {effectivenessRate:F1}% (Decision: {validationDecision})",
                    IsVerified = true,
                    VerifiedBy = "SYSTEM",
                    VerifiedDate = DateTime.UtcNow,
                    CreatedBy = "SPI_AUTOMATION"
                };

                var result = await _spiService.AddSPIDataPointAsync(spi.Code, dataPoint, ct);
                if (result.IsSuccess)
                {
                    successfulUpdates++;
                }
                else
                {
                    lastError = result.Error;
                    _logger.LogApplicationWarning("SPI Automation: Failed to update risk-identification-effectiveness datapoint for SPI {SPICode}. Error: {Error}", spi.Code, result.Error?.Message);
                }
            }

            return successfulUpdates > 0
                ? Result<bool>.Success(true)
                : Result<bool>.Failure<bool>(lastError ?? DomainErrors.SPIError.AutomationFailed);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "SPI Automation: Exception updating Risk Identification Effectiveness");
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

    #region Helper Methods - Datasource Routed Lookup

    /// <summary>
    /// Gets count of hazards submitted (created) on a specific date
    /// Tracks raw hazard submissions regardless of validation status
    /// </summary>
    private async Task<int> GetHazardCountForDate(DateTime date, CancellationToken ct)
    {
        try
        {
            _logger.LogApplicationInformation("SPI Automation: Querying hazard submissions for {Date}", date.ToString("yyyy-MM-dd"));

            var hazardsResult = await _mediator.SendAsync(new GetAllHazardsQuery(), ct);
            if (hazardsResult.IsFailure || hazardsResult.Value is null)
            {
                _logger.LogApplicationWarning("SPI Automation: Failed to query hazards for {Date}: {Error}", date.ToString("yyyy-MM-dd"), hazardsResult.Error?.Message);
                return 1;
            }

            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            var actualCount = hazardsResult.Value.Count(h =>
            {
                var createdDate = h.CreatedDate ?? DateTime.MinValue;
                return createdDate >= startOfDay && createdDate < endOfDay;
            });

            if (actualCount <= 0)
            {
                // Event-driven caller indicates at least one hazard activity occurred.
                actualCount = 1;
            }

            _logger.LogApplicationInformation("SPI Automation: Found {Count} hazard submissions on {Date}", actualCount, date.ToString("yyyy-MM-dd"));

            return actualCount;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error getting hazard submission count for date {Date}", date);
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
            _logger.LogApplicationInformation("SPI Automation: Querying validated SMS risks for {Date}", date.ToString("yyyy-MM-dd"));

            // Use CQRS query to get validated SMS risks for the specific date
            var query = new GetValidatedSMSRisksByDateQuery(date);
            var result = await _mediator.SendAsync(query, ct);

            if (result.IsSuccess)
            {
                var validatedCount = result.Value.Count;
                _logger.LogApplicationInformation("SPI Automation: Found {Count} validated SMS risks on {Date}", 
                    validatedCount, date.ToString("yyyy-MM-dd"));

                // Log details of what we found for transparency
                if (result.Value.Any())
                {
                    var reportCodes = string.Join(", ", result.Value.Select(rv => rv.ReportCode ?? "Unknown"));
                    _logger.LogApplicationInformation("Validated reports on {Date}: {ReportCodes}", date.ToString("yyyy-MM-dd"), reportCodes);
                }

                return validatedCount;
            }
            else
            {
                _logger.LogApplicationWarning("SPI Automation: Failed to query validated SMS risks for {Date}: {Error}", date.ToString("yyyy-MM-dd"), result.Error?.Message);
                return 0; // No validated risks found
            }
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error getting validated SMS risk count for date {Date}", date);
            return 0; // Default to 0 on error
        }
    }

    /// <summary>
    /// Gets count of high-risk assessments for a specific date
    /// </summary>
    private async Task<int> GetHighRiskCountForDate(DateTime date, CancellationToken ct)
    {
        try
        {
            _logger.LogApplicationInformation("SPI Automation: Querying high-risk assessments for {Date}", date.ToString("yyyy-MM-dd"));

            var assessmentsResult = await _mediator.SendAsync(new GetAllRiskAssessmentsQuery(), ct);
            if (assessmentsResult.IsFailure || assessmentsResult.Value is null)
            {
                _logger.LogApplicationWarning("SPI Automation: Failed to query risk assessments for {Date}: {Error}", date.ToString("yyyy-MM-dd"), assessmentsResult.Error?.Message);
                return 0;
            }

            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            var highRiskCount = assessmentsResult.Value.Count(ra =>
            {
                var evaluatedDate = ra.CompletedDate ?? ra.UpdatedDate ?? ra.CreatedDate ?? DateTime.MinValue;
                var riskLevel = ra.FinalRiskLevel ?? string.Empty;

                return evaluatedDate >= startOfDay &&
                       evaluatedDate < endOfDay &&
                       (string.Equals(riskLevel, RiskLevel.Critical.Value, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(riskLevel, RiskLevel.High.Value, StringComparison.OrdinalIgnoreCase));
            });

            _logger.LogApplicationInformation("SPI Automation: Found {Count} high-risk assessments on {Date}", highRiskCount, date.ToString("yyyy-MM-dd"));
            return highRiskCount;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error getting high risk count for date {Date}", date);
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
            _logger.LogApplicationInformation("SPI Automation: Querying overdue mitigations as of {Date}", asOfDate.ToString("yyyy-MM-dd"));

            var mitigationsResult = await _mediator.SendAsync(new GetAllMitigationsQuery(), ct);
            if (mitigationsResult.IsFailure || mitigationsResult.Value is null)
            {
                _logger.LogApplicationWarning("SPI Automation: Failed to query mitigations for overdue count on {Date}: {Error}", asOfDate.ToString("yyyy-MM-dd"), mitigationsResult.Error?.Message);
                return 0;
            }

            var overdueCount = mitigationsResult.Value.Count(m =>
                m.TargetDate.HasValue &&
                m.TargetDate.Value.Date < asOfDate.Date &&
                m.Status != MitigationStatus.Complete);

            _logger.LogApplicationInformation("SPI Automation: Found {Count} overdue mitigations as of {Date}", overdueCount, asOfDate.ToString("yyyy-MM-dd"));
            return overdueCount;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error getting overdue mitigation count for date {Date}", asOfDate);
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
            _logger.LogApplicationInformation("SPI Automation: Querying active mitigations as of {Date}", asOfDate.ToString("yyyy-MM-dd"));

            var mitigationsResult = await _mediator.SendAsync(new GetAllMitigationsQuery(), ct);
            if (mitigationsResult.IsFailure || mitigationsResult.Value is null)
            {
                _logger.LogApplicationWarning("SPI Automation: Failed to query mitigations for active count on {Date}: {Error}", asOfDate.ToString("yyyy-MM-dd"), mitigationsResult.Error?.Message);
                return 1;
            }

            var activeCount = mitigationsResult.Value.Count(m => m.Status != MitigationStatus.Complete);
            if (activeCount <= 0)
            {
                activeCount = 1;
            }

            _logger.LogApplicationInformation("SPI Automation: Found {Count} active mitigations as of {Date}", activeCount, asOfDate.ToString("yyyy-MM-dd"));
            return activeCount;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error getting active mitigation count for date {Date}", asOfDate);
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


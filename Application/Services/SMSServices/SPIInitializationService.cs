//-----------------------------------------------------------------------
// <copyright file="SPIInitializationService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Service for initializing Safety Performance Indicators with default
//                  configurations, target values, and automated calculation settings.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS_Application.Services;

/// <summary>
/// Service for initializing SPIs with default configurations
/// </summary>
public class SPIInitializationService
{
    private readonly SafetyPerformanceIndicatorService _spiService;
    private readonly ILogger<SPIInitializationService> _logger;

    public SPIInitializationService(
        SafetyPerformanceIndicatorService spiService,
        ILogger<SPIInitializationService> logger)
    {
        _spiService = spiService ?? throw new ArgumentNullException(nameof(spiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Initialize default SPIs for the SMS system
    /// </summary>
    public async Task<Result<bool>> InitializeDefaultSPIsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogApplicationInformation("SPI Initialization: Starting default SPI setup");

            var defaultSPIs = GetDefaultSPIConfigurations();
            var createdCount = 0;
            var skippedCount = 0;

            foreach (var spiConfig in defaultSPIs)
            {
                try
                {
                    // Check if SPI already exists
                    var existingResult = await _spiService.GetSafetyPerformanceIndicatorByCodeAsync(spiConfig.Code, cancellationToken);
                    if (existingResult.IsSuccess)
                    {
                        _logger.LogApplicationInformation("SPI Initialization: SPI {Code} already exists - skipping", spiConfig.Code);
                        skippedCount++;
                        continue;
                    }

                    // Create new SPI
                    var createResult = await _spiService.CreateSafetyPerformanceIndicatorAsync(spiConfig, cancellationToken);
                    if (createResult.IsSuccess)
                    {
                        _logger.LogApplicationInformation("SPI Initialization: Created SPI {Code} - {Name}", spiConfig.Code, spiConfig.Name);
                        createdCount++;
                    }
                    else
                    {
                        _logger.LogApplicationWarning("SPI Initialization: Failed to create SPI {Code} - {Error}", 
                            spiConfig.Code, createResult.Error?.Message);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogApplicationError(ex, "SPI Initialization: Error creating SPI {Code}", spiConfig.Code);
                }
            }

            _logger.LogApplicationInformation("SPI Initialization: Complete - {Created} created, {Skipped} skipped", 
                createdCount, skippedCount);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "SPI Initialization: Error during initialization");
            return Result<bool>.Failure<bool>(DomainErrors.SPIError.CreateFailed);
        }
    }

    /// <summary>
    /// Get default SPI configurations for SMS system
    /// </summary>
    private List<SafetyPerformanceIndicator> GetDefaultSPIConfigurations()
    {
        var spis = new List<SafetyPerformanceIndicator>();

        // 1. Hazard Report Rate (Leading Indicator)
        spis.Add(new SafetyPerformanceIndicator(
            new SafetyPerformanceIndicatorID("HAZARD_REPORT_RATE"),
            "Hazard Report Rate",
            "Number of hazard reports submitted per day (automated from External Reporting and Hazard Reporting)",
            SMSSafetyPerformanceIndicatorType.HazardReportRate,
            "SPI_SYSTEM")
        {
            Code = "HAZARD_REPORT_RATE",
            Status = SPIStatus.Active,
            MeasurementUnit = "Reports per day",
            MeasurementFrequency = SPIMeasurementFrequency.Daily,
            CalculationMethod = "Automated count of daily hazard submissions",
            DataSource = "Automated-HazardEvent",
            TargetValue = 5m, // Target 5 reports per day
            AcceptableRange = 2m, // ±2 reports acceptable
            WarningThreshold = 3m, // Warning if more than 3 away from target
            CriticalThreshold = 5m, // Critical if more than 5 away from target
            ResponsibleDepartment = "Safety Management",
            DataOwner = "SMS Administrator",
            ReviewAuthority = "Safety Manager",
            NextReviewDate = DateTime.UtcNow.AddMonths(6),
            AlertsEnabled = true,
            AlertRecipients = "sms@airport.com"
        });

        // 2. Risk Assessment Completion Rate (Process Indicator)
        spis.Add(new SafetyPerformanceIndicator(
            new SafetyPerformanceIndicatorID("RISK_ASSESSMENT_COMPLETION"),
            "Risk Assessment Completion Rate",
            "Percentage of risk assessments completed within target timeline (automated from Technical Assessment)",
            SMSSafetyPerformanceIndicatorType.RiskAssessmentCompletion,
            "SPI_SYSTEM")
        {
            Code = "RISK_ASSESSMENT_COMPLETION",
            Status = SPIStatus.Active,
            MeasurementUnit = "Percentage",
            MeasurementFrequency = SPIMeasurementFrequency.Weekly,
            CalculationMethod = "Automated calculation based on assessment completion timing",
            DataSource = "Automated-RiskAssessment",
            TargetValue = 90m, // Target 90% on-time completion
            AcceptableRange = 10m, // 80-100% acceptable
            WarningThreshold = 15m, // Warning if below 75%
            CriticalThreshold = 25m, // Critical if below 65%
            ResponsibleDepartment = "Risk Assessment",
            DataOwner = "Risk Assessment Manager",
            ReviewAuthority = "SMS Manager",
            NextReviewDate = DateTime.UtcNow.AddMonths(3),
            AlertsEnabled = true,
            AlertRecipients = "risk@airport.com"
        });

        // 3. Mitigation Implementation Rate (Leading Indicator)
        spis.Add(new SafetyPerformanceIndicator(
            new SafetyPerformanceIndicatorID("MITIGATION_IMPLEMENTATION_RATE"),
            "Mitigation Implementation Rate",
            "Effectiveness score of mitigation implementations based on completion timing (automated from Mitigation Management)",
            SMSSafetyPerformanceIndicatorType.MitigationImplementationRate,
            "SPI_SYSTEM")
        {
            Code = "MITIGATION_IMPLEMENTATION_RATE",
            Status = SPIStatus.Active,
            MeasurementUnit = "Effectiveness Score",
            MeasurementFrequency = SPIMeasurementFrequency.Weekly,
            CalculationMethod = "Automated effectiveness scoring based on completion vs target dates",
            DataSource = "Automated-MitigationCompletion",
            TargetValue = 85m, // Target 85% effectiveness score
            AcceptableRange = 15m, // 70-100% acceptable
            WarningThreshold = 20m, // Warning if below 65%
            CriticalThreshold = 35m, // Critical if below 50%
            ResponsibleDepartment = "Safety Management",
            DataOwner = "Mitigation Coordinator",
            ReviewAuthority = "Safety Manager",
            NextReviewDate = DateTime.UtcNow.AddMonths(3),
            AlertsEnabled = true,
            AlertRecipients = "mitigations@airport.com"
        });

        // 4. Corrective Action Closure Rate (Process Indicator)
        spis.Add(new SafetyPerformanceIndicator(
            new SafetyPerformanceIndicatorID("CORRECTIVE_ACTION_CLOSURE"),
            "Corrective Action Closure Rate",
            "Percentage of corrective actions closed on time (automated from daily overdue checks)",
            SMSSafetyPerformanceIndicatorType.CorrectiveActionClosure,
            "SPI_SYSTEM")
        {
            Code = "CORRECTIVE_ACTION_CLOSURE",
            Status = SPIStatus.Active,
            MeasurementUnit = "Percentage",
            MeasurementFrequency = SPIMeasurementFrequency.Daily,
            CalculationMethod = "Automated calculation of (actions closed on time / total active actions) * 100",
            DataSource = "Automated-ScheduledCalculation",
            TargetValue = 95m, // Target 95% on-time closure
            AcceptableRange = 10m, // 85-100% acceptable
            WarningThreshold = 15m, // Warning if below 80%
            CriticalThreshold = 25m, // Critical if below 70%
            ResponsibleDepartment = "Safety Management",
            DataOwner = "Action Coordinator",
            ReviewAuthority = "SMS Manager",
            NextReviewDate = DateTime.UtcNow.AddMonths(3),
            AlertsEnabled = true,
            AlertRecipients = "actions@airport.com"
        });

        // 5. High Risk Exposure (Custom - Lagging Indicator)
        spis.Add(new SafetyPerformanceIndicator(
            new SafetyPerformanceIndicatorID("HIGH_RISK_EXPOSURE"),
            "High Risk Exposure Count",
            "Daily count of Critical and High risk conditions identified (automated from Risk Assessment completion)",
            SMSSafetyPerformanceIndicatorType.SystemEffectiveness, // Using closest available type
            "SPI_SYSTEM")
        {
            Code = "HIGH_RISK_EXPOSURE",
            Status = SPIStatus.Active,
            MeasurementUnit = "Count per day",
            MeasurementFrequency = SPIMeasurementFrequency.Daily,
            CalculationMethod = "Automated count of Critical and High risk assessments completed daily",
            DataSource = "Automated-RiskAssessment",
            TargetValue = 2m, // Target maximum 2 high risks per day
            AcceptableRange = 1m, // 0-3 acceptable
            WarningThreshold = 2m, // Warning if more than 4
            CriticalThreshold = 4m, // Critical if more than 6
            ResponsibleDepartment = "Risk Assessment",
            DataOwner = "Risk Manager",
            ReviewAuthority = "SMS Manager",
            NextReviewDate = DateTime.UtcNow.AddMonths(3),
            AlertsEnabled = true,
            AlertRecipients = "risk@airport.com"
        });

        // 6. Hazard Closure Time (Custom - Process Indicator)
        spis.Add(new SafetyPerformanceIndicator(
            new SafetyPerformanceIndicatorID("HAZARD_CLOSURE_TIME"),
            "Average Hazard Closure Time",
            "Average days to close hazards from submission to resolution (automated from hazard closure events)",
            SMSSafetyPerformanceIndicatorType.InvestigationTimeliness, // Using closest available type
            "SPI_SYSTEM")
        {
            Code = "HAZARD_CLOSURE_TIME",
            Status = SPIStatus.Active,
            MeasurementUnit = "Days",
            MeasurementFrequency = SPIMeasurementFrequency.Weekly,
            CalculationMethod = "Automated calculation of average days between hazard submission and closure",
            DataSource = "Automated-HazardClosure",
            TargetValue = 30m, // Target 30 days to close hazards
            AcceptableRange = 15m, // 15-45 days acceptable
            WarningThreshold = 20m, // Warning if more than 50 days
            CriticalThreshold = 40m, // Critical if more than 70 days
            ResponsibleDepartment = "Safety Management",
            DataOwner = "Hazard Coordinator",
            ReviewAuthority = "Safety Manager",
            NextReviewDate = DateTime.UtcNow.AddMonths(3),
            AlertsEnabled = true,
            AlertRecipients = "hazards@airport.com"
        });

        return spis;
    }
}


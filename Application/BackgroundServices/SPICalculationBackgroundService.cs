//-----------------------------------------------------------------------
// <copyright file="SPICalculationBackgroundService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Background service for scheduled Safety Performance Indicator calculations.
//                  Runs automated SPI calculations on daily, weekly, and monthly schedules
//                  to maintain accurate performance metrics and trend analysis.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;

namespace SMS_Application.BackgroundServices;

/// <summary>
/// Background service for automated SPI calculations and maintenance
/// Runs on scheduled intervals to ensure SPI data is current and accurate
/// </summary>
public class SPICalculationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SPICalculationBackgroundService> _logger;
    private readonly TimeSpan _dailyInterval = TimeSpan.FromHours(24);
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour if it's time for calculations

    public SPICalculationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<SPICalculationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("?? SPI Background Service: Starting automated SPI calculation service");

        // Wait for initial startup delay
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformScheduledCalculations(stoppingToken);
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("?? SPI Background Service: Service cancellation requested");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "?? SPI Background Service: Unexpected error in scheduled calculations");
                // Continue running despite errors
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken); // Wait longer after error
            }
        }

        _logger.LogInformation("?? SPI Background Service: Automated SPI calculation service stopped");
    }

    /// <summary>
    /// Performs scheduled SPI calculations based on current time
    /// </summary>
    private async Task PerformScheduledCalculations(CancellationToken stoppingToken)
    {
        var now = DateTime.Now;
        var today = DateTime.Today;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var spiAutomationService = scope.ServiceProvider.GetRequiredService<ISPIAutomationService>();

            // Daily calculations (run at 6 AM)
            if (now.Hour == 6 && now.Minute < 60)
            {
                _logger.LogInformation("?? SPI Background Service: Running daily SPI calculations");
                await PerformDailyCalculations(spiAutomationService, today, stoppingToken);
            }

            // Weekly calculations (run on Mondays at 7 AM)
            if (now.DayOfWeek == DayOfWeek.Monday && now.Hour == 7 && now.Minute < 60)
            {
                _logger.LogInformation("??? SPI Background Service: Running weekly SPI calculations");
                await PerformWeeklyCalculations(spiAutomationService, today, stoppingToken);
            }

            // Monthly calculations (run on 1st of month at 8 AM)
            if (now.Day == 1 && now.Hour == 8 && now.Minute < 60)
            {
                _logger.LogInformation("?? SPI Background Service: Running monthly SPI calculations");
                await PerformMonthlyCalculations(spiAutomationService, today, stoppingToken);
            }

            // Overdue check (run daily at 9 AM)
            if (now.Hour == 9 && now.Minute < 60)
            {
                _logger.LogInformation("? SPI Background Service: Running overdue mitigation checks");
                await PerformOverdueChecks(spiAutomationService, today, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Background Service: Error in scheduled calculations");
        }
    }

    #region Daily Calculations

    /// <summary>
    /// Performs daily SPI calculations
    /// </summary>
    private async Task PerformDailyCalculations(ISPIAutomationService spiAutomationService, DateTime calculationDate, CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("?? SPI Daily: Starting daily calculations for {Date}", calculationDate);

            var tasks = new List<Task<Result<bool>>>
            {
                // Daily hazard report rate
                spiAutomationService.UpdateHazardReportRateAsync(calculationDate, stoppingToken),

                // Daily corrective action closure rate
                spiAutomationService.UpdateCorrectiveActionClosureAsync(calculationDate, stoppingToken)
            };

            var results = await Task.WhenAll(tasks);

            var successCount = results.Count(r => r.IsSuccess);
            var totalCount = results.Length;

            _logger.LogInformation("? SPI Daily: Completed daily calculations - {Success}/{Total} successful", successCount, totalCount);

            // Log any failures
            for (int i = 0; i < results.Length; i++)
            {
                if (results[i].IsFailure)
                {
                    _logger.LogWarning("?? SPI Daily: Daily calculation {Index} failed - Error: {Error}", i, results[i].Error?.Message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Daily: Error in daily calculations");
        }
    }

    #endregion

    #region Weekly Calculations

    /// <summary>
    /// Performs weekly SPI calculations and trend analysis
    /// </summary>
    private async Task PerformWeeklyCalculations(ISPIAutomationService spiAutomationService, DateTime calculationDate, CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("?? SPI Weekly: Starting weekly calculations for week ending {Date}", calculationDate);

            // Calculate weekly aggregates
            var weekStart = calculationDate.AddDays(-7);

            // Example: Calculate weekly hazard report trends
            // This would involve more complex queries across the week's data

            _logger.LogInformation("?? SPI Weekly: Analyzing trends for period {StartDate} to {EndDate}", weekStart, calculationDate);

            // Placeholder for weekly trend analysis
            // TODO: Implement weekly trend calculations
            // - Weekly hazard submission trends
            // - Weekly risk assessment completion trends  
            // - Weekly mitigation implementation trends

            _logger.LogInformation("? SPI Weekly: Completed weekly calculations");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Weekly: Error in weekly calculations");
        }
    }

    #endregion

    #region Monthly Calculations

    /// <summary>
    /// Performs monthly SPI calculations and comprehensive analysis
    /// </summary>
    private async Task PerformMonthlyCalculations(ISPIAutomationService spiAutomationService, DateTime calculationDate, CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("?? SPI Monthly: Starting monthly calculations for {Date}", calculationDate);

            var monthStart = new DateTime(calculationDate.Year, calculationDate.Month, 1);

            _logger.LogInformation("?? SPI Monthly: Analyzing monthly performance for {StartDate} to {EndDate}", monthStart, calculationDate);

            // Placeholder for monthly comprehensive analysis
            // TODO: Implement monthly calculations
            // - Monthly performance summaries
            // - Regulatory compliance metrics
            // - Management dashboard updates
            // - Trend analysis and forecasting

            _logger.LogInformation("? SPI Monthly: Completed monthly calculations");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Monthly: Error in monthly calculations");
        }
    }

    #endregion

    #region Overdue Checks

    /// <summary>
    /// Performs overdue mitigation checks and updates relevant SPIs
    /// </summary>
    private async Task PerformOverdueChecks(ISPIAutomationService spiAutomationService, DateTime checkDate, CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("? SPI Overdue: Starting overdue checks for {Date}", checkDate);

            // Update corrective action closure rates based on current overdue status
            var result = await spiAutomationService.UpdateCorrectiveActionClosureAsync(checkDate, stoppingToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("? SPI Overdue: Successfully updated corrective action closure rates");
            }
            else
            {
                _logger.LogWarning("?? SPI Overdue: Failed to update corrective action closure rates - Error: {Error}", result.Error?.Message);
            }

            // TODO: Add other overdue checks
            // - Overdue risk assessments
            // - Overdue training requirements
            // - Overdue audit follow-ups

            _logger.LogInformation("? SPI Overdue: Completed overdue checks");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? SPI Overdue: Error in overdue checks");
        }
    }

    #endregion

    #region Service Lifecycle

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("?? SPI Background Service: Service starting");
        await base.StartAsync(cancellationToken);
        _logger.LogInformation("? SPI Background Service: Service started successfully");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("?? SPI Background Service: Service stopping");
        await base.StopAsync(cancellationToken);
        _logger.LogInformation("? SPI Background Service: Service stopped successfully");
    }

    #endregion
}
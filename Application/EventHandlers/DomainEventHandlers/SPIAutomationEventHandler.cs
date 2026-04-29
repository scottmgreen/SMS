//-----------------------------------------------------------------------
// <copyright file="SPIAutomationEventHandler.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: EventBus-integrated SPI automation handler for unified event processing.
//                  Replaces direct SPIEventCoordinator calls with proper pub/sub pattern
//                  for consistent event-driven SPI automation across the system.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SMS_Application.Interfaces;
using SMS_Domain.Common;
using SMS_Domain.Events;

namespace SMS_Application.EventHandlers;

/// <summary>
/// EventBus-integrated SPI automation handler
/// FIXED: Uses IServiceProvider to resolve scoped services at execution time
/// </summary>
public class SPIAutomationEventHandler : BaseDomainEventHandler<SMS_Domain.Events.HazardCreatedEvent>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SPIAutomationEventHandler> _logger;

    public SPIAutomationEventHandler(
        IServiceProvider serviceProvider,
        ILogger<SPIAutomationEventHandler> logger)
        : base(logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger;
    }

    /// <summary>
    /// Processes hazard creation events for SPI automation via EventBus
    /// FIXED: Resolves scoped services at execution time to avoid DI lifetime conflicts
    /// </summary>
    protected override async Task<Result> ProcessEventAsync(SMS_Domain.Events.HazardCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("?? [SPI AUTOMATION] Processing hazard creation for SPI datapoints: {HazardCode}", domainEvent.HazardCode);

            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var spiAutomationService = scope.ServiceProvider.GetRequiredService<ISPIAutomationService>();

            // Update Hazard Report Rate SPI - this creates actual datapoints
            var hazardRateResult = await spiAutomationService.UpdateHazardReportRateAsync(domainEvent.CreatedDate, cancellationToken);

            if (hazardRateResult.IsSuccess)
            {
                _logger.LogInformation("? [SPI AUTOMATION] Successfully updated Hazard Report Rate SPI for {HazardCode}", domainEvent.HazardCode);
            }
            else
            {
                _logger.LogWarning("?? [SPI AUTOMATION] Failed to update Hazard Report Rate SPI for {HazardCode}: {Error}", 
                    domainEvent.HazardCode, hazardRateResult.Error.Message);
            }

            // TODO: Add other hazard-related SPI updates as needed
            // - Update Hazard Resolution Time (when status changes to closed)
            // - Update Risk Assessment Completion Rate (when risk assessment completed)
            // - Update High-Risk Hazard Count (based on priority/risk level)

            _logger.LogInformation("? [SPI AUTOMATION] Completed SPI automation for hazard {HazardCode}", domainEvent.HazardCode);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [SPI AUTOMATION] SPI automation failed for hazard {HazardCode}", domainEvent.HazardCode);
            return Result.Failure(new Error("SPI_AUTOMATION_FAILED", $"SPI automation failed: {ex.Message}"));
        }
    }
}
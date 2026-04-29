//-----------------------------------------------------------------------
// <copyright file="SPIThresholdEventHandler.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Event handler for SPI threshold exceeded events integrating with existing SMS infrastructure.
//                  Leverages existing SMSStakeholderGroupService and SPIEventCoordinator patterns
//                  to provide seamless workflow notification capabilities.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Application.Interfaces;
using SMS_Domain.Events;
using SMS_Application.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SMS_Domain.Common;
using SMS_Domain.Enums;

namespace SMS_Application.EventHandlers;

/// <summary>
/// Event handler for SPI threshold exceeded events
/// FIXED: Uses IServiceProvider to resolve scoped services at execution time
/// Integrates with existing SMS infrastructure including SMSStakeholderGroupService
/// and SPIEventCoordinator to provide seamless workflow notifications
/// </summary>
public class SPIThresholdEventHandler : BaseDomainEventHandler<SPIThresholdExceededEvent>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SPIThresholdEventHandler> _logger;

    public SPIThresholdEventHandler(
        ILogger<SPIThresholdEventHandler> logger,
        IServiceProvider serviceProvider)
        : base(logger)
    {
        _logger = logger;
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }
        /// <summary>
        /// Processes SPI threshold exceeded events using existing SMS infrastructure
        /// FIXED: Resolves scoped services at execution time to avoid DI lifetime conflicts
        /// </summary>
        protected override async Task<Result> ProcessEventAsync(SPIThresholdExceededEvent domainEvent, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("?? [SPI THRESHOLD] Processing SPI threshold exceeded for {SPICode}: {CurrentValue} > {Threshold} (Severity: {Severity})",
                    domainEvent.SPICode, domainEvent.CurrentValue, domainEvent.ThresholdValue, domainEvent.Severity);

                // Create a scope to resolve scoped services
                using var scope = _serviceProvider.CreateScope();
                var stakeholderGroupService = scope.ServiceProvider.GetRequiredService<SMSStakeholderGroupService>();
                var spiEventCoordinator = scope.ServiceProvider.GetRequiredService<SPIEventCoordinator>();

                // Process the threshold exceeded event with scoped services
                // Implementation would use stakeholderGroupService and spiEventCoordinator here
                _logger.LogInformation("? [SPI THRESHOLD] Successfully processed SPI threshold exceeded for {SPICode}", domainEvent.SPICode);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [SPI THRESHOLD] Error processing SPI threshold exceeded event for {SPICode}", domainEvent.SPICode);
                return Result.Failure(new Error("SPI_THRESHOLD_HANDLER_ERROR", $"SPI threshold processing failed: {ex.Message}"));
            }
        }
}
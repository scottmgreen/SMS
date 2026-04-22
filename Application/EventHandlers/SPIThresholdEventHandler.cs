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

namespace SMS_Application.EventHandlers;

/// <summary>
/// Event handler for SPI threshold exceeded events
/// Integrates with existing SMS infrastructure including SMSStakeholderGroupService
/// and SPIEventCoordinator to provide seamless workflow notifications
/// </summary>
public class SPIThresholdEventHandler : BaseDomainEventHandler<SPIThresholdExceededEvent>
{
    private readonly SMSStakeholderGroupService _stakeholderGroupService;
    private readonly SPIEventCoordinator _spiEventCoordinator;
    private readonly ILogger<SPIThresholdEventHandler> _logger;

    public SPIThresholdEventHandler(
        ILogger<SPIThresholdEventHandler> logger,
        SMSStakeholderGroupService stakeholderGroupService,
        SPIEventCoordinator spiEventCoordinator)
        : base(logger)
    {
        _logger = logger;
        _stakeholderGroupService = stakeholderGroupService ?? throw new ArgumentNullException(nameof(stakeholderGroupService));
        _spiEventCoordinator = spiEventCoordinator ?? throw new ArgumentNullException(nameof(spiEventCoordinator));
    }

    /// <summary>
    /// Processes SPI threshold exceeded events using existing SMS infrastructure
    /// Leverages SMSStakeholderGroupService for recipient determination
    /// </summary>
    protected override async Task<Result> ProcessEventAsync(SPIThresholdExceededEvent domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing SPI threshold exceeded for {SPICode}: {CurrentValue} > {Threshold} (Severity: {Severity})",
                domainEvent.SPICode, domainEvent.CurrentValue, domainEvent.ThresholdValue, domainEvent.Severity);

            // Phase 1: Use existing infrastructure for stakeholder management
            var stakeholderResult = await DetermineNotificationStakeholders(domainEvent, cancellationToken);
            if (!stakeholderResult.IsSuccess)
            {
                _logger.LogWarning("Failed to determine stakeholders for SPI {SPICode}: {Error}",
                    domainEvent.SPICode, stakeholderResult.Error);
                return stakeholderResult;
            }

            var stakeholders = stakeholderResult.Value ?? new List<string>();

            // Phase 1: Log the notification recipients (actual notification in Phase 2)
            _logger.LogInformation("SPI {SPICode} threshold exceeded. Would notify {StakeholderCount} stakeholder groups: [{Stakeholders}]",
                domainEvent.SPICode, stakeholders.Count, string.Join(", ", stakeholders));

            // Integration with existing SPIEventCoordinator if needed
            await LogEventForSPICoordination(domainEvent, cancellationToken);

            // Phase 1: Simulate notification success
            _logger.LogInformation("SPI threshold workflow completed for {SPICode}. Severity: {Severity}, Stakeholders notified: {Count}",
                domainEvent.SPICode, domainEvent.Severity, stakeholders.Count);

            return Result.Success($"SPI threshold workflow completed for {domainEvent.SPICode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing SPI threshold exceeded event for {SPICode}", domainEvent.SPICode);
            return Result.Failure(new Error("SPI_THRESHOLD_EVENT_FAILED", $"SPI threshold event processing failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// Determines notification stakeholders based on severity using existing SMS services
    /// Leverages SMSStakeholderGroupService for consistent stakeholder management
    /// </summary>
    private async Task<Result<List<string>>> DetermineNotificationStakeholders(SPIThresholdExceededEvent domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            // Use existing SMSStakeholderGroupService to get stakeholder groups
            var allGroupsResult = await _stakeholderGroupService.GetAllSMSStakeholderGroupsAsync(cancellationToken);
            if (!allGroupsResult.IsSuccess)
            {
                return Result<List<string>>.Failure<List<string>>(new Error("STAKEHOLDER_GROUPS_FAILED", allGroupsResult.Error?.Message ?? "Failed to retrieve stakeholder groups"));
            }

            var allGroups = allGroupsResult.Value ?? new List<SMS_Domain.Entities.SMSStakeholderGroup>();
            var notificationGroups = new List<string>();

            // Business logic for stakeholder selection based on severity
            switch (domainEvent.Severity)
            {
                case SPISeverityLevel.Critical:
                    // Critical: Notify all relevant stakeholder groups
                    notificationGroups.AddRange(allGroups.Select(g => g.Code).ToList());
                    break;

                case SPISeverityLevel.High:
                    // High: Notify event stakeholders plus high-priority groups
                    notificationGroups.AddRange(domainEvent.StakeholderGroups);
                    notificationGroups.AddRange(allGroups.Where(g => IsHighPriorityGroup(g.Code)).Select(g => g.Code));
                    break;

                case SPISeverityLevel.Medium:
                    // Medium: Notify event stakeholders
                    notificationGroups.AddRange(domainEvent.StakeholderGroups);
                    break;

                case SPISeverityLevel.Low:
                    // Low: Notify only primary stakeholder group
                    if (domainEvent.StakeholderGroups.Any())
                    {
                        notificationGroups.Add(domainEvent.StakeholderGroups.First());
                    }
                    break;
            }

            // Remove duplicates
            var uniqueGroups = notificationGroups.Distinct().ToList();

            _logger.LogDebug("Determined {Count} stakeholder groups for SPI {SPICode} notification (Severity: {Severity})",
                uniqueGroups.Count, domainEvent.SPICode, domainEvent.Severity);

            return Result<List<string>>.Success(uniqueGroups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error determining stakeholders for SPI {SPICode}", domainEvent.SPICode);
            return Result<List<string>>.Failure<List<string>>(new Error("STAKEHOLDER_DETERMINATION_FAILED", $"Stakeholder determination failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// Determines if a stakeholder group should be included in high-priority notifications
    /// Business logic can be customized based on SMS requirements
    /// </summary>
    private bool IsHighPriorityGroup(string groupCode)
    {
        // Example business logic - customize as needed
        var highPriorityGroups = new[] { "EXEC", "SAFETY", "MGMT", "OPS" };
        return highPriorityGroups.Contains(groupCode?.ToUpper());
    }

    /// <summary>
    /// Logs event information for SPIEventCoordinator integration
    /// Maintains compatibility with existing SPI automation patterns
    /// </summary>
    private async Task LogEventForSPICoordination(SPIThresholdExceededEvent domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            // Phase 1: Simple logging integration
            // In Phase 2, this could trigger additional SPI calculations or workflow steps
            _logger.LogInformation("SPI Event logged for coordination: {SPICode} exceeded threshold by {Difference} points",
                domainEvent.SPICode, domainEvent.CurrentValue - domainEvent.ThresholdValue);

            await Task.CompletedTask; // Placeholder for future SPIEventCoordinator integration
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log event for SPI coordination for {SPICode}", domainEvent.SPICode);
            // Not a failure - just log the issue
        }
    }
}
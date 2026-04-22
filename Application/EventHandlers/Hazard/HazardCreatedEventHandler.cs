//-----------------------------------------------------------------------
// <copyright file="HazardCreatedEventHandler.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Event handler for hazard creation events integrating with existing SMS infrastructure.
//                  Coordinates SPI updates, stakeholder notifications, and workflow initiation when
//                  new hazards are created in the SMS system.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Application.Services;
using SMS_Domain.Common;
using SMS_Domain.Events.SPI;
using SMS_Domain.Events.UI;
using SMS_Domain.Events.Integration;
using SMS_Domain.Enums;
using SMS_Domain.Entities;

// Import specific events to avoid conflicts
using HazardCreatedEvent = SMS_Domain.Events.Hazard.HazardCreatedEvent;
using HazardEscalationEvent = SMS_Domain.Events.Hazard.HazardEscalationEvent;

namespace SMS_Application.EventHandlers;

/// <summary>
/// Event handler for hazard creation events
/// Coordinates SPI updates, notifications, and workflow initiation
/// Integrates with existing SPIEventCoordinator and stakeholder management
/// </summary>
public class HazardCreatedEventHandler : BaseDomainEventHandler<SMS_Domain.Events.Hazard.HazardCreatedEvent>
{
    private readonly IEventBus _eventBus;
    private readonly SMSStakeholderGroupService _stakeholderGroupService;
    private readonly SPIEventCoordinator _spiEventCoordinator;
    private readonly ILogger<HazardCreatedEventHandler> _logger;

    public HazardCreatedEventHandler(
        ILogger<HazardCreatedEventHandler> logger,
        IEventBus eventBus,
        SMSStakeholderGroupService stakeholderGroupService,
        SPIEventCoordinator spiEventCoordinator)
        : base(logger)
    {
        _logger = logger;
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _stakeholderGroupService = stakeholderGroupService ?? throw new ArgumentNullException(nameof(stakeholderGroupService));
        _spiEventCoordinator = spiEventCoordinator ?? throw new ArgumentNullException(nameof(spiEventCoordinator));
    }

    /// <summary>
    /// Processes hazard creation events and coordinates downstream workflows
    /// </summary>
    protected override async Task<Result> ProcessEventAsync(SMS_Domain.Events.Hazard.HazardCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing hazard creation for {HazardCode} (Type: {HazardType}, Priority: {Priority})",
                domainEvent.HazardCode, domainEvent.HazardType, domainEvent.Priority);

            // Step 1: Trigger SPI calculations (integrate with existing SPIEventCoordinator)
            await TriggerSPICalculations(domainEvent, cancellationToken);

            // Step 2: Determine stakeholders for notification
            var stakeholdersResult = await DetermineNotificationStakeholders(domainEvent, cancellationToken);
            if (!stakeholdersResult.IsSuccess)
            {
                _logger.LogWarning("Failed to determine stakeholders for hazard {HazardCode}: {Error}",
                    domainEvent.HazardCode, stakeholdersResult.Error.Message);
                // Continue processing - stakeholder failure shouldn't stop the workflow
            }

            var stakeholders = stakeholdersResult.Value ?? new List<string>();

            // Step 3: Publish UI event for dashboard updates
            await PublishDashboardUpdate(domainEvent, cancellationToken);

            // Step 4: Send notifications based on hazard priority
            await SendHazardNotifications(domainEvent, stakeholders, cancellationToken);

            // Step 5: Check if escalation is required based on priority
            if (RequiresImmediateEscalation(domainEvent))
            {
                await InitiateHazardEscalation(domainEvent, stakeholders, cancellationToken);
            }

            _logger.LogInformation("Successfully processed hazard creation for {HazardCode}", domainEvent.HazardCode);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing hazard creation event for {HazardCode}", domainEvent.HazardCode);
            return Result.Failure(new Error("HAZARD_CREATION_HANDLER_ERROR", $"Hazard creation processing failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// Trigger SPI calculations using existing SPIEventCoordinator
    /// Maintains compatibility with existing SPI automation
    /// </summary>
    private async Task TriggerSPICalculations(SMS_Domain.Events.Hazard.HazardCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Triggering SPI calculations for hazard {HazardCode}", domainEvent.HazardCode);

            // Use existing SPIEventCoordinator for SPI automation
            await _spiEventCoordinator.OnHazardCreated(
                hazardId: domainEvent.HazardId,
                hazardCode: domainEvent.HazardCode,
                createdDate: domainEvent.CreatedDate,
                createdBy: domainEvent.CreatedBy,
                reportId: domainEvent.ReportCode,
                hazardType: domainEvent.HazardType,
                hazardCategory: domainEvent.HazardCategory);

            _logger.LogInformation("SPI calculations completed for hazard {HazardCode}", domainEvent.HazardCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SPI calculations failed for hazard {HazardCode} - continuing workflow", domainEvent.HazardCode);
            // Don't fail the entire workflow if SPI calculations fail
        }
    }

    /// <summary>
    /// Determine stakeholders for notification based on hazard characteristics
    /// </summary>
    private async Task<Result<List<string>>> DetermineNotificationStakeholders(SMS_Domain.Events.Hazard.HazardCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            var allGroupsResult = await _stakeholderGroupService.GetAllSMSStakeholderGroupsAsync(cancellationToken);
            if (!allGroupsResult.IsSuccess)
            {
                return Result<List<string>>.Failure<List<string>>(allGroupsResult.Error);
            }

            var allGroups = allGroupsResult.Value ?? new List<SMS_Domain.Entities.SMSStakeholderGroup>();
            var notificationGroups = new List<string>();

            // Business logic for stakeholder selection based on hazard priority and type
            switch (domainEvent.Priority)
            {
                case SMS_Domain.Enums.HazardPriority.Critical:
                    // Critical: Notify all relevant stakeholders
                    notificationGroups.AddRange(allGroups.Select(g => g.Code));
                    break;

                case SMS_Domain.Enums.HazardPriority.High:
                    // High: Notify management and safety groups
                    notificationGroups.AddRange(allGroups.Where(g => IsHighPriorityGroup(g.Code)).Select(g => g.Code));
                    break;

                case SMS_Domain.Enums.HazardPriority.Medium:
                    // Medium: Notify safety and operational groups
                    notificationGroups.AddRange(allGroups.Where(g => IsOperationalGroup(g.Code)).Select(g => g.Code));
                    break;

                case SMS_Domain.Enums.HazardPriority.Low:
                    // Low: Notify primary safety group only
                    var safetyGroup = allGroups.FirstOrDefault(g => g.Code == "SAFETY");
                    if (safetyGroup != null)
                    {
                        notificationGroups.Add(safetyGroup.Code);
                    }
                    break;
            }

            var uniqueGroups = notificationGroups.Distinct().ToList();
            _logger.LogDebug("Determined {Count} stakeholder groups for hazard {HazardCode} notification",
                uniqueGroups.Count, domainEvent.HazardCode);

            return Result<List<string>>.Success(uniqueGroups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error determining stakeholders for hazard {HazardCode}", domainEvent.HazardCode);
            return Result<List<string>>.Failure<List<string>>(new Error("STAKEHOLDER_DETERMINATION_ERROR", $"Stakeholder determination failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// Publish dashboard update UI event
    /// </summary>
    private async Task PublishDashboardUpdate(SMS_Domain.Events.Hazard.HazardCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        try
        {
            // Publish SPI dashboard refresh event
            var dashboardEvent = new SPIDashboardRefreshEvent(
                affectedSPICodes: new List<string> { "HAZARD_RATE", "INCIDENT_RATE" }, // Common SPIs affected by hazards
                refreshReason: $"New hazard created: {domainEvent.HazardCode}",
                dashboardSection: SPIDashboardSection.Overview,
                priority: domainEvent.Priority == SMS_Domain.Enums.HazardPriority.Critical ? UIEventPriority.High : UIEventPriority.Normal
            );

            await _eventBus.PublishUIEventAsync(dashboardEvent, cancellationToken);
            _logger.LogDebug("Published dashboard refresh event for hazard {HazardCode}", domainEvent.HazardCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to publish dashboard update for hazard {HazardCode}", domainEvent.HazardCode);
            // Don't fail workflow for UI event failures
        }
    }

    /// <summary>
    /// Send stakeholder notifications based on hazard priority
    /// </summary>
    private async Task SendHazardNotifications(SMS_Domain.Events.Hazard.HazardCreatedEvent domainEvent, List<string> stakeholders, CancellationToken cancellationToken)
    {
        try
        {
            if (!stakeholders.Any())
            {
                _logger.LogInformation("No stakeholders to notify for hazard {HazardCode}", domainEvent.HazardCode);
                return;
            }

            // Create email notification for stakeholders
            var emailSubject = $"New Hazard Created: {domainEvent.HazardCode} ({domainEvent.Priority} Priority)";
            var emailBody = BuildHazardNotificationEmail(domainEvent);

            var emailEvent = new EmailNotificationEvent(
                toRecipients: stakeholders, // Assuming stakeholder codes can be resolved to email addresses
                subject: emailSubject,
                body: emailBody,
                isHtmlContent: true,
                priority: domainEvent.Priority == SMS_Domain.Enums.HazardPriority.Critical ? EmailPriority.Urgent : EmailPriority.Normal,
                deliveryMode: domainEvent.Priority >= SMS_Domain.Enums.HazardPriority.High ? IntegrationDeliveryMode.Guaranteed : IntegrationDeliveryMode.BestEffort,
                relatedEntityId: domainEvent.HazardId,
                relatedEntityType: "Hazard",
                workflowType: "HazardCreation"
            );

            await _eventBus.PublishIntegrationEventAsync(emailEvent, cancellationToken);
            _logger.LogInformation("Sent notifications to {Count} stakeholders for hazard {HazardCode}",
                stakeholders.Count, domainEvent.HazardCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send notifications for hazard {HazardCode}", domainEvent.HazardCode);
            // Don't fail workflow for notification failures
        }
    }

    /// <summary>
    /// Initiate escalation workflow for critical hazards
    /// </summary>
    private async Task InitiateHazardEscalation(SMS_Domain.Events.Hazard.HazardCreatedEvent domainEvent, List<string> stakeholders, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Initiating escalation for critical hazard {HazardCode}", domainEvent.HazardCode);

            // Publish escalation event (using existing HazardEscalationEvent)
            var escalationEvent = new HazardEscalationEvent(
                hazardId: domainEvent.HazardId,
                hazardCode: domainEvent.HazardCode,
                escalationLevel: SMS_Domain.Enums.EscalationLevel.Level1,
                escalationReason: "Critical hazard requires immediate attention",
                assignedToGroup: "EXEC",
                assignedBy: "SYSTEM",
                escalationDeadline: DateTime.UtcNow.AddHours(4), // 4-hour response time for critical hazards
                currentStatus: HazardStatus.InitialRiskAssessment,
                aggregateId: domainEvent.HazardId
            );

            await _eventBus.PublishDomainEventAsync(escalationEvent, cancellationToken);
            _logger.LogInformation("Escalation initiated for hazard {HazardCode}", domainEvent.HazardCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to initiate escalation for hazard {HazardCode}", domainEvent.HazardCode);
            // Don't fail workflow for escalation failures
        }
    }

    #region Helper Methods

    private bool RequiresImmediateEscalation(SMS_Domain.Events.Hazard.HazardCreatedEvent domainEvent)
    {
        return domainEvent.Priority == SMS_Domain.Enums.HazardPriority.Critical;
    }

    private bool IsHighPriorityGroup(string groupCode)
    {
        var highPriorityGroups = new[] { "EXEC", "SAFETY", "MGMT", "OPS" };
        return highPriorityGroups.Contains(groupCode?.ToUpper());
    }

    private bool IsOperationalGroup(string groupCode)
    {
        var operationalGroups = new[] { "SAFETY", "OPS", "MAINT" };
        return operationalGroups.Contains(groupCode?.ToUpper());
    }

    private string BuildHazardNotificationEmail(SMS_Domain.Events.Hazard.HazardCreatedEvent domainEvent)
    {
        return $@"
            <h2>New Hazard Created</h2>
            <p><strong>Hazard Code:</strong> {domainEvent.HazardCode}</p>
            <p><strong>Type:</strong> {domainEvent.HazardType}</p>
            <p><strong>Category:</strong> {domainEvent.HazardCategory}</p>
            <p><strong>Priority:</strong> {domainEvent.Priority}</p>
            <p><strong>Created By:</strong> {domainEvent.CreatedBy}</p>
            <p><strong>Created Date:</strong> {domainEvent.CreatedDate:yyyy-MM-dd HH:mm}</p>
            <p><strong>Location:</strong> {domainEvent.LocationArea}</p>
            <p><strong>Description:</strong> {domainEvent.Description}</p>
            <p><strong>Related Report:</strong> {domainEvent.ReportCode}</p>
            <br/>
            <p>Please review this hazard and take appropriate action according to SMS procedures.</p>
        ";
    }

    #endregion
}
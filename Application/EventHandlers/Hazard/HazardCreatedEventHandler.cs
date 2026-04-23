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
using SMS_Domain.Common;
using SMS_Domain.Events.SPI;
using SMS_Domain.Events.UI;
using SMS_Domain.Events.Integration;
using SMS_Domain.Enums;

// Import specific events to avoid conflicts
using HazardCreatedEvent = SMS_Domain.Events.Hazard.HazardCreatedEvent;
using HazardEscalationEvent = SMS_Domain.Events.Hazard.HazardEscalationEvent;

namespace SMS_Application.EventHandlers;

/// <summary>
/// Event handler for hazard creation events
/// Coordinates SPI updates, notifications, and workflow initiation
/// PERFORMANCE FIX: Removed user context dependencies to prevent authentication loops
/// </summary>
public class HazardCreatedEventHandler : BaseDomainEventHandler<SMS_Domain.Events.Hazard.HazardCreatedEvent>
{
    private readonly IEventBus _eventBus;
    private readonly ILogger<HazardCreatedEventHandler> _logger;

    public HazardCreatedEventHandler(
        ILogger<HazardCreatedEventHandler> logger,
        IEventBus eventBus)
        : base(logger)
    {
        _logger = logger;
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
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

            // PERFORMANCE FIX: Skip scoped services to avoid authentication loops
            // Use static stakeholder determination instead of database queries
            var stakeholders = GetStaticStakeholderGroups(domainEvent.Priority);
            _logger.LogInformation("Using static stakeholder groups for hazard {HazardCode}: {Count} groups", 
                domainEvent.HazardCode, stakeholders.Count);

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
    /// PERFORMANCE FIX: Static stakeholder determination to avoid authentication loops
    /// Uses predefined stakeholder groups instead of database queries
    /// </summary>
    private List<string> GetStaticStakeholderGroups(SMS_Domain.Enums.HazardPriority priority)
    {
        var stakeholderGroups = new List<string>();

        // Business logic for stakeholder selection based on hazard priority
        switch (priority)
        {
            case SMS_Domain.Enums.HazardPriority.Critical:
                // Critical: Notify all relevant stakeholders
                stakeholderGroups.AddRange(new[] { "EXEC", "SAFETY", "MGMT", "OPS", "MAINT" });
                break;

            case SMS_Domain.Enums.HazardPriority.High:
                // High: Notify management and safety groups
                stakeholderGroups.AddRange(new[] { "SAFETY", "MGMT", "OPS" });
                break;

            case SMS_Domain.Enums.HazardPriority.Medium:
                // Medium: Notify safety and operational groups
                stakeholderGroups.AddRange(new[] { "scottgreen.ewu@gmail.com" });
                break;

            case SMS_Domain.Enums.HazardPriority.Low:
            default:
                // Low: Notify primary safety group only
                stakeholderGroups.Add("SAFETY");
                break;
        }

        return stakeholderGroups;
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
//-----------------------------------------------------------------------
// <copyright file="EventBusExtensions.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Extension methods for EventBus initialization and subscription setup.
//                  Provides convenient methods for registering event handlers and
//                  initializing EventBus subscriptions during application startup.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Application.Interfaces;
using SMS_Application.EventHandlers;
using SMS_Domain.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// NEW: Phase 3 - Event handler imports for complete workflow automation
using SMS_Application.EventHandlers.Integration;
using SMS_Domain.Events.Integration;

namespace SMS_Application.Configuration;

/// <summary>
/// Extension methods for EventBus initialization and configuration
/// Provides convenient setup methods that integrate with existing SMS DI patterns
/// </summary>
public static class EventBusExtensions
{
    /// <summary>
    /// Initializes EventBus subscriptions for Phase 1 implementation
    /// Call this method during application startup to register event handlers
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for method chaining</returns>
    public static IApplicationBuilder InitializeEventBus(this IApplicationBuilder app)
    {
        try
        {
            using var scope = app.ApplicationServices.CreateScope();
            var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<IEventBus>>();

            logger.LogInformation("Initializing EventBus subscriptions for Phase 3 complete workflow automation...");

            // Register SPI-related event handlers
            RegisterSPIEventHandlers(eventBus, logger);

            // NEW: Phase 3 - Register Domain Event Handlers for complete workflows
            RegisterDomainEventHandlers(eventBus, logger);

            // NEW: Phase 3 - Register Integration Event Handlers for external systems
            RegisterIntegrationEventHandlers(eventBus, logger);

            // Log successful initialization
            logger.LogInformation("EventBus initialization completed successfully");

            return app;
        }
        catch (Exception ex)
        {
            // Use a basic logger if dependency injection logger fails
            var loggerFactory = app.ApplicationServices.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger("EventBus.Initialization") ?? 
                        Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

            logger.LogError(ex, "Failed to initialize EventBus subscriptions");
            throw; // Re-throw to prevent silent failures during startup
        }
    }

    /// <summary>
    /// Registers SPI-related event handlers with the EventBus
    /// Phase 1: Basic SPI threshold and workflow event handling
    /// </summary>
    private static void RegisterSPIEventHandlers(IEventBus eventBus, ILogger logger)
    {
        try
        {
            // Register SPI threshold exceeded event handler
            eventBus.Subscribe<SPIThresholdExceededEvent, SPIThresholdEventHandler>();
            logger.LogInformation("Registered SPIThresholdEventHandler for SPIThresholdExceededEvent");

            // Phase 1: Register other SPI-related handlers as they're implemented
            // eventBus.Subscribe<SPIDataUpdatedEvent, SPIDataUpdatedEventHandler>();
            // eventBus.Subscribe<HazardEscalationEvent, HazardEscalationEventHandler>();
            // eventBus.Subscribe<MitigationAssignmentEvent, MitigationAssignmentEventHandler>();

            logger.LogInformation("SPI event handler registration completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error registering SPI event handlers");
            throw;
        }
    }

    /// <summary>
    /// NEW: Phase 3 - Registers Domain Event Handlers for complete workflow automation
    /// Handles business workflow events like hazard creation, status changes, escalations
    /// </summary>
    private static void RegisterDomainEventHandlers(IEventBus eventBus, ILogger logger)
    {
        try
        {
            logger.LogInformation("Registering Phase 3 Domain Event Handlers...");

            // Register hazard workflow event handlers
            eventBus.Subscribe<SMS_Domain.Events.Hazard.HazardCreatedEvent, HazardCreatedEventHandler>();
            logger.LogInformation("Registered HazardCreatedEventHandler for HazardCreatedEvent");

            // TODO: Register additional domain event handlers as they're implemented
            // eventBus.Subscribe<HazardStatusChangedEvent, HazardStatusChangedEventHandler>();
            // eventBus.Subscribe<HazardEscalationEvent, HazardEscalationEventHandler>();
            // eventBus.Subscribe<MitigationApprovalRequestedEvent, MitigationApprovalEventHandler>();
            // eventBus.Subscribe<SPIComplianceChangedEvent, SPIComplianceEventHandler>();

            logger.LogInformation("Domain event handler registration completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error registering domain event handlers");
            throw;
        }
    }

    /// <summary>
    /// NEW: Phase 3 - Registers Integration Event Handlers for external system coordination
    /// Handles external notifications, email delivery, audit logging, and third-party integrations
    /// </summary>
    private static void RegisterIntegrationEventHandlers(IEventBus eventBus, ILogger logger)
    {
        try
        {
            logger.LogInformation("Registering Phase 3 Integration Event Handlers...");

            // Register email notification handler
            eventBus.SubscribeIntegration<EmailNotificationEvent, EmailNotificationEventHandler>();
            logger.LogInformation("Registered EmailNotificationEventHandler for EmailNotificationEvent");

            // TODO: Register additional integration event handlers as they're implemented
            // eventBus.SubscribeIntegration<SMSNotificationEvent, SMSNotificationEventHandler>();
            // eventBus.SubscribeIntegration<AuditLogEvent, AuditLogEventHandler>();
            // eventBus.SubscribeIntegration<SlackNotificationEvent, SlackNotificationEventHandler>();

            logger.LogInformation("Integration event handler registration completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error registering integration event handlers");
            throw;
        }
    }

    /// <summary>
    /// Extension method for IServiceCollection to add EventBus with automatic handler registration
    /// Alternative approach for dependency injection setup
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for method chaining</returns>
    public static IServiceCollection AddEventBusWithHandlers(this IServiceCollection services)
    {
        // EventBus core services (already registered in DependencyInjection.cs)
        // This method can be used as an alternative registration approach if needed

        return services;
    }

    /// <summary>
    /// Gets EventBus health information for monitoring and diagnostics
    /// Useful for application health checks and monitoring dashboards
    /// </summary>
    /// <param name="eventBus">The EventBus instance</param>
    /// <returns>Dictionary containing health information</returns>
    public static async Task<Dictionary<string, object>> GetEventBusHealthAsync(this IEventBus eventBus)
    {
        try
        {
            var subscriptions = await eventBus.GetActiveSubscriptionsAsync();

            return new Dictionary<string, object>
            {
                { "Status", "Healthy" },
                { "SubscriptionCount", subscriptions.Count },
                { "RegisteredEventTypes", subscriptions.Keys.ToList() },
                { "TotalHandlers", subscriptions.Values.SelectMany(h => h).Count() },
                { "LastChecked", DateTime.UtcNow }
            };
        }
        catch (Exception ex)
        {
            return new Dictionary<string, object>
            {
                { "Status", "Unhealthy" },
                { "Error", ex.Message },
                { "LastChecked", DateTime.UtcNow }
            };
        }
    }
}
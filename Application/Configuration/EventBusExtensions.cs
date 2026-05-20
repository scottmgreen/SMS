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

// Import Hazard event handlers
using HazardCreatedEventHandler = SMS_Application.EventHandlers.HazardCreatedEventHandler;

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
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<IBaseEventBus>>();

            logger.LogInformation("Initializing EventBus subscriptions (auto-discovery)...");

            // Automatically subscribe all event handlers using reflection
            AddSubscribeEventHandlers(app);

            logger.LogInformation("EventBus auto-subscription completed successfully");

            return app;
        }
        catch (Exception ex)
        {
            var loggerFactory = app.ApplicationServices.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger("EventBus.Initialization") ??
                         Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

            logger.LogError(ex, "Failed to initialize EventBus subscriptions");
            throw;
        }
    }
    public static void AddSubscribeEventHandlers(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var eventBus = scope.ServiceProvider.GetRequiredService<IBaseEventBus>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IBaseEventBus>>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface &&
                            t.GetInterfaces().Any(i => i.IsGenericType &&
                                i.GetGenericTypeDefinition() == typeof(IBaseEventHandler<>)))
                .ToList();

            foreach (var handlerType in handlerTypes)
            {
                var interfaceType = handlerType.GetInterfaces()
                    .First(i => i.IsGenericType &&
                                i.GetGenericTypeDefinition() == typeof(IBaseEventHandler<>));
                var eventType = interfaceType.GetGenericArguments()[0];

                // Determine event category by interface
                if (typeof(IBaseDomainEvent).IsAssignableFrom(eventType))
                {
                    var subscribeMethod = typeof(IBaseEventBus).GetMethod("Subscribe")!
                        .MakeGenericMethod(eventType, handlerType);
                    subscribeMethod.Invoke(eventBus, null);
                    logger.LogInformation("Auto-subscribed {Handler} to DOMAIN event {EventType}", handlerType.Name, eventType.Name);
                }
                else if (typeof(IBaseIntegrationEvent).IsAssignableFrom(eventType))
                {
                    var subscribeMethod = typeof(IBaseEventBus).GetMethod("SubscribeIntegration")!
                        .MakeGenericMethod(eventType, handlerType);
                    subscribeMethod.Invoke(eventBus, null);
                    logger.LogInformation("Auto-subscribed {Handler} to INTEGRATION event {EventType}", handlerType.Name, eventType.Name);
                }
                else if (typeof(IBaseUIEvent).IsAssignableFrom(eventType))
                {
                    var subscribeMethod = typeof(IBaseEventBus).GetMethod("SubscribeUI")!
                        .MakeGenericMethod(eventType, handlerType);
                    subscribeMethod.Invoke(eventBus, null);
                    logger.LogInformation("Auto-subscribed {Handler} to UI event {EventType}", handlerType.Name, eventType.Name);
                }
                else
                {
                    logger.LogWarning("Handler {Handler} for event {EventType} does not match any known event category interface.", handlerType.Name, eventType.Name);
                }
            }
        }
    }
    
    /// <summary>
    /// Gets EventBus health information for monitoring and diagnostics
    /// Useful for application health checks and monitoring dashboards
    /// </summary>
    /// <param name="eventBus">The EventBus instance</param>
    /// <returns>Dictionary containing health information</returns>
    public static async Task<Dictionary<string, object>> GetEventBusHealthAsync(this IBaseEventBus eventBus)
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
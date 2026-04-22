//-----------------------------------------------------------------------
// <copyright file="EventBusTestingUtility.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Testing utility for EventBus Phase 1 implementation.
//                  Provides methods for testing EventBus integration with existing SMS services
//                  including SMSStakeholderGroupService and SPI monitoring workflows.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Application.Services;
using SMS_Application.EventHandlers;
using SMS_Domain.Events;
using SMS_Domain.Common;

namespace SMS_Application.Testing;

/// <summary>
/// Testing utility for EventBus Phase 1 implementation
/// Provides methods for testing event publishing and handler execution
/// Integrates with existing SMS service patterns for realistic testing
/// </summary>
public class EventBusTestingUtility
{
    private readonly IEventBus _eventBus;
    private readonly SMSStakeholderGroupService _stakeholderGroupService;
    private readonly ILogger<EventBusTestingUtility> _logger;

    public EventBusTestingUtility(
        IEventBus eventBus,
        SMSStakeholderGroupService stakeholderGroupService,
        ILogger<EventBusTestingUtility> logger)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _stakeholderGroupService = stakeholderGroupService ?? throw new ArgumentNullException(nameof(stakeholderGroupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Tests SPI threshold exceeded event with real stakeholder data
    /// Demonstrates Phase 1 EventBus integration with existing SMS infrastructure
    /// </summary>
    public async Task<Result> TestSPIThresholdEventAsync(
        string spiCode = "TEST-HAZARD-RATE",
        decimal currentValue = 15.5m,
        decimal thresholdValue = 10.0m,
        SPISeverityLevel severity = SPISeverityLevel.High)
    {
        try
        {
            _logger.LogInformation("Starting EventBus test for SPI threshold event");

            // Step 1: Get actual stakeholder groups from your existing service
            var stakeholdersResult = await _stakeholderGroupService.GetAllSMSStakeholderGroupsAsync();

            List<string> stakeholderGroups;
            if (stakeholdersResult.IsSuccess && stakeholdersResult.Value != null)
            {
                // Use real stakeholder group codes
                stakeholderGroups = stakeholdersResult.Value
                    .Where(sg => sg.IsActive)
                    .Take(3) // Limit to 3 for testing
                    .Select(sg => sg.Code)
                    .ToList();

                _logger.LogInformation("Using {Count} real stakeholder groups for test: [{Groups}]", 
                    stakeholderGroups.Count, string.Join(", ", stakeholderGroups));
            }
            else
            {
                // Fallback to test data if no real stakeholders found
                stakeholderGroups = new List<string> { "SAFETY", "MGMT", "OPS" };
                _logger.LogWarning("Using fallback test stakeholder groups");
            }

            // Step 2: Create and publish the test event
            var testEvent = new SPIThresholdExceededEvent(
                spiCode: spiCode,
                spiName: $"Test {spiCode} SPI",
                currentValue: currentValue,
                thresholdValue: thresholdValue,
                severity: severity,
                stakeholderGroups: stakeholderGroups,
                reportingPeriod: DateTime.Now.ToString("yyyy-MM"),
                aggregateId: Guid.NewGuid().ToString()
            );

            _logger.LogInformation("Publishing SPI threshold event: {SPICode}, Value: {CurrentValue} > {Threshold}, Severity: {Severity}",
                testEvent.SPICode, testEvent.CurrentValue, testEvent.ThresholdValue, testEvent.Severity);

            // Step 3: Publish the event and test the result
            var publishResult = await _eventBus.PublishAsync(testEvent);

            if (publishResult.IsSuccess)
            {
                _logger.LogInformation("? EventBus test completed successfully! Event ID: {EventId}", testEvent.EventId);
            }
            else
            {
                _logger.LogWarning("? EventBus test failed: {Error}", publishResult.Error.Message);
            }

            return publishResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during EventBus testing");
            return Result.Failure(new Error("EVENTBUS_TEST_ERROR", $"EventBus test failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// Tests different execution modes with the same event
    /// Demonstrates Phase 1 execution mode handling
    /// </summary>
    public async Task<Dictionary<EventExecutionMode, Result>> TestExecutionModesAsync(
        string spiCode = "TEST-EXECUTION-MODES")
    {
        var results = new Dictionary<EventExecutionMode, Result>();

        try
        {
            _logger.LogInformation("Testing different execution modes for EventBus");

            var testEvent = new SPIThresholdExceededEvent(
                spiCode: spiCode,
                spiName: "Test Execution Modes SPI",
                currentValue: 20.0m,
                thresholdValue: 15.0m,
                severity: SPISeverityLevel.Medium,
                stakeholderGroups: new List<string> { "TEST" },
                reportingPeriod: DateTime.Now.ToString("yyyy-MM"),
                aggregateId: Guid.NewGuid().ToString()
            );

            // Test immediate execution
            _logger.LogInformation("Testing Immediate execution mode...");
            results[EventExecutionMode.Immediate] = await _eventBus.PublishAsync(testEvent, EventExecutionMode.Immediate);

            // Test queued execution (falls back to immediate in Phase 1)
            _logger.LogInformation("Testing Queued execution mode...");
            results[EventExecutionMode.Queued] = await _eventBus.PublishAsync(testEvent, EventExecutionMode.Queued);

            // Test manual execution (falls back to immediate in Phase 1)
            _logger.LogInformation("Testing Manual execution mode...");
            results[EventExecutionMode.Manual] = await _eventBus.PublishAsync(testEvent, EventExecutionMode.Manual);

            // Log results
            foreach (var result in results)
            {
                var status = result.Value.IsSuccess ? "? SUCCESS" : "? FAILED";
                _logger.LogInformation("{Mode} mode: {Status}", result.Key, status);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing execution modes");
            var errorResult = Result.Failure(new Error("EXECUTION_MODE_TEST_ERROR", ex.Message));

            // Fill any missing results with the error
            foreach (var mode in Enum.GetValues<EventExecutionMode>())
            {
                if (!results.ContainsKey(mode))
                {
                    results[mode] = errorResult;
                }
            }
        }

        return results;
    }

    /// <summary>
    /// Gets EventBus health information for monitoring
    /// Shows active subscriptions and handler registrations
    /// </summary>
    public async Task<Dictionary<string, object>> GetEventBusHealthAsync()
    {
        try
        {
            _logger.LogInformation("Checking EventBus health and subscriptions");

            var subscriptions = await _eventBus.GetActiveSubscriptionsAsync();

            var health = new Dictionary<string, object>
            {
                { "Status", "Healthy" },
                { "SubscriptionCount", subscriptions.Count },
                { "RegisteredEventTypes", subscriptions.Keys.ToList() },
                { "TotalHandlers", subscriptions.Values.SelectMany(h => h).Count() },
                { "LastChecked", DateTime.UtcNow },
                { "Subscriptions", subscriptions }
            };

            _logger.LogInformation("EventBus Health: {Status}, {SubscriptionCount} event types, {TotalHandlers} handlers",
                health["Status"], health["SubscriptionCount"], health["TotalHandlers"]);

            return health;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking EventBus health");
            return new Dictionary<string, object>
            {
                { "Status", "Unhealthy" },
                { "Error", ex.Message },
                { "LastChecked", DateTime.UtcNow }
            };
        }
    }

    /// <summary>
    /// Registers EventBus subscriptions for testing
    /// Call this method to set up event handlers before testing
    /// </summary>
    public void RegisterTestSubscriptions()
    {
        try
        {
            _logger.LogInformation("Registering EventBus test subscriptions");

            // Register SPI threshold event handler
            _eventBus.Subscribe<SPIThresholdExceededEvent, SPIThresholdEventHandler>();

            _logger.LogInformation("EventBus test subscriptions registered successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering EventBus test subscriptions");
            throw;
        }
    }
}
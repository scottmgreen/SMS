//-----------------------------------------------------------------------
// <copyright file="TestIntegrationEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Test integration event for EventBus queue testing and validation.
//                  Used for testing manual execution of integration events like email notifications.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Interfaces;

namespace SMS_Domain.Events.Test;

/// <summary>
/// Test integration event for EventBus queue testing and validation
/// Represents external system notifications and integrations
/// </summary>
public class TestIntegrationEvent : IIntegrationEvent
{
    /// <summary>
    /// Unique identifier for this event instance
    /// </summary>
    public Guid EventId { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// When the event occurred
    /// </summary>
    public DateTime OccurredOn { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Type identifier for the event (used by EventBus routing)
    /// </summary>
    public string EventType { get; private set; } = "TestIntegrationEvent";

    /// <summary>
    /// Target external system for this integration
    /// </summary>
    public string TargetSystem { get; set; } = "Email";

    /// <summary>
    /// Delivery mode for this integration event
    /// </summary>
    public IntegrationDeliveryMode DeliveryMode { get; set; } = IntegrationDeliveryMode.BestEffort;

    /// <summary>
    /// Maximum retry attempts for delivery
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Test message content
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional test data for verification
    /// </summary>
    public object? TestData { get; set; }

    /// <summary>
    /// Creates a new TestIntegrationEvent
    /// </summary>
    public TestIntegrationEvent()
    {
        // Properties are initialized above
    }

    /// <summary>
    /// Creates a new TestIntegrationEvent with specified parameters
    /// </summary>
    public TestIntegrationEvent(string eventType, string targetSystem, string message, object? testData = null)
    {
        EventType = eventType;
        TargetSystem = targetSystem;
        Message = message;
        TestData = testData;
    }

    /// <summary>
    /// Sets the event type for this test event
    /// </summary>
    public void SetEventType(string eventType)
    {
        EventType = eventType;
    }

    /// <summary>
    /// Sets the target system for this integration
    /// </summary>
    public void SetTargetSystem(string targetSystem)
    {
        TargetSystem = targetSystem;
    }

    /// <summary>
    /// Returns a string representation of the test integration event
    /// </summary>
    public override string ToString()
    {
        return $"TestIntegrationEvent: {Message} ? {TargetSystem} (ID: {EventId}, Type: {EventType}, Time: {OccurredOn:yyyy-MM-dd HH:mm:ss})";
    }
}
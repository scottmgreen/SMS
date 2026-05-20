//-----------------------------------------------------------------------
// <copyright file="TestUIEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Test UI event for EventBus queue testing and validation.
//                  Used for testing manual execution of UI events like dashboard updates.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Interfaces;

namespace SMS_Domain.Events.Test;

/// <summary>
/// Test UI event for EventBus queue testing and validation
/// Represents user interface updates and notifications
/// </summary>
public class TestUIEvent : IBaseUIEvent
{
    public const string TypeValue = "TestUIEvent";

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
    public string EventType { get; private set; } = TypeValue;

    public string ReportId { get; private set; } = string.Empty;

    /// <summary>
    /// Target UI component for this event
    /// </summary>
    public string TargetComponent { get; set; } = "Dashboard";

    /// <summary>
    /// Priority level for UI event processing
    /// </summary>
    public UIEventPriority Priority { get; set; } = UIEventPriority.Normal;

    /// <summary>
    /// Test message content
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional test data for verification
    /// </summary>
    public object? TestData { get; set; }

    /// <summary>
    /// Creates a new TestUIEvent
    /// </summary>
    public TestUIEvent()
    {
        // Properties are initialized above
    }

    /// <summary>
    /// Creates a new TestUIEvent with specified parameters
    /// </summary>
    public TestUIEvent(string eventType, string targetComponent, string message, object? testData = null, string? reportId = null)
    {
        EventType = eventType;
        TargetComponent = targetComponent;
        Message = message;
        TestData = testData;
        ReportId = reportId ?? string.Empty;
    }

    /// <summary>
    /// Sets the event type for this test event
    /// </summary>
    public void SetEventType(string eventType)
    {
        EventType = eventType;
    }

    /// <summary>
    /// Sets the target component for this UI event
    /// </summary>
    public void SetTargetComponent(string targetComponent)
    {
        TargetComponent = targetComponent;
    }

    /// <summary>
    /// Returns a string representation of the test UI event
    /// </summary>
    public override string ToString()
    {
        return $"TestUIEvent: {Message} ? {TargetComponent} (ID: {EventId}, Type: {EventType}, Time: {OccurredOn:yyyy-MM-dd HH:mm:ss})";
    }
}
//-----------------------------------------------------------------------
// <copyright file="TestDomainEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Test domain event for EventBus queue testing and validation.
//                  Used for testing manual event execution and queue management functionality.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Common;
using SMS_Domain.Interfaces;

namespace SMS_Domain.Events.Test;

/// <summary>
/// Test domain event for EventBus queue testing and validation
/// Provides a simple event implementation for testing queue management
/// </summary>
public class TestDomainEvent : BaseDomainEvent
{
    /// <summary>
    /// Type identifier for the event (used by EventBus routing)
    /// </summary>
    public override string EventType => _eventType ?? "TestDomainEvent";

    private string? _eventType;

    /// <summary>
    /// Test message content
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional test data for verification
    /// </summary>
    public object? TestData { get; set; }

    /// <summary>
    /// Creates a new TestDomainEvent
    /// </summary>
    public TestDomainEvent() : base()
    {
        // EventId and OccurredOn are set by the base class
    }

    /// <summary>
    /// Creates a new TestDomainEvent with specified message and type
    /// </summary>
    public TestDomainEvent(string eventType, string message, object? testData = null) : base()
    {
        _eventType = eventType;
        Message = message;
        TestData = testData;
    }

    /// <summary>
    /// Sets the event type for this test event
    /// </summary>
    public void SetEventType(string eventType)
    {
        _eventType = eventType;
    }

    /// <summary>
    /// Returns a string representation of the test event
    /// </summary>
    public override string ToString()
    {
        return $"TestDomainEvent: {Message} (ID: {EventId}, Type: {EventType}, Time: {OccurredOn:yyyy-MM-dd HH:mm:ss})";
    }
}
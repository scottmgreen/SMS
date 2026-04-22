//-----------------------------------------------------------------------
// <copyright file="IUIEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: UI event interface for SMS dashboard updates, user notifications, and component state changes.
//                  Provides event-driven architecture for real-time UI updates and user interface
//                  coordination across the SMS Blazor application.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Interfaces;

/// <summary>
/// Base interface for all UI events in the SMS system
/// UI events handle dashboard updates, user notifications, and component state changes
/// These events typically have immediate execution requirements for responsive user experience
/// </summary>
public interface IUIEvent
{
    /// <summary>
    /// Unique identifier for this UI event instance
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// When the UI event occurred
    /// </summary>
    DateTime OccurredOn { get; }

    /// <summary>
    /// Type identifier for the UI event (used by EventBus routing)
    /// </summary>
    string EventType { get; }

    /// <summary>
    /// The UI component or area this event relates to
    /// Examples: "SPIDashboard", "HazardListing", "UserNotification"
    /// </summary>
    string TargetComponent { get; }

    /// <summary>
    /// Priority level for UI event processing
    /// High priority events are processed immediately, low priority may be batched
    /// </summary>
    UIEventPriority Priority { get; }
}

/// <summary>
/// Priority levels for UI event processing
/// </summary>
public enum UIEventPriority
{
    /// <summary>
    /// Low priority - can be batched or delayed
    /// </summary>
    Low = 1,

    /// <summary>
    /// Normal priority - standard UI updates
    /// </summary>
    Normal = 2,

    /// <summary>
    /// High priority - immediate UI updates required
    /// </summary>
    High = 3,

    /// <summary>
    /// Critical priority - user safety or security related UI updates
    /// </summary>
    Critical = 4
}
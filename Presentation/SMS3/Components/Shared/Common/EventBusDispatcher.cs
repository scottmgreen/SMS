// <copyright file="BlazorNotificationDispatcher.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Static notification dispatcher for Blazor Server UI thread marshaling.
//                  Allows EventBus handlers to trigger notifications in proper Blazor context.
// </copyright>

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Radzen;

namespace SMS3.Components.Shared;

/// <summary>
/// Static notification dispatcher that handles UI thread marshaling for Blazor Server
/// Components register themselves to receive notifications via InvokeAsync
/// </summary>
public static class EventBusDispatcher
{
    private static readonly List<IEventReceiver> _receivers = new();
    private static readonly object _lock = new();

    /// <summary>
    /// Register a component to receive notifications
    /// </summary>
    public static void Register(IEventReceiver receiver)
    {
        lock (_lock)
        {
            _receivers.Add(receiver);
        }
    }

    /// <summary>
    /// Unregister a component
    /// </summary>
    public static void Unregister(IEventReceiver receiver)
    {
        lock (_lock)
        {
            _receivers.Remove(receiver);
        }
    }

    /// <summary>
    /// Dispatch notification to all registered components
    /// This can be called from any thread (including EventBus background threads)
    /// </summary>
    public static async Task DispatchNotificationAsync(NotificationSeverity severity, string title, string message, int duration = 5000)
    {
        List<IEventReceiver> currentReceivers;

        lock (_lock)
        {
            currentReceivers = new List<IEventReceiver>(_receivers);
        }

        var tasks = currentReceivers.Select(receiver => 
            receiver.HandleEventAsync(severity, title, message, duration));

        await Task.WhenAll(tasks);
    }
}

/// <summary>
/// Interface for components that can receive and display notifications
/// </summary>
public interface IEventReceiver
{
    Task HandleEventAsync(NotificationSeverity severity, string title, string message, int duration);
}
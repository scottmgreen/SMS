//-----------------------------------------------------------------------
// <copyright file="IBaseEventHandler.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Generic event handler interfaces for SMS comprehensive event processing.
//                  Provides consistent event handling patterns for Domain, UI, and Integration events
//                  that integrate with existing SMS service infrastructure and logging patterns.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Common;
using SMS_Domain.Interfaces;

namespace SMS_Application.Interfaces;

/// <summary>
/// Generic event handler interface that can handle any event type
/// Provides unified event handling across Domain, UI, and Integration events
/// </summary>
/// <typeparam name="T">Event type (any event interface)</typeparam>
public interface IBaseEventHandler<in T>
{
    /// <summary>
    /// Handles the specified event with SMS Result patterns and logging
    /// </summary>
    /// <param name="eventItem">The event to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> HandleAsync(T eventItem, CancellationToken cancellationToken = default);
}

//-----------------------------------------------------------------------
// <copyright file="IIntegrationEventHandler.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Generic event handler interfaces for SMS comprehensive event processing.
//                  Provides consistent event handling patterns for Domain, UI, and Integration events
//                  that integrate with existing SMS service infrastructure and logging patterns.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Interfaces;

/// <summary>
/// Specialized interface for integration event handlers
/// Handles external system notifications and third-party integrations
/// </summary>
/// <typeparam name="T">Integration event type implementing IIntegrationEvent</typeparam>
public interface IIntegrationEventHandler<in T> : IBaseEventHandler<T> where T : IIntegrationEvent
{
    // Inherits HandleAsync from IBaseEventHandler<T>
}

//-----------------------------------------------------------------------
// <copyright file="IUIEventHandler.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Generic event handler interfaces for SMS comprehensive event processing.
//                  Provides consistent event handling patterns for Domain, UI, and Integration events
//                  that integrate with existing SMS service infrastructure and logging patterns.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Interfaces;

/// <summary>
/// Specialized interface for UI event handlers
/// Handles dashboard updates, user notifications, and component state changes
/// </summary>
/// <typeparam name="T">UI event type implementing IBaseUIEvent</typeparam>
public interface IUIEventHandler<in T> : IBaseEventHandler<T> where T : IBaseUIEvent
{
    // Inherits HandleAsync from IBaseEventHandler<T>
}

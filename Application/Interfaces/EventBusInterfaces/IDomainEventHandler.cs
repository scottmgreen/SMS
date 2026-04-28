//-----------------------------------------------------------------------
// <copyright file="IDomainEventHandler.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Generic event handler interfaces for SMS comprehensive event processing.
//                  Provides consistent event handling patterns for Domain, UI, and Integration events
//                  that integrate with existing SMS service infrastructure and logging patterns.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Interfaces;

/// <summary>
/// Specialized interface for domain event handlers
/// Inherits from IBaseEventHandler for consistent handling patterns
/// </summary>
/// <typeparam name="T">Domain event type implementing IBaseDomainEvent</typeparam>
public interface IDomainEventHandler<in T> : IBaseEventHandler<T> where T : IBaseDomainEvent
{
    // Inherits HandleAsync from IBaseEventHandler<T>
}

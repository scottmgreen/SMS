//-----------------------------------------------------------------------
// <copyright file="IMessenger.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Interface contract defining operations and ensuring clean architecture boundaries.
//                  Defines contract for application services ensuring clean architecture
//                  boundaries and dependency inversion compliance.
// </copyright>
//-----------------------------------------------------------------------



namespace SMS_Application.Interfaces;

public interface IMessenger
{
    void Subscribe<TEvent>(Action<TEvent> action);
    void Unsubscribe<TEvent>(Action<TEvent> action);
    void Publish<TEvent>(TEvent @event);
}


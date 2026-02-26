//-----------------------------------------------------------------------
// <copyright file="BaseDomainEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Base domain event class providing event infrastructure for SMS domain-driven design patterns.
//                  Shared domain infrastructure providing base classes
//                  and common functionality for Domain-Driven Design.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Common;

public abstract class BaseDomainEvent : IBaseDomainEvent //, INotification
{
    public BaseDomainEvent(DateTime datetime)
    {
        OccurredOn = datetime;
    }
    public DateTime OccurredOn { get; protected set; } = DateTime.UtcNow;
}


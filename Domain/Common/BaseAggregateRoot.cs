//-----------------------------------------------------------------------
// <copyright file="BaseAggregateRoot.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Base infrastructure class providing common functionality for SMS domain components.
//                  Shared domain infrastructure providing base classes
//                  and common functionality for Domain-Driven Design.
// </copyright>
//-----------------------------------------------------------------------

// -----------------------------------------------------------------------------
// <copyright file="BaseAggregateRoot.cs" company="">
//     Author: Scott Green
//     Date: 2025-07-24
//     Summary: Abstract base class for aggregate roots, supporting domain events.
// </copyright>
// ----------------------------------------------------------------------------->

namespace SMS_Domain.Common;

/// <summary>
/// Abstract base class for aggregate roots, supporting domain events.
/// </summary>
public abstract class BaseAggregateRoot
{
    private readonly List<IBaseDomainEvent> _domainEvents = new();

    /// <summary>
    /// Gets or sets the unique identifier for the aggregate root.
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Gets the collection of domain events associated with this aggregate root.
    /// </summary>
    public IReadOnlyCollection<IBaseDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to the aggregate root.
    /// </summary>
    /// <param name="domainEvent">The domain event to add.</param>
    public void AddDomainEvent(IBaseDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>
    /// Removes a domain event from the aggregate root.
    /// </summary>
    /// <param name="domainEvent">The domain event to remove.</param>
    public void RemoveDomainEvent(IBaseDomainEvent domainEvent) => _domainEvents.Remove(domainEvent);

    /// <summary>
    /// Clears all domain events from the aggregate root.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}


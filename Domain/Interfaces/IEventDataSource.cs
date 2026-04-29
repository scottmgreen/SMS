// <copyright file="IEventDataSource.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Interface for domain events that can serve as SPI data sources.
//                  Provides metadata for UI display and SPI configuration.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Interfaces;

namespace SMS_Domain.Interfaces;

/// <summary>
/// Interface for domain events that can serve as SPI data sources
/// Provides metadata for UI display and automatic discovery
/// </summary>
public interface IEventDataSource : IBaseDomainEvent
{
    /// <summary>
    /// Display name for the UI dropdown (e.g., "Hazard Management")
    /// </summary>
    string DataSourceDisplayName { get; }

    /// <summary>
    /// Category for grouping related events (e.g., "Safety", "Compliance", "Training")
    /// </summary>
    string DataSourceCategory { get; }

    /// <summary>
    /// Description of what data this event provides for SPI calculations
    /// </summary>
    string DataSourceDescription { get; }

    /// <summary>
    /// Indicates if this event should be automatically included in SPI calculations
    /// </summary>
    bool IsAutomaticDataSource { get; }

    /// <summary>
    /// Priority for ordering in UI dropdowns (1 = highest priority)
    /// </summary>
    int DisplayPriority { get; }
}
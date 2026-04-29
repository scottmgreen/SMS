//-----------------------------------------------------------------------
// <copyright file="IHasAuditFields.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Interface contract defining operations and ensuring clean architecture boundaries.
//                  Defines contract for application services ensuring clean architecture
//                  boundaries and dependency inversion compliance.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Interfaces;

/// <summary>
/// Interface for commands that can have audit fields automatically set
/// </summary>
public interface IHasAuditFields : IAuditableCommand
{
    /// <summary>
    /// Set audit fields for creation
    /// </summary>
    /// <param name="userId">The user performing the action</param>
    /// <param name="timestamp">The timestamp of the action</param>
    void SetCreatedBy(string userId, DateTime timestamp);

    /// <summary>
    /// Set audit fields for updates
    /// </summary>
    /// <param name="userId">The user performing the action</param>
    /// <param name="timestamp">The timestamp of the action</param>
    void SetUpdatedBy(string userId, DateTime timestamp);
}

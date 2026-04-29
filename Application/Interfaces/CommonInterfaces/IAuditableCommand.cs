//-----------------------------------------------------------------------
// <copyright file="IAuditableCommand.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Interface contract defining operations and ensuring clean architecture boundaries.
//                  Defines contract for application services ensuring clean architecture
//                  boundaries and dependency inversion compliance.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Interfaces;

/// <summary>
/// Marker interface for commands that contain entities with audit fields
/// </summary>
public interface IAuditableCommand
{
}

/// <summary>
/// Interface for commands that create new entities
/// </summary>
public interface ICreateCommand : IHasAuditFields
{
}

/// <summary>
/// Interface for commands that update existing entities
/// </summary>
public interface IUpdateCommand : IHasAuditFields
{
}

/// <summary>
/// Interface for commands that delete existing entities
/// </summary>
public interface IDeleteCommand : IAuditableCommand
{
    /// <summary>
    /// Set audit fields for deletion
    /// </summary>
    /// <param name="userId">The user performing the deletion</param>
    /// <param name="timestamp">The timestamp of the deletion</param>
    void SetDeletedBy(string userId, DateTime timestamp);
}

/// <summary>
/// Interface for queries that read data (for auditing data access)
/// Maintains CQRS pattern consistency - Queries are separate from Commands
/// </summary>
public interface IReadQuery : IAuditableCommand
{
    /// <summary>
    /// Set audit fields for data access
    /// </summary>
    /// <param name="userId">The user performing the read</param>
    /// <param name="timestamp">The timestamp of the access</param>
    void SetAccessedBy(string userId, DateTime timestamp);
    
    /// <summary>
    /// Get the resource being accessed for audit logging
    /// </summary>
    string GetResourceIdentifier();
    
    /// <summary>
    /// Get the access type (e.g., "GetById", "GetAll", "Search")
    /// </summary>
    string GetAccessType();
}

/// <summary>
/// OPTIONAL Interface for commands that want enhanced audit tracking
/// Commands can OPTIONALLY implement this for better audit trail detail
/// BACKWARD COMPATIBLE - existing commands still work without this
/// </summary>
public interface IEnhancedAuditCommand : IAuditableCommand
{
    /// <summary>
    /// Get the resource being acted upon for audit logging
    /// </summary>
    string GetResourceIdentifier();
    
    /// <summary>
    /// Get the action type (shortened command name without "Command" suffix)
    /// </summary>
    string GetActionType();
}

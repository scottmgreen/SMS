//-----------------------------------------------------------------------
// <copyright file="ICommandAccessAuditService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Service for auditing command execution and business actions
//                  Tracks who performed what actions and when for compliance and security
//                  CONSISTENT with QueryAccessAuditService for unified CQRS audit trail
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Interfaces;

/// <summary>
/// Service for auditing command execution and business action operations
/// Provides comprehensive audit trail for all write operations in the system
/// MIRRORS QueryAccessAuditService for complete CQRS audit consistency
/// </summary>
public interface ICommandAccessAuditService
{
    /// <summary>
    /// Log a command execution event
    /// </summary>
    Task LogCommandExecutionAsync(string userId, string commandType, string resourceIdentifier, string actionType, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Log command execution with additional context
    /// </summary>
    Task LogCommandExecutionAsync(string userId, string commandType, string resourceIdentifier, string actionType, string additionalContext, CancellationToken cancellationToken = default);
}

//-----------------------------------------------------------------------
// <copyright file="IQueryAccessAuditService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Service for auditing data access through queries
//                  Tracks who accessed what data and when for compliance and security
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Interfaces;

/// <summary>
/// Service for auditing query access and data retrieval operations
/// Provides comprehensive audit trail for all read operations in the system
/// </summary>
public interface IQueryAccessAuditService
{
    /// <summary>
    /// Log a query access event
    /// </summary>
    Task LogQueryAccessAsync(string userId, string queryType, string resourceIdentifier, string accessType, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Log query access with additional context
    /// </summary>
    Task LogQueryAccessAsync(string userId, string queryType, string resourceIdentifier, string accessType, string additionalContext, CancellationToken cancellationToken = default);
}

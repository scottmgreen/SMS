//-----------------------------------------------------------------------
// <copyright file="QueryAccessAuditService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Service for auditing data access through queries
//                  Tracks who accessed what data and when for compliance and security
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Domain.Enums;

namespace SMS_Application.Services;

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

/// <summary>
/// Implementation of query access audit service
/// </summary>
public class QueryAccessAuditService : IQueryAccessAuditService
{
    private readonly SystemService _systemService;
    private readonly ILogger<QueryAccessAuditService> _logger;

    public QueryAccessAuditService(
        SystemService systemService,
        ILogger<QueryAccessAuditService> logger)
    {
        _systemService = systemService ?? throw new ArgumentNullException(nameof(systemService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task LogQueryAccessAsync(string userId, string queryType, string resourceIdentifier, string accessType, CancellationToken cancellationToken = default)
    {
        await LogQueryAccessAsync(userId, queryType, resourceIdentifier, accessType, null, cancellationToken);
    }

    public async Task LogQueryAccessAsync(string userId, string queryType, string resourceIdentifier, string accessType, string additionalContext, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("?? Query Access: User '{UserId}' accessed {ResourceIdentifier} via {QueryType} ({AccessType})", 
                userId, resourceIdentifier, queryType, accessType);

            // Extract entity name from query type for better module organization
            // Example: "GetHazardLocationsByHazardCodeQuery" -> "HazardLocation"
            var moduleName = ExtractModuleNameFromQueryType(queryType);
            
            // Use the shortened query name (without "Query" suffix) as the function name for better specificity
            var functionName = queryType.Replace("Query", "");

            // Create audit log entry for data access
            var auditEntry = new AuditLogEntry(new AuditLogEntryID(Guid.NewGuid().ToString()))
            {
                UserID = userId,
                EventDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                MessageType = AuditMessageType.CrudRead, // ?? SMART ENUM: Type-safe, all queries are read operations
                Severity = "INFORMATION",
                Module = moduleName, // Now shows entity name like "HazardLocation" instead of generic "SMS_QueryAudit"
                Function = functionName, // Now shows "GetHazardLocationsByHazardCode" instead of generic "GetByHazard"
                Description = $"User '{userId}' performed {accessType} on {resourceIdentifier} using {queryType}" +
                             (string.IsNullOrEmpty(additionalContext) ? "" : $". Context: {additionalContext}")
            };

            var result = await _systemService.AddAuditLogEntryAsync(auditEntry, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("?? Failed to log query access audit for user {UserId}", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error logging query access audit for user {UserId}", userId);
            // Don't throw - audit logging failure shouldn't break query operations
        }
    }

    /// <summary>
    /// Extracts a meaningful module name from the query type for better audit organization
    /// Examples:
    /// - "GetHazardLocationsByHazardCodeQuery" -> "HazardLocation"
    /// - "GetAllSMSApplicationUsersQuery" -> "SMSApplicationUser"
    /// - "GetReportByCodeQuery" -> "Report"
    /// </summary>
    private string ExtractModuleNameFromQueryType(string queryType)
    {
        try
        {
            // Remove "Query" suffix first
            var withoutQuery = queryType.Replace("Query", "");
            
            // Handle different query patterns
            if (withoutQuery.StartsWith("GetAll"))
            {
                // "GetAllHazardLocations" -> "HazardLocation"
                return withoutQuery.Substring(6).TrimEnd('s'); // Remove "GetAll" and trailing 's'
            }
            else if (withoutQuery.Contains("By"))
            {
                // "GetHazardLocationsByHazardCode" -> "HazardLocation"
                var beforeBy = withoutQuery.Substring(3); // Remove "Get"
                var byIndex = beforeBy.IndexOf("By");
                if (byIndex > 0)
                {
                    var entityPart = beforeBy.Substring(0, byIndex);
                    return entityPart.TrimEnd('s'); // Remove trailing 's' if present
                }
            }
            else if (withoutQuery.StartsWith("Get"))
            {
                // "GetHazardByCode" -> "Hazard" 
                var afterGet = withoutQuery.Substring(3);
                var byIndex = afterGet.IndexOf("By");
                if (byIndex > 0)
                {
                    return afterGet.Substring(0, byIndex);
                }
                return afterGet;
            }
            
            // Fallback: return the query type without "Query"
            return withoutQuery;
        }
        catch
        {
            // Fallback to original behavior if extraction fails
            return "SMS_QueryAudit";
        }
    }
}
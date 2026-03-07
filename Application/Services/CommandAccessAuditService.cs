//-----------------------------------------------------------------------
// <copyright file="CommandAccessAuditService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Service for auditing command execution and business actions
//                  Tracks who performed what actions and when for compliance and security
//                  CONSISTENT with QueryAccessAuditService for unified CQRS audit trail
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Domain.Enums;

namespace SMS_Application.Services;

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

/// <summary>
/// Implementation of command execution audit service
/// CONSISTENT PATTERN with QueryAccessAuditService for unified maintenance
/// </summary>
public class CommandAccessAuditService : ICommandAccessAuditService
{
    private readonly SystemService _systemService;
    private readonly ILogger<CommandAccessAuditService> _logger;

    public CommandAccessAuditService(
        SystemService systemService,
        ILogger<CommandAccessAuditService> logger)
    {
        _systemService = systemService ?? throw new ArgumentNullException(nameof(systemService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task LogCommandExecutionAsync(string userId, string commandType, string resourceIdentifier, string actionType, CancellationToken cancellationToken = default)
    {
        await LogCommandExecutionAsync(userId, commandType, resourceIdentifier, actionType, null, cancellationToken);
    }

    public async Task LogCommandExecutionAsync(string userId, string commandType, string resourceIdentifier, string actionType, string additionalContext, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("?? Command Execution: User '{UserId}' executed {ResourceIdentifier} via {CommandType} ({ActionType})", 
                userId, resourceIdentifier, commandType, actionType);

            // Extract entity name from command type for better module organization (SAME PATTERN as QueryAccessAuditService)
            // Example: "CreateHazardLocationCommand" -> "HazardLocation"
            var moduleName = ExtractModuleNameFromCommandType(commandType);
            
            // Use the shortened command name (without "Command" suffix) as the function name for better specificity
            var functionName = commandType.Replace("Command", "");

            // Create audit log entry for command execution
            var auditEntry = new AuditLogEntry(new AuditLogEntryID(Guid.NewGuid().ToString()))
            {
                UserID = userId,
                EventDateTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                MessageType = AuditMessageType.DetermineFromCommandType(commandType), // ?? SMART ENUM: Type-safe, intelligent categorization
                Severity = "INFORMATION",
                Module = moduleName, // Shows entity name like "HazardLocation" instead of generic "SMS_CommandAudit"
                Function = functionName, // Shows "CreateHazardLocation" instead of generic "Create"
                Description = $"User '{userId}' performed {actionType} on {resourceIdentifier} using {commandType}" +
                             (string.IsNullOrEmpty(additionalContext) ? "" : $". Context: {additionalContext}")
            };

            var result = await _systemService.AddAuditLogEntryAsync(auditEntry, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("?? Failed to log command execution audit for user {UserId}", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error logging command execution audit for user {UserId}", userId);
            // Don't throw - audit logging failure shouldn't break command operations
        }
    }

    /// <summary>
    /// Extracts a meaningful module name from the command type for better audit organization
    /// IDENTICAL PATTERN to QueryAccessAuditService.ExtractModuleNameFromQueryType()
    /// Examples:
    /// - "CreateHazardLocationCommand" -> "HazardLocation"
    /// - "UpdateSMSApplicationUserCommand" -> "SMSApplicationUser"  
    /// - "DeleteReportCommand" -> "Report"
    /// </summary>
    private string ExtractModuleNameFromCommandType(string commandType)
    {
        try
        {
            // Remove "Command" suffix first
            var withoutCommand = commandType.Replace("Command", "");
            
            // Handle different command patterns
            if (withoutCommand.StartsWith("Create"))
            {
                // "CreateHazardLocation" -> "HazardLocation"
                return withoutCommand.Substring(6); // Remove "Create"
            }
            else if (withoutCommand.StartsWith("Update"))
            {
                // "UpdateSMSApplicationUser" -> "SMSApplicationUser"
                return withoutCommand.Substring(6); // Remove "Update"
            }
            else if (withoutCommand.StartsWith("Delete"))
            {
                // "DeleteReport" -> "Report"
                return withoutCommand.Substring(6); // Remove "Delete"
            }
            else if (withoutCommand.StartsWith("Record"))
            {
                // "RecordAuthenticationSuccess" -> "Authentication"
                var afterRecord = withoutCommand.Substring(6);
                // Extract the main entity (first meaningful word)
                if (afterRecord.Contains("Authentication"))
                    return "Authentication";
                if (afterRecord.Contains("Logout"))
                    return "Authentication";
                return afterRecord;
            }
            else if (withoutCommand.StartsWith("Reset"))
            {
                // "ResetHazardScores" -> "Hazard"
                return withoutCommand.Substring(5).Split(new[] { "Scores", "Password", "Status" }, StringSplitOptions.None)[0];
            }
            else if (withoutCommand.StartsWith("Assign"))
            {
                // "AssignMitigation" -> "Mitigation"
                return withoutCommand.Substring(6); // Remove "Assign"
            }
            else if (withoutCommand.StartsWith("Validate"))
            {
                // "ValidateReport" -> "Report" 
                return withoutCommand.Substring(8); // Remove "Validate"
            }
            
            // Fallback: return the command type without "Command"
            return withoutCommand;
        }
        catch
        {
            // Fallback to original behavior if extraction fails
            return "SMS_CommandAudit";
        }
    }
}
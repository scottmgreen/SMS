//-----------------------------------------------------------------------
// <copyright file="AuditFieldsPipeline.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Comprehensive audit pipeline combining field setting and query auditing.
//                  FIXED: Unified pipeline to prevent StackOverflowException from multiple registrations.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;

namespace SMS_Application.Messaging.Pipelines;

/// <summary>
/// Comprehensive audit pipeline that handles both command audit fields and query access logging
/// ENHANCED: Now includes command execution audit logging for complete CQRS audit coverage
/// </summary>
public class AuditFieldsPipeline<TRequest, TResult> : IPipeline<TRequest, TResult>
    where TRequest : IRequest<TResult>
    where TResult : Result
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IQueryAccessAuditService _queryAuditService;
    private readonly ICommandAccessAuditService _commandAuditService;
    private readonly ILogger<AuditFieldsPipeline<TRequest, TResult>> _logger;

    public AuditFieldsPipeline(
        ICurrentUserService currentUserService,
        IQueryAccessAuditService queryAuditService,
        ICommandAccessAuditService commandAuditService,
        ILogger<AuditFieldsPipeline<TRequest, TResult>> logger)
    {
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _queryAuditService = queryAuditService ?? throw new ArgumentNullException(nameof(queryAuditService));
        _commandAuditService = commandAuditService ?? throw new ArgumentNullException(nameof(commandAuditService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResult> HandleAsync(
        TRequest request,
        RequestPipelineDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var requestType = request.GetType().Name;
        var currentUserId = _currentUserService.UserDisplayName;
        var isAuthenticated = _currentUserService.IsAuthenticated;

        _logger.LogInformation("🔄 Comprehensive Audit Pipeline: Processing {RequestType}, User: {UserId}, Auth: {IsAuth}",
            requestType, currentUserId, isAuthenticated);

        // Step 1: Set audit fields BEFORE executing (for commands and queries)
        SetAuditFields(request, currentUserId);

        // Step 2: Execute the handler
        var result = await next().ConfigureAwait(false);

        // Step 3a: Log query access AFTER execution (for read queries only)
        await LogQueryAccessIfApplicable(request, result, currentUserId, cancellationToken);

        // Step 3b: Log command execution AFTER execution (for commands only) - CONSISTENCY!
        await LogCommandExecutionIfApplicable(request, result, currentUserId, cancellationToken);

        _logger.LogInformation("✅ Comprehensive Audit Pipeline: Completed {RequestType}", requestType);
        return result;
    }

    /// <summary>
    /// Enhanced audit field setting with comprehensive CQRS pattern support (Commands + Queries)
    /// </summary>
    private void SetAuditFields(TRequest request, string currentUserId)
    {
        var timestamp = DateTime.UtcNow;
        var requestType = request.GetType().Name;

        try
        {
            // Handle Create Commands
            if (request is ICreateCommand createCommand)
            {
                _logger.LogInformation("✅ CQRS-C: Setting CreatedBy='{UserId}' for CREATE command: {RequestType}",
                    currentUserId, requestType);
                createCommand.SetCreatedBy(currentUserId, timestamp);
                return;
            }

            // Handle Update Commands
            if (request is IUpdateCommand updateCommand)
            {
                _logger.LogInformation("✅ CQRS-U: Setting UpdatedBy='{UserId}' for UPDATE command: {RequestType}",
                    currentUserId, requestType);
                updateCommand.SetUpdatedBy(currentUserId, timestamp);
                return;
            }

            // Handle Delete Commands
            if (request is IDeleteCommand deleteCommand)
            {
                _logger.LogInformation("✅ CQRS-D: Setting DeletedBy='{UserId}' for DELETE command: {RequestType}",
                    currentUserId, requestType);
                deleteCommand.SetDeletedBy(currentUserId, timestamp);
                return;
            }

            // Handle Read Queries (CQRS Query Pattern)
            if (request is IReadQuery readQuery)
            {
                _logger.LogInformation("🔍 CQRS-R: Setting AccessedBy='{UserId}' for READ query: {RequestType}, Resource: {Resource}",
                    currentUserId, requestType, readQuery.GetResourceIdentifier());
                readQuery.SetAccessedBy(currentUserId, timestamp);
                return;
            }

            // Handle legacy IHasAuditFields (for backward compatibility)
            if (request is IHasAuditFields auditableCommand)
            {
                _logger.LogInformation("✅ Legacy: Setting audit fields via IHasAuditFields for {RequestType}", requestType);
                if (IsCreateCommand(requestType))
                {
                    auditableCommand.SetCreatedBy(currentUserId, timestamp);
                }
                else if (IsUpdateCommand(requestType))
                {
                    auditableCommand.SetUpdatedBy(currentUserId, timestamp);
                }
                else
                {
                    auditableCommand.SetUpdatedBy(currentUserId, timestamp);
                }
                return;
            }

            _logger.LogDebug("ℹ️ Request {RequestType} does not implement audit interfaces - no tracking", requestType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error setting audit fields for request {RequestType}", requestType);
        }
    }

    /// <summary>
    /// Log query access for read operations (FIXED: Combined into single pipeline)
    /// </summary>
    private async Task LogQueryAccessIfApplicable(TRequest request, TResult result, string currentUserId, CancellationToken cancellationToken)
    {
        // Only log access for read queries
        if (request is IReadQuery readQuery)
        {
            try
            {
                var requestType = request.GetType().Name;
                
                if (result.IsSuccess)
                {
                    await _queryAuditService.LogQueryAccessAsync(
                        currentUserId,
                        requestType,
                        readQuery.GetResourceIdentifier(),
                        readQuery.GetAccessType(),
                        cancellationToken);

                    _logger.LogInformation("🔍 Query Access Logged: {QueryType} by {UserId} → {Resource}", 
                        requestType, currentUserId, readQuery.GetResourceIdentifier());
                }
                else
                {
                    // Log failed queries for security monitoring
                    await _queryAuditService.LogQueryAccessAsync(
                        currentUserId,
                        requestType,
                        readQuery.GetResourceIdentifier(),
                        $"{readQuery.GetAccessType()}_FAILED",
                        $"Query failed: {result.Error?.Message}",
                        cancellationToken);

                    _logger.LogWarning("⚠️ Failed Query Logged: {QueryType} by {UserId} → FAILED", 
                        requestType, currentUserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error logging query access for {RequestType}", request.GetType().Name);
                // Don't throw - audit logging failure shouldn't break business operations
            }
        }
    }

    /// <summary>
    /// Log command execution for business actions (CONSISTENT: Mirrors LogQueryAccessIfApplicable)
    /// </summary>
    private async Task LogCommandExecutionIfApplicable(TRequest request, TResult result, string currentUserId, CancellationToken cancellationToken)
    {
        // Log execution for all command types (Create, Update, Delete)
        if (request is IAuditableCommand && !IsReadQuery(request))
        {
            try
            {
                var requestType = request.GetType().Name;
                
                // BACKWARD COMPATIBLE: Use enhanced interface only if implemented, otherwise use fallback extraction
                string resourceIdentifier, actionType;
                if (request is IEnhancedAuditCommand enhancedCommand)
                {
                    resourceIdentifier = enhancedCommand.GetResourceIdentifier();
                    actionType = enhancedCommand.GetActionType();
                }
                else
                {
                    // Fallback extraction for all existing commands (maintains backward compatibility)
                    resourceIdentifier = ExtractResourceIdentifierFromCommand(request);
                    actionType = ExtractActionTypeFromCommand(request, requestType);
                }
                
                if (result.IsSuccess)
                {
                    await _commandAuditService.LogCommandExecutionAsync(
                        currentUserId,
                        requestType,
                        resourceIdentifier,
                        actionType,
                        cancellationToken);

                    _logger.LogInformation("🔧 Command Execution Logged: {CommandType} by {UserId} → {Resource}", 
                        requestType, currentUserId, resourceIdentifier);
                }
                else
                {
                    // Log failed commands for security and business monitoring
                    await _commandAuditService.LogCommandExecutionAsync(
                        currentUserId,
                        requestType,
                        resourceIdentifier,
                        $"{actionType}_FAILED",
                        $"Command failed: {result.Error?.Message}",
                        cancellationToken);

                    _logger.LogWarning("⚠️ Failed Command Logged: {CommandType} by {UserId} → FAILED", 
                        requestType, currentUserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error logging command execution for {RequestType}", request.GetType().Name);
                // Don't throw - audit logging failure shouldn't break business operations
            }
        }
    }

    /// <summary>
    /// Check if the request is a read query (to avoid double-logging)
    /// </summary>
    private static bool IsReadQuery(TRequest request) => request is IReadQuery;

    /// <summary>
    /// Extract resource identifier from command for audit logging (CONSISTENT with Query pattern)
    /// </summary>
    private string ExtractResourceIdentifierFromCommand(TRequest request)
    {
        try
        {
            var requestType = request.GetType().Name;
            
            // Try to get entity information from command properties
            var properties = request.GetType().GetProperties();
            
            // Look for entity objects or ID properties
            foreach (var prop in properties)
            {
                var value = prop.GetValue(request);
                if (value == null) continue;

                // Handle entity objects with Code property
                var codeProperty = value.GetType().GetProperty("Code");
                if (codeProperty?.GetValue(value) is string code && !string.IsNullOrEmpty(code))
                {
                    var entityType = value.GetType().Name;
                    return $"{entityType}:Code:{code}";
                }

                // Handle ID properties
                if (prop.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && value is not null)
                {
                    var idValue = value.ToString();
                    if (!string.IsNullOrEmpty(idValue))
                    {
                        var entityType = prop.Name.Replace("Id", "");
                        return $"{entityType}:Id:{idValue}";
                    }
                }

                // Handle direct Code properties
                if (prop.Name.Equals("Code", StringComparison.OrdinalIgnoreCase) && value is string directCode)
                {
                    return $"Entity:Code:{directCode}";
                }

                // Handle UserName for authentication commands
                if (prop.Name.Equals("UserName", StringComparison.OrdinalIgnoreCase) && value is string userName)
                {
                    return $"User:UserName:{userName}";
                }
            }

            // Fallback: use command type
            return ExtractEntityNameFromCommandType(requestType);
        }
        catch
        {
            return "Unknown:Resource";
        }
    }

    /// <summary>
    /// Extract action type from command (CONSISTENT with Query GetAccessType pattern)
    /// </summary>
    private string ExtractActionTypeFromCommand(TRequest request, string requestType)
    {
        // Use the shortened command name (removing "Command" suffix) - CONSISTENT with Query pattern
        return requestType.Replace("Command", "");
    }

    /// <summary>
    /// Extract entity name from command type for fallback resource identification
    /// </summary>
    private string ExtractEntityNameFromCommandType(string commandType)
    {
        var withoutCommand = commandType.Replace("Command", "");
        
        if (withoutCommand.StartsWith("Create"))
            return withoutCommand.Substring(6);
        else if (withoutCommand.StartsWith("Update"))
            return withoutCommand.Substring(6);
        else if (withoutCommand.StartsWith("Delete"))
            return withoutCommand.Substring(6);
        
        return withoutCommand;
    }

    /// <summary>
    /// Determine if command is a create operation based on naming patterns
    /// </summary>
    private static bool IsCreateCommand(string commandName)
    {
        return commandName.StartsWith("Create", StringComparison.OrdinalIgnoreCase) ||
               commandName.Contains("Add", StringComparison.OrdinalIgnoreCase) ||
               commandName.Contains("Register", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determine if command is an update operation based on naming patterns
    /// </summary>
    private static bool IsUpdateCommand(string commandName)
    {
        return commandName.StartsWith("Update", StringComparison.OrdinalIgnoreCase) ||
               commandName.StartsWith("Modify", StringComparison.OrdinalIgnoreCase) ||
               commandName.StartsWith("Edit", StringComparison.OrdinalIgnoreCase) ||
               commandName.Contains("Save", StringComparison.OrdinalIgnoreCase) ||
               commandName.Contains("Change", StringComparison.OrdinalIgnoreCase) ||
               commandName.Contains("Set", StringComparison.OrdinalIgnoreCase) ||
               commandName.Contains("Reset", StringComparison.OrdinalIgnoreCase) ||
               commandName.Contains("Activate", StringComparison.OrdinalIgnoreCase) ||
               commandName.Contains("Deactivate", StringComparison.OrdinalIgnoreCase) ||
               commandName.Contains("Record", StringComparison.OrdinalIgnoreCase) ||
               commandName.Contains("Assign", StringComparison.OrdinalIgnoreCase);
    }
}

//-----------------------------------------------------------------------
// <copyright file="AuditLogPipeline.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enhanced audit logging pipeline for comprehensive business action tracking.
//                  UPDATED: Now uses feature flags to control audit verbosity and prevent duplication
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.FeatureManagement;
using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Application.Common;

namespace SMS_Application.Messaging.Pipelines;

/// <summary>
/// Enhanced pipeline for comprehensive audit logging of business actions
/// Integrates with CurrentUserService and SystemService for reliable audit trails
/// UPDATED: Now respects EnableBusinessAuditLog feature flag to prevent duplicate logging
/// </summary>
public class AuditLogPipeline<TRequest, TResult> : IPipeline<TRequest, TResult> 
    where TRequest : IRequest<TResult> 
    where TResult : Result
{
    private readonly SystemService _systemService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFeatureManager _featureManager;
    private readonly ILogger<AuditLogPipeline<TRequest, TResult>> _logger;

    public AuditLogPipeline(
        SystemService systemService,
        ICurrentUserService currentUserService,
        IFeatureManager featureManager,
        ILogger<AuditLogPipeline<TRequest, TResult>> logger)
    {
        _systemService = systemService ?? throw new ArgumentNullException(nameof(systemService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResult> HandleAsync(TRequest request, RequestPipelineDelegate<TResult> next, CancellationToken cancellation = default)
    {
        cancellation.ThrowIfCancellationRequested();

        var commandName = request.GetType().Name;
        var currentUserId = _currentUserService.UserDisplayName;
        var timestamp = DateTime.UtcNow;

        // Check if business audit logging is enabled via feature flag
        var isBusinessAuditEnabled = await _featureManager.IsEnabledAsync("EnableBusinessAuditLog").ConfigureAwait(false);

        _logger.LogDebug("📋 Business Audit: Processing {CommandType} by user {UserId} - Feature Enabled: {Enabled}", 
            commandName, currentUserId, isBusinessAuditEnabled);

        try
        {
            // Execute command handler
            var result = await next().ConfigureAwait(false);

            // Log audit entry after execution (only if feature is enabled)
            if (isBusinessAuditEnabled)
            {
                await LogBusinessActionAsync(request, result, currentUserId, timestamp, cancellation).ConfigureAwait(false);
            }

            return result;
        }
        catch (Exception ex)
        {
            // Log failed business actions for audit compliance (only if feature is enabled)
            if (isBusinessAuditEnabled)
            {
                await LogFailedActionAsync(request, ex, currentUserId, timestamp, cancellation).ConfigureAwait(false);
            }

            throw; // Re-throw the original exception
        }
    }

    /// <summary>
    /// Log successful business actions with comprehensive audit information
    /// </summary>
    private async Task LogBusinessActionAsync(TRequest request, TResult result, string userId, DateTime timestamp, CancellationToken cancellation)
    {
        try
        {
            if (!ShouldAuditCommand(request))
                return;

            var auditEntry = new AuditLogEntry(new AuditLogEntryID("AL-0000"))
            {
                UserID = userId,
                EventDateTime = timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                Workstation = Environment.MachineName,
                Module = request.GetType().Namespace ?? "SMS_Application",
                Function = request.GetType().Name,
                MessageType = GetAuditEventType(request),
                Description = GetAuditDescription(request, result),
                Severity = result.IsSuccess ? "INFORMATION" : "WARNING"
            };

            var auditResult = await _systemService.AddAuditLogEntryAsync(auditEntry, cancellation).ConfigureAwait(false);
            
            if (auditResult.IsSuccess)
            {
                _logger.LogDebug("✅ Business Audit: Entry created for {CommandType}", request.GetType().Name);
            }
            else
            {
                _logger.LogWarning("⚠️ Business Audit: Failed to create entry for {CommandType}: {Error}", 
                    request.GetType().Name, auditResult.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating business audit log entry for {CommandType}", request.GetType().Name);
            // Don't throw - audit logging failure shouldn't break business operations
        }
    }

    /// <summary>
    /// Log failed business actions for comprehensive audit trails
    /// </summary>
    private async Task LogFailedActionAsync(TRequest request, Exception exception, string userId, DateTime timestamp, CancellationToken cancellation)
    {
        try
        {
            var auditEntry = new AuditLogEntry(new AuditLogEntryID("AL-0000"))
            {
                UserID = userId,
                EventDateTime = timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                Workstation = Environment.MachineName,
                Module = request.GetType().Namespace ?? "SMS_Application",
                Function = request.GetType().Name,
                MessageType = "COMMAND_FAILED",
                Description = $"Command {request.GetType().Name} failed: {exception.Message}",
                Severity = "ERROR"
            };

            await _systemService.AddAuditLogEntryAsync(auditEntry, cancellation).ConfigureAwait(false);
        }
        catch (Exception auditEx)
        {
            _logger.LogError(auditEx, "❌ Critical: Failed to log error audit entry for {CommandType}", request.GetType().Name);
        }
    }

    /// <summary>
    /// Determine if a command should be audited based on implemented interfaces
    /// </summary>
    private static bool ShouldAuditCommand(TRequest request)
    {
        return EntityInformationExtractor.IsAuditableCommand(request) || EntityInformationExtractor.IsReadQuery(request);
    }

    /// <summary>
    /// Get audit event type based on command interfaces
    /// </summary>
    private static string GetAuditEventType(TRequest request)
    {
        return EntityInformationExtractor.GetActionType(request) switch
        {
            "CREATE" => "ENTITY_CREATED",
            "UPDATE" => "ENTITY_UPDATED",
            "DELETE" => "ENTITY_DELETED",
            "READ" => "DATA_ACCESSED",
            _ => GetLegacyEventType(request)
        };
    }

    /// <summary>
    /// Get legacy event type for backward compatibility
    /// </summary>
    private static string GetLegacyEventType(TRequest request)
    {
        var requestName = request.GetType().Name;
        
        if (requestName.Contains("Validate"))
            return "VALIDATION_PERFORMED";
        if (requestName.Contains("Authenticate"))
            return "AUTHENTICATION_ATTEMPT";
            
        return "BUSINESS_ACTION";
    }

    /// <summary>
    /// Generate comprehensive audit description using shared utility
    /// </summary>
    private static string GetAuditDescription(TRequest request, TResult result)
    {
        var action = GetAuditEventType(request);
        var status = result.IsSuccess ? "successfully" : "with errors";
        var entityInfo = EntityInformationExtractor.GetEntityInfo(request);
        
        return $"User {action.ToLower().Replace('_', ' ')} {status} via {request.GetType().Name}. Entity: {entityInfo}";
    }
}





//-----------------------------------------------------------------------
// <copyright file="AuditLogPipeline.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enhanced audit logging pipeline for comprehensive business action tracking.
//                  Implements cross-cutting concerns in the request/response pipeline.
//                  Handles logging, auditing, validation, and other aspects.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.FeatureManagement;
using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;

namespace SMS_Application.Messaging.Pipelines;

/// <summary>
/// Enhanced pipeline for comprehensive audit logging of business actions
/// Integrates with CurrentUserService and SystemService for reliable audit trails
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
        var currentUserId = _currentUserService.UserCode;
        var timestamp = DateTime.UtcNow;

        _logger.LogInformation("✅ Clean Architecture: Audit pipeline processing {CommandType} by user {UserId}", 
            commandName, currentUserId);

        try
        {
            // Execute command handler
            var result = await next().ConfigureAwait(false);

            // Log audit entry after execution (only if feature is enabled)
            if (await _featureManager.IsEnabledAsync("AuditLogEnabled").ConfigureAwait(false))
            {
                await LogBusinessActionAsync(request, result, currentUserId, timestamp, cancellation).ConfigureAwait(false);
            }

            return result;
        }
        catch (Exception ex)
        {
            // Log failed business actions for audit compliance
            if (await _featureManager.IsEnabledAsync("AuditLogEnabled").ConfigureAwait(false))
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
            if (!ShouldAuditCommand(request.GetType().Name))
                return;

            var auditEntry = new AuditLogEntry(new AuditLogEntryID(Guid.NewGuid().ToString()))
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
                _logger.LogInformation("✅ Clean Architecture: Audit entry created for {CommandType}", request.GetType().Name);
            }
            else
            {
                _logger.LogWarning("⚠️ Failed to create audit entry for {CommandType}: {Error}", 
                    request.GetType().Name, auditResult.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating audit log entry for {CommandType}", request.GetType().Name);
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
            var auditEntry = new AuditLogEntry(new AuditLogEntryID(Guid.NewGuid().ToString()))
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
    /// Determine if a command should be audited based on business requirements
    /// </summary>
    private static bool ShouldAuditCommand(string commandName)
    {
        var auditableCommands = new[]
        {
            "CreateHazardCommand", "UpdateHazardCommand", "DeleteHazardCommand",
            "CreateReportCommand", "UpdateReportCommand", "DeleteReportCommand",
            "CreateInvestigationCommand", "UpdateInvestigationCommand", "DeleteInvestigationCommand",
            "CreateMitigationCommand", "UpdateMitigationCommand", "DeleteMitigationCommand",
            "CreateRiskAssessmentCommand", "UpdateRiskAssessmentCommand", "DeleteRiskAssessmentCommand",
            "CreateSMSApplicationUserCommand", "UpdateSMSApplicationUserCommand", "DeleteSMSApplicationUserCommand",
            "CreateSMSOrganizationalUserCommand", "UpdateSMSOrganizationalUserCommand", "DeleteSMSOrganizationalUserCommand",
            "CreateSMSStakeholderUserCommand", "UpdateSMSStakeholderUserCommand", "DeleteSMSStakeholderUserCommand",
            "UpdateReportStatusCommand", "SaveStep", "UpdateProgressCommand"
        };

        return auditableCommands.Any(cmd => commandName.Contains(cmd, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get audit event type based on command characteristics
    /// </summary>
    private static string GetAuditEventType(TRequest request)
    {
        return request switch
        {
            ICreateCommand => "ENTITY_CREATED",
            IUpdateCommand => "ENTITY_UPDATED",
            IDeleteCommand => "ENTITY_DELETED",
            _ when request.GetType().Name.Contains("Validate") => "VALIDATION_PERFORMED",
            _ when request.GetType().Name.Contains("Authenticate") => "AUTHENTICATION_ATTEMPT",
            _ => "BUSINESS_ACTION"
        };
    }

    /// <summary>
    /// Generate comprehensive audit description
    /// </summary>
    private static string GetAuditDescription(TRequest request, TResult result)
    {
        var action = GetAuditEventType(request);
        var status = result.IsSuccess ? "successfully" : "with errors";
        var entityInfo = ExtractEntityInfo(request);
        
        return $"User {action.ToLower().Replace('_', ' ')} {status} via {request.GetType().Name}. Entity: {entityInfo}";
    }

    /// <summary>
    /// Extract entity information from the command for audit trails
    /// </summary>
    private static string ExtractEntityInfo(TRequest request)
    {
        try
        {
            var properties = request.GetType().GetProperties();
            var entityProps = properties.Where(p => 
                p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) || 
                p.Name.EndsWith("Code", StringComparison.OrdinalIgnoreCase)).ToList();
            
            if (entityProps.Any())
            {
                var values = entityProps.Take(2) // Limit to avoid long descriptions
                    .Select(p => $"{p.Name}:{p.GetValue(request)}")
                    .Where(v => !string.IsNullOrEmpty(v));
                return string.Join(", ", values);
            }

            return request.GetType().Name.Replace("Command", "").Replace("Query", "");
        }
        catch
        {
            return "Unknown";
        }
    }
}





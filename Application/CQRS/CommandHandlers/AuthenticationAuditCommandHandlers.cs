//-----------------------------------------------------------------------
// <copyright file="AuthenticationAuditCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers for authentication audit operations - NOW USING AUDIT PIPELINE
//                  Processes authentication audit commands using proper CQRS pattern with audit pipeline.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.CommandHandlers;

/// <summary>
/// Handler for recording successful authentication events - NOW USING AUDIT PIPELINE
/// </summary>
public class RecordAuthenticationSuccessCommandHandler : BaseCommandBundle, IRequestHandler<RecordAuthenticationSuccessCommand, Result<bool>>
{
    private readonly SystemService _systemService;
    private readonly ILogger<RecordAuthenticationSuccessCommandHandler> _logger;

    public RecordAuthenticationSuccessCommandHandler(
        SystemService systemService,
        ILogger<RecordAuthenticationSuccessCommandHandler> logger)
    {
        _systemService = systemService ?? throw new ArgumentNullException(nameof(systemService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(RecordAuthenticationSuccessCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("? CQRS-AUTH: Recording authentication SUCCESS for user {UserName} ({UserType}) - AUDIT PIPELINE ACTIVE", 
                request.UserName, request.UserType);

            // Now the AuditFieldsPipeline has already set request.CreatedBy and request.CreatedDate!
            _logger.LogInformation("?? AUDIT PIPELINE: CreatedBy='{CreatedBy}' at {CreatedDate}", 
                request.CreatedBy, request.CreatedDate);

            // Create comprehensive audit log entry
            var auditEntry = new AuditLogEntry(new AuditLogEntryID(Guid.NewGuid().ToString()))
            {
                UserID = request.UserName,
                EventDateTime = request.AuthenticationTime.ToString("yyyy-MM-dd HH:mm:ss"),
                MessageType = "AUTHENTICATION_SUCCESS",
                Severity = "INFORMATION", 
                Module = "SMS_Authentication",
                Function = "UserLogin",
                Description = $"User '{request.UserDisplayName}' ({request.UserType}) successfully authenticated from {request.IPAddress}. Session: {request.SessionId}. Audit: CreatedBy={request.CreatedBy}"
            };

            var result = await _systemService.AddAuditLogEntryAsync(auditEntry, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("? CQRS-AUTH: Authentication success audit recorded for {UserName} via AUDIT PIPELINE", request.UserName);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogWarning("?? Failed to record authentication success audit for {UserName}", request.UserName);
                return Result<bool>.Failure<bool>(new Error("AUDIT_FAILED", "Failed to record authentication audit"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error recording authentication success audit for {UserName}", request.UserName);
            return Result<bool>.Failure<bool>(new Error("AUDIT_ERROR", ex.Message));
        }
    }
}

/// <summary>
/// Handler for recording failed authentication attempts - NOW USING AUDIT PIPELINE
/// </summary>
public class RecordAuthenticationFailureCommandHandler : BaseCommandBundle, IRequestHandler<RecordAuthenticationFailureCommand, Result<bool>>
{
    private readonly SystemService _systemService;
    private readonly ILogger<RecordAuthenticationFailureCommandHandler> _logger;

    public RecordAuthenticationFailureCommandHandler(
        SystemService systemService,
        ILogger<RecordAuthenticationFailureCommandHandler> logger)
    {
        _systemService = systemService ?? throw new ArgumentNullException(nameof(systemService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(RecordAuthenticationFailureCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogWarning("?? CQRS-AUTH: SECURITY ALERT - Authentication FAILURE for user {UserName} from {IPAddress} - AUDIT PIPELINE ACTIVE", 
                request.UserName, request.IPAddress);

            // Now the AuditFieldsPipeline has already set request.CreatedBy and request.CreatedDate!
            _logger.LogInformation("?? AUDIT PIPELINE: CreatedBy='{CreatedBy}' at {CreatedDate}", 
                request.CreatedBy, request.CreatedDate);

            // Create security-focused audit log entry
            var auditEntry = new AuditLogEntry(new AuditLogEntryID(Guid.NewGuid().ToString()))
            {
                UserID = request.UserName,
                EventDateTime = request.AttemptTime.ToString("yyyy-MM-dd HH:mm:ss"),
                MessageType = "AUTHENTICATION_FAILURE",
                Severity = "WARNING",
                Module = "SMS_Authentication", 
                Function = "UserLogin",
                Description = $"?? SECURITY ALERT: Failed authentication attempt for '{request.UserName}' from {request.IPAddress}. Reason: {request.FailureReason}. Attempt #{request.AttemptCount}. Audit: CreatedBy={request.CreatedBy}"
            };

            var result = await _systemService.AddAuditLogEntryAsync(auditEntry, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("? CQRS-AUTH: Authentication failure audit recorded for {UserName} via AUDIT PIPELINE", request.UserName);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogError("?? Critical: Failed to record authentication failure audit for {UserName}", request.UserName);
                return Result<bool>.Failure<bool>(new Error("SECURITY_AUDIT_FAILED", "Failed to record security audit"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? Critical: Error recording authentication failure audit for {UserName}", request.UserName);
            return Result<bool>.Failure<bool>(new Error("SECURITY_AUDIT_ERROR", ex.Message));
        }
    }
}

/// <summary>
/// Handler for recording user logout events - NOW USING AUDIT PIPELINE
/// </summary>
public class RecordLogoutCommandHandler : BaseCommandBundle, IRequestHandler<RecordLogoutCommand, Result<bool>>
{
    private readonly SystemService _systemService;
    private readonly ILogger<RecordLogoutCommandHandler> _logger;

    public RecordLogoutCommandHandler(
        SystemService systemService,
        ILogger<RecordLogoutCommandHandler> logger)
    {
        _systemService = systemService ?? throw new ArgumentNullException(nameof(systemService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(RecordLogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("? CQRS-AUTH: Recording LOGOUT for user {UserName} ({LogoutType}) - AUDIT PIPELINE ACTIVE", 
                request.UserName, request.LogoutType);

            // Now the AuditFieldsPipeline has already set request.CreatedBy and request.CreatedDate!
            _logger.LogInformation("?? AUDIT PIPELINE: CreatedBy='{CreatedBy}' at {CreatedDate}", 
                request.CreatedBy, request.CreatedDate);

            var auditEntry = new AuditLogEntry(new AuditLogEntryID(Guid.NewGuid().ToString()))
            {
                UserID = request.UserName,
                EventDateTime = request.LogoutTime.ToString("yyyy-MM-dd HH:mm:ss"),
                MessageType = "USER_LOGOUT",
                Severity = "INFORMATION",
                Module = "SMS_Authentication",
                Function = "UserLogout", 
                Description = $"User '{request.UserName}' ({request.UserType}) logged out ({request.LogoutType}). Session duration: {request.SessionDuration:hh\\:mm\\:ss}. Session: {request.SessionId}. Audit: CreatedBy={request.CreatedBy}"
            };

            var result = await _systemService.AddAuditLogEntryAsync(auditEntry, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("? CQRS-AUTH: Logout audit recorded for {UserName} via AUDIT PIPELINE", request.UserName);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogWarning("?? Failed to record logout audit for {UserName}", request.UserName);
                return Result<bool>.Failure<bool>(new Error("LOGOUT_AUDIT_FAILED", "Failed to record logout audit"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error recording logout audit for {UserName}", request.UserName);
            return Result<bool>.Failure<bool>(new Error("LOGOUT_AUDIT_ERROR", ex.Message));
        }
    }
}
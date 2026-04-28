//-----------------------------------------------------------------------
// <copyright file="AuthenticationAuditCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers for authentication audit operations - PIPELINE ONLY
//                  ✅ REFACTORED: Simplified handlers that rely entirely on audit pipeline
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.CommandHandlers;

/// <summary>
/// ✅ PIPELINE ONLY: Handler for authentication success - no manual audit creation
/// All audit logging handled automatically by AuditFieldsPipeline
/// </summary>
public class RecordAuthenticationSuccessCommandHandler : BaseCommandBundle, IBaseRequestHandler<RecordAuthenticationSuccessCommand, Result<bool>>
{
    private readonly ILogger<RecordAuthenticationSuccessCommandHandler> _logger;

    public RecordAuthenticationSuccessCommandHandler(ILogger<RecordAuthenticationSuccessCommandHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(RecordAuthenticationSuccessCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("✅ AUTH-SUCCESS: User '{UserName}' ({UserType}) authenticated from {IPAddress}. Session: {SessionId}", 
                request.UserName, request.UserType, request.IPAddress, request.SessionId);

            // ✅ PIPELINE APPROACH: No manual audit creation!
            // The AuditFieldsPipeline automatically:
            // 1. Sets CreatedBy/CreatedDate via SetCreatedBy() 
            // 2. Creates audit log entry via LogCommandExecutionIfApplicable()
            // 3. Uses consistent format via ICommandAccessAuditService
            // 4. Handles all cross-cutting audit concerns

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ AUTH-ERROR: Error processing authentication success for {UserName}", request.UserName);
            return Result<bool>.Failure<bool>(new Error("AUTH_SUCCESS_ERROR", ex.Message));
        }
    }
}

/// <summary>
/// ✅ PIPELINE ONLY: Handler for authentication failure - no manual audit creation
/// All audit logging handled automatically by AuditFieldsPipeline
/// </summary>
public class RecordAuthenticationFailureCommandHandler : BaseCommandBundle, IBaseRequestHandler<RecordAuthenticationFailureCommand, Result<bool>>
{
    private readonly ILogger<RecordAuthenticationFailureCommandHandler> _logger;

    public RecordAuthenticationFailureCommandHandler(ILogger<RecordAuthenticationFailureCommandHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(RecordAuthenticationFailureCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogWarning("🚨 AUTH-FAILURE: SECURITY ALERT - Authentication FAILED for '{UserName}' from {IPAddress}. Reason: {FailureReason}", 
                request.UserName, request.IPAddress, request.FailureReason);

            // ✅ PIPELINE APPROACH: No manual audit creation!
            // Security events are automatically audited by the pipeline
            // with proper failure tracking and consistent formatting

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ AUTH-ERROR: Critical error processing authentication failure for {UserName}", request.UserName);
            return Result<bool>.Failure<bool>(new Error("AUTH_FAILURE_ERROR", ex.Message));
        }
    }
}

/// <summary>
/// ✅ PIPELINE ONLY: Handler for logout events - no manual audit creation
/// All audit logging handled automatically by AuditFieldsPipeline
/// </summary>
public class RecordLogoutCommandHandler : BaseCommandBundle, IBaseRequestHandler<RecordAuthenticationLogoutCommand, Result<bool>>
{
    private readonly ILogger<RecordLogoutCommandHandler> _logger;

    public RecordLogoutCommandHandler(ILogger<RecordLogoutCommandHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(RecordAuthenticationLogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("✅ LOGOUT: User '{UserName}' logged out ({LogoutType}). Session duration: {SessionDuration}", 
                request.UserName, request.LogoutType, request.SessionDuration);

            // ✅ PIPELINE APPROACH: No manual audit creation!
            // Logout events are automatically tracked by the pipeline
            // with consistent session tracking and audit formatting

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ LOGOUT-ERROR: Error processing logout for {UserName}", request.UserName);
            return Result<bool>.Failure<bool>(new Error("LOGOUT_ERROR", ex.Message));
        }
    }
}
//-----------------------------------------------------------------------
// <copyright file="CommandAuditPipeline.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Pipeline behavior for auditing command (write) operations
//                  Tracks all data modification for compliance and security monitoring
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;

namespace SMS_Application.Messaging.Pipelines;

/// <summary>
/// Pipeline behavior that audits all command operations implementing ICreateCommand, IUpdateCommand, IDeleteCommand
/// Provides comprehensive audit trail for data modification operations
/// </summary>
public class CommandAuditPipeline<TRequest, TResult> : IPipeline<TRequest, TResult>
    where TRequest : IRequest<TResult>
    where TResult : Result
{
    private readonly ICommandAccessAuditService _commandAuditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CommandAuditPipeline<TRequest, TResult>> _logger;

    public CommandAuditPipeline(
        ICommandAccessAuditService commandAuditService,
        ICurrentUserService currentUserService,
        ILogger<CommandAuditPipeline<TRequest, TResult>> logger)
    {
        _commandAuditService = commandAuditService ?? throw new ArgumentNullException(nameof(commandAuditService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResult> HandleAsync(
        TRequest request,
        RequestPipelineDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var commandType = request.GetType().Name;
        var currentUserId = _currentUserService.UserDisplayName;
        var isAuthenticated = _currentUserService.IsAuthenticated;

        // Only audit commands that implement command interfaces
        if (IsAuditableCommand(request))
        {
            var commandAction = GetCommandAction(request);
            
            _logger.LogInformation("?? Command Audit: Processing {CommandAction} command {CommandType} by user {UserId}", 
                commandAction, commandType, currentUserId);

            try
            {
                // Execute the command first
                var result = await next().ConfigureAwait(false);

                // Log the command execution AFTER successful execution
                if (result.IsSuccess)
                {
                    await _commandAuditService.LogCommandExecutionAsync(
                        currentUserId,
                        commandType,
                        GetResourceIdentifier(request),
                        commandAction,
                        cancellationToken);

                    _logger.LogInformation("? Command Audit: Successfully audited {CommandAction} {CommandType} by {UserId}", 
                        commandAction, commandType, currentUserId);
                }
                else
                {
                    // Still log failed commands for security monitoring
                    await _commandAuditService.LogCommandExecutionAsync(
                        currentUserId,
                        commandType,
                        GetResourceIdentifier(request),
                        $"{commandAction}_FAILED",
                        $"Command failed: {result.Error?.Message}",
                        cancellationToken);

                    _logger.LogWarning("?? Command Audit: Logged failed {CommandAction} {CommandType} by {UserId}", 
                        commandAction, commandType, currentUserId);
                }

                return result;
            }
            catch (Exception ex)
            {
                // Log command exceptions for security monitoring
                await _commandAuditService.LogCommandExecutionAsync(
                    currentUserId,
                    commandType,
                    GetResourceIdentifier(request),
                    $"{commandAction}_ERROR",
                    $"Command exception: {ex.Message}",
                    cancellationToken);

                _logger.LogError(ex, "? Command Audit: Logged command exception for {CommandAction} {CommandType} by {UserId}", 
                    commandAction, commandType, currentUserId);

                throw; // Re-throw the original exception
            }
        }
        else
        {
            // For non-command requests, just execute without audit logging
            _logger.LogDebug("?? Command Audit: Skipping audit for non-command request {CommandType}", commandType);
            return await next().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Determines if the request is an auditable command
    /// </summary>
    private static bool IsAuditableCommand(TRequest request)
    {
        return request is ICreateCommand or IUpdateCommand or IDeleteCommand;
    }

    /// <summary>
    /// Gets the command action type for audit logging
    /// </summary>
    private static string GetCommandAction(TRequest request)
    {
        return request switch
        {
            ICreateCommand => "CREATE",
            IUpdateCommand => "UPDATE", 
            IDeleteCommand => "DELETE",
            _ => "UNKNOWN"
        };
    }

    /// <summary>
    /// Gets the resource identifier for the command
    /// Uses enhanced audit interface if available, otherwise falls back to command type name
    /// </summary>
    private static string GetResourceIdentifier(TRequest request)
    {
        return request switch
        {
            // Check for enhanced audit command first
            IEnhancedAuditCommand enhancedCmd => enhancedCmd.GetResourceIdentifier(),
            
            // For basic commands, use the command type name as resource identifier
            ICreateCommand => request.GetType().Name.Replace("Command", "").Replace("Create", ""),
            IUpdateCommand => request.GetType().Name.Replace("Command", "").Replace("Update", ""),
            IDeleteCommand => request.GetType().Name.Replace("Command", "").Replace("Delete", ""),
            
            // Fallback
            _ => request.GetType().Name
        };
    }
}
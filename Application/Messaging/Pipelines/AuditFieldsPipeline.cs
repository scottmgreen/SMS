//-----------------------------------------------------------------------
// <copyright file="AuditFieldsPipeline.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enhanced audit fields pipeline for automatic audit field population.
//                  Implements cross-cutting concerns in the request/response pipeline.
//                  Handles logging, auditing, validation, and other aspects.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;

namespace SMS_Application.Messaging.Pipelines;

/// <summary>
/// Enhanced pipeline behavior that automatically sets audit fields on commands
/// Supports ICreateCommand, IUpdateCommand, and IDeleteCommand with proper user tracking
/// </summary>
public class AuditFieldsPipeline<TRequest, TResult> : IPipeline<TRequest, TResult>
    where TRequest : IRequest<TResult>
    where TResult : Result
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuditFieldsPipeline<TRequest, TResult>> _logger;

    public AuditFieldsPipeline(
        ICurrentUserService currentUserService,
        ILogger<AuditFieldsPipeline<TRequest, TResult>> logger)
    {
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
        var currentUserId = _currentUserService.UserCode;
        var isAuthenticated = _currentUserService.IsAuthenticated;

        _logger.LogInformation("✅ Clean Architecture: Audit fields pipeline processing {CommandType}, User: {UserId}, Authenticated: {IsAuthenticated}",
            commandType, currentUserId, isAuthenticated);

        // Set audit fields BEFORE executing the command
        SetAuditFields(request, currentUserId);

        // Execute the command handler
        var result = await next().ConfigureAwait(false);

        _logger.LogInformation("✅ Clean Architecture: Audit fields pipeline completed for {CommandType}", commandType);

        return result;
    }

    /// <summary>
    /// Enhanced audit field setting with comprehensive command type support
    /// </summary>
    /// <param name="request">The command request</param>
    /// <param name="currentUserId">Current user identifier</param>
    private void SetAuditFields(TRequest request, string currentUserId)
    {
        var timestamp = DateTime.UtcNow;
        var commandType = request.GetType().Name;

        try
        {
            // Handle Create Commands
            if (request is ICreateCommand createCommand)
            {
                _logger.LogInformation("✅ Clean Architecture: Setting CreatedBy='{UserId}' for CREATE command: {CommandType}",
                    currentUserId, commandType);

                createCommand.SetCreatedBy(currentUserId, timestamp);
                return;
            }

            // Handle Update Commands
            if (request is IUpdateCommand updateCommand)
            {
                _logger.LogInformation("✅ Clean Architecture: Setting UpdatedBy='{UserId}' for UPDATE command: {CommandType}",
                    currentUserId, commandType);

                updateCommand.SetUpdatedBy(currentUserId, timestamp);
                return;
            }

            // Handle Delete Commands
            if (request is IDeleteCommand deleteCommand)
            {
                _logger.LogInformation("✅ Clean Architecture: Setting DeletedBy='{UserId}' for DELETE command: {CommandType}",
                    currentUserId, commandType);

                deleteCommand.SetDeletedBy(currentUserId, timestamp);
                return;
            }

            // Handle legacy IHasAuditFields (for backward compatibility)
            if (request is IHasAuditFields auditableCommand)
            {
                _logger.LogInformation("✅ Clean Architecture: Setting audit fields via IHasAuditFields for {CommandType}", commandType);

                // Determine appropriate audit field based on command name patterns
                if (IsCreateCommand(commandType))
                {
                    auditableCommand.SetCreatedBy(currentUserId, timestamp);
                }
                else if (IsUpdateCommand(commandType))
                {
                    auditableCommand.SetUpdatedBy(currentUserId, timestamp);
                }
                else
                {
                    // Default to UpdatedBy for unknown command types
                    auditableCommand.SetUpdatedBy(currentUserId, timestamp);
                }
                return;
            }

            // Log when no audit fields are set
            _logger.LogDebug("ℹ️ Command {CommandType} does not implement audit interfaces - no audit fields set", commandType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error setting audit fields for command {CommandType}", commandType);
            // Don't throw - audit field setting failure shouldn't break business operations
        }
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

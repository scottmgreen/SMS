//-----------------------------------------------------------------------
// <copyright file="AuditFieldsSetterPipeline.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Single-responsibility pipeline that ONLY sets audit fields on commands.
//                  Refactored from the mega-AuditFieldsPipeline to follow SRP.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;

namespace SMS_Application.Messaging.Pipelines;

/// <summary>
/// Single-responsibility pipeline that sets audit fields on commands before execution
/// Does NOT handle audit logging - that's handled by separate pipelines
/// </summary>
public class AuditFieldsSetterPipeline<TRequest, TResult> : IPipeline<TRequest, TResult>
    where TRequest : IRequest<TResult>
    where TResult : Result
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuditFieldsSetterPipeline<TRequest, TResult>> _logger;

    public AuditFieldsSetterPipeline(
        ICurrentUserService currentUserService,
        ILogger<AuditFieldsSetterPipeline<TRequest, TResult>> logger)
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

        var requestType = request.GetType().Name;
        var currentUserId = _currentUserService.UserDisplayName;

        _logger.LogDebug("?? Audit Fields Setter: Processing {RequestType}", requestType);

        // Step 1: Set audit fields BEFORE executing (ONLY responsibility of this pipeline)
        SetAuditFields(request, currentUserId);

        // Step 2: Execute the handler and return result
        var result = await next().ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Set audit fields on commands and queries that implement audit interfaces
    /// SINGLE RESPONSIBILITY: Only sets fields, does not log anything
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
                _logger.LogDebug("?? Setting CreatedBy='{UserId}' for CREATE command: {RequestType}",
                    currentUserId, requestType);
                createCommand.SetCreatedBy(currentUserId, timestamp);
                return;
            }

            // Handle Update Commands
            if (request is IUpdateCommand updateCommand)
            {
                _logger.LogDebug("?? Setting UpdatedBy='{UserId}' for UPDATE command: {RequestType}",
                    currentUserId, requestType);
                updateCommand.SetUpdatedBy(currentUserId, timestamp);
                return;
            }

            // Handle Delete Commands
            if (request is IDeleteCommand deleteCommand)
            {
                _logger.LogDebug("?? Setting DeletedBy='{UserId}' for DELETE command: {RequestType}",
                    currentUserId, requestType);
                deleteCommand.SetDeletedBy(currentUserId, timestamp);
                return;
            }

            // Handle Read Queries (CQRS Query Pattern)
            if (request is IReadQuery readQuery)
            {
                _logger.LogDebug("?? Setting AccessedBy='{UserId}' for READ query: {RequestType}",
                    currentUserId, requestType);
                readQuery.SetAccessedBy(currentUserId, timestamp);
                return;
            }

            // Handle legacy IHasAuditFields (for backward compatibility)
            if (request is IHasAuditFields auditableCommand)
            {
                _logger.LogDebug("?? Setting audit fields via IHasAuditFields for {RequestType}", requestType);
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

            _logger.LogDebug("?? Request {RequestType} does not implement audit interfaces - no tracking", requestType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error setting audit fields for request {RequestType}", requestType);
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
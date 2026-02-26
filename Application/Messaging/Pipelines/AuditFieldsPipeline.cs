//-----------------------------------------------------------------------
// <copyright file="AuditFieldsPipeline.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application layer component providing functionality for the SMS safety management system.
//                  Implements cross-cutting concerns in the request/response pipeline.
//                  Handles logging, auditing, validation, and other aspects.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.Pipelines;

/// <summary>
/// Pipeline behavior that automatically sets audit fields on commands
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
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<TResult> HandleAsync(
        TRequest request,
        RequestPipelineDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // ?? VERBOSE LOGGING for debugging
        _logger.LogInformation("?? AuditFieldsPipeline executing for {RequestType}", request.GetType().Name);

        // Set audit fields BEFORE executing the command
        if (request is IHasAuditFields auditableCommand)
        {
            _logger.LogInformation("? Request implements IHasAuditFields - setting audit fields");
            SetAuditFields(auditableCommand);
        }
        else
        {
            _logger.LogInformation("? Request does NOT implement IHasAuditFields - skipping audit fields");
        }

        // Execute the command handler
        var result = await next().ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Sets the appropriate audit fields based on command type
    /// </summary>
    /// <param name="auditableCommand">The command with audit fields</param>
    private void SetAuditFields(IHasAuditFields auditableCommand)
    {
        var currentUser = _currentUserService.UserId;
        var timestamp = DateTime.UtcNow;

        _logger.LogInformation("?? Current user from service: '{CurrentUser}', IsAuthenticated: {IsAuth}",
            currentUser, _currentUserService.IsAuthenticated);

        try
        {
            // Set audit fields based on command type
            switch (auditableCommand)
            {
                case ICreateCommand createCommand:
                    createCommand.SetCreatedBy(currentUser, timestamp);
                    _logger.LogInformation("? Set CreatedBy to '{UserId}' for {CommandType}",
                        currentUser, createCommand.GetType().Name);
                    break;

                case IUpdateCommand updateCommand:
                    updateCommand.SetUpdatedBy(currentUser, timestamp);
                    _logger.LogInformation("? Set UpdatedBy to '{UserId}' for {CommandType}",
                        currentUser, updateCommand.GetType().Name);
                    break;

                default:
                    // For commands that implement IHasAuditFields but not specific create/update
                    // Try both - the command implementation will decide which to use
                    auditableCommand.SetCreatedBy(currentUser, timestamp);
                    auditableCommand.SetUpdatedBy(currentUser, timestamp);
                    _logger.LogInformation("? Set both audit fields to '{UserId}' for {CommandType}",
                        currentUser, auditableCommand.GetType().Name);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Failed to set audit fields for command {CommandType}",
                auditableCommand.GetType().Name);
            // Don't throw - audit field setting should not break the command
        }
    }
}

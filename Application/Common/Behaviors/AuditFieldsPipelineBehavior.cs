//-----------------------------------------------------------------------
// <copyright file="AuditFieldsPipelineBehavior.cs" company="SMS Safety Management System">
//     Author: SMS Development Team  
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Pipeline behavior for automatically setting audit fields on commands
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;

namespace SMS_Application.Behaviors;

/// <summary>
/// Delegate representing the next behavior in the pipeline
/// </summary>
/// <typeparam name="TResponse">The response type</typeparam>
/// <returns>The response</returns>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

/// <summary>
/// Defines a pipeline behavior that can process requests in the mediator pipeline
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handle the request and pass it to the next behavior in the pipeline
    /// </summary>
    /// <param name="request">The request to handle</param>
    /// <param name="next">The next behavior in the pipeline</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The response</returns>
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}

/// <summary>
/// Pipeline behavior that automatically sets audit fields (CreatedBy, UpdatedBy, DeletedBy, timestamps)
/// for commands that implement ICreateCommand, IUpdateCommand, or IDeleteCommand
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class AuditFieldsPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuditFieldsPipelineBehavior<TRequest, TResponse>> _logger;

    public AuditFieldsPipelineBehavior(
        ICurrentUserService currentUserService,
        ILogger<AuditFieldsPipelineBehavior<TRequest, TResponse>> logger)
    {
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var commandName = typeof(TRequest).Name;
        var shouldAudit = ShouldAuditCommand(request);

        if (shouldAudit)
        {
            _logger.LogApplicationInformation("Audit Pipeline: Processing {CommandName} by {User}", commandName, _currentUserService.UserCode);
            SetAuditFields(request, "AUTO");
        }

        // Execute the command
        var response = await next();

        _logger.LogApplicationInformation("Audit Pipeline: Completed {CommandName}", commandName);
        return response;
    }

    /// <summary>
    /// Determines if a command should be audited based on its implemented interfaces
    /// </summary>
    private static bool ShouldAuditCommand(IRequest request)
    {
        var requestType = request.GetType();
        
        // Simple interface check - no magic strings needed
        return typeof(ICreateCommand).IsAssignableFrom(requestType) ||
               typeof(IUpdateCommand).IsAssignableFrom(requestType) ||
               typeof(IDeleteCommand).IsAssignableFrom(requestType) ||
               typeof(IReadQuery).IsAssignableFrom(requestType);
    }

    /// <summary>
    /// Sets audit fields using the interface methods - simple and clean
    /// </summary>
    private void SetAuditFields(TRequest request, string auditAction)
    {
        var currentUser = _currentUserService.UserCode;
        var currentTime = DateTime.UtcNow;

        // Use the interface methods that already exist
        if (request is ICreateCommand createCommand)
            createCommand.SetCreatedBy(currentUser, currentTime);
        else if (request is IUpdateCommand updateCommand)
            updateCommand.SetUpdatedBy(currentUser, currentTime);
        else if (request is IDeleteCommand deleteCommand)
            deleteCommand.SetDeletedBy(currentUser, currentTime);
        else if (request is IReadQuery readQuery)
            readQuery.SetAccessedBy(currentUser, currentTime);

        _logger.LogApplicationInformation("Audit fields set for {CommandName} by {User}", typeof(TRequest).Name, currentUser);
    }
}


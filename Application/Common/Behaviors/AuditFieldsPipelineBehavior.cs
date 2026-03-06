//-----------------------------------------------------------------------
// <copyright file="AuditFieldsPipelineBehavior.cs" company="SMS Safety Management System">
//     Author: SMS Development Team  
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Pipeline behavior for automatically setting audit fields on commands
// </copyright>
//-----------------------------------------------------------------------

using SMS_Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace SMS_Application.Common.Behaviors;

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
        var timestamp = DateTime.UtcNow;
        var currentUserId = _currentUserService.UserCode;

        // Handle Create Commands
        if (request is ICreateCommand createCommand)
        {
            _logger.LogInformation("✅ Clean Architecture: Setting audit fields for CREATE command: {CommandType}, User: {UserId}", 
                typeof(TRequest).Name, currentUserId);
                
            createCommand.SetCreatedBy(currentUserId, timestamp);
        }
        // Handle Update Commands  
        else if (request is IUpdateCommand updateCommand)
        {
            _logger.LogInformation("✅ Clean Architecture: Setting audit fields for UPDATE command: {CommandType}, User: {UserId}", 
                typeof(TRequest).Name, currentUserId);
                
            updateCommand.SetUpdatedBy(currentUserId, timestamp);
        }
        // Handle Delete Commands
        else if (request is IDeleteCommand deleteCommand)
        {
            _logger.LogInformation("✅ Clean Architecture: Setting audit fields for DELETE command: {CommandType}, User: {UserId}", 
                typeof(TRequest).Name, currentUserId);
                
            deleteCommand.SetDeletedBy(currentUserId, timestamp);
        }

        return await next();
    }
}
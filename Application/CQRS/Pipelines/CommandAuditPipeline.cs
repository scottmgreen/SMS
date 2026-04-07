//-----------------------------------------------------------------------
// <copyright file="CommandAuditPipeline.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Pipeline behavior for auditing command (write) operations
//                  Tracks all data modification for compliance and security monitoring
//                  UPDATED: Now uses feature flags to control audit verbosity
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using SMS_Application.Interfaces;
using SMS_Application.Common;

namespace SMS_Application.Messaging.Pipelines;

/// <summary>
/// Pipeline behavior that audits all command operations implementing ICreateCommand, IUpdateCommand, IDeleteCommand
/// Provides comprehensive audit trail for data modification operations
/// UPDATED: Now respects EnableCommandAudit feature flag to reduce chattiness
/// </summary>
public class CommandAuditPipeline<TRequest, TResult> : IPipeline<TRequest, TResult>
    where TRequest : IRequest<TResult>
    where TResult : Result
{
    private readonly ICommandAccessAuditService _commandAuditService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFeatureManager _featureManager;
    private readonly ILogger<CommandAuditPipeline<TRequest, TResult>> _logger;

    public CommandAuditPipeline(
        ICommandAccessAuditService commandAuditService,
        ICurrentUserService currentUserService,
        IFeatureManager featureManager,
        ILogger<CommandAuditPipeline<TRequest, TResult>> logger)
    {
        _commandAuditService = commandAuditService ?? throw new ArgumentNullException(nameof(commandAuditService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
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

        // Check if command auditing is enabled via feature flag
        var isCommandAuditEnabled = await _featureManager.IsEnabledAsync("EnableCommandAudit");

        // Only audit commands if feature is enabled AND request implements command interfaces
        if (isCommandAuditEnabled && EntityInformationExtractor.IsAuditableCommand(request))
        {
            var commandAction = EntityInformationExtractor.GetActionType(request);
            var resourceIdentifier = EntityInformationExtractor.GetResourceIdentifier(request);
            
            _logger.LogDebug("?? Command Audit: Processing {CommandAction} command {CommandType} by user {UserId}", 
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
                        resourceIdentifier,
                        commandAction,
                        cancellationToken);

                    _logger.LogDebug("? Command Audit: Successfully audited {CommandAction} {CommandType} by {UserId}", 
                        commandAction, commandType, currentUserId);
                }
                else
                {
                    // Still log failed commands for security monitoring
                    await _commandAuditService.LogCommandExecutionAsync(
                        currentUserId,
                        commandType,
                        resourceIdentifier,
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
                    resourceIdentifier,
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
            // Command auditing disabled or not an auditable command - just execute without audit logging
            _logger.LogTrace("?? Command Audit: Skipping audit - Feature: {Enabled}, IsAuditable: {IsAuditable}", 
                isCommandAuditEnabled, EntityInformationExtractor.IsAuditableCommand(request));
            return await next().ConfigureAwait(false);
        }
    }
}
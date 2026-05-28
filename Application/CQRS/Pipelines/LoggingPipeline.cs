//-----------------------------------------------------------------------
// <copyright file="LoggingPipeline.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Single-responsibility logging pipeline for request/response monitoring.
//                  Refactored to remove audit functionality and focus only on logging.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Infrastructure.Configuration;
using SMS_Application.Interfaces;
using SMS_Application.Common;

namespace SMS_Application.Pipelines;

/// <summary>
/// Single-responsibility logging pipeline for request/response monitoring and performance tracking
/// Does NOT handle audit logging - that's handled by separate audit pipelines
/// </summary>
public class LoggingPipeline<TRequest, TResult> : IBasePipeline<TRequest, TResult> 
    where TRequest : IRequest<TResult> 
    where TResult : Result
{
    private readonly ILogger<LoggingPipeline<TRequest, TResult>> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly string _logHeader;

    public LoggingPipeline(
        ILogger<LoggingPipeline<TRequest, TResult>> logger,
        ICurrentUserService currentUserService,
        LogSupport logSupport)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _logHeader = logSupport?.GenerateLogHeaderWithTimestamp() ?? $"[SMS-{Environment.MachineName}]";
    }

    public async Task<TResult> HandleAsync(TRequest request, RequestPipelineDelegate<TResult> next, CancellationToken cancellation = default)
    {
        cancellation.ThrowIfCancellationRequested();

        var commandType = request.GetType().Name;
        var currentUserId = _currentUserService.UserCode;
        var correlationId = Guid.NewGuid().ToString("N")[..8]; // Short correlation ID for tracking
        var startTimestamp = TimeProvider.System.GetTimestamp();

        // Enhanced pre-execution logging
        _logger.LogApplicationInformation("Request Logging: {LogHeader} PreExecute => {CommandType} | User: {UserId} | Correlation: {CorrelationId} | Time: {Timestamp}", 
            _logHeader, commandType, currentUserId, correlationId, DateTime.UtcNow.ToString("HH:mm:ss.fff"));

        try
        {
            // Execute the command handler
            var result = await next().ConfigureAwait(false);

            var elapsedTime = TimeProvider.System.GetElapsedTime(startTimestamp);

            // Enhanced post-execution logging based on result
            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Request Logging: {LogHeader} PostExecute => {CommandType} | SUCCESS | Duration: {Duration}ms | Correlation: {CorrelationId}", 
                    _logHeader, commandType, elapsedTime.TotalMilliseconds, correlationId);

                // Log performance warnings for slow operations
                if (elapsedTime.TotalMilliseconds > 5000) // 5 second threshold
                {
                    _logger.LogApplicationWarning("Performance Warning: {CommandType} took {Duration}ms (>5s) | User: {UserId} | Correlation: {CorrelationId}", 
                        commandType, elapsedTime.TotalMilliseconds, currentUserId, correlationId);
                }
            }
            else
            {
                _logger.LogApplicationError("Request Logging: {LogHeader} PostExecute => {CommandType} | FAILED | Duration: {Duration}ms | Error: {ErrorMessage} | Correlation: {CorrelationId}", 
                    _logHeader, commandType, elapsedTime.TotalMilliseconds, result.Error?.Message ?? "Unknown error", correlationId);
            }

            // Log additional context for important commands
            LogAdditionalContext(request, result, correlationId, elapsedTime);

            return result;
        }
        catch (OperationCanceledException)
        {
            var elapsedTime = TimeProvider.System.GetElapsedTime(startTimestamp);
            _logger.LogApplicationWarning("Request Logging: {LogHeader} Cancelled => {CommandType} | Duration: {Duration}ms | User: {UserId} | Correlation: {CorrelationId}", 
                _logHeader, commandType, elapsedTime.TotalMilliseconds, currentUserId, correlationId);
            throw;
        }
        catch (Exception ex)
        {
            var elapsedTime = TimeProvider.System.GetElapsedTime(startTimestamp);
            _logger.LogApplicationCritical(ex, "Request Logging: {LogHeader} Exception => {CommandType} | Duration: {Duration}ms | User: {UserId} | Correlation: {CorrelationId} | Exception: {ExceptionMessage}", 
                _logHeader, commandType, elapsedTime.TotalMilliseconds, currentUserId, correlationId, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Log additional context for important commands (performance and business impact logging only)
    /// </summary>
    private void LogAdditionalContext(TRequest request, TResult result, string correlationId, TimeSpan duration)
    {
        try
        {
            // Only log detailed context for important commands to avoid noise
            if (!EntityInformationExtractor.IsAuditableCommand(request) && !EntityInformationExtractor.IsReadQuery(request))
                return;

            var commandType = request.GetType().Name;
            var actionType = EntityInformationExtractor.GetActionType(request);
            var entityInfo = EntityInformationExtractor.GetEntityInfo(request);

            _logger.LogApplicationInformation("Request Context: {CommandType} | Action: {ActionType} | Entity: {EntityInfo} | Success: {IsSuccess} | Duration: {Duration}ms | Correlation: {CorrelationId}", 
                commandType, actionType, entityInfo, result.IsSuccess, duration.TotalMilliseconds, correlationId);

            // Log business impact for critical operations
            if (EntityInformationExtractor.IsBusinessCriticalCommand(commandType))
            {
                _logger.LogApplicationInformation("Business Impact: {CommandType} | User: {UserId} | Entity: {EntityInfo} | Correlation: {CorrelationId}", 
                    commandType, _currentUserService.UserCode, entityInfo, correlationId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogApplicationWarning(ex, "Error logging additional context for {CommandType} | Correlation: {CorrelationId}", 
                request.GetType().Name, correlationId);
        }
    }
}







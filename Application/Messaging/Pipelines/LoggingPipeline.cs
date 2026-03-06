//-----------------------------------------------------------------------
// <copyright file="LoggingPipeline.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enhanced logging pipeline for comprehensive request/response monitoring.
//                  Implements cross-cutting concerns in the request/response pipeline.
//                  Handles logging, auditing, validation, and other aspects.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Infrastructure.Configuration;
using SMS_Application.Interfaces;

namespace SMS_Application.Messaging.Pipelines;

/// <summary>
/// Enhanced logging pipeline with comprehensive request/response monitoring and performance tracking
/// </summary>
public class LoggingPipeline<TRequest, TResult> : IPipeline<TRequest, TResult> 
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
        _logger.LogInformation("✅ Clean Architecture: {LogHeader} PreExecute => {CommandType} | User: {UserId} | Correlation: {CorrelationId} | Time: {Timestamp}", 
            _logHeader, commandType, currentUserId, correlationId, DateTime.UtcNow.ToString("HH:mm:ss.fff"));

        try
        {
            // Execute the command handler
            var result = await next().ConfigureAwait(false);

            var elapsedTime = TimeProvider.System.GetElapsedTime(startTimestamp);

            // Enhanced post-execution logging based on result
            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Clean Architecture: {LogHeader} PostExecute => {CommandType} | SUCCESS | Duration: {Duration}ms | Correlation: {CorrelationId}", 
                    _logHeader, commandType, elapsedTime.TotalMilliseconds, correlationId);

                // Log performance warnings for slow operations
                if (elapsedTime.TotalMilliseconds > 5000) // 5 second threshold
                {
                    _logger.LogWarning("⚠️ Performance Warning: {CommandType} took {Duration}ms (>5s) | User: {UserId} | Correlation: {CorrelationId}", 
                        commandType, elapsedTime.TotalMilliseconds, currentUserId, correlationId);
                }
            }
            else
            {
                _logger.LogError("❌ Clean Architecture: {LogHeader} PostExecute => {CommandType} | FAILED | Duration: {Duration}ms | Error: {ErrorMessage} | Correlation: {CorrelationId}", 
                    _logHeader, commandType, elapsedTime.TotalMilliseconds, result.Error?.Message ?? "Unknown error", correlationId);
            }

            // Log additional context for audit commands
            LogAuditCommandContext(request, result, correlationId, elapsedTime);

            return result;
        }
        catch (OperationCanceledException)
        {
            var elapsedTime = TimeProvider.System.GetElapsedTime(startTimestamp);
            _logger.LogWarning("🚫 Clean Architecture: {LogHeader} Cancelled => {CommandType} | Duration: {Duration}ms | User: {UserId} | Correlation: {CorrelationId}", 
                _logHeader, commandType, elapsedTime.TotalMilliseconds, currentUserId, correlationId);
            throw;
        }
        catch (Exception ex)
        {
            var elapsedTime = TimeProvider.System.GetElapsedTime(startTimestamp);
            _logger.LogCritical(ex, "💥 Clean Architecture: {LogHeader} Exception => {CommandType} | Duration: {Duration}ms | User: {UserId} | Correlation: {CorrelationId} | Exception: {ExceptionMessage}", 
                _logHeader, commandType, elapsedTime.TotalMilliseconds, currentUserId, correlationId, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Log additional context for auditable commands
    /// </summary>
    private void LogAuditCommandContext(TRequest request, TResult result, string correlationId, TimeSpan duration)
    {
        try
        {
            // Only log detailed context for auditable commands to avoid noise
            if (request is not IAuditableCommand)
                return;

            var commandType = request.GetType().Name;
            var auditEventType = GetAuditEventType(request);
            var entityInfo = ExtractEntityInfo(request);

            _logger.LogInformation("📋 Clean Architecture: Audit Context => {CommandType} | Event: {EventType} | Entity: {EntityInfo} | Success: {IsSuccess} | Duration: {Duration}ms | Correlation: {CorrelationId}", 
                commandType, auditEventType, entityInfo, result.IsSuccess, duration.TotalMilliseconds, correlationId);

            // Log business impact for critical operations
            if (IsBusinessCriticalCommand(commandType))
            {
                _logger.LogInformation("🏢 Clean Architecture: Business Impact => {CommandType} | User: {UserId} | Entity: {EntityInfo} | Correlation: {CorrelationId}", 
                    commandType, _currentUserService.UserCode, entityInfo, correlationId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Error logging audit context for {CommandType} | Correlation: {CorrelationId}", 
                request.GetType().Name, correlationId);
        }
    }

    /// <summary>
    /// Get audit event type based on command characteristics
    /// </summary>
    private static string GetAuditEventType(TRequest request)
    {
        return request switch
        {
            ICreateCommand => "CREATE",
            IUpdateCommand => "UPDATE",
            IDeleteCommand => "DELETE",
            _ when request.GetType().Name.Contains("Validate") => "VALIDATE",
            _ when request.GetType().Name.Contains("Authenticate") => "AUTH",
            _ => "ACTION"
        };
    }

    /// <summary>
    /// Extract entity information for logging context
    /// </summary>
    private static string ExtractEntityInfo(TRequest request)
    {
        try
        {
            var properties = request.GetType().GetProperties();
            var keyProps = properties.Where(p => 
                p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) || 
                p.Name.EndsWith("Code", StringComparison.OrdinalIgnoreCase) ||
                p.Name.Equals("UserName", StringComparison.OrdinalIgnoreCase)).ToList();
            
            if (keyProps.Any())
            {
                var values = keyProps.Take(3) // Limit to first 3 to avoid log bloat
                    .Select(p => $"{p.Name}={p.GetValue(request)}")
                    .Where(v => !string.IsNullOrEmpty(v));
                return string.Join(", ", values);
            }

            return request.GetType().Name.Replace("Command", "").Replace("Query", "");
        }
        catch
        {
            return "Unknown";
        }
    }

    /// <summary>
    /// Determine if command has significant business impact
    /// </summary>
    private static bool IsBusinessCriticalCommand(string commandType)
    {
        var criticalPatterns = new[]
        {
            "Delete", "Create", "UpdateStatus", "Validate", "Authenticate", 
            "Deactivate", "Reset", "Assign", "Report", "Assessment"
        };

        return criticalPatterns.Any(pattern => 
            commandType.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }
}





//-----------------------------------------------------------------------
// <copyright file="LoggingPipeline.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application layer component providing functionality for the SMS safety management system.
//                  Implements cross-cutting concerns in the request/response pipeline.
//                  Handles logging, auditing, validation, and other aspects.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;

using SMS_Infrastructure.Configuration;

namespace SMS_Application.Messaging.Pipelines;


public class LoggingPipeline<TRequest, TResult> : IPipeline<TRequest, TResult> where TRequest : IRequest<TResult> where TResult : Result
{
    private readonly string _logheader;
    private readonly ILogger<LoggingPipeline<TRequest, TResult>> _logger;

    public LoggingPipeline(ILogger<LoggingPipeline<TRequest, TResult>> logger, LogSupport logsupport)
    {
        _logger = logger;
        _logheader = logsupport.GenerateLogHeaderWithTimestamp();
    }
    public async Task<TResult> HandleAsync(TRequest request, RequestPipelineDelegate<TResult> next, CancellationToken cancellation = default)
    {
        cancellation.ThrowIfCancellationRequested();

        var start = TimeProvider.System.GetTimestamp();
        // Log before executing the command handler
        _logger.LogApplicationInformation(ApplicationEventIds.Information, $"{_logheader} PreExecute => {request.GetType().Name} {DateTime.Now.ToString("HH:mm:ss")}");
        var result = await next();

        var diff = TimeProvider.System.GetElapsedTime(start);

        // Log after executing the command handler
        if (result.IsSuccess)
        {
            _logger.LogApplicationInformation(ApplicationEventIds.Information, $"{_logheader} PostExecute => {request.GetType().Name} {diff.TotalMilliseconds}ms");
        }
        else
        {
            _logger.LogCritical($"Logging Pipeline Validation => {result.Error.Message} {DateTime.Now.ToString("HH:mm:ss")}");
        }


        return result;
    }
}





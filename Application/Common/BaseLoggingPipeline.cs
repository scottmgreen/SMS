//-----------------------------------------------------------------------
// <copyright file="BaseLoggingPipeline.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Shared utility providing common functionality for Application layer components.
//                  Implements cross-cutting concerns in the request/response pipeline.
//                  Handles logging, auditing, validation, and other aspects.
// </copyright>
//-----------------------------------------------------------------------

//using SMS_Application.Messaging.Pipelines;
//using SMS_Domain.Common;

//using Microsoft.Extensions.Logging;

//namespace SMS_Application.Common;

//public abstract class BaseLoggingPipeline<TRequest, TResult> : IRequestPipelineBehavior<TRequest, TResult>
//    where TRequest : IRequest<TResult>
//    where TResult : Result
//{
//    private readonly ILogger<LoggingPipeline<TRequest, TResult>> _logger;
//    private readonly string _logheader;
//    public ILogger<LoggingPipeline<TRequest, TResult>> Logger => _logger;
//    public string LogHeader => _logheader;

//    public BaseLoggingPipeline(ILogger<LoggingPipeline<TRequest, TResult>> logger, string logheader)
//    {
//        _logger = logger;
//        _logheader = logheader;
//    }

//    public Task<TResult> HandleAsync(TRequest request, RequestPipelineDelegate<TResult> next, CancellationToken cancellationToken)
//    {
//        throw new NotImplementedException();
//    }
//}

//using CBT3_Application.Messaging.Pipelines;
//using CBT3_Domain.Common;

//using Microsoft.Extensions.Logging;

//namespace CBT3_Application.Common;

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
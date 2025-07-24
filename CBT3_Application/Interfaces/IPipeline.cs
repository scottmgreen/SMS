


using CBT3_Domain.Common;
namespace CBT3_Application.Interfaces;

public interface IPipeline<in TRequest,TResult> where TRequest :notnull
{
    Task<TResult> HandleAsync(TRequest request, RequestPipelineDelegate<TResult> next,CancellationToken cancellationToken );
}


public delegate Task<TResult> RequestPipelineDelegate<TResult>();
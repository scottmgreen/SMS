

namespace SMS_Application.Interfaces;

public interface IRequestHandler<in TRequest> where TRequest : class, IRequest
{
    Task HandleAsync(TRequest request, CancellationToken cancellation = default);
    
}
public interface IRequestHandler<in TRequest, TResponse> where TRequest : class, IRequest<TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellation = default);
    
}


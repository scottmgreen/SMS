namespace CBT3_Application.Interfaces;

public interface IMediator
{
    // Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request);

    //Task SendAsync<TRequest>(TRequest request, CancellationToken cancellation = default) where TRequest : class, IRequest;

    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellation);
}

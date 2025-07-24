//-----------------------------------------------------------------------
// <copyright file="MediatorService.cs" company="">
//     Author: Scott Green
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace CBT3_Application.Services
{

    public sealed class Mediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;
        public Mediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellation)
        {

            var behaviors = _serviceProvider.GetServices<IPipeline<IRequest<TResponse>, TResponse>>().ToList();

            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var handler = _serviceProvider.GetRequiredService(handlerType);
            var methodInfo = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.HandleAsync));

            if (!behaviors.Any())
            {
                return await (Task<TResponse>)methodInfo!.Invoke(handler, new object[] { request, cancellation }!);
            }

            RequestPipelineDelegate<TResponse> next = () => (Task<TResponse>)methodInfo!.Invoke(handler, new object[] { request, cancellation }!);

            behaviors.Reverse();

            foreach (var behavior in behaviors)
            {
                var currentNext = next;
                next = () => behavior.HandleAsync(request, currentNext, cancellation);
            }

            return await next();


        }

        //public async Task SendAsync<TRequest>(TRequest request, CancellationToken cancellation) where TRequest : class, IRequest
        //{
        //    var handler = _serviceProvider.GetRequiredService<IRequestHandler<TRequest>>();
        //    await handler.HandleAsync(request, cancellation);
        //}


    }


}




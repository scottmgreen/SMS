//-----------------------------------------------------------------------
// <copyright file="MediatorService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application service providing business logic operations for SMS domain entities.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

//-----------------------------------------------------------------------
// <copyright file="MediatorService.cs" company="">
//     Author: Scott Green
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace SMS_Application.Services
{

    public sealed class Mediator : IBaseMediator
    {
        private readonly IServiceProvider _serviceProvider;
        public Mediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellation)
        {

            var behaviors = _serviceProvider.GetServices<IBasePipeline<IRequest<TResponse>, TResponse>>().ToList();

            var handlerType = typeof(IBaseRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var handler = _serviceProvider.GetRequiredService(handlerType);
            var methodInfo = handlerType.GetMethod(nameof(IBaseRequestHandler<IRequest<TResponse>, TResponse>.HandleAsync));

            if (!behaviors.Any())
            {
                return await (Task<TResponse>)methodInfo!.Invoke(handler, new object[] { request, cancellation }!);
            }

            RequestPipelineDelegate<TResponse> next = () => (Task<TResponse>)methodInfo!.Invoke(handler, new object[] { request, cancellation }!);

            // ?? FIX: Don't reverse! Keep the registration order
            // behaviors.Reverse(); // REMOVED - This was causing audit fields to set AFTER command execution

            foreach (var behavior in behaviors)
            {
                var currentNext = next;
                next = () => behavior.HandleAsync(request, currentNext, cancellation);
            }

            return await next();


        }

        //public async Task SendAsync<TRequest>(TRequest request, CancellationToken cancellation) where TRequest : class, IRequest
        //{
        //    var handler = _serviceProvider.GetRequiredService<IBaseRequestHandler<TRequest>>();
        //    await handler.HandleAsync(request, cancellation);
        //}


    }


}







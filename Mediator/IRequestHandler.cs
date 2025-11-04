using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBT3_Mediator;
public interface IRequestHandler<in TRequest> where TRequest : IRequest
{
    public Task HandleAsync(TRequest request, CancellationToken ct =default);

}
public interface IRequestHandler<in TRequest,TResponse> where TRequest : IRequest<TResponse>
{
    public Task<TResponse> HandleAsync(TRequest request, CancellationToken ct = default);

}

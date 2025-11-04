using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBT3_Mediator;
public interface IMediator
{
  public  Task SendAsync<TRequest>(TRequest request, CancellationToken ct = default) where TRequest : class , IRequest;

  public  Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken ct = default) ;

}

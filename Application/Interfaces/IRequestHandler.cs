//-----------------------------------------------------------------------
// <copyright file="IRequestHandler.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Interface contract defining operations and ensuring clean architecture boundaries.
//                  Defines contract for application services ensuring clean architecture
//                  boundaries and dependency inversion compliance.
// </copyright>
//-----------------------------------------------------------------------



namespace SMS_Application.Interfaces;

public interface IRequestHandler<in TRequest> where TRequest : class, IRequest
{
    Task HandleAsync(TRequest request, CancellationToken cancellation = default);

}
public interface IRequestHandler<in TRequest, TResponse> where TRequest : class, IRequest<TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellation = default);

}



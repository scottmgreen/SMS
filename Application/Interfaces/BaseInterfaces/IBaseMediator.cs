//-----------------------------------------------------------------------
// <copyright file="IBaseMediator.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Mediator contract defining request/response handling in CQRS architecture.
//                  Central interface for coordinating commands and queries through the
//                  application layer without tight coupling between components.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Interfaces;

/// <summary>
/// CQRS Mediator Interface
/// Central contract for coordinating commands and queries in the CQRS architecture.
/// Provides decoupled communication between presentation layer and business logic.
/// 
/// Methods:
/// - SendAsync: Sends requests (commands/queries) and returns responses asynchronously
/// </summary>
public interface IBaseMediator
{
    // Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request);

    //Task SendAsync<TRequest>(TRequest request, CancellationToken cancellation = default) where TRequest : class, IRequest;

    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellation);
}

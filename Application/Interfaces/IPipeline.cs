//-----------------------------------------------------------------------
// <copyright file="IPipeline.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Pipeline contract defining cross-cutting concern processing interface.
//                  Defines contract for application services ensuring clean architecture
//                  boundaries and dependency inversion compliance.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Interfaces;

public interface IPipeline<in TRequest, TResult> where TRequest : notnull
{
    Task<TResult> HandleAsync(TRequest request, RequestPipelineDelegate<TResult> next, CancellationToken cancellationToken);
}


public delegate Task<TResult> RequestPipelineDelegate<TResult>();

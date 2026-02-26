//-----------------------------------------------------------------------
// <copyright file="IRequest.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Interface contract defining operations and ensuring clean architecture boundaries.
//                  Defines contract for application services ensuring clean architecture
//                  boundaries and dependency inversion compliance.
// </copyright>
//-----------------------------------------------------------------------



namespace SMS_Application.Interfaces;

public interface IRequest<in TResponse> : IBaseRequest { }
public interface IRequest { }


//-----------------------------------------------------------------------
// <copyright file="ICurrentUserService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application service providing business logic operations for SMS domain entities.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Interfaces;

/// <summary>
/// Service to get current user information for audit trails
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's ID/Name for audit purposes
    /// </summary>
    string UserId { get; }

    /// <summary>
    /// Gets the current user's display name
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Indicates if a user is currently authenticated
    /// </summary>
    bool IsAuthenticated { get; }
}

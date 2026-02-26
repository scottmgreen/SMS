//-----------------------------------------------------------------------
// <copyright file="ISMSSessionService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application service providing business logic operations for SMS domain entities.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Interfaces;

/// <summary>
/// SMS Session Management Service - Direct Session Handling
/// Uses SMS Domain Entities directly, no wrapper models
/// </summary>
public interface ISMSSessionService
{
    /// <summary>
    /// Creates SMS session using Domain Entity data directly
    /// Same exact logic as Login.cshtml.cs
    /// </summary>
    /// <param name="user">SMS Domain Entity (BaseUser)</param>
    /// <param name="userType">SMS User Type (Smart Enum)</param>
    Task CreateSMSSessionAsync(BaseUser user, SMSUserType userType);

    /// <summary>
    /// Clears SMS session data
    /// </summary>
    Task ClearSMSSessionAsync();

    /// <summary>
    /// Checks if current session is authenticated
    /// </summary>
    bool IsAuthenticated();

    /// <summary>
    /// Gets current user ID from session
    /// </summary>
    string? GetCurrentUserId();

    /// <summary>
    /// Gets current user display name from session
    /// </summary>
    string? GetCurrentUserDisplayName();

    /// <summary>
    /// Gets current user type from session
    /// </summary>
    string? GetCurrentUserType();
}

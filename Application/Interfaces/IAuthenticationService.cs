//-----------------------------------------------------------------------
// <copyright file="IAuthenticationService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Authentication service interface for user authentication operations.
//                  Provides presentation-layer independent authentication logic.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS_Application.Interfaces;

/// <summary>
/// Authentication service interface providing presentation-layer independent authentication
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user by email and password, discovering the user type automatically
    /// </summary>
    /// <param name="email">User email address</param>
    /// <param name="password">Plain text password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication result containing user entity and type if successful</returns>
    Task<AuthenticationResult> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default);
}

/// <summary>
/// Authentication result containing user information and authentication status
/// </summary>
public class AuthenticationResult
{
    public bool IsSuccess { get; set; }
    public BaseUser? User { get; set; }
    public SMSUserType UserType { get; set; }
    public string? ErrorMessage { get; set; }

    public static AuthenticationResult Success(BaseUser user, SMSUserType userType)
        => new() { IsSuccess = true, User = user, UserType = userType };

    public static AuthenticationResult Failure(string errorMessage)
        => new() { IsSuccess = false, ErrorMessage = errorMessage };
}
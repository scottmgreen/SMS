//-----------------------------------------------------------------------
// <copyright file="ICurrentUserService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enhanced interface for current user context with full authentication state support.
//                  Provides comprehensive user information including roles and permissions.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Interfaces;

/// <summary>
/// Enhanced Current User Service Interface - Matches AuthenticationState functionality
/// Provides full user context including authentication state, roles, and permissions
/// </summary>
public interface ICurrentUserService
{
    #region Basic Auth Properties
    
    /// <summary>
    /// Check if current user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }
    
    /// <summary>
    /// Current user code (UserId)
    /// </summary>
    string UserCode { get; }
    
    /// <summary>
    /// Current user type (Application, Organizational, Stakeholder)
    /// </summary>
    string? UserType { get; }
    
    /// <summary>
    /// Current user display name
    /// </summary>
    string UserDisplayName { get; }
    
    /// <summary>
    /// Current user email
    /// </summary>
    string? Email { get; }
    
    /// <summary>
    /// Current user first name
    /// </summary>
    string? FirstName { get; }
    
    /// <summary>
    /// Current user last name
    /// </summary>
    string? LastName { get; }
    
    /// <summary>
    /// Login timestamp
    /// </summary>
    DateTime? LoginTime { get; }

    #endregion

    #region Full User and Role Access

    /// <summary>
    /// Current user role code
    /// </summary>
    string? UserRoleCode { get; }

    /// <summary>
    /// Current user role name
    /// </summary>
    string? UserRoleName { get; }

    /// <summary>
    /// Current user permissions
    /// </summary>
    List<SMSUserRolePermission> Permissions { get; }

    #endregion

    #region Permission Check Methods (Same as AuthenticationState)

    /// <summary>
    /// Check if user can CREATE in a specific module
    /// </summary>
    bool CanCreate(string module);

    /// <summary>
    /// Check if user can READ in a specific module
    /// </summary>
    bool CanRead(string module);

    /// <summary>
    /// Check if user can UPDATE in a specific module
    /// </summary>
    bool CanUpdate(string module);

    /// <summary>
    /// Check if user can DELETE in a specific module
    /// </summary>
    bool CanDelete(string module);

    /// <summary>
    /// Get user type as Smart Enum
    /// </summary>
    SMSUserType? GetUserTypeEnum();

    /// <summary>
    /// Get all modules the user has access to
    /// </summary>
    List<string> GetAccessibleModules();

    /// <summary>
    /// Clear authentication for logout
    /// </summary>
    Task ClearAuthentication();

    #endregion

    #region 2FA State Management

    /// <summary>
    /// Check if user has 2FA enabled (requires 2FA verification)
    /// </summary>
    bool RequiresTwoFactorAuth { get; }

    /// <summary>
    /// Check if user is currently pending 2FA verification (password authenticated but 2FA not verified)
    /// </summary>
    bool IsPending2FAVerification { get; }

    /// <summary>
    /// Check if user is fully authenticated (password + 2FA verified, or 2FA not required)
    /// </summary>
    bool IsFullyAuthenticated { get; }

    #endregion
}

//-----------------------------------------------------------------------
// <copyright file="IAuthorizationService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application layer authorization service interface for presentation-independent permission checking.
//                  Provides clean separation between authorization logic and presentation layer.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS_Application.Interfaces;

/// <summary>
/// Application layer authorization service providing presentation-independent permission checking.
/// Handles all authorization logic that was previously scattered across presentation components.
/// </summary>
public interface IAuthorizationService
{
    #region Basic Permission Checks

    /// <summary>
    /// Check if user can CREATE in a specific module
    /// </summary>
    Task<bool> CanCreateAsync(string userId, string module, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user can READ in a specific module
    /// </summary>
    Task<bool> CanReadAsync(string userId, string module, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user can UPDATE in a specific module
    /// </summary>
    Task<bool> CanUpdateAsync(string userId, string module, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user can DELETE in a specific module
    /// </summary>
    Task<bool> CanDeleteAsync(string userId, string module, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user has ANY permission in a module
    /// </summary>
    Task<bool> CanAccessAsync(string userId, string module, CancellationToken cancellationToken = default);

    #endregion

    #region Advanced Permission Checks

    /// <summary>
    /// Get user type as Smart Enum
    /// </summary>
    Task<SMSUserType?> GetUserTypeAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all modules the user has access to
    /// </summary>
    Task<List<string>> GetAccessibleModulesAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user's full permission list
    /// </summary>
    Task<List<SMSUserRolePermission>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default);

    #endregion

    #region User Context

    /// <summary>
    /// Get complete user authorization context
    /// </summary>
    Task<UserAuthorizationContext?> GetUserContextAsync(string userId, CancellationToken cancellationToken = default);

    #endregion
}

/// <summary>
/// User authorization context containing all authorization-related information
/// </summary>
public class UserAuthorizationContext
{
    public string UserId { get; set; } = string.Empty;
    public string UserDisplayName { get; set; } = string.Empty;
    public string? UserType { get; set; }
    public string? UserRoleCode { get; set; }
    public string? UserRoleName { get; set; }
    public List<SMSUserRolePermission> Permissions { get; set; } = new();
    public bool IsAuthenticated { get; set; }

    // Convenience methods
    public bool CanCreate(string module) => IsAuthenticated && Permissions.Any(p => p.SMSModule == module && p.Create);
    public bool CanRead(string module) => IsAuthenticated && Permissions.Any(p => p.SMSModule == module && p.Read);
    public bool CanUpdate(string module) => IsAuthenticated && Permissions.Any(p => p.SMSModule == module && p.Update);
    public bool CanDelete(string module) => IsAuthenticated && Permissions.Any(p => p.SMSModule == module && p.Delete);
    public bool CanAccess(string module) => IsAuthenticated && Permissions.Any(p => p.SMSModule == module && (p.Create || p.Read || p.Update || p.Delete));
    
    public List<string> GetAccessibleModules() => IsAuthenticated 
        ? Permissions.Where(p => p.Read || p.Create || p.Update || p.Delete)
                    .Select(p => p.SMSModule ?? "Unknown")
                    .Distinct()
                    .ToList()
        : new List<string>();
}
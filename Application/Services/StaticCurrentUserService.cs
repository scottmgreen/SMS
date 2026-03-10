//-----------------------------------------------------------------------
// <copyright file="StaticCurrentUserService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Static current user service that doesn't depend on HttpContext or Sessions
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;

namespace SMS_Application.Services;

/// <summary>
/// Static Current User Service - No HttpContext or Session dependencies
/// Uses static storage for authentication state when HttpContext is not available
/// </summary>
public class StaticCurrentUserService : ICurrentUserService
{
    private readonly ILogger<StaticCurrentUserService> _logger;
    
    // Static storage for authentication state
    private static string? _currentUserId;
    private static string? _currentUserType;
    private static string? _currentDisplayName;
    private static string? _currentFirstName;
    private static string? _currentLastName;
    private static string? _currentEmail;
    private static string? _currentUserRoleCode;
    private static string? _currentUserRoleName;
    private static List<SMSUserRolePermission>? _currentPermissions;
    private static DateTime? _loginTime;
    private static bool _isAuthenticated = false;

    public StaticCurrentUserService(ILogger<StaticCurrentUserService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Basic Auth Properties

    public bool IsAuthenticated => _isAuthenticated;

    public string UserCode => _currentUserId ?? "SYSTEM";

    public string? UserType => _currentUserType;

    public string UserDisplayName => _currentDisplayName ?? "System User";

    public string? Email => _currentEmail;

    public string? FirstName => _currentFirstName;

    public string? LastName => _currentLastName;

    public DateTime? LoginTime => _loginTime;

    #endregion

    #region Full User and Role Access

    public string? UserRoleCode => _currentUserRoleCode;

    public string? UserRoleName => _currentUserRoleName;

    public List<SMSUserRolePermission> Permissions => _currentPermissions ?? new List<SMSUserRolePermission>();

    #endregion

    #region Permission Check Methods

    public bool CanCreate(string module)
    {
        return IsAuthenticated && Permissions.Any(p => p.SMSModule == module && p.Create);
    }

    public bool CanRead(string module)
    {
        return IsAuthenticated && Permissions.Any(p => p.SMSModule == module && p.Read);
    }

    public bool CanUpdate(string module)
    {
        return IsAuthenticated && Permissions.Any(p => p.SMSModule == module && p.Update);
    }

    public bool CanDelete(string module)
    {
        return IsAuthenticated && Permissions.Any(p => p.SMSModule == module && p.Delete);
    }

    public SMSUserType? GetUserTypeEnum()
    {
        if (!IsAuthenticated || string.IsNullOrEmpty(UserType))
            return null;

        return SMSUserType.FromValue(UserType);
    }

    public List<string> GetAccessibleModules()
    {
        if (!IsAuthenticated) return new List<string>();
        
        var permissions = Permissions;
        return permissions
            .Where(p => p.Read || p.Create || p.Update || p.Delete)
            .Select(p => p.SMSModule ?? "Unknown")
            .Distinct()
            .ToList();
    }

    #endregion

    #region Static Methods for Setting Authentication State

    public static void SetAuthenticationState(BaseUser user, SMSUserType userType)
    {
        _currentUserId = user.Code;
        _currentUserType = userType.Value;
        _currentDisplayName = user.DisplayName;
        _currentFirstName = user.FirstName.Value;
        _currentLastName = user.LastName.Value;
        _currentEmail = user.UserName.Value;
        _loginTime = DateTime.UtcNow;
        _isAuthenticated = true;

        if (user.UserRole != null)
        {
            _currentUserRoleCode = user.UserRole.Code;
            _currentUserRoleName = user.UserRole.Name;

            // Convert permissions
            if (user.UserRole.Permissions != null && user.UserRole.Permissions.Any())
            {
                _currentPermissions = user.UserRole.Permissions.ToList();
            }
        }
    }

    public static void ClearAuthenticationState()
    {
        _currentUserId = null;
        _currentUserType = null;
        _currentDisplayName = null;
        _currentFirstName = null;
        _currentLastName = null;
        _currentEmail = null;
        _currentUserRoleCode = null;
        _currentUserRoleName = null;
        _currentPermissions = null;
        _loginTime = null;
        _isAuthenticated = false;
    }

    #endregion

    public async Task ClearAuthentication()
    {
        ClearAuthenticationState();
    }
}
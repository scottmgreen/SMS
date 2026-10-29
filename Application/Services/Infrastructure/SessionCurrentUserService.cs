//-----------------------------------------------------------------------
// <copyright file="SessionCurrentUserService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Session-based current user service that replaces StaticCurrentUserService
//                  Uses HttpContext.Session for secure, per-user authentication state
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System.Security.Claims;
using System.Threading;

namespace SMS_Application.Services;

/// <summary>
/// Session-Based Current User Service - Secure session-based authentication
/// Replaces StaticCurrentUserService with proper session isolation and security
/// Uses exact same interface and permission logic as StaticCurrentUserService
/// ENHANCED: Now supports circuit-based storage for Blazor Server
/// </summary>
public class SessionCurrentUserService : ICurrentUserService
{
    private static readonly ConcurrentDictionary<string, Dictionary<string, string>> _sessionAuthSnapshots = new(StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<string, Dictionary<string, string>> _userAuthSnapshots = new(StringComparer.OrdinalIgnoreCase);
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SessionCurrentUserService> _logger;
    private readonly ISMSSessionService _smsSessionService;
    private readonly Dictionary<string, string> _sessionSnapshot = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _lastReadSourceByKey = new(StringComparer.Ordinal);
    private readonly object _permissionsCacheLock = new();
    private string? _cachedPermissionsRaw;
    private string? _cachedPermissionsRoleCode;
    private List<SMSUserRolePermission>? _cachedPermissions;
    private bool? _lastLoggedPending2FaState;

    public SessionCurrentUserService(
        IHttpContextAccessor httpContextAccessor, 
        ILogger<SessionCurrentUserService> logger,
        ISMSSessionService smsSessionService)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _smsSessionService = smsSessionService ?? throw new ArgumentNullException(nameof(smsSessionService));
    }

    public static void UpsertSessionSnapshot(string sessionId, IReadOnlyDictionary<string, string> values)
    {
        if (string.IsNullOrWhiteSpace(sessionId) || values == null || values.Count == 0)
        {
            return;
        }

        _sessionAuthSnapshots.AddOrUpdate(
            sessionId,
            _ => new Dictionary<string, string>(values, StringComparer.Ordinal),
            (_, existing) =>
            {
                foreach (var kvp in values)
                {
                    existing[kvp.Key] = kvp.Value;
                }

                return existing;
            });

        if (values.TryGetValue("SMS_UserCode", out var userCode) && !string.IsNullOrWhiteSpace(userCode))
        {
            _userAuthSnapshots.AddOrUpdate(
                userCode,
                _ => new Dictionary<string, string>(values, StringComparer.Ordinal),
                (_, existing) =>
                {
                    foreach (var kvp in values)
                    {
                        existing[kvp.Key] = kvp.Value;
                    }

                    return existing;
                });
        }
    }

    public static void RemoveSessionSnapshot(string? sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return;
        }

        if (_sessionAuthSnapshots.TryRemove(sessionId, out var removedSnapshot)
            && removedSnapshot != null
            && removedSnapshot.TryGetValue("SMS_UserCode", out var userCode)
            && !string.IsNullOrWhiteSpace(userCode))
        {
            _userAuthSnapshots.TryRemove(userCode, out _);
        }
    }

    public static void ClearAllSessionSnapshots()
    {
        _sessionAuthSnapshots.Clear();
        _userAuthSnapshots.Clear();
    }

    public void HydrateFromAuthPayload(IReadOnlyDictionary<string, string> values)
    {
        if (values == null || values.Count == 0)
        {
            return;
        }

        foreach (var kvp in values)
        {
            if (!string.IsNullOrWhiteSpace(kvp.Key) && !string.IsNullOrWhiteSpace(kvp.Value))
            {
                _sessionSnapshot[kvp.Key] = kvp.Value;
            }
        }
    }

    public sealed class AuthStateDiagnostics
    {
        public bool SessionAvailable { get; init; }
        public string? SessionId { get; init; }
        public bool SessionHasIsAuthenticated { get; init; }
        public bool SessionHasPermissions { get; init; }
        public bool LocalSnapshotHasIsAuthenticated { get; init; }
        public bool LocalSnapshotHasPermissions { get; init; }
        public bool ConcurrentSnapshotHasIsAuthenticated { get; init; }
        public bool ConcurrentSnapshotHasPermissions { get; init; }
        public int ConcurrentSnapshotFieldCount { get; init; }
        public string IsAuthenticatedReadSource { get; init; } = "none";
        public string PermissionsReadSource { get; init; } = "none";
    }

    public AuthStateDiagnostics GetAuthStateDiagnostics()
    {
        var context = _httpContextAccessor.HttpContext;
        var session = TryGetSession(context);
        var sessionId = session?.Id;

        var sessionHasIsAuthenticated = !string.IsNullOrWhiteSpace(session?.GetString("IsAuthenticated"));
        var sessionHasPermissions = !string.IsNullOrWhiteSpace(session?.GetString("SMS_UserPermissions"));

        var localSnapshotHasIsAuthenticated = _sessionSnapshot.TryGetValue("IsAuthenticated", out var localAuth)
                                            && !string.IsNullOrWhiteSpace(localAuth);
        var localSnapshotHasPermissions = _sessionSnapshot.TryGetValue("SMS_UserPermissions", out var localPerms)
                                        && !string.IsNullOrWhiteSpace(localPerms);

        var concurrentSnapshotHasIsAuthenticated = false;
        var concurrentSnapshotHasPermissions = false;
        var concurrentSnapshotFieldCount = 0;

        if (!string.IsNullOrWhiteSpace(sessionId) && _sessionAuthSnapshots.TryGetValue(sessionId, out var concurrentSnapshot))
        {
            concurrentSnapshotFieldCount = concurrentSnapshot.Count;
            concurrentSnapshotHasIsAuthenticated = concurrentSnapshot.TryGetValue("IsAuthenticated", out var concurrentAuth)
                                                && !string.IsNullOrWhiteSpace(concurrentAuth);
            concurrentSnapshotHasPermissions = concurrentSnapshot.TryGetValue("SMS_UserPermissions", out var concurrentPerms)
                                            && !string.IsNullOrWhiteSpace(concurrentPerms);
        }

        return new AuthStateDiagnostics
        {
            SessionAvailable = session != null,
            SessionId = sessionId,
            SessionHasIsAuthenticated = sessionHasIsAuthenticated,
            SessionHasPermissions = sessionHasPermissions,
            LocalSnapshotHasIsAuthenticated = localSnapshotHasIsAuthenticated,
            LocalSnapshotHasPermissions = localSnapshotHasPermissions,
            ConcurrentSnapshotHasIsAuthenticated = concurrentSnapshotHasIsAuthenticated,
            ConcurrentSnapshotHasPermissions = concurrentSnapshotHasPermissions,
            ConcurrentSnapshotFieldCount = concurrentSnapshotFieldCount,
            IsAuthenticatedReadSource = _lastReadSourceByKey.TryGetValue("IsAuthenticated", out var authSource) ? authSource : "none",
            PermissionsReadSource = _lastReadSourceByKey.TryGetValue("SMS_UserPermissions", out var permSource) ? permSource : "none"
        };
    }

    #region Basic Auth Properties - Direct from Session

    public bool IsAuthenticated 
    { 
        get 
        { 
            if (SafeGetSessionBool("IsAuthenticated"))
            {
                return true;
            }

            if (!HasAuthenticatedPrincipal())
            {
                return false;
            }

            var hasAuthenticatedClaim = string.Equals(
                GetPrincipalClaimValue(_httpContextAccessor.HttpContext, "IsAuthenticated"),
                "true",
                StringComparison.OrdinalIgnoreCase);

            if (!hasAuthenticatedClaim)
            {
                return false;
            }

            var hasUserCode = !string.IsNullOrWhiteSpace(SafeGetSessionString("SMS_UserCode"));
            if (!hasUserCode)
            {
                return false;
            }

            var hasRole = !string.IsNullOrWhiteSpace(SafeGetSessionString("SMS_UserRoleCode"));
            var hasPermissions = !string.IsNullOrWhiteSpace(SafeGetSessionString("SMS_UserPermissions"));
            return hasRole || hasPermissions;
        } 
    }

    public string UserCode 
    { 
        get 
        { 
            return SafeGetSessionString("SMS_UserCode") ?? string.Empty;
        } 
    }

    public string? UserType 
    { 
        get 
        { 
            return SafeGetSessionString("SMS_UserType");
        } 
    }

    public string UserDisplayName 
    { 
        get 
        { 
            return SafeGetSessionString("SMS_DisplayName") ?? UserCode ?? SystemActorConstants.FlyPdxApiSource;
        } 
    }

    public string? Email => SafeGetSessionString("SMS_Email");

    public string? FirstName => SafeGetSessionString("SMS_FirstName");

    public string? LastName => SafeGetSessionString("SMS_LastName");

    public DateTime? LoginTime => SafeGetSessionDateTime("SMS_LoginTime");

    #endregion

    #region Full User and Role Access

    public string? UserRoleCode => SafeGetSessionString("SMS_UserRoleCode");

    public string? UserRoleName => SafeGetSessionString("SMS_UserRoleName");

    public List<SMSUserRolePermission> Permissions => SafeReconstructPermissionsFromSession();

    #endregion

    #region Permission Check Methods - Identical Logic to StaticCurrentUserService

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

    #region 2FA State Management

    /// <summary>
    /// Check if user has 2FA enabled (requires 2FA verification)
    /// </summary>
    public bool RequiresTwoFactorAuth 
    { 
        get 
        { 
            return SafeGetSessionBool("SMS_TwoFactorEnabled");
        } 
    }

    /// <summary>
    /// Check if user is currently pending 2FA verification (password authenticated but 2FA not verified)
    /// This checks if there's pending 2FA data indicating the user is in the intermediate state
    /// CRITICAL: Must distinguish between pending 2FA and full authentication
    /// </summary>
    public bool IsPending2FAVerification 
    { 
        get 
        {
            try
            {
                // FIRST: Check if user is already fully authenticated
                // If IsAuthenticated is true, we should NOT be in pending 2FA state
                if (IsAuthenticated)
                {
                    // Check for specific pending 2FA markers (not full auth data)
                    var context = _httpContextAccessor.HttpContext;
                    if (context?.Items.ContainsKey("Pending2FA_UserData") == true || 
                        context?.Items.ContainsKey("Pending2FA_UserType") == true ||
                        context?.Items.ContainsKey("Pending2FA_StoredAt") == true)
                    {
                        LogPending2FaTransition(true);
                        return true;
                    }

                    // Check session for pending 2FA specific markers
                    var pending2FAData = SafeGetSessionString("Pending2FA_UserData");
                    var pending2FAStoredAt = SafeGetSessionString("Pending2FA_StoredAt");
                    if (!string.IsNullOrEmpty(pending2FAData) || !string.IsNullOrEmpty(pending2FAStoredAt))
                    {
                        LogPending2FaTransition(true);
                        return true;
                    }

                    // If user is authenticated but no pending 2FA markers found, they are fully authenticated
                    LogPending2FaTransition(false);
                    return false;
                }

                // User is not authenticated at all, so definitely not pending 2FA
                LogPending2FaTransition(false);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogApplicationError(ex, "Error checking pending 2FA status");
                return false;
            }
        } 
    }

    /// <summary>
    /// Check if user is fully authenticated (password + 2FA verified, or 2FA not required)
    /// This is what should be used for NavMenu display logic
    /// </summary>
    public bool IsFullyAuthenticated 
    { 
        get 
        {
            // User is fully authenticated if:
            // 1. They are authenticated AND
            // 2. Either they don't require 2FA OR they are not pending 2FA verification
            return IsAuthenticated && (!RequiresTwoFactorAuth || !IsPending2FAVerification);
        } 
    }

    #endregion

    #region Session Management

    /// <summary>
    /// Clear session authentication - used for logout
    /// Clears all authentication data from session, circuit storage, and context items
    /// </summary>
    public async Task ClearAuthentication()
    {
        var context = _httpContextAccessor.HttpContext;
        var userId = GetCurrentUserId();
        
        try
        {
            _logger.LogApplicationInformation("Clearing session authentication for user: {UserId}", userId);

            // Clear session data if available
            var session = TryGetSession(context);
            if (session != null)
            {
                try
                {
                    session.Clear();
                    _logger.LogApplicationInformation("Session data cleared for user: {UserId}", userId);
                }
                catch (Exception ex)
                {
                    _logger.LogApplicationWarning(ex, "Could not clear session data for user: {UserId}", userId);
                }
            }

            // Clear HttpContext.Items
            if (context != null)
            {
                var smsKeys = context.Items.Keys
                    .Where(key => key is string keyStr && 
                                 (keyStr.StartsWith("SMS_") || keyStr == "IsAuthenticated" || keyStr.Contains("CIRCUIT_ID")))
                    .ToList();

                foreach (var key in smsKeys)
                {
                    context.Items.Remove(key);
                }
                _logger.LogApplicationInformation("Context items cleared for user: {UserId}", userId);
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error clearing authentication for user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get current user ID for logging and clearing operations
    /// </summary>
    private string GetCurrentUserId()
    {
        try
        {
            return UserCode ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    #endregion

    #region Private Session Helper Methods - SAFE VERSIONS

    /// <summary>
    /// SAFE: Get string value from session with comprehensive error handling and startup protection
    /// Uses session as the authority only.
    /// </summary>
    private string? SafeGetSessionString(string key)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                if (_sessionSnapshot.TryGetValue(key, out var scopedValue) && !string.IsNullOrEmpty(scopedValue))
                {
                    _lastReadSourceByKey[key] = "snapshot-local";
                    return scopedValue;
                }

                var ambientUserCode = GetPrincipalClaimValue(null, "SMS_UserCode");
                if (!string.IsNullOrWhiteSpace(ambientUserCode)
                    && _userAuthSnapshots.TryGetValue(ambientUserCode, out var userSnapshot)
                    && userSnapshot.TryGetValue(key, out var userSnapshotValue)
                    && !string.IsNullOrWhiteSpace(userSnapshotValue))
                {
                    _sessionSnapshot[key] = userSnapshotValue;
                    _lastReadSourceByKey[key] = "snapshot-user";
                    return userSnapshotValue;
                }

                var principalClaimValue = GetPrincipalClaimValue(null, key);
                if (!string.IsNullOrWhiteSpace(principalClaimValue))
                {
                    _sessionSnapshot[key] = principalClaimValue;
                    _lastReadSourceByKey[key] = "claims";
                    return principalClaimValue;
                }

                _lastReadSourceByKey[key] = "none";
                return null;
            }

            // Try session first (reads are valid even after response has started)
            var session = TryGetSession(httpContext);
            if (session != null)
            {
                try
                {
                    var sessionValue = session.GetString(key);
                    if (!string.IsNullOrEmpty(sessionValue))
                    {
                        _sessionSnapshot[key] = sessionValue;

                        if (!string.IsNullOrWhiteSpace(session.Id))
                        {
                            _sessionAuthSnapshots.AddOrUpdate(
                                session.Id,
                                _ => new Dictionary<string, string>(StringComparer.Ordinal) { [key] = sessionValue },
                                (_, existing) =>
                                {
                                    existing[key] = sessionValue;
                                    return existing;
                                });
                        }

                        _lastReadSourceByKey[key] = "session";
                        return sessionValue;
                    }
                }
                catch (InvalidOperationException)
                {
                    // Session not available
                }
            }

            if (session != null && !string.IsNullOrWhiteSpace(session.Id) && _sessionAuthSnapshots.TryGetValue(session.Id, out var cachedSessionValues))
            {
                if (cachedSessionValues.TryGetValue(key, out var sessionCachedValue) && !string.IsNullOrEmpty(sessionCachedValue))
                {
                    _sessionSnapshot[key] = sessionCachedValue;
                    _lastReadSourceByKey[key] = "snapshot-concurrent";
                    return sessionCachedValue;
                }
            }

            var userCode = GetPrincipalClaimValue(httpContext, "SMS_UserCode");
            if (!string.IsNullOrWhiteSpace(userCode)
                && _userAuthSnapshots.TryGetValue(userCode, out var userSnapshotByCode)
                && userSnapshotByCode.TryGetValue(key, out var userSnapshotByCodeValue)
                && !string.IsNullOrWhiteSpace(userSnapshotByCodeValue))
            {
                _sessionSnapshot[key] = userSnapshotByCodeValue;
                _lastReadSourceByKey[key] = "snapshot-user";
                return userSnapshotByCodeValue;
            }

            var claimValue = GetPrincipalClaimValue(httpContext, key);
            if (!string.IsNullOrWhiteSpace(claimValue))
            {
                _sessionSnapshot[key] = claimValue;
                _lastReadSourceByKey[key] = "claims";
                return claimValue;
            }

            _lastReadSourceByKey[key] = "none";
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error reading session key: {Key}", key);
            return null;
        }
    }

    /// <summary>
    /// SAFE: Get boolean value from session
    /// </summary>
    private bool SafeGetSessionBool(string key)
    {
        var value = SafeGetSessionString(key);
        return bool.TryParse(value, out var result) && result;
    }

    /// <summary>
    /// SAFE: Get DateTime value from session
    /// </summary>
    private DateTime? SafeGetSessionDateTime(string key)
    {
        var value = SafeGetSessionString(key);
        return DateTime.TryParse(value, out var result) ? result : null;
    }

    /// <summary>
    /// SAFE: Reconstruct SMSUserRolePermission objects from session string data
    /// Uses EXACT same logic as AuthorizationService.LoadPermissionsFromSessionAsync()
    /// </summary>
    private List<SMSUserRolePermission> SafeReconstructPermissionsFromSession()
    {
        try
        {
            var permissionsString = SafeGetSessionString("SMS_UserPermissions");
            var userRoleCode = SafeGetSessionString("SMS_UserRoleCode");
            
            if (string.IsNullOrEmpty(permissionsString) || string.IsNullOrEmpty(userRoleCode))
            {
                lock (_permissionsCacheLock)
                {
                    _cachedPermissionsRaw = null;
                    _cachedPermissionsRoleCode = null;
                    _cachedPermissions = null;
                }

                return new List<SMSUserRolePermission>();
            }

            lock (_permissionsCacheLock)
            {
                if (string.Equals(_cachedPermissionsRaw, permissionsString, StringComparison.Ordinal)
                    && string.Equals(_cachedPermissionsRoleCode, userRoleCode, StringComparison.Ordinal)
                    && _cachedPermissions is not null)
                {
                    return _cachedPermissions;
                }
            }

            // Parse the permission strings using IDENTICAL logic from SMSSessionService
            var permissions = new List<SMSUserRolePermission>();
            var permissionPairs = permissionsString.Split('|');
            
            foreach (var pair in permissionPairs)
            {
                var parts = pair.Split(':');
                if (parts.Length == 2)
                {
                    var module = parts[0];
                    var actions = parts[1].Split(",");
                    
                    // Create permission entity with proper ID
                    var permissionId = new SMSUserRolePermissionID($"PRM-{module}");
                    var permission = new SMSUserRolePermission(permissionId)
                    {
                        Code = $"PRM-{module}",
                        SMSUserRoleCode = userRoleCode,
                        SMSModule = module,
                        Create = actions.Contains("Create"),
                        Read = actions.Contains("Read"),
                        Update = actions.Contains("Update"),
                        Delete = actions.Contains("Delete")
                    };
                    
                    permissions.Add(permission);
                }
            }
            
            lock (_permissionsCacheLock)
            {
                _cachedPermissionsRaw = permissionsString;
                _cachedPermissionsRoleCode = userRoleCode;
                _cachedPermissions = permissions;
            }

            return permissions;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationDebug(ex, "Error reconstructing permissions from session - returning empty list");
            return new List<SMSUserRolePermission>();
        }
    }

    private bool HasAuthenticatedPrincipal()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context?.User?.Identity?.IsAuthenticated == true)
        {
            return true;
        }

        var principalCurrent = ClaimsPrincipal.Current;
        if (principalCurrent?.Identity?.IsAuthenticated == true)
        {
            return true;
        }

        var threadPrincipal = Thread.CurrentPrincipal as ClaimsPrincipal;
        return threadPrincipal?.Identity?.IsAuthenticated == true;
    }

    private void LogPending2FaTransition(bool isPending)
    {
        if (_lastLoggedPending2FaState == isPending)
        {
            return;
        }

        _lastLoggedPending2FaState = isPending;
        _logger.LogApplicationInformation("Current user pending-2FA state transition: IsPending2FA={IsPending2FA}, UserCode={UserCode}", isPending, UserCode);
    }

    #endregion

    #region Legacy Methods - Kept for compatibility

    /// <summary>
    /// LEGACY: Get string value from session with comprehensive error handling
    /// </summary>
    private string? GetSessionString(string key) => SafeGetSessionString(key);

    /// <summary>
    /// LEGACY: Get boolean value from session
    /// </summary>
    private bool GetSessionBool(string key) => SafeGetSessionBool(key);

    /// <summary>
    /// LEGACY: Get DateTime value from session
    /// </summary>
    private DateTime? GetSessionDateTime(string key) => SafeGetSessionDateTime(key);

    /// <summary>
    /// Safe access to session without triggering InvalidOperationException when session middleware isn't available.
    /// </summary>
    private ISession? TryGetSession(HttpContext? context)
    {
        if (context == null)
        {
            return null;
        }

        try
        {
            var sessionFeature = context.Features.Get<ISessionFeature>();
            if (sessionFeature?.Session != null)
            {
                return sessionFeature.Session;
            }

            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    /// <summary>
    /// LEGACY: Reconstruct SMSUserRolePermission objects from session string data
    /// </summary>
    private List<SMSUserRolePermission> ReconstructPermissionsFromSession() => SafeReconstructPermissionsFromSession();

    private static string? GetPrincipalClaimValue(HttpContext? context, string key)
    {
        var user = context?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            user = ClaimsPrincipal.Current;
        }

        if (user?.Identity?.IsAuthenticated != true)
        {
            user = Thread.CurrentPrincipal as ClaimsPrincipal;
        }

        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return key switch
        {
            "IsAuthenticated" => "true",
            "SMS_UserCode" => user.FindFirstValue("SMS_UserCode") ?? user.FindFirstValue(ClaimTypes.NameIdentifier),
            "SMS_UserType" => user.FindFirstValue("SMS_UserType"),
            "SMS_DisplayName" => user.FindFirstValue(ClaimTypes.Name),
            "SMS_UserRoleCode" => user.FindFirstValue("SMS_UserRoleCode"),
            "SMS_UserRoleName" => user.FindFirstValue("SMS_UserRoleName"),
            "SMS_UserPermissions" => user.FindFirstValue("SMS_UserPermissions"),
            _ => null
        };
    }

    #endregion
}


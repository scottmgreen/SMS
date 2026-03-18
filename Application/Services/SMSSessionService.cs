//-----------------------------------------------------------------------
// <copyright file="SMSSessionService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application service providing business logic operations for SMS domain entities.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SMS_Application.Configuration;

namespace SMS_Application.Services;

/// <summary>
/// SMS Session Management Service Implementation for Blazor Server
/// Handles the session lifecycle differences between Razor Pages and Blazor Server
/// Enhanced with configuration-driven session management and comprehensive logging
/// </summary>
public class SMSSessionService : ISMSSessionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SMSSessionService> _logger;
    private readonly SessionConfiguration _sessionConfig;
    private readonly TwoFactorAuthConfiguration _twoFactorConfig;

    public SMSSessionService(
        IHttpContextAccessor httpContextAccessor, 
        ILogger<SMSSessionService> logger,
        SessionConfiguration sessionConfig,
        TwoFactorAuthConfiguration twoFactorConfig)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _sessionConfig = sessionConfig;
        _twoFactorConfig = twoFactorConfig;
    }

    /// <summary>
    /// Creates SMS session using simple session approach
    /// BLAZOR SERVER COMPATIBLE: Handles response-already-started scenarios
    /// </summary>
    public async Task CreateSMSSessionAsync(BaseUser user, SMSUserType userType)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            _logger.LogError("HttpContext is null - cannot create SMS session");
            throw new InvalidOperationException("HttpContext not available");
        }

        try
        {
            var session = context.Session;
            
            // 🔧 BLAZOR SERVER FIX: Check if response has already started
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("⚠️ Response has already started - falling back to HttpContext.Items storage for user {UserId}", user.Code);
                await CreateFallbackAuthenticationState(user, userType);
                return;
            }

            // Load session first
            await session.LoadAsync();

            // **SESSION DATA SETUP** - Only if response hasn't started
            session.SetString("SMS_UserId", user.Code);
            session.SetString("SMS_UserCode", user.Code);
            session.SetString("SMS_UserType", userType.Value);
            session.SetString("SMS_Email", user.UserName.Value);
            session.SetString("SMS_DisplayName", user.DisplayName);
            session.SetString("SMS_FirstName", user.FirstName.Value);
            session.SetString("SMS_LastName", user.LastName.Value);
            session.SetString("SMS_LoginTime", DateTime.UtcNow.ToString("O"));
            session.SetString("IsAuthenticated", "true");

            // Store user role information
            if (user.UserRole != null)
            {
                session.SetString("SMS_UserRoleCode", user.UserRole.Code ?? string.Empty);
                session.SetString("SMS_UserRoleName", user.UserRole.Name ?? string.Empty);

                _logger.LogInformation("User role stored - Code: {RoleCode}, Name: {RoleName}", 
                    user.UserRole.Code, user.UserRole.Name);

                // Store permissions as simple string data - no JSON needed
                if (user.UserRole.Permissions != null && user.UserRole.Permissions.Any())
                {
                    var permissionPairs = new List<string>();
                    foreach (var perm in user.UserRole.Permissions)
                    {
                        // Store as "Module:Create,Read,Update,Delete"
                        var actions = new List<string>();
                        if (perm.Create) actions.Add("Create");
                        if (perm.Read) actions.Add("Read");
                        if (perm.Update) actions.Add("Update");
                        if (perm.Delete) actions.Add("Delete");
                        
                        permissionPairs.Add($"{perm.SMSModule}:{string.Join(",", actions)}");
                    }
                    
                    session.SetString("SMS_UserPermissions", string.Join("|", permissionPairs));
                    _logger.LogInformation("Stored {PermissionCount} permissions for user", user.UserRole.Permissions.Count);
                }
            }
            else
            {
                _logger.LogWarning("🔍 DEBUG: No UserRole found for user {UserId}", user.Code);
            }

            // Store user type-specific data
            switch (userType)
            {
                case var type when type == SMSUserType.Application && user is SMSApplicationUser appUser:
                    session.SetString("SMS_ApplicationUserCode", appUser.Code ?? string.Empty);
                    break;

                case var type when type == SMSUserType.Organizational && user is SMSOrganizationalUser orgUser:
                    session.SetString("SMS_Department", orgUser.Department ?? string.Empty);
                    session.SetString("SMS_Position", orgUser.Position ?? string.Empty);
                    session.SetString("SMS_OrganizationLevel", orgUser.OrganizationLevel ?? string.Empty);
                    break;

                case var type when type == SMSUserType.Stakeholder && user is SMSStakeholderUser stakeholderUser:
                    session.SetString("SMS_Organization", stakeholderUser.Organization ?? string.Empty);
                    session.SetString("SMS_StakeholderType", stakeholderUser.StakeholderType ?? string.Empty);
                    break;
            }

            // Commit the session
            await session.CommitAsync();

            // 🔐 CONFIGURATION-DRIVEN LOGGING: Log session creation if enabled
            if (_sessionConfig.LogSessionActivity)
            {
                _logger.LogInformation("✅ SMS Session created successfully for user {UserId} - Timeout: {TimeoutMinutes}min, Secure: {SecureCookies}", 
                    user.Code, _sessionConfig.TimeoutMinutes, _sessionConfig.SecureCookies);
            }
            else
            {
                _logger.LogInformation("✅ SMS Session created successfully for user {UserId}", user.Code);
            }

        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("after the response has started"))
        {
            _logger.LogWarning("⚠️ Session creation failed - response already started. Using fallback storage for user {UserId}", user.Code);
            await CreateFallbackAuthenticationState(user, userType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to create SMS session for user {UserId}", user.Code);
            
            // Try fallback approach
            try
            {
                _logger.LogInformation("🔄 Attempting fallback authentication storage for user {UserId}", user.Code);
                await CreateFallbackAuthenticationState(user, userType);
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "❌ Fallback authentication storage also failed for user {UserId}", user.Code);
                throw new InvalidOperationException($"Both session and fallback authentication failed: {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// Create authentication state using HttpContext.Items when session fails
    /// BLAZOR SERVER COMPATIBLE: Works even after response has started
    /// </summary>
    private async Task CreateFallbackAuthenticationState(BaseUser user, SMSUserType userType)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) throw new InvalidOperationException("HttpContext not available");

        _logger.LogInformation("🔄 Creating fallback authentication state for user {UserId}", user.Code);

        try
        {
            // 🔧 BLAZOR SERVER APPROACH: Use HttpContext.Items (works after response started)
            var authData = new Dictionary<string, string>
            {
                ["SMS_UserId"] = user.Code,
                ["SMS_UserCode"] = user.Code,
                ["SMS_UserType"] = userType.Value,
                ["SMS_DisplayName"] = user.DisplayName,
                ["SMS_Email"] = user.UserName.Value,
                ["SMS_FirstName"] = user.FirstName.Value,
                ["SMS_LastName"] = user.LastName.Value,
                ["IsAuthenticated"] = "true",
                ["SMS_LoginTime"] = DateTime.UtcNow.ToString("O")
            };

            if (user.UserRole != null)
            {
                authData["SMS_UserRoleCode"] = user.UserRole.Code ?? string.Empty;
                authData["SMS_UserRoleName"] = user.UserRole.Name ?? string.Empty;
                
                // Store permissions in Items as well
                if (user.UserRole.Permissions != null && user.UserRole.Permissions.Any())
                {
                    var permissionPairs = new List<string>();
                    foreach (var perm in user.UserRole.Permissions)
                    {
                        var actions = new List<string>();
                        if (perm.Create) actions.Add("Create");
                        if (perm.Read) actions.Add("Read");
                        if (perm.Update) actions.Add("Update");
                        if (perm.Delete) actions.Add("Delete");
                        
                        permissionPairs.Add($"{perm.SMSModule}:{string.Join(",", actions)}");
                    }
                    authData["SMS_UserPermissions"] = string.Join("|", permissionPairs);
                }
            }

            // Store in HttpContext.Items (available even after response started)
            foreach (var kvp in authData)
            {
                context.Items[kvp.Key] = kvp.Value;
            }

            // 🔧 TRACK KEYS: Store the list of keys we set for easy cleanup later
            context.Items["SMS_AUTH_KEYS"] = string.Join("|", authData.Keys);

            // 🔐 CONFIGURATION-DRIVEN LOGGING: Log fallback authentication if enabled  
            if (_sessionConfig.LogSessionActivity)
            {
                _logger.LogInformation("✅ Fallback authentication state created using HttpContext.Items for user {UserId} - Session unavailable due to response timing", user.Code);
            }
            else
            {
                _logger.LogInformation("✅ Fallback authentication state created using HttpContext.Items for user {UserId}", user.Code);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to create fallback authentication state for user {UserId}", user.Code);
            throw;
        }
    }

    /// <summary>
    /// Clears SMS session data (ENHANCED for Blazor Server)
    /// Clears both session and HttpContext.Items fallback data
    /// </summary>
    public async Task ClearSMSSessionAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            _logger.LogWarning("HttpContext is null - cannot clear SMS session");
            return;
        }

        try
        {
            var userId = context.Session?.GetString("SMS_UserId") ?? 
                         context.Items["SMS_UserId"]?.ToString();

            // Clear session if available
            if (context.Session != null)
            {
                context.Session.Clear();
                if (_sessionConfig.LogSessionActivity)
                {
                    _logger.LogInformation("🔐 Session cleared for user: {UserId} - Timeout was: {TimeoutMinutes}min", userId, _sessionConfig.TimeoutMinutes);
                }
                else
                {
                    _logger.LogInformation("Session cleared for user: {UserId}", userId);
                }
            }

            // 🔧 BLAZOR SERVER: Clear HttpContext.Items fallback data dynamically
            // Option 1: Use tracked keys if available
            if (context.Items.TryGetValue("SMS_AUTH_KEYS", out var trackedKeysObj) && 
                trackedKeysObj is string trackedKeys)
            {
                var keysToRemove = trackedKeys.Split('|');
                foreach (var key in keysToRemove)
                {
                    context.Items.Remove(key);
                }
                context.Items.Remove("SMS_AUTH_KEYS"); // Remove the tracker itself
            }
            else
            {
                // Option 2: Fallback to prefix-based clearing
                var smsKeys = context.Items.Keys
                    .Where(key => key is string keyStr && 
                                 (keyStr.StartsWith("SMS_") || keyStr == "IsAuthenticated"))
                    .ToList();

                foreach (var key in smsKeys)
                {
                    context.Items.Remove(key);
                }
            }

            if (_sessionConfig.LogSessionActivity)
            {
                _logger.LogInformation("✅ Authentication cleared (both session and items) for user: {UserId} - Secure cleanup completed", userId);
            }
            else
            {
                _logger.LogInformation("✅ Authentication cleared (both session and items) for user: {UserId}", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing SMS session and items");
            throw;
        }
    }

    /// <summary>
    /// Enhanced all getter methods to check both Session and HttpContext.Items for robust fallback support, and implemented IsUsingFallbackMethod
    /// </summary>
    public string? GetCurrentUserId()
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            // Try session first
            var sessionUserId = context.Session.GetString("SMS_UserId");
            if (!string.IsNullOrEmpty(sessionUserId))
                return sessionUserId;

            // Fallback to Items
            var itemsUserId = context.Items["SMS_UserId"]?.ToString();
            return itemsUserId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user ID");
            return null;
        }
    }

    /// <summary>
    /// Gets current user display name (ENHANCED: Checks both session and Items)
    /// </summary>
    public string? GetCurrentUserDisplayName()
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            // Try session first
            var sessionDisplayName = context.Session.GetString("SMS_DisplayName");
            if (!string.IsNullOrEmpty(sessionDisplayName))
                return sessionDisplayName;

            // Fallback to Items
            var itemsDisplayName = context.Items["SMS_DisplayName"]?.ToString();
            return itemsDisplayName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user display name");
            return null;
        }
    }

    /// <summary>
    /// Gets current user type from session
    /// </summary>
    public string? GetCurrentUserType()
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            // Try session first
            var sessionUserType = context.Session.GetString("SMS_UserType");
            if (!string.IsNullOrEmpty(sessionUserType))
                return sessionUserType;

            // Fallback to Items
            var itemsUserType = context.Items["SMS_UserType"]?.ToString();
            return itemsUserType;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get current user type from session");
            return null;
        }
    }

    /// <summary>
    /// Checks if current session is authenticated (Enhanced: Checks both session and Items)
    /// </summary>
    public bool IsAuthenticated()
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return false;

            // Try session first
            var sessionAuth = context.Session.GetString("IsAuthenticated");
            if (!string.IsNullOrEmpty(sessionAuth) && sessionAuth == "true")
                return true;

            // Fallback to Items
            var itemsAuth = context.Items["IsAuthenticated"]?.ToString();
            return !string.IsNullOrEmpty(itemsAuth) && itemsAuth == "true";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check authentication status");
            return false;
        }
    }

    // 🔐 Two-Factor Authentication Session Methods

    /// <summary>
    /// Store user temporarily for 2FA verification after password authentication
    /// </summary>
    public async Task StorePending2FAUserAsync(BaseUser user, SMSUserType userType)
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null)
            {
                _logger.LogError("❌ HttpContext not available for storing pending 2FA user");
                throw new InvalidOperationException("HttpContext not available");
            }

            _logger.LogInformation("🔐 Storing pending 2FA user: {UserCode} ({UserType})", user.Code, userType.Value);

            // 🔧 BLAZOR SERVER FIX: Check if response has already started
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("⚠️ Response has already started - using fallback storage for pending 2FA user {UserCode}", user.Code);
                await StorePending2FAUserFallbackAsync(user, userType, context);
                return;
            }

            var session = context.Session;
            
            // Load session first
            await session.LoadAsync();

            // Store comprehensive user info for 2FA verification (including role/permissions)
            session.SetString("SMS_Pending2FA_UserId", user.Code);
            session.SetString("SMS_Pending2FA_UserType", userType.Value);
            session.SetString("SMS_Pending2FA_UserName", user.UserName.Value);
            session.SetString("SMS_Pending2FA_DisplayName", user.DisplayName);
            session.SetString("SMS_Pending2FA_FirstName", user.FirstName.Value);
            session.SetString("SMS_Pending2FA_LastName", user.LastName.Value);
            session.SetString("SMS_Pending2FA_TwoFactorSecretKey", user.TwoFactorSecretKey ?? string.Empty);
            session.SetString("SMS_Pending2FA_Timestamp", DateTimeOffset.UtcNow.ToString("O")); // Use DateTimeOffset for timezone safety

            // 🔐 STORE ROLE AND PERMISSIONS DATA FOR NAVMENU
            if (user.UserRole != null)
            {
                session.SetString("SMS_Pending2FA_UserRoleCode", user.UserRole.Code ?? string.Empty);
                session.SetString("SMS_Pending2FA_UserRoleName", user.UserRole.Name ?? string.Empty);
                
                // Store permissions as simple string data - same format as normal session
                if (user.UserRole.Permissions != null && user.UserRole.Permissions.Any())
                {
                    var permissionPairs = new List<string>();
                    foreach (var perm in user.UserRole.Permissions)
                    {
                        // Store as "Module:Create,Read,Update,Delete"
                        var actions = new List<string>();
                        if (perm.Create) actions.Add("Create");
                        if (perm.Read) actions.Add("Read");
                        if (perm.Update) actions.Add("Update");
                        if (perm.Delete) actions.Add("Delete");
                        
                        permissionPairs.Add($"{perm.SMSModule}:{string.Join(",", actions)}");
                    }
                    
                    session.SetString("SMS_Pending2FA_UserPermissions", string.Join("|", permissionPairs));
                    _logger.LogInformation("🔐 Stored {PermissionCount} permissions for pending 2FA user", user.UserRole.Permissions.Count);
                }
            }

            // Store user type-specific data
            switch (userType)
            {
                case var type when type == SMSUserType.Application && user is SMSApplicationUser appUser:
                    session.SetString("SMS_Pending2FA_ApplicationUserCode", appUser.Code ?? string.Empty);
                    session.SetString("SMS_Pending2FA_SMSUserType", appUser.SMSUserType?.Value ?? string.Empty);
                    break;

                case var type when type == SMSUserType.Organizational && user is SMSOrganizationalUser orgUser:
                    session.SetString("SMS_Pending2FA_Department", orgUser.Department ?? string.Empty);
                    session.SetString("SMS_Pending2FA_Position", orgUser.Position ?? string.Empty);
                    break;

                case var type when type == SMSUserType.Stakeholder && user is SMSStakeholderUser stakeholderUser:
                    session.SetString("SMS_Pending2FA_Organization", stakeholderUser.Organization ?? string.Empty);
                    session.SetString("SMS_Pending2FA_StakeholderType", stakeholderUser.StakeholderType ?? string.Empty);
                    break;
            }

            await session.CommitAsync();
            
            _logger.LogInformation("✅ Pending 2FA user stored successfully: {UserCode}", user.Code);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("after the response has started"))
        {
            _logger.LogWarning("⚠️ Session storage failed - response already started. Using fallback storage for pending 2FA user {UserCode}", user.Code);
            await StorePending2FAUserFallbackAsync(user, userType, _httpContextAccessor.HttpContext!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to store pending 2FA user: {UserCode}", user.Code);
            
            // Try fallback approach
            try
            {
                _logger.LogInformation("🔄 Attempting fallback storage for pending 2FA user {UserCode}", user.Code);
                await StorePending2FAUserFallbackAsync(user, userType, _httpContextAccessor.HttpContext!);
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "❌ Fallback storage also failed for pending 2FA user {UserCode}", user.Code);
                throw new InvalidOperationException($"Both session and fallback storage failed for pending 2FA user: {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// Fallback storage for pending 2FA user using HttpContext.Items
    /// </summary>
    private async Task StorePending2FAUserFallbackAsync(BaseUser user, SMSUserType userType, HttpContext context)
    {
        try
        {
            _logger.LogInformation("🔄 Creating fallback pending 2FA storage for user {UserCode}", user.Code);

            // Store comprehensive user data in HttpContext.Items (including role/permissions)
            var pending2FAData = new Dictionary<string, string>
            {
                ["SMS_Pending2FA_UserId"] = user.Code,
                ["SMS_Pending2FA_UserType"] = userType.Value,
                ["SMS_Pending2FA_UserName"] = user.UserName.Value,
                ["SMS_Pending2FA_DisplayName"] = user.DisplayName,
                ["SMS_Pending2FA_FirstName"] = user.FirstName.Value,
                ["SMS_Pending2FA_LastName"] = user.LastName.Value,
                ["SMS_Pending2FA_TwoFactorSecretKey"] = user.TwoFactorSecretKey ?? string.Empty,
                ["SMS_Pending2FA_Timestamp"] = DateTimeOffset.UtcNow.ToString("O") // Use DateTimeOffset for timezone safety
            };

            // 🔐 ADD ROLE AND PERMISSIONS DATA FOR NAVMENU
            if (user.UserRole != null)
            {
                pending2FAData["SMS_Pending2FA_UserRoleCode"] = user.UserRole.Code ?? string.Empty;
                pending2FAData["SMS_Pending2FA_UserRoleName"] = user.UserRole.Name ?? string.Empty;
                
                // Store permissions in same format as normal session
                if (user.UserRole.Permissions != null && user.UserRole.Permissions.Any())
                {
                    var permissionPairs = new List<string>();
                    foreach (var perm in user.UserRole.Permissions)
                    {
                        // Store as "Module:Create,Read,Update,Delete"
                        var actions = new List<string>();
                        if (perm.Create) actions.Add("Create");
                        if (perm.Read) actions.Add("Read");
                        if (perm.Update) actions.Add("Update");
                        if (perm.Delete) actions.Add("Delete");
                        
                        permissionPairs.Add($"{perm.SMSModule}:{string.Join(",", actions)}");
                    }
                    
                    pending2FAData["SMS_Pending2FA_UserPermissions"] = string.Join("|", permissionPairs);
                    _logger.LogInformation("🔐 Stored {PermissionCount} permissions in fallback storage", user.UserRole.Permissions.Count);
                }
            }

            // Add user type-specific data
            switch (userType)
            {
                case var type when type == SMSUserType.Application && user is SMSApplicationUser appUser:
                    pending2FAData["SMS_Pending2FA_ApplicationUserCode"] = appUser.Code ?? string.Empty;
                    pending2FAData["SMS_Pending2FA_SMSUserType"] = appUser.SMSUserType?.Value ?? string.Empty;
                    break;

                case var type when type == SMSUserType.Organizational && user is SMSOrganizationalUser orgUser:
                    pending2FAData["SMS_Pending2FA_Department"] = orgUser.Department?.Value ?? string.Empty;
                    pending2FAData["SMS_Pending2FA_Position"] = orgUser.Position ?? string.Empty;
                    break;

                case var type when type == SMSUserType.Stakeholder && user is SMSStakeholderUser stakeholderUser:
                    pending2FAData["SMS_Pending2FA_Organization"] = stakeholderUser.Organization ?? string.Empty;
                    pending2FAData["SMS_Pending2FA_StakeholderType"] = stakeholderUser.StakeholderType ?? string.Empty;
                    break;
            }

            // Store each item in HttpContext.Items
            foreach (var kvp in pending2FAData)
            {
                context.Items[kvp.Key] = kvp.Value;
            }

            // Track keys for cleanup
            context.Items["SMS_Pending2FA_KEYS"] = string.Join("|", pending2FAData.Keys);

            _logger.LogInformation("✅ Fallback pending 2FA storage created using HttpContext.Items for user {UserCode}", user.Code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to create fallback pending 2FA storage for user {UserCode}", user.Code);
            throw;
        }
        
        await Task.CompletedTask; // Ensure async method
    }

    /// <summary>
    /// Retrieve pending 2FA user data
    /// </summary>
    public (BaseUser User, SMSUserType UserType)? GetPending2FAUser()
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            string? userId = null;
            string? userTypeValue = null;

            // Try session first
            var session = context.Session;
            if (session != null)
            {
                userId = session.GetString("SMS_Pending2FA_UserId");
                userTypeValue = session.GetString("SMS_Pending2FA_UserType");
            }

            // If session data not found, try HttpContext.Items fallback
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userTypeValue))
            {
                userId = context.Items["SMS_Pending2FA_UserId"]?.ToString();
                userTypeValue = context.Items["SMS_Pending2FA_UserType"]?.ToString();
                
                if (!string.IsNullOrEmpty(userId))
                {
                    _logger.LogInformation("🔄 Retrieved pending 2FA user from fallback storage: {UserCode}", userId);
                }
            }
            
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userTypeValue))
            {
                return null;
            }

            // Check if the pending 2FA session has expired - use configuration value
            var timestampString = session?.GetString("SMS_Pending2FA_Timestamp") ?? 
                                  context.Items["SMS_Pending2FA_Timestamp"]?.ToString();
            
            if (!string.IsNullOrEmpty(timestampString))
            {
                if (DateTimeOffset.TryParse(timestampString, out var timestamp))
                {
                    var timeElapsed = DateTimeOffset.UtcNow.Subtract(timestamp).TotalMinutes;
                    _logger.LogInformation("🔐 Pending 2FA session age: {Minutes} minutes (max: {MaxMinutes})", 
                        timeElapsed, _twoFactorConfig.TwoFASessionTimeoutMinutes);
                    _logger.LogInformation("🔐 Timestamp stored: {StoredTime}, Current time: {CurrentTime}", timestamp, DateTimeOffset.UtcNow);
                    
                    if (timeElapsed > _twoFactorConfig.TwoFASessionTimeoutMinutes)
                    {
                        _logger.LogWarning("⚠️ Pending 2FA session expired for user: {UserCode} (age: {Minutes} minutes, max: {MaxMinutes})", 
                            userId, timeElapsed, _twoFactorConfig.TwoFASessionTimeoutMinutes);
                        // Clear expired session
                        _ = Task.Run(async () => await ClearPending2FAUserAsync());
                        return null;
                    }
                    else
                    {
                        _logger.LogInformation("✅ Pending 2FA session is valid (age: {Minutes} minutes)", timeElapsed);
                    }
                }
                else
                {
                    _logger.LogWarning("⚠️ Could not parse pending 2FA timestamp: {TimestampString}", timestampString);
                }
            }
            else
            {
                _logger.LogWarning("⚠️ No timestamp found for pending 2FA session");
            }

            var userType = SMSUserType.FromValue(userTypeValue);
            if (userType == null) return null;

            // Reconstruct user based on type (simplified - you'd normally fetch from database)
            BaseUser user = userType.Value switch
            {
                "APPLICATION" => CreatePendingApplicationUserFromStorage(session, context),
                "ORGANIZATIONAL" => CreatePendingOrganizationalUserFromStorage(session, context),
                "STAKEHOLDER" => CreatePendingStakeholderUserFromStorage(session, context),
                _ => throw new InvalidOperationException($"Unknown user type: {userType.Value}")
            };

            return (user, userType);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve pending 2FA user");
            return null;
        }
    }

    /// <summary>
    /// Clear pending 2FA user data
    /// </summary>
    public async Task ClearPending2FAUserAsync()
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return;

            var userId = context.Session?.GetString("SMS_Pending2FA_UserId") ?? 
                         context.Items["SMS_Pending2FA_UserId"]?.ToString();

            // Clear session data if available
            var session = context.Session;
            if (session != null)
            {
                // Remove all pending 2FA session keys
                session.Remove("SMS_Pending2FA_UserId");
                session.Remove("SMS_Pending2FA_UserType");
                session.Remove("SMS_Pending2FA_UserName");
                session.Remove("SMS_Pending2FA_DisplayName");
                session.Remove("SMS_Pending2FA_FirstName");
                session.Remove("SMS_Pending2FA_LastName");
                session.Remove("SMS_Pending2FA_TwoFactorSecretKey");
                session.Remove("SMS_Pending2FA_Timestamp");
                session.Remove("SMS_Pending2FA_ApplicationUserCode");
                session.Remove("SMS_Pending2FA_SMSUserType");
                session.Remove("SMS_Pending2FA_Department");
                session.Remove("SMS_Pending2FA_Position");
                session.Remove("SMS_Pending2FA_Organization");
                session.Remove("SMS_Pending2FA_StakeholderType");

                await session.CommitAsync();
            }

            // 🔧 BLAZOR SERVER: Clear HttpContext.Items fallback data
            if (context.Items.TryGetValue("SMS_Pending2FA_KEYS", out var trackedKeysObj) && 
                trackedKeysObj is string trackedKeys)
            {
                var keysToRemove = trackedKeys.Split('|');
                foreach (var key in keysToRemove)
                {
                    context.Items.Remove(key);
                }
                context.Items.Remove("SMS_Pending2FA_KEYS"); // Remove the tracker itself
            }
            else
            {
                // Fallback: Remove known pending 2FA keys from Items
                var pending2FAKeys = context.Items.Keys
                    .Where(key => key is string keyStr && keyStr.StartsWith("SMS_Pending2FA_"))
                    .ToList();

                foreach (var key in pending2FAKeys)
                {
                    context.Items.Remove(key);
                }
            }
            
            if (!string.IsNullOrEmpty(userId))
            {
                _logger.LogInformation("✅ Pending 2FA user data cleared for: {UserCode}", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to clear pending 2FA user data");
        }
    }

    /// <summary>
    /// Check if there's a user pending 2FA verification
    /// </summary>
    public bool HasPending2FAUser()
    {
        try
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            return !string.IsNullOrEmpty(session?.GetString("SMS_Pending2FA_UserId"));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check pending 2FA user status");
            return false;
        }
    }

    #region Private Helper Methods for Pending 2FA Users

    private SMSApplicationUser CreatePendingApplicationUserFromStorage(ISession? session, HttpContext context)
    {
        // Helper method to get value from either session or Items
        string GetValue(string key) => 
            session?.GetString(key) ?? 
            context.Items[key]?.ToString() ?? 
            string.Empty;

        var userId = GetValue("SMS_Pending2FA_UserId");
        var userName = GetValue("SMS_Pending2FA_UserName");
        var firstName = GetValue("SMS_Pending2FA_FirstName");
        var lastName = GetValue("SMS_Pending2FA_LastName");
        var secretKey = GetValue("SMS_Pending2FA_TwoFactorSecretKey");
        
        // Get role and permissions data
        var userRoleCode = GetValue("SMS_Pending2FA_UserRoleCode");
        var userRoleName = GetValue("SMS_Pending2FA_UserRoleName");
        var userPermissions = GetValue("SMS_Pending2FA_UserPermissions");

        // Create a comprehensive user object for 2FA verification with proper ID
        var applicationUserId = new SMSApplicationUserID(userId);
        var user = new SMSApplicationUser(applicationUserId)
        {
            Code = userId,
            FirstName = FirstName.Create(firstName).Value,
            LastName = LastName.Create(lastName).Value,
            UserName = UserName.Create(userName).Value,
            TwoFactorSecretKey = secretKey,
            TwoFactorEnabled = !string.IsNullOrEmpty(secretKey)
        };

        // 🔐 RECONSTRUCT USERROLE AND PERMISSIONS FOR NAVMENU
        if (!string.IsNullOrEmpty(userRoleCode))
        {
            var userRoleId = new SMSUserRoleID(userRoleCode);
            user.UserRole = new SMSUserRole(userRoleId)
            {
                Code = userRoleCode,
                Name = userRoleName,
                Permissions = ReconstructPermissionsFromString(userPermissions)
            };
            
            _logger.LogInformation("🔐 Reconstructed UserRole for pending 2FA user: {RoleCode} with {PermissionCount} permissions", 
                userRoleCode, user.UserRole.Permissions?.Count ?? 0);
        }

        return user;
    }

    private SMSOrganizationalUser CreatePendingOrganizationalUserFromStorage(ISession? session, HttpContext context)
    {
        // Helper method to get value from either session or Items
        string GetValue(string key) => 
            session?.GetString(key) ?? 
            context.Items[key]?.ToString() ?? 
            string.Empty;

        var userId = GetValue("SMS_Pending2FA_UserId");
        var userName = GetValue("SMS_Pending2FA_UserName");
        var firstName = GetValue("SMS_Pending2FA_FirstName");
        var lastName = GetValue("SMS_Pending2FA_LastName");
        var secretKey = GetValue("SMS_Pending2FA_TwoFactorSecretKey");
        var department = GetValue("SMS_Pending2FA_Department");
        
        // Get role and permissions data
        var userRoleCode = GetValue("SMS_Pending2FA_UserRoleCode");
        var userRoleName = GetValue("SMS_Pending2FA_UserRoleName");
        var userPermissions = GetValue("SMS_Pending2FA_UserPermissions");

        var organizationalUserId = new SMSOrganizationalUserID(userId);
        var user = new SMSOrganizationalUser(organizationalUserId)
        {
            Code = userId,
            FirstName = FirstName.Create(firstName).Value,
            LastName = LastName.Create(lastName).Value,
            UserName = UserName.Create(userName).Value,
            Department = string.IsNullOrEmpty(department) ? null : SMSDepartment.FromValue(department),
            TwoFactorSecretKey = secretKey,
            TwoFactorEnabled = !string.IsNullOrEmpty(secretKey)
        };

        // 🔐 RECONSTRUCT USERROLE AND PERMISSIONS FOR NAVMENU
        if (!string.IsNullOrEmpty(userRoleCode))
        {
            var userRoleId = new SMSUserRoleID(userRoleCode);
            user.UserRole = new SMSUserRole(userRoleId)
            {
                Code = userRoleCode,
                Name = userRoleName,
                Permissions = ReconstructPermissionsFromString(userPermissions)
            };
            
            _logger.LogInformation("🔐 Reconstructed UserRole for pending 2FA user: {RoleCode} with {PermissionCount} permissions", 
                userRoleCode, user.UserRole.Permissions?.Count ?? 0);
        }

        return user;
    }

    private SMSStakeholderUser CreatePendingStakeholderUserFromStorage(ISession? session, HttpContext context)
    {
        // Helper method to get value from either session or Items
        string GetValue(string key) => 
            session?.GetString(key) ?? 
            context.Items[key]?.ToString() ?? 
            string.Empty;

        var userId = GetValue("SMS_Pending2FA_UserId");
        var userName = GetValue("SMS_Pending2FA_UserName");
        var firstName = GetValue("SMS_Pending2FA_FirstName");
        var lastName = GetValue("SMS_Pending2FA_LastName");
        var secretKey = GetValue("SMS_Pending2FA_TwoFactorSecretKey");
        var organization = GetValue("SMS_Pending2FA_Organization");
        var stakeholderType = GetValue("SMS_Pending2FA_StakeholderType");
        
        // Get role and permissions data
        var userRoleCode = GetValue("SMS_Pending2FA_UserRoleCode");
        var userRoleName = GetValue("SMS_Pending2FA_UserRoleName");
        var userPermissions = GetValue("SMS_Pending2FA_UserPermissions");

        var stakeholderUserId = new SMSStakeholderUserID(userId);
        var user = new SMSStakeholderUser(stakeholderUserId)
        {
            Code = userId,
            FirstName = FirstName.Create(firstName).Value,
            LastName = LastName.Create(lastName).Value,
            UserName = UserName.Create(userName).Value,
            Organization = organization,
            StakeholderType = stakeholderType,
            TwoFactorSecretKey = secretKey,
            TwoFactorEnabled = !string.IsNullOrEmpty(secretKey)
        };

        // 🔐 RECONSTRUCT USERROLE AND PERMISSIONS FOR NAVMENU
        if (!string.IsNullOrEmpty(userRoleCode))
        {
            var userRoleId = new SMSUserRoleID(userRoleCode);
            user.UserRole = new SMSUserRole(userRoleId)
            {
                Code = userRoleCode,
                Name = userRoleName,
                Permissions = ReconstructPermissionsFromString(userPermissions)
            };
            
            _logger.LogInformation("🔐 Reconstructed UserRole for pending 2FA user: {RoleCode} with {PermissionCount} permissions", 
                userRoleCode, user.UserRole.Permissions?.Count ?? 0);
        }

        return user;
    }

    /// <summary>
    /// Reconstruct permissions from the string format used in session storage
    /// </summary>
    private List<SMSUserRolePermission>? ReconstructPermissionsFromString(string permissionString)
    {
        if (string.IsNullOrEmpty(permissionString))
            return null;

        try
        {
            var permissions = new List<SMSUserRolePermission>();
            
            // Parse "Module1:Create,Read,Update,Delete|Module2:Read,Update" format
            var permissionPairs = permissionString.Split('|', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var pair in permissionPairs)
            {
                var parts = pair.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    var module = parts[0];
                    var actions = parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries);
                    
                    var permissionId = new SMSUserRolePermissionID($"PRM_{module}_{Guid.NewGuid().ToString("N")[..8]}");
                    var permission = new SMSUserRolePermission(permissionId)
                    {
                        SMSModule = module,
                        Create = actions.Contains("Create"),
                        Read = actions.Contains("Read"),
                        Update = actions.Contains("Update"),
                        Delete = actions.Contains("Delete")
                    };
                    
                    permissions.Add(permission);
                }
            }
            
            return permissions;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to reconstruct permissions from string: {PermissionString}", permissionString);
            return null;
        }
    }

    #endregion
}

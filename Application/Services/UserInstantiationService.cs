//-----------------------------------------------------------------------
// <copyright file="UserInstantiationService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Service for complete user instantiation with roles and permissions
//                  using CQRS pattern via Mediator - NO direct repository access.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Domain.Enums;
using SMS_Domain.Errors;
using System.Text.Json;

namespace SMS_Application.Services;

/// <summary>
/// Service for complete user instantiation with roles and permissions
/// Uses CQRS pattern exclusively - NO direct repository access
/// Ensures users are fully populated regardless of authentication method
/// </summary>
public class UserInstantiationService : IUserInstantiationService
{
    private readonly ILogger<UserInstantiationService> _logger;
    private readonly IMediator _mediator;
    private readonly IUserCompletenessValidator _completenessValidator;

    public UserInstantiationService(
        ILogger<UserInstantiationService> logger,
        IMediator mediator,
        IUserCompletenessValidator completenessValidator)
    {
        _logger = logger;
        _mediator = mediator;
        _completenessValidator = completenessValidator;
    }

    /// <summary>
    /// Get fully instantiated user by code with complete roles and permissions
    /// Uses CQRS queries via Mediator for all user types
    /// </summary>
    public async Task<Result<(BaseUser User, SMSUserType UserType)>> GetCompleteUserAsync(string userCode, SMSUserType userType, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("?? Getting complete user instantiation for {UserCode} ({UserType}) via CQRS", userCode, userType.Value);

            BaseUser? user = null;
            Result result;

            // Use CQRS queries via Mediator for each user type
            switch (userType.Value.ToUpperInvariant())
            {
                case "APPLICATION":
                    var appQuery = new GetSMSApplicationUserByCodeQuery(userCode);
                    var appResult = await _mediator.SendAsync(appQuery, cancellationToken);
                    if (appResult.IsSuccess)
                    {
                        user = appResult.Value;
                        _logger.LogInformation("? Application user retrieved via CQRS with {PermissionCount} permissions", 
                            user.UserRole?.Permissions?.Count ?? 0);
                    }
                    result = appResult;
                    break;

                case "ORGANIZATIONAL":
                    var orgQuery = new GetSMSOrganizationalUserByCodeQuery(userCode);
                    var orgResult = await _mediator.SendAsync(orgQuery, cancellationToken);
                    if (orgResult.IsSuccess)
                    {
                        user = orgResult.Value;
                        _logger.LogInformation("? Organizational user retrieved via CQRS with {PermissionCount} permissions", 
                            user.UserRole?.Permissions?.Count ?? 0);
                    }
                    result = orgResult;
                    break;

                case "STAKEHOLDER":
                    var stakeQuery = new GetSMSStakeholderUserByCodeQuery(userCode);
                    var stakeResult = await _mediator.SendAsync(stakeQuery, cancellationToken);
                    if (stakeResult.IsSuccess)
                    {
                        user = stakeResult.Value;
                        _logger.LogInformation("? Stakeholder user retrieved via CQRS with {PermissionCount} permissions", 
                            user.UserRole?.Permissions?.Count ?? 0);
                    }
                    result = stakeResult;
                    break;

                default:
                    _logger.LogError("? Unknown user type: {UserType}", userType.Value);
                    return Result<(BaseUser, SMSUserType)>.Failure<(BaseUser, SMSUserType)>(
                        DomainErrors.BaseUserError.InvalidUserType);
            }

            if (result.IsFailure)
            {
                _logger.LogError("? Failed to retrieve user {UserCode} ({UserType}) via CQRS: {Error}", 
                    userCode, userType.Value, result.Error?.Message);
                return Result<(BaseUser, SMSUserType)>.Failure<(BaseUser, SMSUserType)>(result.Error);
            }

            // Validate user completeness
            if (!_completenessValidator.IsUserComplete(user!))
            {
                var missing = _completenessValidator.GetMissingComponents(user!);
                _logger.LogWarning("?? User {UserCode} ({UserType}) is incomplete - Missing: {MissingComponents}", 
                    userCode, userType.Value, string.Join(", ", missing));
            }

            var completenessScore = _completenessValidator.GetCompletenessScore(user!, userType);
            _logger.LogInformation("? Complete user instantiation successful for {UserCode} ({UserType}) - Score: {Score}/100, Role: {RoleCode}, Permissions: {PermissionCount}", 
                userCode, userType.Value, completenessScore, user!.UserRole?.Code ?? "None", user.UserRole?.Permissions?.Count ?? 0);

            return Result<(BaseUser, SMSUserType)>.Success((user, userType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error getting complete user {UserCode} ({UserType}) via CQRS", userCode, userType.Value);
            return Result<(BaseUser, SMSUserType)>.Failure<(BaseUser, SMSUserType)>(
                DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    /// <summary>
    /// Get fully instantiated user by username with complete roles and permissions
    /// Uses CQRS queries via Mediator for all user types
    /// </summary>
    public async Task<Result<(BaseUser User, SMSUserType UserType)>> GetCompleteUserByUsernameAsync(string username, SMSUserType userType, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("?? Getting complete user instantiation by username {Username} ({UserType}) via CQRS", username, userType.Value);

            BaseUser? user = null;
            Result result;

            // Use CQRS queries via Mediator for each user type
            switch (userType.Value.ToUpperInvariant())
            {
                case "APPLICATION":
                    var appQuery = new GetSMSApplicationUserByUserNameQuery(username);
                    var appResult = await _mediator.SendAsync(appQuery, cancellationToken);
                    if (appResult.IsSuccess)
                    {
                        user = appResult.Value;
                        _logger.LogInformation("? Application user retrieved by username via CQRS with {PermissionCount} permissions", 
                            user.UserRole?.Permissions?.Count ?? 0);
                    }
                    result = appResult;
                    break;

                case "ORGANIZATIONAL":
                    var orgQuery = new GetSMSOrganizationalUserByUserNameQuery(username);
                    var orgResult = await _mediator.SendAsync(orgQuery, cancellationToken);
                    if (orgResult.IsSuccess)
                    {
                        user = orgResult.Value;
                        _logger.LogInformation("? Organizational user retrieved by username via CQRS with {PermissionCount} permissions", 
                            user.UserRole?.Permissions?.Count ?? 0);
                    }
                    result = orgResult;
                    break;

                case "STAKEHOLDER":
                    var stakeQuery = new GetSMSStakeholderUserByUserNameQuery(username);
                    var stakeResult = await _mediator.SendAsync(stakeQuery, cancellationToken);
                    if (stakeResult.IsSuccess)
                    {
                        user = stakeResult.Value;
                        _logger.LogInformation("? Stakeholder user retrieved by username via CQRS with {PermissionCount} permissions", 
                            user.UserRole?.Permissions?.Count ?? 0);
                    }
                    result = stakeResult;
                    break;

                default:
                    _logger.LogError("? Unknown user type: {UserType}", userType.Value);
                    return Result<(BaseUser, SMSUserType)>.Failure<(BaseUser, SMSUserType)>(
                        DomainErrors.BaseUserError.InvalidUserType);
            }

            if (result.IsFailure)
            {
                _logger.LogError("? Failed to retrieve user by username {Username} ({UserType}) via CQRS: {Error}", 
                    username, userType.Value, result.Error?.Message);
                return Result<(BaseUser, SMSUserType)>.Failure<(BaseUser, SMSUserType)>(result.Error);
            }

            // Validate user completeness
            if (!_completenessValidator.IsUserComplete(user!))
            {
                var missing = _completenessValidator.GetMissingComponents(user!);
                _logger.LogWarning("?? User {Username} ({UserType}) is incomplete - Missing: {MissingComponents}", 
                    username, userType.Value, string.Join(", ", missing));
            }

            var completenessScore = _completenessValidator.GetCompletenessScore(user!, userType);
            _logger.LogInformation("? Complete user instantiation by username successful for {Username} ({UserType}) - Score: {Score}/100, Role: {RoleCode}, Permissions: {PermissionCount}", 
                username, userType.Value, completenessScore, user!.UserRole?.Code ?? "None", user.UserRole?.Permissions?.Count ?? 0);

            return Result<(BaseUser, SMSUserType)>.Success((user, userType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error getting complete user by username {Username} ({UserType}) via CQRS", username, userType.Value);
            return Result<(BaseUser, SMSUserType)>.Failure<(BaseUser, SMSUserType)>(
                DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    /// <summary>
    /// Serialize complete user data for storage (Session/Circuit/Context)
    /// Preserves all role and permission information
    /// </summary>
    public Dictionary<string, string> SerializeCompleteUser(BaseUser user, SMSUserType userType)
    {
        try
        {
            var userData = new Dictionary<string, string>
            {
                ["SMS_UserId"] = user.Code,
                ["SMS_UserCode"] = user.Code,
                ["SMS_UserType"] = userType.Value,
                ["SMS_DisplayName"] = user.DisplayName,
                ["SMS_Email"] = user.UserName.Value,
                ["SMS_FirstName"] = user.FirstName.Value,
                ["SMS_LastName"] = user.LastName.Value,
                ["IsAuthenticated"] = "true",
                ["SMS_LoginTime"] = DateTime.UtcNow.ToString("O"),
                ["SMS_TwoFactorEnabled"] = user.TwoFactorEnabled.ToString(),
                ["SMS_TwoFactorSecretKey"] = user.TwoFactorSecretKey ?? string.Empty,
                ["SMS_FailedTwoFactorAttempts"] = user.FailedTwoFactorAttempts.ToString(),
                ["SMS_LastLoginDate"] = user.LastLoginDate?.ToString("O") ?? string.Empty,
                ["SMS_IsActive"] = user.IsActive.ToString(),
                ["SMS_SerializedAt"] = DateTime.UtcNow.ToString("O"),
                ["SMS_CompletenessScore"] = _completenessValidator.GetCompletenessScore(user, userType).ToString()
            };

            // ?? SERIALIZE COMPLETE ROLE INFORMATION
            if (user.UserRole != null)
            {
                userData["SMS_UserRoleCode"] = user.UserRole.Code ?? string.Empty;
                userData["SMS_UserRoleName"] = user.UserRole.Name ?? string.Empty;
                
                // Serialize permissions as JSON for complete data preservation
                if (user.UserRole.Permissions != null && user.UserRole.Permissions.Any())
                {
                    var permissionData = user.UserRole.Permissions.Select(p => new
                    {
                        Module = p.SMSModule,
                        Create = p.Create,
                        Read = p.Read,
                        Update = p.Update,
                        Delete = p.Delete,
                        Code = p.Code
                    }).ToList();

                    userData["SMS_UserPermissionsJson"] = JsonSerializer.Serialize(permissionData);

                    // Also store in legacy format for compatibility
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
                    userData["SMS_UserPermissions"] = string.Join("|", permissionPairs);
                    
                    _logger.LogInformation("?? Serialized {PermissionCount} permissions for user {UserCode}", 
                        user.UserRole.Permissions.Count, user.Code);
                }
            }

            // Add user type-specific data
            switch (userType.Value.ToUpperInvariant())
            {
                case "APPLICATION" when user is SMSApplicationUser appUser:
                    userData["SMS_ApplicationUserCode"] = appUser.Code ?? string.Empty;
                    break;
                case "ORGANIZATIONAL" when user is SMSOrganizationalUser orgUser:
                    userData["SMS_Department"] = orgUser.Department?.Value ?? string.Empty;
                    userData["SMS_Position"] = orgUser.Position ?? string.Empty;
                    userData["SMS_OrganizationLevel"] = orgUser.OrganizationLevel ?? string.Empty;
                    break;
                case "STAKEHOLDER" when user is SMSStakeholderUser stakeholderUser:
                    userData["SMS_Organization"] = stakeholderUser.Organization ?? string.Empty;
                    userData["SMS_StakeholderType"] = stakeholderUser.StakeholderType ?? string.Empty;
                    break;
            }

            _logger.LogInformation("? Complete user serialization successful for {UserCode} ({UserType}) - {DataCount} fields", 
                user.Code, userType.Value, userData.Count);

            return userData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error serializing complete user {UserCode} ({UserType})", user.Code, userType.Value);
            throw;
        }
    }

    /// <summary>
    /// Deserialize and fully instantiate user from stored data
    /// Will refetch from database via CQRS if stored data is incomplete
    /// </summary>
    public async Task<Result<(BaseUser User, SMSUserType UserType)>> DeserializeCompleteUserAsync(Dictionary<string, string> userData, CancellationToken cancellationToken = default)
    {
        try
        {
            var userCode = userData.GetValueOrDefault("SMS_UserCode", "");
            var userTypeValue = userData.GetValueOrDefault("SMS_UserType", "Application");
            var userType = SMSUserType.FromValue(userTypeValue);

            if (string.IsNullOrEmpty(userCode))
            {
                _logger.LogError("? Cannot deserialize user - missing UserCode");
                return Result<(BaseUser, SMSUserType)>.Failure<(BaseUser, SMSUserType)>(
                    DomainErrors.BaseUserError.NullOrEmpty);
            }

            _logger.LogInformation("?? Deserializing complete user {UserCode} ({UserType})", userCode, userType.Value);

            // Check if we have JSON permission data (complete format)
            var hasJsonPermissions = userData.ContainsKey("SMS_UserPermissionsJson");
            var completenessScore = int.TryParse(userData.GetValueOrDefault("SMS_CompletenessScore", "0"), out var score) ? score : 0;
            
            if (hasJsonPermissions && completenessScore >= 90)
            {
                // We have complete serialized data, use it directly instead of refetching
                _logger.LogInformation("? Found complete JSON permission data for user {UserCode} (Score: {Score}) - using cached data", userCode, completenessScore);
                
                // ?? CRITICAL FIX: Don't call GetCompleteUserAsync - it creates infinite loops!
                // Instead, create a minimal user object from the existing data
                try
                {
                    var reconstructedUserType = SMSUserType.FromValue(userTypeValue);
                    
                    // Create a minimal SMSApplicationUser from cached data
                    var minimalUser = CreateMinimalUserFromCachedData(userData, reconstructedUserType);
                    if (minimalUser != null)
                    {
                        _logger.LogInformation("?? Created minimal user from cached data for {UserCode} - avoiding database call", userCode);
                        return Result<(BaseUser, SMSUserType)>.Success((minimalUser, reconstructedUserType));
                    }
                    else
                    {
                        _logger.LogWarning("?? Failed to create minimal user from cached data for {UserCode}", userCode);
                        return Result<(BaseUser, SMSUserType)>.Failure<(BaseUser, SMSUserType)>(
                            DomainErrors.GeneralError.UnProcessableRequest);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "?? Failed to create minimal user from cached data for {UserCode}", userCode);
                    return Result<(BaseUser, SMSUserType)>.Failure<(BaseUser, SMSUserType)>(
                        DomainErrors.GeneralError.UnProcessableRequest);
                }
            }
            else
            {
                // Only fetch fresh data if we truly don't have complete data
                _logger.LogInformation("?? Incomplete user data found (Score: {Score}), returning failure to avoid infinite loop", 
                    completenessScore);
                    
                // ?? CRITICAL: Don't call GetCompleteUserAsync during deserialization - it creates loops!
                return Result<(BaseUser, SMSUserType)>.Failure<(BaseUser, SMSUserType)>(
                    DomainErrors.GeneralError.UnProcessableRequest);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error deserializing complete user");
            return Result<(BaseUser, SMSUserType)>.Failure<(BaseUser, SMSUserType)>(
                DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    /// <summary>
    /// Validate user completeness (has all required roles/permissions)
    /// </summary>
    public bool IsUserComplete(BaseUser user)
    {
        return _completenessValidator.IsUserComplete(user);
    }

    /// <summary>
    /// Get list of missing components for incomplete user
    /// </summary>
    public List<string> GetMissingComponents(BaseUser user)
    {
        return _completenessValidator.GetMissingComponents(user);
    }

    /// <summary>
    /// Ensure user has complete role and permission data
    /// Will refetch via CQRS if incomplete
    /// </summary>
    public async Task<Result<BaseUser>> EnsureUserCompletenessAsync(BaseUser user, SMSUserType userType, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_completenessValidator.IsUserComplete(user))
            {
                _logger.LogDebug("? User {UserCode} is already complete", user.Code);
                return Result<BaseUser>.Success(user);
            }

            _logger.LogInformation("?? User {UserCode} is incomplete, refetching via CQRS", user.Code);
            var result = await GetCompleteUserAsync(user.Code, userType, cancellationToken);
            
            if (result.IsSuccess)
            {
                return Result<BaseUser>.Success(result.Value.User);
            }
            else
            {
                return Result<BaseUser>.Failure<BaseUser>(result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error ensuring user completeness for {UserCode}", user.Code);
            return Result<BaseUser>.Failure<BaseUser>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    /// <summary>
    /// Create minimal user object from cached session/circuit data to avoid database calls during deserialization
    /// CRITICAL: This prevents infinite loops during authentication strategy retrieval
    /// </summary>
    private BaseUser? CreateMinimalUserFromCachedData(Dictionary<string, string> userData, SMSUserType userType)
    {
        try
        {
            var userCode = userData.GetValueOrDefault("SMS_UserCode", "");
            var email = userData.GetValueOrDefault("SMS_Email", "");
            var firstName = userData.GetValueOrDefault("SMS_FirstName", "");
            var lastName = userData.GetValueOrDefault("SMS_LastName", "");
            
            if (string.IsNullOrEmpty(userCode))
            {
                return null;
            }

            // Create basic user based on type - using the exact pattern from Mappers.cs
            switch (userType.Value.ToUpperInvariant())
            {
                case "APPLICATION":
                    var appUserId = new SMSApplicationUserID(userCode);
                    var appUser = new SMSApplicationUser(appUserId);

                    appUser.Code = userCode;
                    
                    // Use the same pattern as Mappers.cs for value objects
                    var userNameResult = UserName.Create(email);
                    if (userNameResult.IsSuccess)
                        appUser.UserName = userNameResult.Value;

                    var firstNameResult = FirstName.Create(firstName);
                    if (firstNameResult.IsSuccess)
                        appUser.FirstName = firstNameResult.Value;

                    var lastNameResult = LastName.Create(lastName);
                    if (lastNameResult.IsSuccess)
                        appUser.LastName = lastNameResult.Value;

                    appUser.TwoFactorEnabled = bool.TryParse(userData.GetValueOrDefault("SMS_TwoFactorEnabled", "false"), out var twoFAEnabled) && twoFAEnabled;
                    appUser.TwoFactorSecretKey = userData.GetValueOrDefault("SMS_TwoFactorSecretKey", "");
                    appUser.FailedTwoFactorAttempts = int.TryParse(userData.GetValueOrDefault("SMS_FailedTwoFactorAttempts", "0"), out var failedAttempts) ? failedAttempts : 0;
                    appUser.IsActive = bool.TryParse(userData.GetValueOrDefault("SMS_IsActive", "true"), out var isActive) && isActive;
                    appUser.LastLoginDate = DateTime.TryParse(userData.GetValueOrDefault("SMS_LastLoginDate", ""), out var lastLogin) ? lastLogin : null;

                    return appUser;

                case "ORGANIZATIONAL":
                    var orgUserId = new SMSOrganizationalUserID(userCode);
                    var orgUser = new SMSOrganizationalUser(orgUserId);

                    orgUser.Code = userCode;
                    orgUser.Position = userData.GetValueOrDefault("SMS_Position", "");

                    // Set value objects using same pattern
                    var orgUserNameResult = UserName.Create(email);
                    if (orgUserNameResult.IsSuccess)
                        orgUser.UserName = orgUserNameResult.Value;

                    var orgFirstNameResult = FirstName.Create(firstName);
                    if (orgFirstNameResult.IsSuccess)
                        orgUser.FirstName = orgFirstNameResult.Value;

                    var orgLastNameResult = LastName.Create(lastName);
                    if (orgLastNameResult.IsSuccess)
                        orgUser.LastName = orgLastNameResult.Value;

                    orgUser.TwoFactorEnabled = bool.TryParse(userData.GetValueOrDefault("SMS_TwoFactorEnabled", "false"), out var orgTwoFAEnabled) && orgTwoFAEnabled;
                    orgUser.IsActive = bool.TryParse(userData.GetValueOrDefault("SMS_IsActive", "true"), out var orgIsActive) && orgIsActive;

                    return orgUser;

                case "STAKEHOLDER":
                    var stakeUserId = new SMSStakeholderUserID(userCode);
                    var stakeUser = new SMSStakeholderUser(stakeUserId);

                    stakeUser.Code = userCode;
                    stakeUser.Organization = userData.GetValueOrDefault("SMS_Organization", "");
                    stakeUser.StakeholderType = userData.GetValueOrDefault("SMS_StakeholderType", "");

                    // Set value objects using same pattern
                    var stakeUserNameResult = UserName.Create(email);
                    if (stakeUserNameResult.IsSuccess)
                        stakeUser.UserName = stakeUserNameResult.Value;

                    var stakeFirstNameResult = FirstName.Create(firstName);
                    if (stakeFirstNameResult.IsSuccess)
                        stakeUser.FirstName = stakeFirstNameResult.Value;

                    var stakeLastNameResult = LastName.Create(lastName);
                    if (stakeLastNameResult.IsSuccess)
                        stakeUser.LastName = stakeLastNameResult.Value;

                    stakeUser.TwoFactorEnabled = bool.TryParse(userData.GetValueOrDefault("SMS_TwoFactorEnabled", "false"), out var stakeTwoFAEnabled) && stakeTwoFAEnabled;
                    stakeUser.IsActive = bool.TryParse(userData.GetValueOrDefault("SMS_IsActive", "true"), out var stakeIsActive) && stakeIsActive;

                    return stakeUser;

                default:
                    _logger.LogWarning("Unknown user type for minimal user creation: {UserType}", userType.Value);
                    return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error creating minimal user from cached data");
            return null;
        }
    }
}
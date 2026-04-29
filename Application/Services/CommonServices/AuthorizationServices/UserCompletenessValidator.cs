//-----------------------------------------------------------------------
// <copyright file="UserCompletenessValidator.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Service for validating user completeness with roles and permissions.
//                  Ensures users have all required data for proper authentication.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Domain.Enums;
using System.Text;

namespace SMS_Application.Services;

/// <summary>
/// Service for validating user completeness with roles and permissions
/// Ensures users have all required data for proper authentication
/// </summary>
public class UserCompletenessValidator : IUserCompletenessValidator
{
    private readonly ILogger<UserCompletenessValidator> _logger;

    public UserCompletenessValidator(ILogger<UserCompletenessValidator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Check if user has all required components for complete authentication
    /// </summary>
    public bool IsUserComplete(BaseUser user)
    {
        if (user == null) return false;

        return HasValidBasicInfo(user) && 
               HasValidRole(user) && 
               HasValidPermissions(user);
    }

    /// <summary>
    /// Get detailed list of missing components for incomplete user
    /// </summary>
    public List<string> GetMissingComponents(BaseUser user)
    {
        var missing = new List<string>();

        if (user == null)
        {
            missing.Add("User is null");
            return missing;
        }

        // Basic Info Validation
        if (string.IsNullOrEmpty(user.Code))
            missing.Add("User Code");
        
        if (string.IsNullOrEmpty(user.DisplayName))
            missing.Add("Display Name");
        
        if (user.UserName?.Value == null)
            missing.Add("User Name");
        
        if (user.FirstName?.Value == null)
            missing.Add("First Name");
        
        if (user.LastName?.Value == null)
            missing.Add("Last Name");

        // Role Validation
        if (user.UserRole == null)
        {
            missing.Add("User Role (entire role is missing)");
        }
        else
        {
            if (string.IsNullOrEmpty(user.UserRole.Code))
                missing.Add("User Role Code");
            
            if (string.IsNullOrEmpty(user.UserRole.Name))
                missing.Add("User Role Name");
        }

        // Permission Validation
        if (user.UserRole?.Permissions == null)
        {
            missing.Add("User Permissions (permissions collection is null)");
        }
        else if (!user.UserRole.Permissions.Any())
        {
            missing.Add("User Permissions (permissions collection is empty)");
        }
        else
        {
            var invalidPermissions = user.UserRole.Permissions
                .Where(p => string.IsNullOrEmpty(p.SMSModule))
                .Count();
            
            if (invalidPermissions > 0)
                missing.Add($"Invalid Permissions ({invalidPermissions} permissions missing module assignment)");
        }

        return missing;
    }

    /// <summary>
    /// Validate user basic information (Code, Names, Email)
    /// </summary>
    public bool HasValidBasicInfo(BaseUser user)
    {
        if (user == null) return false;

        return !string.IsNullOrEmpty(user.Code) &&
               !string.IsNullOrEmpty(user.DisplayName) &&
               user.UserName?.Value != null &&
               user.FirstName?.Value != null &&
               user.LastName?.Value != null;
    }

    /// <summary>
    /// Validate user has proper role assignment
    /// </summary>
    public bool HasValidRole(BaseUser user)
    {
        if (user?.UserRole == null) return false;

        return !string.IsNullOrEmpty(user.UserRole.Code) &&
               !string.IsNullOrEmpty(user.UserRole.Name);
    }

    /// <summary>
    /// Validate user role has permissions
    /// </summary>
    public bool HasValidPermissions(BaseUser user)
    {
        if (user?.UserRole?.Permissions == null) return false;

        if (!user.UserRole.Permissions.Any()) return false;

        // Check that permissions have valid module assignments
        return user.UserRole.Permissions.All(p => !string.IsNullOrEmpty(p.SMSModule));
    }

    /// <summary>
    /// Validate user type-specific information
    /// </summary>
    public bool HasValidTypeSpecificInfo(BaseUser user, SMSUserType userType)
    {
        if (user == null) return false;

        switch (userType.Value.ToUpperInvariant())
        {
            case "APPLICATION":
                // Application users need basic info (already validated)
                return user is SMSApplicationUser;

            case "ORGANIZATIONAL":
                if (user is SMSOrganizationalUser orgUser)
                {
                    return orgUser.Department?.Value != null &&
                           !string.IsNullOrEmpty(orgUser.Position) &&
                           !string.IsNullOrEmpty(orgUser.OrganizationLevel);
                }
                return false;

            case "STAKEHOLDER":
                if (user is SMSStakeholderUser stakeUser)
                {
                    return !string.IsNullOrEmpty(stakeUser.Organization) &&
                           !string.IsNullOrEmpty(stakeUser.StakeholderType);
                }
                return false;

            default:
                _logger.LogWarning("Unknown user type for validation: {UserType}", userType.Value);
                return false;
        }
    }

    /// <summary>
    /// Get completeness score (0-100) for user
    /// </summary>
    public int GetCompletenessScore(BaseUser user, SMSUserType userType)
    {
        if (user == null) return 0;

        int score = 0;
        int maxScore = 0;

        // Basic Info (40 points)
        maxScore += 40;
        if (HasValidBasicInfo(user)) score += 40;

        // Role (30 points)
        maxScore += 30;
        if (HasValidRole(user)) score += 30;

        // Permissions (20 points)
        maxScore += 20;
        if (HasValidPermissions(user)) score += 20;

        // Type-specific (10 points)
        maxScore += 10;
        if (HasValidTypeSpecificInfo(user, userType)) score += 10;

        return maxScore == 0 ? 0 : (score * 100) / maxScore;
    }

    /// <summary>
    /// Generate completeness report for logging/debugging
    /// </summary>
    public string GenerateCompletenessReport(BaseUser user, SMSUserType userType)
    {
        var report = new StringBuilder();
        report.AppendLine($"User Completeness Report for {user?.Code ?? "NULL"} ({userType.Value})");
        report.AppendLine($"Overall Complete: {IsUserComplete(user)}");
        report.AppendLine($"Completeness Score: {GetCompletenessScore(user, userType)}/100");
        
        report.AppendLine("\nValidation Details:");
        report.AppendLine($"  ? Basic Info: {HasValidBasicInfo(user)}");
        report.AppendLine($"  ? Role Info: {HasValidRole(user)}");
        report.AppendLine($"  ? Permissions: {HasValidPermissions(user)}");
        report.AppendLine($"  ? Type-Specific: {HasValidTypeSpecificInfo(user, userType)}");

        var missing = GetMissingComponents(user);
        if (missing.Any())
        {
            report.AppendLine("\nMissing Components:");
            foreach (var component in missing)
            {
                report.AppendLine($"  ? {component}");
            }
        }

        if (user?.UserRole?.Permissions != null)
        {
            report.AppendLine($"\nPermissions Summary: {user.UserRole.Permissions.Count} total");
            var moduleGroups = user.UserRole.Permissions.GroupBy(p => p.SMSModule);
            foreach (var group in moduleGroups)
            {
                var perm = group.First();
                var actions = new List<string>();
                if (perm.Create) actions.Add("C");
                if (perm.Read) actions.Add("R");
                if (perm.Update) actions.Add("U");
                if (perm.Delete) actions.Add("D");
                report.AppendLine($"  ?? {group.Key}: {string.Join(",", actions)}");
            }
        }

        return report.ToString();
    }

    /// <summary>
    /// Validate minimum required permissions for user type
    /// </summary>
    public bool HasMinimumRequiredPermissions(BaseUser user, SMSUserType userType)
    {
        if (!HasValidPermissions(user)) return false;

        var permissions = user.UserRole!.Permissions!;

        // Define minimum required modules per user type
        var requiredModules = userType.Value.ToUpperInvariant() switch
        {
            "APPLICATION" => new[] { "Users", "System" },
            "ORGANIZATIONAL" => new[] { "Reports", "Hazards" },
            "STAKEHOLDER" => new[] { "Reports" },
            _ => Array.Empty<string>()
        };

        // Check that user has at least Read permission on required modules
        foreach (var requiredModule in requiredModules)
        {
            var modulePermission = permissions.FirstOrDefault(p => p.SMSModule == requiredModule);
            if (modulePermission == null || !modulePermission.Read)
            {
                _logger.LogWarning("User {UserCode} missing required {Module} permission", user.Code, requiredModule);
                return false;
            }
        }

        return true;
    }
}
using SMS_Domain.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Shared.Common;
using SMS_Infrastructure.Interfaces;

namespace SMS_Application.Services;

/// <summary>
/// SMS Role Service implementation providing role management and authorization
/// </summary>
public class SMSRoleService : ISMSRoleService
{
    private readonly ISMSUserRoleRepository _userRoleRepository;

    public SMSRoleService(ISMSUserRoleRepository userRoleRepository)
    {
        _userRoleRepository = userRoleRepository ?? throw new ArgumentNullException(nameof(userRoleRepository));
    }

    public async Task<SMSUserRole> AssignRoleToUserAsync(
        string userID,
        string userType,
        SMSRole role,
        string department,
        string assignedBy,
        DateTime? effectiveDate = null,
        DateTime? expirationDate = null,
        string? notes = null)
    {
        var userRole = SMSUserRole.Create(
            userID,
            userType,
            role,
            department,
            assignedBy,
            effectiveDate,
            expirationDate,
            notes);

        var result = await _userRoleRepository.AddAsync(userRole);
        if (result.IsSuccess)
            return result.Value;

        throw new InvalidOperationException("Failed to assign role to user");
    }

    public async Task RemoveRoleFromUserAsync(string userID, string roleValue, string removedBy, string reason)
    {
        var userRoles = await _userRoleRepository.GetByUserIdAsync(userID);
        if (userRoles.IsSuccess)
        {
            var roleToRemove = userRoles.Value.FirstOrDefault(ur => ur.RoleValue == roleValue && ur.IsActive);
            if (roleToRemove != null)
            {
                roleToRemove.Deactivate(removedBy, reason);
                await _userRoleRepository.UpdateAsync(roleToRemove);
            }
        }
    }

    public async Task<IEnumerable<SMSUserRole>> GetActiveUserRolesAsync(string userID)
    {
        var result = await _userRoleRepository.GetActiveRolesByUserIdAsync(userID);
        return result.IsSuccess ? result.Value : Enumerable.Empty<SMSUserRole>();
    }

    public async Task<IEnumerable<SMSUserRole>> GetUsersWithRoleAsync(SMSRole role)
    {
        var result = await _userRoleRepository.GetByRoleValueAsync(role.Value);
        return result.IsSuccess ? result.Value.Where(ur => ur.IsActive) : Enumerable.Empty<SMSUserRole>();
    }

    public async Task<bool> UserHasRoleAsync(string userID, SMSRole role)
    {
        var userRoles = await GetActiveUserRolesAsync(userID);
        return userRoles.Any(ur => ur.RoleValue == role.Value);
    }

    public async Task<bool> UserHasRoleInCategoryAsync(string userID, string category)
    {
        var userRoles = await GetActiveUserRolesAsync(userID);
        var rolesInCategory = SMSRole.GetRolesByCategory(category);
        
        return userRoles.Any(ur => rolesInCategory.Any(rc => rc.Value == ur.RoleValue));
    }

    public async Task<int> GetUserMaxAuthorityLevelAsync(string userID)
    {
        var userRoles = await GetActiveUserRolesAsync(userID);
        if (!userRoles.Any())
            return 0;

        var maxAuthority = 0;
        foreach (var userRole in userRoles)
        {
            var role = SMSRole.GetAllValues().FirstOrDefault(r => r.Value == userRole.RoleValue);
            if (role != null && role.AuthorityLevel > maxAuthority)
            {
                maxAuthority = role.AuthorityLevel;
            }
        }

        return maxAuthority;
    }

    public async Task<bool> UserCanApproveAsync(string userID, int requiredAuthorityLevel)
    {
        var maxAuthority = await GetUserMaxAuthorityLevelAsync(userID);
        return maxAuthority >= requiredAuthorityLevel;
    }

    public async Task<IEnumerable<SMSUserRole>> GetUsersWhoCanApproveAsync(int requiredAuthorityLevel)
    {
        var allUserRoles = await _userRoleRepository.GetAllActiveAsync();
        if (allUserRoles.IsFailure)
            return Enumerable.Empty<SMSUserRole>();

        var eligibleRoles = new List<SMSUserRole>();
        foreach (var userRole in allUserRoles.Value)
        {
            var role = SMSRole.GetAllValues().FirstOrDefault(r => r.Value == userRole.RoleValue);
            if (role != null && role.AuthorityLevel >= requiredAuthorityLevel)
            {
                eligibleRoles.Add(userRole);
            }
        }

        return eligibleRoles;
    }

    public async Task<IEnumerable<SMSUserRole>> GetExpiringRoleAssignmentsAsync(int daysFromNow)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(daysFromNow);
        var result = await _userRoleRepository.GetExpiringRolesAsync(cutoffDate);
        return result.IsSuccess ? result.Value : Enumerable.Empty<SMSUserRole>();
    }

    public async Task<bool> ValidateUserAuthorizationAsync(string userID, SMSRole requiredRole)
    {
        return await UserHasRoleAsync(userID, requiredRole);
    }

    public async Task<IEnumerable<SMSUserRole>> GetRoleAssignmentHistoryAsync(string? userID = null, string? roleValue = null)
    {
        if (!string.IsNullOrEmpty(userID))
        {
            var result = await _userRoleRepository.GetByUserIdAsync(userID);
            return result.IsSuccess ? result.Value : Enumerable.Empty<SMSUserRole>();
        }

        if (!string.IsNullOrEmpty(roleValue))
        {
            var result = await _userRoleRepository.GetByRoleValueAsync(roleValue);
            return result.IsSuccess ? result.Value : Enumerable.Empty<SMSUserRole>();
        }

        var allResult = await _userRoleRepository.GetAllAsync();
        return allResult.IsSuccess ? allResult.Value : Enumerable.Empty<SMSUserRole>();
    }

    public async Task<IEnumerable<SMSUserRole>> BulkAssignRoleAsync(
        IEnumerable<string> userIDs,
        string userType,
        SMSRole role,
        string department,
        string assignedBy)
    {
        var assignments = new List<SMSUserRole>();
        
        foreach (var userID in userIDs)
        {
            try
            {
                var assignment = await AssignRoleToUserAsync(userID, userType, role, department, assignedBy);
                assignments.Add(assignment);
            }
            catch
            {
                // Continue with other assignments if one fails
                continue;
            }
        }

        return assignments;
    }

    public async Task<Dictionary<string, int>> GetRoleAssignmentStatsAsync()
    {
        var stats = new Dictionary<string, int>();
        
        var allRoles = await _userRoleRepository.GetAllActiveAsync();
        if (allRoles.IsSuccess)
        {
            var roleGroups = allRoles.Value.GroupBy(ur => ur.RoleValue);
            foreach (var group in roleGroups)
            {
                stats[group.Key] = group.Count();
            }
        }

        return stats;
    }
}
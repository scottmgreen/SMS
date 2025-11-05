using SMS_Domain.Common;
using SMS_Domain.ValueObjects;

namespace SMS_Domain.Entities;

/// <summary>
/// Represents an SMS application user entity for system administrators and application-level users
/// </summary>
public sealed class SMSApplicationUser : BaseUser
{
    public SMSApplicationUserID ApplicationUserId { get; private set; }
    public string ApplicationRole { get; private set; }
    public string PermissionLevel { get; private set; }

    // For Entity Framework
    private SMSApplicationUser() : base()
    {
        ApplicationUserId = new SMSApplicationUserID(Guid.NewGuid().ToString());
        ApplicationRole = string.Empty;
        PermissionLevel = string.Empty;
    }

    private SMSApplicationUser(
        string code,
        FirstName firstName,
        LastName lastName,
        UserName userName,
        Password password,
        string applicationRole,
        string permissionLevel,
        string createdBy) : base(code, firstName, lastName, userName, password, createdBy)
    {
        ApplicationUserId = new SMSApplicationUserID(UserId.Value);
        ApplicationRole = applicationRole;
        PermissionLevel = permissionLevel;
    }

    public static SMSApplicationUser Create(
        string code,
        FirstName firstName,
        LastName lastName,
        UserName userName,
        Password password,
        string applicationRole,
        string permissionLevel,
        string createdBy)
    {
        return new SMSApplicationUser(
            code,
            firstName,
            lastName,
            userName,
            password,
            applicationRole,
            permissionLevel,
            createdBy);
    }

    /// <summary>
    /// Updates the application-specific properties
    /// </summary>
    public void UpdateApplicationInfo(string applicationRole, string permissionLevel)
    {
        ApplicationRole = applicationRole;
        PermissionLevel = permissionLevel;
    }

    /// <summary>
    /// Checks if the user has a specific application role
    /// </summary>
    public bool HasApplicationRole(string role)
    {
        return string.Equals(ApplicationRole, role, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the user has sufficient permission level
    /// </summary>
    public bool HasPermissionLevel(string requiredLevel)
    {
        // Define permission hierarchy (could be moved to an enum or constants)
        var levels = new[] { "Read", "Write", "Admin", "SuperAdmin" };
        
        var userLevelIndex = Array.IndexOf(levels, PermissionLevel);
        var requiredLevelIndex = Array.IndexOf(levels, requiredLevel);
        
        return userLevelIndex >= requiredLevelIndex;
    }

    public override string GetUserType() => "ApplicationUser";

    public override string GetDepartmentInfo() => $"Application Role: {ApplicationRole}, Permission Level: {PermissionLevel}";

    /// <summary>
    /// Gets application-specific user information for display
    /// </summary>
    public string GetApplicationSummary()
    {
        return $"{DisplayName} ({ApplicationRole} - {PermissionLevel})";
    }
}
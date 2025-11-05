using SMS_Domain.Common;
using SMS_Domain.ValueObjects;

namespace SMS_Domain.Entities;

/// <summary>
/// Represents an SMS organizational user entity for internal Port of Portland employees
/// </summary>
public sealed class SMSOrganizationalUser : BaseUser
{
    public SMSOrganizationalUserID OrganizationalUserId { get; private set; }
    public string Department { get; private set; }
    public string Position { get; private set; }
    public string OrganizationLevel { get; private set; }

    // For Entity Framework
    private SMSOrganizationalUser() : base()
    {
        OrganizationalUserId = new SMSOrganizationalUserID(Guid.NewGuid().ToString());
        Department = string.Empty;
        Position = string.Empty;
        OrganizationLevel = string.Empty;
    }

    private SMSOrganizationalUser(
        string code,
        FirstName firstName,
        LastName lastName,
        UserName userName,
        Password password,
        string department,
        string position,
        string organizationLevel,
        string createdBy) : base(code, firstName, lastName, userName, password, createdBy)
    {
        OrganizationalUserId = new SMSOrganizationalUserID(UserId.Value);
        Department = department;
        Position = position;
        OrganizationLevel = organizationLevel;
    }

    public static SMSOrganizationalUser Create(
        string code,
        FirstName firstName,
        LastName lastName,
        UserName userName,
        Password password,
        string department,
        string position,
        string organizationLevel,
        string createdBy)
    {
        return new SMSOrganizationalUser(
            code,
            firstName,
            lastName,
            userName,
            password,
            department,
            position,
            organizationLevel,
            createdBy);
    }

    /// <summary>
    /// Updates the organizational-specific properties
    /// </summary>
    public void UpdateOrganizationalInfo(string department, string position, string organizationLevel)
    {
        Department = department;
        Position = position;
        OrganizationLevel = organizationLevel;
    }

    /// <summary>
    /// Checks if the user belongs to a specific department
    /// </summary>
    public bool IsInDepartment(string department)
    {
        return string.Equals(Department, department, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the user has a specific position
    /// </summary>
    public bool HasPosition(string position)
    {
        return string.Equals(Position, position, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the user is at or above a specific organization level
    /// </summary>
    public bool HasOrganizationLevel(string requiredLevel)
    {
        // Define organization hierarchy levels
        var levels = new[] { "Staff", "Supervisor", "Manager", "Director", "Executive" };
        
        var userLevelIndex = Array.IndexOf(levels, OrganizationLevel);
        var requiredLevelIndex = Array.IndexOf(levels, requiredLevel);
        
        return userLevelIndex >= requiredLevelIndex;
    }

    /// <summary>
    /// Gets the user's organizational hierarchy display
    /// </summary>
    public string GetOrganizationalHierarchy()
    {
        return $"{Department} - {Position} ({OrganizationLevel})";
    }

    public override string GetUserType() => "OrganizationalUser";

    public override string GetDepartmentInfo() => Department;

    /// <summary>
    /// Gets organizational-specific user information for display
    /// </summary>
    public string GetOrganizationalSummary()
    {
        return $"{DisplayName} - {Position}, {Department}";
    }

    /// <summary>
    /// Checks if this user can supervise another organizational user
    /// </summary>
    public bool CanSupervise(SMSOrganizationalUser otherUser)
    {
        // Same department and higher organization level
        return IsInDepartment(otherUser.Department) && 
               HasOrganizationLevel("Supervisor") &&
               Array.IndexOf(new[] { "Staff", "Supervisor", "Manager", "Director", "Executive" }, OrganizationLevel) >
               Array.IndexOf(new[] { "Staff", "Supervisor", "Manager", "Director", "Executive" }, otherUser.OrganizationLevel);
    }
}
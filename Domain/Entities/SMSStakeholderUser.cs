using SMS_Domain.Common;
using SMS_Domain.ValueObjects;

namespace SMS_Domain.Entities;

/// <summary>
/// Represents an SMS stakeholder user entity for external partners, airlines, contractors, etc.
/// </summary>
public sealed class SMSStakeholderUser : BaseUser
{
    public SMSStakeholderUserID StakeholderUserId { get; private set; }
    public string StakeholderType { get; private set; }
    public string Organization { get; private set; }
    public string AccessLevel { get; private set; }

    // For Entity Framework
    private SMSStakeholderUser() : base()
    {
        StakeholderUserId = new SMSStakeholderUserID(Guid.NewGuid().ToString());
        StakeholderType = string.Empty;
        Organization = string.Empty;
        AccessLevel = string.Empty;
    }

    private SMSStakeholderUser(
        string code,
        FirstName firstName,
        LastName lastName,
        UserName userName,
        Password password,
        string stakeholderType,
        string organization,
        string accessLevel,
        string createdBy) : base(code, firstName, lastName, userName, password, createdBy)
    {
        StakeholderUserId = new SMSStakeholderUserID(UserId.Value);
        StakeholderType = stakeholderType;
        Organization = organization;
        AccessLevel = accessLevel;
    }

    public static SMSStakeholderUser Create(
        string code,
        FirstName firstName,
        LastName lastName,
        UserName userName,
        Password password,
        string stakeholderType,
        string organization,
        string accessLevel,
        string createdBy)
    {
        return new SMSStakeholderUser(
            code,
            firstName,
            lastName,
            userName,
            password,
            stakeholderType,
            organization,
            accessLevel,
            createdBy);
    }

    /// <summary>
    /// Updates the stakeholder-specific properties
    /// </summary>
    public void UpdateStakeholderInfo(string stakeholderType, string organization, string accessLevel)
    {
        StakeholderType = stakeholderType;
        Organization = organization;
        AccessLevel = accessLevel;
    }

    /// <summary>
    /// Checks if the user is of a specific stakeholder type
    /// </summary>
    public bool IsStakeholderType(string type)
    {
        return string.Equals(StakeholderType, type, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the user belongs to a specific organization
    /// </summary>
    public bool IsFromOrganization(string organization)
    {
        return string.Equals(Organization, organization, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the user has sufficient access level
    /// </summary>
    public bool HasAccessLevel(string requiredLevel)
    {
        // Define access hierarchy for stakeholders
        var levels = new[] { "Limited", "Standard", "Extended", "Full" };
        
        var userLevelIndex = Array.IndexOf(levels, AccessLevel);
        var requiredLevelIndex = Array.IndexOf(levels, requiredLevel);
        
        return userLevelIndex >= requiredLevelIndex;
    }

    /// <summary>
    /// Checks if this is an airline stakeholder
    /// </summary>
    public bool IsAirlineStakeholder => IsStakeholderType("Airline");

    /// <summary>
    /// Checks if this is a ground handler stakeholder
    /// </summary>
    public bool IsGroundHandlerStakeholder => IsStakeholderType("Ground Handler");

    /// <summary>
    /// Checks if this is a contractor stakeholder
    /// </summary>
    public bool IsContractorStakeholder => IsStakeholderType("Contractor");

    /// <summary>
    /// Checks if this is a tenant stakeholder
    /// </summary>
    public bool IsTenantStakeholder => IsStakeholderType("Tenant");

    /// <summary>
    /// Checks if this is a regulatory stakeholder
    /// </summary>
    public bool IsRegulatoryStakeholder => IsStakeholderType("Regulatory");

    /// <summary>
    /// Gets the stakeholder's external organization display
    /// </summary>
    public string GetStakeholderSummary()
    {
        return $"{Organization} ({StakeholderType})";
    }

    public override string GetUserType() => "StakeholderUser";

    public override string GetDepartmentInfo() => $"{Organization} - {StakeholderType}";

    /// <summary>
    /// Gets stakeholder-specific user information for display
    /// </summary>
    public string GetStakeholderDetails()
    {
        return $"{DisplayName} - {StakeholderType} from {Organization} (Access: {AccessLevel})";
    }

    /// <summary>
    /// Checks if this stakeholder requires AOA (Airport Operations Area) access
    /// </summary>
    public bool RequiresAOAAccess()
    {
        return IsAirlineStakeholder || IsGroundHandlerStakeholder || IsContractorStakeholder;
    }

    /// <summary>
    /// Gets the appropriate badge/access type for this stakeholder
    /// </summary>
    public string GetBadgeType()
    {
        return StakeholderType switch
        {
            "Airline" => "Airline Operations Badge",
            "Ground Handler" => "Ground Operations Badge",
            "Contractor" => "Contractor Badge",
            "Tenant" => "Tenant Access Badge",
            "Regulatory" => "Official Visitor Badge",
            _ => "General Visitor Badge"
        };
    }
}
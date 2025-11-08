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
    public StakeholderPermissions Permissions { get; private set; }

    // For Entity Framework
    private SMSStakeholderUser() : base()
    {
        StakeholderUserId = new SMSStakeholderUserID(Guid.NewGuid().ToString());
        StakeholderType = string.Empty;
        Organization = string.Empty;
        AccessLevel = string.Empty;
        Permissions = StakeholderPermissions.Limited;
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
        StakeholderPermissions permissions,
        string createdBy) : base(code, firstName, lastName, userName, password, createdBy)
    {
        StakeholderUserId = new SMSStakeholderUserID(UserId.Value);
        StakeholderType = stakeholderType;
        Organization = organization;
        AccessLevel = accessLevel;
        Permissions = permissions;
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
        // Create permissions based on access level
        var permissionsResult = StakeholderPermissions.Create(accessLevel);
        var permissions = permissionsResult.IsSuccess ? permissionsResult.Value : StakeholderPermissions.Limited;

        return new SMSStakeholderUser(
            code,
            firstName,
            lastName,
            userName,
            password,
            stakeholderType,
            organization,
            accessLevel,
            permissions,
            createdBy);
    }

    public static SMSStakeholderUser CreateWithCustomPermissions(
        string code,
        FirstName firstName,
        LastName lastName,
        UserName userName,
        Password password,
        string stakeholderType,
        string organization,
        StakeholderPermissions permissions,
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
            permissions.GetAccessLevel(),
            permissions,
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
        
        // Update permissions based on new access level
        var permissionsResult = StakeholderPermissions.Create(accessLevel);
        if (permissionsResult.IsSuccess)
        {
            Permissions = permissionsResult.Value;
        }
    }

    /// <summary>
    /// Updates stakeholder permissions directly
    /// </summary>
    public void UpdatePermissions(StakeholderPermissions permissions)
    {
        Permissions = permissions;
        AccessLevel = permissions.GetAccessLevel();
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
    /// Checks if the user has a specific stakeholder permission
    /// </summary>
    public bool HasPermission(string permission)
    {
        return Permissions.HasPermission(permission);
    }

    /// <summary>
    /// Checks if user can access specific data types
    /// </summary>
    public bool CanAccessData(string dataType)
    {
        return dataType.ToUpperInvariant() switch
        {
            "PUBLIC_REPORTS" => Permissions.CanViewPublicReports,
            "ORGANIZATION_DATA" => Permissions.CanViewOrganizationSpecificData,
            "AIRSIDE_DATA" => Permissions.CanViewAirsideOperationalData,
            "HISTORICAL_DATA" => Permissions.CanViewHistoricalData,
            "AOA_INFORMATION" => Permissions.CanAccessAOAInformation,
            "WEATHER_DATA" => Permissions.CanViewWeatherData,
            "NOTAMS" => Permissions.CanViewNOTAMs,
            "OPERATIONAL_BRIEFINGS" => Permissions.CanAccessOperationalBriefings,
            _ => false
        };
    }

    /// <summary>
    /// Checks if user can perform specific participation actions
    /// </summary>
    public bool CanParticipate(string action)
    {
        return action.ToUpperInvariant() switch
        {
            "COMMITTEES" => Permissions.CanParticipateInCommittees,
            "SUBMIT_HAZARD_REPORTS" => Permissions.CanSubmitHazardReports,
            "COMMENT_ON_REPORTS" => Permissions.CanCommentOnReports,
            "RECEIVE_NOTIFICATIONS" => Permissions.CanReceiveNotifications,
            _ => false
        };
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
        return Permissions.RequiresAOAAccess(StakeholderType);
    }

    /// <summary>
    /// Gets the appropriate badge/access type for this stakeholder
    /// </summary>
    public string GetBadgeType()
    {
        return Permissions.GetBadgeType(StakeholderType);
    }

    /// <summary>
    /// Checks if stakeholder has operational access needs
    /// </summary>
    public bool HasOperationalAccess()
    {
        return IsAirlineStakeholder || IsGroundHandlerStakeholder || IsContractorStakeholder;
    }

    /// <summary>
    /// Checks if stakeholder is a regulatory authority
    /// </summary>
    public bool IsRegulatoryAuthority()
    {
        return IsRegulatoryStakeholder && AccessLevel == "Full";
    }

    /// <summary>
    /// Gets stakeholder access summary for administrative purposes
    /// </summary>
    public string GetAccessSummary()
    {
        var accessTypes = new List<string>();
        
        if (Permissions.CanViewPublicReports) accessTypes.Add("Public Reports");
        if (Permissions.CanViewOrganizationSpecificData) accessTypes.Add("Organization Data");
        if (Permissions.CanViewAirsideOperationalData) accessTypes.Add("Airside Operations");
        if (Permissions.CanSubmitHazardReports) accessTypes.Add("Hazard Reporting");
        if (Permissions.CanParticipateInCommittees) accessTypes.Add("Committee Participation");
        if (Permissions.CanAccessAOAInformation) accessTypes.Add("AOA Access");

        return accessTypes.Any() ? string.Join(", ", accessTypes) : "Limited Access";
    }

    /// <summary>
    /// Gets recommended training based on stakeholder type and access level
    /// </summary>
    public string[] GetRecommendedTraining()
    {
        var training = new List<string> { "SMS Overview", "Safety Reporting Basics" };

        if (HasOperationalAccess())
        {
            training.AddRange(new[] { "Airside Safety", "Ground Operations Safety" });
        }

        if (Permissions.CanSubmitHazardReports)
        {
            training.Add("Hazard Identification and Reporting");
        }

        if (Permissions.CanParticipateInCommittees)
        {
            training.Add("Committee Participation and Safety Culture");
        }

        if (RequiresAOAAccess())
        {
            training.AddRange(new[] { "AOA Security", "Vehicle Operations" });
        }

        return training.ToArray();
    }
}
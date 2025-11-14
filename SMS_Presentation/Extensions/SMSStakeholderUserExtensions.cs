using SMS_Domain.Entities;

namespace SMS.Presentation.Extensions;

/// <summary>
/// Extension methods for SMSStakeholderUser to provide UI compatibility properties
/// </summary>
public static class SMSStakeholderUserExtensions
{
    /// <summary>
    /// Gets the organization name for UI compatibility (maps to Organization property)
    /// </summary>
    public static string OrganizationName(this SMSStakeholderUser stakeholder)
    {
        return stakeholder.Organization;
    }

    /// <summary>
    /// Gets the stakeholder category for UI compatibility (maps to StakeholderType property)
    /// </summary>
    public static string StakeholderCategory(this SMSStakeholderUser stakeholder)
    {
        return stakeholder.StakeholderType;
    }

    /// <summary>
    /// Gets the stakeholder type for UI compatibility (same as StakeholderType)
    /// </summary>
    public static string StakeholderType_UI(this SMSStakeholderUser stakeholder)
    {
        return stakeholder.StakeholderType;
    }

    /// <summary>
    /// Gets display summary for stakeholder
    /// </summary>
    public static string GetStakeholderDisplaySummary(this SMSStakeholderUser stakeholder)
    {
        return $"{stakeholder.DisplayName} ({stakeholder.Organization}) - {stakeholder.StakeholderType}";
    }

    /// <summary>
    /// Checks if stakeholder is active (for UI filtering)
    /// </summary>
    public static bool IsActiveStakeholder(this SMSStakeholderUser stakeholder)
    {
        return stakeholder.IsActive;
    }

    /// <summary>
    /// Gets stakeholder access level description for UI display
    /// </summary>
    public static string GetAccessLevelDescription(this SMSStakeholderUser stakeholder)
    {
        return stakeholder.AccessLevel switch
        {
            "Limited" => "Limited Access - View Only",
            "Standard" => "Standard Access - View & Comment",
            "Extended" => "Extended Access - Full Participation",
            "Full" => "Full Access - All Operations",
            _ => $"Custom Access - {stakeholder.AccessLevel}"
        };
    }

    /// <summary>
    /// Gets CSS class for stakeholder type badge
    /// </summary>
    public static string GetStakeholderTypeBadgeClass(this SMSStakeholderUser stakeholder)
    {
        return stakeholder.StakeholderType.ToLowerInvariant() switch
        {
            "airline" => "badge bg-primary",
            "ground handler" => "badge bg-success", 
            "contractor" => "badge bg-warning text-dark",
            "tenant" => "badge bg-info",
            "government agency" => "badge bg-danger",
            "regulatory body" => "badge bg-dark",
            _ => "badge bg-secondary"
        };
    }

    /// <summary>
    /// Gets icon class for stakeholder type
    /// </summary>
    public static string GetStakeholderTypeIcon(this SMSStakeholderUser stakeholder)
    {
        return stakeholder.StakeholderType.ToLowerInvariant() switch
        {
            "airline" => "fas fa-plane",
            "ground handler" => "fas fa-truck",
            "contractor" => "fas fa-hard-hat", 
            "tenant" => "fas fa-building",
            "government agency" => "fas fa-flag",
            "regulatory body" => "fas fa-gavel",
            "emergency services" => "fas fa-truck-medical",
            "vendor" or "service provider" => "fas fa-handshake",
            _ => "fas fa-user-tie"
        };
    }

    /// <summary>
    /// Checks if stakeholder has sufficient access for risk assessment participation
    /// </summary>
    public static bool CanParticipateInRiskAssessment(this SMSStakeholderUser stakeholder)
    {
        return stakeholder.IsActive && 
               (stakeholder.AccessLevel == "Extended" || 
                stakeholder.AccessLevel == "Full" ||
                stakeholder.Permissions.CanParticipateInCommittees);
    }

    /// <summary>
    /// Gets stakeholder selection priority (for UI ordering)
    /// </summary>
    public static int GetSelectionPriority(this SMSStakeholderUser stakeholder)
    {
        // Higher priority stakeholders appear first in selection lists
        return stakeholder.StakeholderType.ToLowerInvariant() switch
        {
            "airline" => 1,
            "ground handler" => 2,
            "government agency" => 3,
            "regulatory body" => 4,
            "emergency services" => 5,
            "tenant" => 6,
            "contractor" => 7,
            "vendor" or "service provider" => 8,
            _ => 9
        };
    }

    /// <summary>
    /// Gets recommended stakeholder groups this user should be part of
    /// </summary>
    public static List<string> GetRecommendedGroups(this SMSStakeholderUser stakeholder)
    {
        var groups = new List<string>();

        switch (stakeholder.StakeholderType.ToLowerInvariant())
        {
            case "airline":
                groups.AddRange(new[] { "AIRLINE", "AIRLINE_OPS", "AIRSIDE_USERS" });
                break;
            case "ground handler":
                groups.AddRange(new[] { "VENDOR", "GROUND_HANDLING", "AIRSIDE_USERS" });
                break;
            case "contractor":
                groups.AddRange(new[] { "VENDOR", "CONTRACTORS" });
                break;
            case "tenant":
                groups.AddRange(new[] { "TENANTS", "AIRSIDE_USERS" });
                break;
            case "government agency":
            case "regulatory body":
                groups.AddRange(new[] { "REGULATORY", "OVERSIGHT" });
                break;
            case "emergency services":
                groups.AddRange(new[] { "FIRE", "EMERGENCY_RESPONSE" });
                break;
            default:
                groups.Add("VENDOR");
                break;
        }

        return groups;
    }
}

/// <summary>
/// Extension methods for SMSApplicationUser to provide UI compatibility properties
/// </summary>
public static class SMSApplicationUserExtensions
{
    /// <summary>
    /// Gets the email address for UI compatibility
    /// </summary>
    public static string Email(this SMSApplicationUser user)
    {
        // Return a placeholder email based on username
        return $"{user.UserName.Value}@pdxairport.org";
    }

    /// <summary>
    /// Gets the organization name for UI compatibility
    /// </summary>
    public static string OrganizationName(this SMSApplicationUser user)
    {
        return "PDX SMS Team";
    }

    /// <summary>
    /// Gets safe display string for ID
    /// </summary>
    public static string GetSafeIdString(this SMSApplicationUser user)
    {
        return user.Id.Value;
    }

    /// <summary>
    /// Gets safe display name with fallback
    /// </summary>
    public static string GetSafeDisplayName(this SMSApplicationUser user)
    {
        return user.DisplayName ?? user.GetSafeIdString();
    }
}
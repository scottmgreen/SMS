using SMS_Domain.Common;
using SMS_Shared.Common;
using SMS_Domain.Errors;

namespace SMS_Domain.ValueObjects;

/// <summary>
/// Stakeholder permissions for SMS Stakeholder Users
/// Controls external user access to specific areas and data
/// </summary>
public sealed class StakeholderPermissions : BaseValueObject
{
    // Data Access Permissions
    public bool CanViewPublicReports { get; }
    public bool CanViewOrganizationSpecificData { get; }
    public bool CanViewAirsideOperationalData { get; }
    public bool CanViewHistoricalData { get; }
    
    // Participation Permissions
    public bool CanParticipateInCommittees { get; }
    public bool CanSubmitHazardReports { get; }
    public bool CanCommentOnReports { get; }
    public bool CanReceiveNotifications { get; }
    
    // Operational Permissions
    public bool CanAccessAOAInformation { get; }
    public bool CanViewWeatherData { get; }
    public bool CanViewNOTAMs { get; }
    public bool CanAccessOperationalBriefings { get; }

    private StakeholderPermissions(
        bool canViewPublicReports, bool canViewOrganizationSpecificData, bool canViewAirsideOperationalData, bool canViewHistoricalData,
        bool canParticipateInCommittees, bool canSubmitHazardReports, bool canCommentOnReports, bool canReceiveNotifications,
        bool canAccessAOAInformation, bool canViewWeatherData, bool canViewNOTAMs, bool canAccessOperationalBriefings)
    {
        CanViewPublicReports = canViewPublicReports;
        CanViewOrganizationSpecificData = canViewOrganizationSpecificData;
        CanViewAirsideOperationalData = canViewAirsideOperationalData;
        CanViewHistoricalData = canViewHistoricalData;
        CanParticipateInCommittees = canParticipateInCommittees;
        CanSubmitHazardReports = canSubmitHazardReports;
        CanCommentOnReports = canCommentOnReports;
        CanReceiveNotifications = canReceiveNotifications;
        CanAccessAOAInformation = canAccessAOAInformation;
        CanViewWeatherData = canViewWeatherData;
        CanViewNOTAMs = canViewNOTAMs;
        CanAccessOperationalBriefings = canAccessOperationalBriefings;
    }

    /// <summary>
    /// Creates stakeholder permissions based on access level
    /// </summary>
    public static Result<StakeholderPermissions> Create(string accessLevel) =>
        Result.Create(accessLevel, DomainErrors.SMSStakeholderUserError.AccessLevelRequired)
            .Ensure(a => !string.IsNullOrWhiteSpace(a), DomainErrors.SMSStakeholderUserError.AccessLevelRequired)
            .Ensure(a => IsValidAccessLevel(a), DomainErrors.SMSStakeholderUserError.InvalidAccessLevel)
            .Map(a => CreateFromLevel(a));

    /// <summary>
    /// Creates stakeholder permissions from individual permission flags
    /// </summary>
    public static StakeholderPermissions CreateCustom(
        bool canViewPublicReports = true, bool canViewOrganizationSpecificData = false, bool canViewAirsideOperationalData = false, bool canViewHistoricalData = false,
        bool canParticipateInCommittees = false, bool canSubmitHazardReports = false, bool canCommentOnReports = false, bool canReceiveNotifications = true,
        bool canAccessAOAInformation = false, bool canViewWeatherData = false, bool canViewNOTAMs = false, bool canAccessOperationalBriefings = false)
    {
        return new StakeholderPermissions(
            canViewPublicReports, canViewOrganizationSpecificData, canViewAirsideOperationalData, canViewHistoricalData,
            canParticipateInCommittees, canSubmitHazardReports, canCommentOnReports, canReceiveNotifications,
            canAccessAOAInformation, canViewWeatherData, canViewNOTAMs, canAccessOperationalBriefings);
    }

    private static bool IsValidAccessLevel(string level)
    {
        var validLevels = new[] { "Limited", "Standard", "Extended", "Full" };
        return validLevels.Contains(level, StringComparer.OrdinalIgnoreCase);
    }

    private static StakeholderPermissions CreateFromLevel(string level)
    {
        return level.ToUpperInvariant() switch
        {
            "LIMITED" => Limited,
            "STANDARD" => Standard,
            "EXTENDED" => Extended,
            "FULL" => Full,
            _ => Limited
        };
    }

    // Predefined permission sets based on access level
    public static StakeholderPermissions Limited => new(
        canViewPublicReports: true, canViewOrganizationSpecificData: false, canViewAirsideOperationalData: false, canViewHistoricalData: false,
        canParticipateInCommittees: false, canSubmitHazardReports: false, canCommentOnReports: false, canReceiveNotifications: true,
        canAccessAOAInformation: false, canViewWeatherData: false, canViewNOTAMs: false, canAccessOperationalBriefings: false);

    public static StakeholderPermissions Standard => new(
        canViewPublicReports: true, canViewOrganizationSpecificData: true, canViewAirsideOperationalData: false, canViewHistoricalData: false,
        canParticipateInCommittees: true, canSubmitHazardReports: true, canCommentOnReports: true, canReceiveNotifications: true,
        canAccessAOAInformation: false, canViewWeatherData: true, canViewNOTAMs: true, canAccessOperationalBriefings: false);

    public static StakeholderPermissions Extended => new(
        canViewPublicReports: true, canViewOrganizationSpecificData: true, canViewAirsideOperationalData: true, canViewHistoricalData: true,
        canParticipateInCommittees: true, canSubmitHazardReports: true, canCommentOnReports: true, canReceiveNotifications: true,
        canAccessAOAInformation: true, canViewWeatherData: true, canViewNOTAMs: true, canAccessOperationalBriefings: true);

    public static StakeholderPermissions Full => new(
        canViewPublicReports: true, canViewOrganizationSpecificData: true, canViewAirsideOperationalData: true, canViewHistoricalData: true,
        canParticipateInCommittees: true, canSubmitHazardReports: true, canCommentOnReports: true, canReceiveNotifications: true,
        canAccessAOAInformation: true, canViewWeatherData: true, canViewNOTAMs: true, canAccessOperationalBriefings: true);

    /// <summary>
    /// Checks if a specific stakeholder permission is granted
    /// </summary>
    public bool HasPermission(string permission)
    {
        return permission.ToUpperInvariant() switch
        {
            "VIEW_PUBLIC_REPORTS" => CanViewPublicReports,
            "VIEW_ORGANIZATION_DATA" => CanViewOrganizationSpecificData,
            "VIEW_AIRSIDE_DATA" => CanViewAirsideOperationalData,
            "VIEW_HISTORICAL_DATA" => CanViewHistoricalData,
            "PARTICIPATE_IN_COMMITTEES" => CanParticipateInCommittees,
            "SUBMIT_HAZARD_REPORTS" => CanSubmitHazardReports,
            "COMMENT_ON_REPORTS" => CanCommentOnReports,
            "RECEIVE_NOTIFICATIONS" => CanReceiveNotifications,
            "ACCESS_AOA_INFORMATION" => CanAccessAOAInformation,
            "VIEW_WEATHER_DATA" => CanViewWeatherData,
            "VIEW_NOTAMS" => CanViewNOTAMs,
            "ACCESS_OPERATIONAL_BRIEFINGS" => CanAccessOperationalBriefings,
            _ => false
        };
    }

    /// <summary>
    /// Checks if stakeholder type requires AOA access
    /// </summary>
    public bool RequiresAOAAccess(string stakeholderType)
    {
        var aoaRequiredTypes = new[] { "Airline", "Ground Handler", "Contractor" };
        return aoaRequiredTypes.Contains(stakeholderType, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets appropriate badge type for stakeholder
    /// </summary>
    public string GetBadgeType(string stakeholderType)
    {
        return stakeholderType.ToUpperInvariant() switch
        {
            "AIRLINE" => "Airline Operations Badge",
            "GROUND HANDLER" => "Ground Operations Badge",
            "CONTRACTOR" => "Contractor Badge",
            "TENANT" => "Tenant Access Badge",
            "REGULATORY" => "Official Visitor Badge",
            _ => "General Visitor Badge"
        };
    }

    /// <summary>
    /// Gets the access level string representation
    /// </summary>
    public string GetAccessLevel()
    {
        if (CanAccessOperationalBriefings && CanViewAirsideOperationalData) return "Full";
        if (CanViewAirsideOperationalData) return "Extended";
        if (CanSubmitHazardReports) return "Standard";
        return "Limited";
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return CanViewPublicReports;
        yield return CanViewOrganizationSpecificData;
        yield return CanViewAirsideOperationalData;
        yield return CanViewHistoricalData;
        yield return CanParticipateInCommittees;
        yield return CanSubmitHazardReports;
        yield return CanCommentOnReports;
        yield return CanReceiveNotifications;
        yield return CanAccessAOAInformation;
        yield return CanViewWeatherData;
        yield return CanViewNOTAMs;
        yield return CanAccessOperationalBriefings;
    }

    public override string ToString() => $"StakeholderPermissions({GetAccessLevel()})";
}
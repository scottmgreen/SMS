namespace SMS_Domain.Entities;

/// <summary>
/// Represents an SMS organizational user entity for internal Port of Portland employees
/// </summary>
public sealed class SMSOrganizationalUser : BaseUser
{
    public SMSOrganizationalUser(SMSOrganizationalUserID id) : base(id, "SYSTEM", DateTime.UtcNow)
    {
        OrganizationalUserId = id;
    }
    public SMSOrganizationalUserID OrganizationalUserId { get; private set; }
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    /// <summary>
    /// OrganizationLevel (AE, RE, RM, SMS Manager, SMS Coordinator, SMS Team)
    /// </summary>
    public string OrganizationLevel { get; set; } = string.Empty;


    public string? SMSRole { get; set; }

    /// <summary>
    /// Authority level for risk approval (Strategic, Executive, Operational, Process, Support)
    /// </summary>
    public string? AuthorityLevel { get; set; }

    /// <summary>
    /// Risk levels this user can approve (Critical/High, High/Escalated, Medium/Low, etc.)
    /// </summary>
    public string? RiskApprovalAuthority { get; set; }
}
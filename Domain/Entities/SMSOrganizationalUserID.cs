namespace SMS_Domain.Entities;

/// <summary>
/// Represents a unique identifier for SMS Organizational User entities
/// </summary>
public sealed class SMSOrganizationalUserID : BaseUserID
{
    public SMSOrganizationalUserID(string id) : base(id) { }

    // Implicit conversion from string for convenience
    public static implicit operator SMSOrganizationalUserID(string value) => new(value);
}
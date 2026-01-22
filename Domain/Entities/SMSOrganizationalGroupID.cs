namespace Domain.Entities;

/// <summary>
/// Value object representing a unique identifier for SMS Organizational Groups
/// Follows the same pattern as other group IDs in the system
/// </summary>
public sealed class SMSOrganizationalGroupID : BaseUserID
{
    public SMSOrganizationalGroupID(string id) : base(id) { }

    // Implicit conversion from string for convenience
    public static implicit operator SMSOrganizationalGroupID(string value) => new(value);
}
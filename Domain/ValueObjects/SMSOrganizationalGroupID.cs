using SMS_Domain.Common;

namespace SMS_Domain.ValueObjects;

/// <summary>
/// Value object representing a unique identifier for SMS Organizational Groups
/// Follows the same pattern as other group IDs in the system
/// </summary>
public sealed class SMSOrganizationalGroupID : BaseID<string>
{
    /// <summary>
    /// Creates a new SMSOrganizationalGroupID from a string value
    /// </summary>
    /// <param name="value">The group code string</param>
    public SMSOrganizationalGroupID(string value) : base(value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Organizational group ID cannot be null or empty.", nameof(value));
    }

    /// <summary>
    /// Implicit conversion from string to SMSOrganizationalGroupID
    /// </summary>
    public static implicit operator SMSOrganizationalGroupID(string value) => new(value);

    /// <summary>
    /// Implicit conversion from SMSOrganizationalGroupID to string
    /// </summary>
    public static implicit operator string(SMSOrganizationalGroupID id) => id.Value;

    /// <summary>
    /// Returns the string representation of the organizational group ID
    /// </summary>
    public override string ToString() => Value;
}
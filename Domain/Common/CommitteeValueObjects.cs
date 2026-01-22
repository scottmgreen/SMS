namespace SMS_Domain.Common;

/// <summary>
/// Committee identifier value object
/// </summary>
public sealed class CommitteeID : BaseID<string>
{
    public CommitteeID(string value) : base(value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Committee ID cannot be null or empty", nameof(value));
    }

    public static implicit operator CommitteeID(string value) => new(value);
    public static implicit operator string(CommitteeID id) => id.Value;
}

/// <summary>
/// Committee meeting identifier value object
/// </summary>
public sealed class MeetingID : BaseID<string>
{
    public MeetingID(string value) : base(value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Meeting ID cannot be null or empty", nameof(value));
    }

    public static implicit operator MeetingID(string value) => new(value);
    public static implicit operator string(MeetingID id) => id.Value;
}

/// <summary>
/// Committee membership identifier value object
/// </summary>
public sealed class MembershipID : BaseID<string>
{
    public MembershipID(string value) : base(value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Membership ID cannot be null or empty", nameof(value));
    }

    public static implicit operator MembershipID(string value) => new(value);
    public static implicit operator string(MembershipID id) => id.Value;
}

/// <summary>
/// Risk approval identifier value object
/// </summary>
public sealed class RiskApprovalID : BaseID<string>
{
    public RiskApprovalID(string value) : base(value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Risk Approval ID cannot be null or empty", nameof(value));
    }

    public static implicit operator RiskApprovalID(string value) => new(value);
    public static implicit operator string(RiskApprovalID id) => id.Value;
}

/// <summary>
/// Meeting attendee identifier value object
/// </summary>
public sealed class AttendeeID : BaseID<string>
{
    public AttendeeID(string value) : base(value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Attendee ID cannot be null or empty", nameof(value));
    }

    public static implicit operator AttendeeID(string value) => new(value);
    public static implicit operator string(AttendeeID id) => id.Value;
}

/// <summary>
/// Agenda item identifier value object
/// </summary>
public sealed class AgendaItemID : BaseID<string>
{
    public AgendaItemID(string value) : base(value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Agenda Item ID cannot be null or empty", nameof(value));
    }

    public static implicit operator AgendaItemID(string value) => new(value);
    public static implicit operator string(AgendaItemID id) => id.Value;
}
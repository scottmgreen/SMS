namespace SMS_Domain.Enums;

/// <summary>
/// Investigation Status Smart Enumeration
/// Represents the various stages of an investigation in the SMS investigation process
/// </summary>
public sealed class InvestigationStatus : BaseEnum<InvestigationStatus>
{
    public static readonly InvestigationStatus InProgress = new("IN_PROGRESS", nameof(InProgress));
    public static readonly InvestigationStatus Completed = new("COMPLETED", nameof(Completed));
    public static readonly InvestigationStatus OnHold = new("ON_HOLD", nameof(OnHold));
    public static readonly InvestigationStatus Cancelled = new("CANCELLED", nameof(Cancelled));

    public InvestigationStatus(string value, string name) : base(value, name)
    {
    }

    
}



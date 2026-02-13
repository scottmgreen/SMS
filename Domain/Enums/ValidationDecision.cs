namespace SMS_Domain.Enums;

/// <summary>
/// SMS Report validation decision types for determining if a report constitutes an SMS risk
/// </summary>
public abstract class ValidationDecision : BaseEnum<ValidationDecision>
{
    protected ValidationDecision(string value, string name) : base(value, name)
    {
       
    }

    

    #region Validation Decision Types

    /// <summary>Report constitutes an SMS risk and requires formal risk assessment</summary>
    public static readonly ValidationDecision SmsRisk = new SmsRiskDecision();

    /// <summary>Report does not constitute an SMS risk and should be referred or closed</summary>
    public static readonly ValidationDecision NotSmsRisk = new NotSmsRiskDecision();

    /// <summary>More information is needed before determining SMS risk classification</summary>
    public static readonly ValidationDecision NeedsInvestigation = new NeedsInvestigationDecision();

    #endregion

    #region Implementations

    private sealed class SmsRiskDecision : ValidationDecision
    {
        public SmsRiskDecision() : base("SMS_RISK", "SMS RISK")
        {
        }
    }

    private sealed class NotSmsRiskDecision : ValidationDecision
    {
        public NotSmsRiskDecision() : base("NOT_SMS_RISK", "NOT SMS RISK")
        {
        }
    }

    private sealed class NeedsInvestigationDecision : ValidationDecision
    {
        public NeedsInvestigationDecision() : base("NEEDS_INVESTIGATION", "NEEDS INVESTIGATION")
        {
        }
    }

    #endregion

    /// <summary>
    /// Gets all available validation decision values
    /// </summary>
    public static IEnumerable<ValidationDecision> GetAllValues()
    {
        return typeof(ValidationDecision)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(ValidationDecision))
            .Select(f => (ValidationDecision)f.GetValue(null)!)
            .Where(vd => vd != null);
    }

    

    
    

    /// <summary>
    /// Parse a string value to ValidationDecision
    /// </summary>
    public static ValidationDecision FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Validation decision value cannot be null or empty", nameof(value));

        return GetAllValues().FirstOrDefault(vd => vd.Value.Equals(value, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"Invalid validation decision value: {value}", nameof(value));
    }

    /// <summary>
    /// Try to parse a string value to ValidationDecision
    /// </summary>
    public static bool TryFromValue(string? value, out ValidationDecision? validationDecision)
    {
        validationDecision = null;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        validationDecision = GetAllValues().FirstOrDefault(vd => vd.Value.Equals(value, StringComparison.OrdinalIgnoreCase));
        return validationDecision != null;
    }
}
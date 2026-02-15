namespace SMS_Domain.Enums;

public abstract class ValidationStatus : BaseEnum<ValidationStatus>
{
    protected ValidationStatus(string value, string name) : base(value, name)
    {

    }



    #region Validation Decision Types

    /// <summary>Report constitutes an SMS risk and requires formal risk assessment</summary>
    public static readonly ValidationStatus ValidationNeeded = new NeedsValidationStatus();

    /// <summary>Report does not constitute an SMS risk and should be referred or closed</summary>
    public static readonly ValidationStatus ValidationComplete = new ValidationCompletedStatus();

    /// <summary>Report does not constitute an SMS risk and should be referred or closed</summary>
    public static readonly ValidationStatus Revised = new ValidationRevisedStatus();

    #endregion

    #region Implementations

    private sealed class NeedsValidationStatus : ValidationStatus
    {
        public NeedsValidationStatus() : base("NEEDS_VALIDATION", "NEEDS_VALIDATION")
        {
        }
    }

    private sealed class ValidationCompletedStatus : ValidationStatus
    {
        public ValidationCompletedStatus() : base("VALIDATION_COMPLETED", "VALIDATION_COMPLETED")
        {
        }
    }
    private sealed class ValidationRevisedStatus : ValidationStatus
    {
        public ValidationRevisedStatus() : base("VALIDATION_REVISED", "VALIDATION_REVISED")
        {
        }
    }

    #endregion

    /// <summary>
    /// Gets all available validation decision values
    /// </summary>
    public static IEnumerable<ValidationStatus> GetAllValues()
    {
        return typeof(ValidationStatus)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(ValidationStatus))
            .Select(f => (ValidationStatus)f.GetValue(null)!)
            .Where(vd => vd != null);
    }






    /// <summary>
    /// Parse a string value to ValidationDecision
    /// </summary>
    public static ValidationStatus FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Validation decision value cannot be null or empty", nameof(value));

        return GetAllValues().FirstOrDefault(vd => vd.Value.Equals(value, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"Invalid validation decision value: {value}", nameof(value));
    }

    /// <summary>
    /// Try to parse a string value to ValidationDecision
    /// </summary>
    public static bool TryFromValue(string? value, out ValidationStatus? validationDecision)
    {
        validationDecision = null;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        validationDecision = GetAllValues().FirstOrDefault(vd => vd.Value.Equals(value, StringComparison.OrdinalIgnoreCase));
        return validationDecision != null;
    }
}
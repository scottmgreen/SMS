using System.Reflection;
using SMS_Domain.Common;

namespace SMS_Domain.Enums;

/// <summary>
/// SMS Report validation decision types for determining if a report constitutes an SMS risk
/// </summary>
public abstract class ValidationDecision : BaseEnum<ValidationDecision>
{
    protected ValidationDecision(string value, string name, string description, string actionRequired, bool requiresAssessment) : base(value, name)
    {
        Description = description;
        ActionRequired = actionRequired;
        RequiresAssessment = requiresAssessment;
    }

    public string Description { get; }
    public string ActionRequired { get; }
    public bool RequiresAssessment { get; }

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
        public SmsRiskDecision() : base("SMS_RISK", "SMS Risk",
            "Report constitutes an SMS risk requiring formal risk assessment and mitigation planning",
            "Proceed with formal risk assessment workflow", true)
        {
        }
    }

    private sealed class NotSmsRiskDecision : ValidationDecision
    {
        public NotSmsRiskDecision() : base("NOT_SMS_RISK", "Not SMS Risk",
            "Report does not constitute an SMS risk under 14 CFR Part 139 Subpart E requirements",
            "Refer to appropriate department or close with documentation", false)
        {
        }
    }

    private sealed class NeedsInvestigationDecision : ValidationDecision
    {
        public NeedsInvestigationDecision() : base("NEEDS_INVESTIGATION", "Needs Investigation",
            "Additional information required to determine SMS risk classification",
            "Gather more information before making final determination", false)
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
    /// Gets validation decisions that require formal risk assessment
    /// </summary>
    public static IEnumerable<ValidationDecision> GetAssessmentRequiredDecisions()
    {
        return GetAllValues().Where(vd => vd.RequiresAssessment);
    }

    /// <summary>
    /// Gets validation decisions that complete the validation process
    /// </summary>
    public static IEnumerable<ValidationDecision> GetFinalDecisions()
    {
        return GetAllValues().Where(vd => vd != NeedsInvestigation);
    }

    /// <summary>
    /// Checks if this decision requires proceeding to risk assessment
    /// </summary>
    public bool ShouldProceedToAssessment => this == SmsRisk;

    /// <summary>
    /// Checks if this decision closes the validation process
    /// </summary>
    public bool ClosesValidation => this == NotSmsRisk;

    /// <summary>
    /// Checks if this decision requires additional investigation
    /// </summary>
    public bool RequiresMoreInformation => this == NeedsInvestigation;

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
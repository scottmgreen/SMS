using System.Reflection;
using SMS_Domain.Common;

namespace SMS_Domain.Enums;

/// <summary>
/// Risk Assessment Category Enumeration - BUSINESS RULE ENFORCED
/// ONLY these values are allowed: Technical, Preliminary
/// </summary>
public abstract class RiskAssessmentCategory : BaseEnum<RiskAssessmentCategory>
{
    protected RiskAssessmentCategory(string value, string name, string description, int maxSteps, bool isFullProcess) : base(value, name)
    {
        Description = description;
        MaxSteps = maxSteps;
        IsFullProcess = isFullProcess;
    }

    public string Description { get; }
    public int MaxSteps { get; }
    public bool IsFullProcess { get; }

    #region Risk Assessment Categories

    /// <summary>Full 5-step SMS Technical Risk Assessment (TRA) - CurrentStep reflects last step finished (1-5)</summary>
    public static readonly RiskAssessmentCategory Technical = new TechnicalCategory();

    /// <summary>Streamlined 1-step Preliminary Risk Assessment - CurrentStep is always 1</summary>
    public static readonly RiskAssessmentCategory Preliminary = new PreliminaryCategory();

    #endregion

    #region Implementations

    private sealed class TechnicalCategory : RiskAssessmentCategory
    {
        public TechnicalCategory() : base("Technical", "Technical",
            "Full 5-step SMS Technical Risk Assessment (TRA) process", 5, true)
        {
        }
    }

    private sealed class PreliminaryCategory : RiskAssessmentCategory
    {
        public PreliminaryCategory() : base("Preliminary", "Preliminary",
            "Streamlined 1-step Preliminary Risk Assessment process", 1, false)
        {
        }
    }

    #endregion

    /// <summary>
    /// Gets all available risk assessment category values
    /// </summary>
    public static IEnumerable<RiskAssessmentCategory> GetAllValues()
    {
        return typeof(RiskAssessmentCategory)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(RiskAssessmentCategory))
            .Select(f => (RiskAssessmentCategory)f.GetValue(null)!)
            .Where(rac => rac != null);
    }

    /// <summary>
    /// Gets categories that use the full SMS process
    /// </summary>
    public static IEnumerable<RiskAssessmentCategory> GetFullProcessCategories()
    {
        return GetAllValues().Where(rac => rac.IsFullProcess);
    }

    /// <summary>
    /// Gets categories that use streamlined process
    /// </summary>
    public static IEnumerable<RiskAssessmentCategory> GetStreamlinedCategories()
    {
        return GetAllValues().Where(rac => !rac.IsFullProcess);
    }

    /// <summary>
    /// Validates if a step number is valid for this category
    /// </summary>
    public bool IsValidStep(int stepNumber)
    {
        return stepNumber >= 1 && stepNumber <= MaxSteps;
    }

    /// <summary>
    /// Checks if this is a technical assessment category
    /// </summary>
    public bool IsTechnical => this == Technical;

    /// <summary>
    /// Checks if this is a preliminary assessment category
    /// </summary>
    public bool IsPreliminary => this == Preliminary;

    /// <summary>
    /// Gets the completion percentage for a given current step
    /// </summary>
    public decimal GetCompletionPercentage(int currentStep)
    {
        if (currentStep <= 0) return 0m;
        if (currentStep >= MaxSteps) return 100m;
        return (decimal)currentStep / MaxSteps * 100m;
    }
}
namespace SMS_Domain.Enums;

/// <summary>
/// Risk Assessment Status Smart Enumeration -
/// </summary>
public abstract class RiskAssessmentStage : BaseEnum<RiskAssessmentStage>
{
    protected RiskAssessmentStage(string value, string name) : base(value, name)
    {

    }


    #region ✅ APPROVED FINAL RISK ASSESSMENT STATUS VALUES FROM StatusList.txt

    /// <summary>Risk assessor has been assigned to the assessment</summary>
    public static readonly RiskAssessmentStage DescribingSystem = new DescribeSystemStage();

    /// <summary>Assessment has been scheduled with assessor and stakeholders</summary>
    public static readonly RiskAssessmentStage IdentifyingHazards = new IdentifyingHazardsStage();

    /// <summary>Assessment is currently underway with active evaluation</summary>
    public static readonly RiskAssessmentStage AnalyizingRisk = new AnalyizingRiskStage();

    /// <summary>Assessment has been completed with final results</summary>
    public static readonly RiskAssessmentStage AssessingRisk = new AssessingRiskStage();

    /// <summary>Assessment has been completed with final results</summary>
    public static readonly RiskAssessmentStage MitigatingRisk = new MitigatingRiskStage();

    #endregion

    #region Implementations

    private sealed class DescribeSystemStage : RiskAssessmentStage
    {
        public DescribeSystemStage() : base("DESCRIBING_SYSTEM", "Describing System")
        {
        }
    }
    private sealed class IdentifyingHazardsStage : RiskAssessmentStage
    {
        public IdentifyingHazardsStage() : base("IDENTIFYING_HAZARDS", "Identifying Hazards")
        {
        }
    }
    private sealed class AnalyizingRiskStage : RiskAssessmentStage
    {
        public AnalyizingRiskStage() : base("ANALYIZING_RISK", "Analyizing Risk")
        {
        }
    }
    private sealed class AssessingRiskStage : RiskAssessmentStage
    {
        public AssessingRiskStage() : base("ASSESSING_RISK", "Assessing Risk")
        {
        }
    }
    private sealed class MitigatingRiskStage : RiskAssessmentStage
    {
        public MitigatingRiskStage() : base("MITIGATING_RISK", "Mitigating Risk")
        {
        }
    }
    #endregion

    /// <summary>
    /// Gets all available risk assessment status values
    /// </summary>
    public static IEnumerable<RiskAssessmentStage> GetAllValues()
    {
        return typeof(RiskAssessmentStage)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(RiskAssessmentStage))
            .Select(f => (RiskAssessmentStage)f.GetValue(null)!)
            .Where(ras => ras != null);
    }

}
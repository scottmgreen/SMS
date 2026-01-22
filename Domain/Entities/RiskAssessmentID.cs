namespace SMS_Domain.Entities;

/// <summary>
/// Risk Assessment ID Value Object - Mission Critical
/// </summary>
public sealed class RiskAssessmentID : BaseID<string>
{
    public RiskAssessmentID(string id) : base(id) { }
}

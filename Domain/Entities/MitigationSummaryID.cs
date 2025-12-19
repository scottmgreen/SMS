namespace SMS_Domain.Entities;

/// <summary>
/// Mitigation Summary ID following the standardized pattern
/// </summary>
public sealed class MitigationSummaryId : BaseID<string>
{
    public MitigationSummaryId(string id) : base(id) { }
}
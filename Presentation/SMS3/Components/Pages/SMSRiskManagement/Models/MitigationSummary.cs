namespace SMS3.Components.Pages.SMSRiskManagement.Models;

/// <summary>
/// ? NEW: Summary information for a mitigation within a report context
/// </summary>
public class MitigationSummary
{
    public string MitigationCode { get; set; } = string.Empty;
    public string MitigationName { get; set; } = string.Empty;
    public string HazardCode { get; set; } = string.Empty;
    public RiskLevel HazardRiskLevel { get; set; } = default!;
    public string HazardDescription { get; set; } = string.Empty;
    public MitigationStatus Status { get; set; } = MitigationStatus.PendingApproval;
    public string AssignedTo { get; set; } = string.Empty;

    public string ApprovedBy { get; set; } = string.Empty;

    public string AssignedDepartment { get; set; } = string.Empty;
    public DateTime? TargetDate { get; set; }
    
    public bool IsOverdue => TargetDate.HasValue && TargetDate.Value < DateTime.UtcNow && Status != MitigationStatus.Complete;
}



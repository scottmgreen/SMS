
using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS3.Components.Pages.SMSListings.Models;

/// <summary>
/// View model for mitigation listing that includes Report ID and Hazard ID
/// </summary>
public class MitigationModel
{
    public Mitigation Mitigation { get; set; } = default!;
    public string Code { get; set; } = string.Empty;
    public string HazardCode { get; set; } = string.Empty;
    public string ReportCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MitigationStatus Status { get; set; } = default!;
    public int Progress { get; set; }
    public DateTime? TargetDate { get; set; }
}

namespace SMS_Domain.Entities;

/// <summary>
/// Hazard Domain Entity - Mission Critical
/// Represents identified hazards within the SMS system with comprehensive properties
/// to support the Risk Assessment Wizard and hazard management workflows
/// </summary>
public sealed class HazardReportTracking : BaseAuditableEntity
{
    // Public constructor for domain usage
    public HazardReportTracking(HazardReportTrackingID id) : base(id, "SYSTEM", DateTime.UtcNow) { }



    #region Core Properties

    public string HazardCode { get; set; } = string.Empty;
    public string? ReportCode { get; set; } = string.Empty;
    public string TrackingCode { get; set; } = string.Empty;


    #endregion Core Properties

}
